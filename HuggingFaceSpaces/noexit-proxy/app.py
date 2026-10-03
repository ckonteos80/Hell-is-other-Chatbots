import os
import hashlib
from fastapi import FastAPI, Header, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Dict, List, Literal, Optional
import httpx
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

HF_ROUTER_URL = "https://router.huggingface.co/v1/chat/completions"

# Two ways a request is paid for:
#   - Signed-in players send their own Hugging Face token (OAuth, scope inference-api).
#     It is forwarded as-is, so inference is billed to that player.
#   - Visitors without a token play for free on the Space owner's token (HF_TOKEN secret)
#     until they have used FREE_TOKENS_PER_VISITOR tokens; then they must sign in.
HF_TOKEN = os.environ.get("HF_TOKEN")

# Free allowance per visitor, in tokens as reported by the provider. 0 turns free play off.
FREE_TOKENS_PER_VISITOR = int(os.environ.get("FREE_TOKENS_PER_VISITOR", "40000"))

# Free play is limited to these models and to this reply length, so the allowance
# cannot be spent on an expensive model by calling this proxy directly.
FREE_MODELS = {
    m.strip()
    for m in os.environ.get("FREE_MODELS", "Qwen/Qwen2.5-72B-Instruct").split(",")
    if m.strip()
}
FREE_MAX_TOKENS_PER_REQUEST = int(os.environ.get("FREE_MAX_TOKENS_PER_REQUEST", "600"))

if not HF_TOKEN:
    logger.warning("HF_TOKEN not set — free play is unavailable, every player must sign in")

# Tokens used on the free path, per visitor address. Kept in memory only, so it
# resets whenever the Space restarts.
free_usage: Dict[str, int] = {}

app = FastAPI(title="Proxy for Unity")

# Enable CORS for Unity WebGL builds
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify your domain
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class Message(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    model: str
    temperature: float
    messages: List[Message]
    # Kept so existing Unity builds (which always send it) still validate.
    # Only "hf" is served; "openai" is rejected below.
    provider: Literal["openai", "hf"] = "hf"
    # Token cap for the reply. Unity sends 0 to mean "no cap".
    max_tokens: Optional[int] = None
    # Per-request read timeout. Optional: callers that omit it (Unity) keep the
    # previous fixed 60s behaviour. Character generation asks for more because it
    # is a once-per-session call where latency does not matter.
    timeout: float = 60.0
    # Thinking budget for reasoning models ("low", "high", "max" on GLM-5.3 / Kimi K3).
    # Optional and only forwarded when set: at their default (max) these models run
    # past the HF router's ~120s gateway timeout, and every other caller stays unchanged.
    reasoning_effort: Optional[str] = None

def free_play_enabled() -> bool:
    return bool(HF_TOKEN) and FREE_TOKENS_PER_VISITOR > 0

def visitor_id(http_request: Request) -> str:
    # Behind the Hugging Face proxy the caller's address is the first entry of
    # X-Forwarded-For; the direct connection address is the proxy itself.
    forwarded = http_request.headers.get("x-forwarded-for", "")
    address = forwarded.split(",")[0].strip()
    if not address and http_request.client:
        address = http_request.client.host
    return address or "unknown"

def visitor_tag(visitor: str) -> str:
    # Short hash for logs, so raw addresses are never written out.
    return hashlib.sha256(visitor.encode()).hexdigest()[:8]

def bearer_token(authorization: Optional[str]) -> Optional[str]:
    if authorization and authorization.lower().startswith("bearer "):
        token = authorization[7:].strip()
        if token:
            return token
    return None

def sign_in_required(code: str, message: str) -> HTTPException:
    # The game looks for these codes to show its "Sign in with Hugging Face" prompt.
    return HTTPException(status_code=403, detail={"code": code, "message": message})

@app.get("/")
async def root():
    return {
        "status": "running",
        "message": "Proxy Active",
        "auth": "Send your Hugging Face token in the Authorization header, or play free up to the per-visitor allowance",
        "free_play": free_play_enabled(),
        "endpoints": {
            "chat": "POST /chat",
            "free_status": "GET /free/status"
        }
    }

@app.get("/free/status")
async def free_status(http_request: Request):
    if not free_play_enabled():
        return {"enabled": False, "limit": 0, "remaining": 0}
    used = free_usage.get(visitor_id(http_request), 0)
    return {
        "enabled": True,
        "limit": FREE_TOKENS_PER_VISITOR,
        "remaining": max(FREE_TOKENS_PER_VISITOR - used, 0),
    }

@app.post("/chat")
async def proxy_chat(
    request: ChatRequest,
    http_request: Request,
    authorization: Optional[str] = Header(None),
):
    if request.provider != "hf":
        raise HTTPException(
            status_code=400,
            detail=f"Provider '{request.provider}' is not available on this proxy"
        )

    player_token = bearer_token(authorization)
    free = player_token is None
    visitor = visitor_id(http_request)
    max_tokens = request.max_tokens

    if free:
        if not free_play_enabled():
            raise sign_in_required("sign_in_required", "Sign in with Hugging Face to play")
        if request.model not in FREE_MODELS:
            raise sign_in_required("sign_in_required", "This model is not available in free play")
        if free_usage.get(visitor, 0) >= FREE_TOKENS_PER_VISITOR:
            raise sign_in_required("free_limit_reached", "Free play is used up. Sign in with Hugging Face to continue")
        if max_tokens is None or max_tokens <= 0 or max_tokens > FREE_MAX_TOKENS_PER_REQUEST:
            max_tokens = FREE_MAX_TOKENS_PER_REQUEST

    # Clamp: never unbounded. This Space runs a single uvicorn worker, so a hung
    # upstream connection would otherwise hold the only worker until restart.
    read_timeout = min(max(request.timeout, 1.0), 600.0)

    try:
        # Never log tokens or raw visitor addresses.
        logger.info(
            f"Path: {'free ' + visitor_tag(visitor) if free else 'token'} | "
            f"Model: {request.model} | Messages: {len(request.messages)} | "
            f"Max tokens: {max_tokens} | Timeout: {read_timeout}s | "
            f"Effort: {request.reasoning_effort}"
        )

        body = {
            "model": request.model,
            "temperature": request.temperature,
            "messages": [
                {"role": msg.role, "content": msg.content}
                for msg in request.messages
            ]
        }
        if max_tokens is not None and max_tokens > 0:
            body["max_tokens"] = max_tokens
        if request.reasoning_effort is not None:
            body["reasoning_effort"] = request.reasoning_effort

        # Long read, short connect: a dead provider should fail fast rather than
        # burn the whole budget before the first byte arrives.
        async with httpx.AsyncClient(
            timeout=httpx.Timeout(read_timeout, connect=10.0)
        ) as client:
            response = await client.post(
                HF_ROUTER_URL,
                json=body,
                headers={
                    "Authorization": f"Bearer {HF_TOKEN if free else player_token}",
                    "Content-Type": "application/json"
                }
            )

            if response.status_code != 200:
                logger.error(f"HF API error {response.status_code}: {response.text}")
                if free and response.status_code in (401, 402, 403):
                    # The owner's token was rejected or is out of credit. That is not
                    # the visitor's problem to fix, but they can carry on signed in.
                    raise sign_in_required("sign_in_required", "Free play is unavailable right now. Sign in with Hugging Face to play")
                raise HTTPException(
                    status_code=response.status_code,
                    detail=response.text
                )

            result = response.json()

            if free:
                usage = result.get("usage") or {}
                spent = usage.get("total_tokens")
                if not isinstance(spent, int) or spent <= 0:
                    # Provider did not report usage: estimate at roughly 4 characters per token.
                    characters = sum(len(msg.content) for msg in request.messages)
                    for choice in result.get("choices") or []:
                        characters += len(((choice.get("message") or {}).get("content")) or "")
                    spent = characters // 4 + 1
                free_usage[visitor] = free_usage.get(visitor, 0) + spent
                logger.info(
                    f"Free visitor {visitor_tag(visitor)} used {spent} tokens, "
                    f"{free_usage[visitor]}/{FREE_TOKENS_PER_VISITOR} so far"
                )

            logger.info("Successfully proxied request")
            return result

    except HTTPException:
        # Pass upstream status codes (401, 402, 400...) through unchanged so the
        # game can tell "not signed in" and "out of credits" apart from a crash.
        raise
    except httpx.HTTPError as e:
        logger.error(f"HTTP error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Proxy error: {str(e)}")
    except Exception as e:
        logger.error(f"Unexpected error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Server error: {str(e)}")

@app.get("/health")
async def health():
    return {"status": "healthy", "free_play": free_play_enabled()}
