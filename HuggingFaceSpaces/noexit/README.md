---
title: NoExit
emoji: 🚪
colorFrom: gray
colorTo: red
sdk: static
pinned: false
license: mit
hf_oauth: true
hf_oauth_scopes:
  - inference-api
---

# NoExit

A real-time, text-driven 2D game built in Unity where the player is locked in a room with two AI-generated characters who remember everything you tell them.

Inspired by Sartre's *Huis Clos* — there is no exit.

---

## Gameplay

Type freely to converse with two characters whose names, backstories, and personalities are generated at runtime. Every piece of personal information the player reveals is silently extracted and stored — and the characters will bring it up again.

- Type text to talk; press **Enter** to submit
- Press **Enter** again to dismiss a dialogue box
- Arrow keys move the player character
- Point and click movement


---

## Technical Architecture

### Engine & Rendering
- **Unity 2D** (Universal Render Pipeline)
- **TextMeshPro** for all UI text
- **Light2D** for atmosphere and reactive flicker effects
- WebGL build served via Hugging Face Spaces

---

### AI Pipeline

All AI calls route through Hugging Face Spaces. Visitors can play free up to a per-visitor token allowance; after that, or from the start if they prefer, they sign in with their Hugging Face account and dialogue and character generation run on their own Inference Providers credits. No API keys are exposed to the client.

#### Dialogue Model
**Endpoint:** `jejunepixels-noexit-proxy.hf.space/chat`

A proxy Space that forwards chat completion requests. Includes automatic retry logic (up to 2 retries, 10s delay) to handle cold starts.

#### Personal Information Extractor
Scans every line of dialogue — from both the player and the characters — for personally identifying information using a small LLM. Two switchable backends:

| Backend | Endpoint | Model |
|---|---|---|
| Custom FastAPI | `jejunepixels-qwen3-4B-info-extractor-fastapi.hf.space/extract` | Qwen3 4B |
| HF Gradio Space | `jejunepixels-qwen3-0-6b-info-extractor-api.hf.space/extract` | Qwen3 0.6B |

Returns extracted information as a string, or `"none"` if nothing personal was found.

---

### Character Generation

At game start, two characters are generated procedurally via sequential LLM calls: first a name, then a full backstory and personality description using that name. The resulting description becomes each character's permanent system prompt for all future dialogue.

---

### Addressing System

Once a character has accumulated personal information about the player, an LLM routing call (capped at 10 tokens) decides which character should respond to the next player message — Character 1, Character 2, or both. Falls back to both responding after 3 failed attempts.

---

### Reactive Events

A timer-based event system fires unprompted questions at the player during pauses in conversation. If the player has revealed personal information, the question targets a specific piece of what was shared. Otherwise it asks a generic get-to-know-you question. Each fired event schedules the next one randomly.

---

### Dialogue Display

- Words reveal one by one with a typewriter effect
- Up to 3 dialogue boxes can be active simultaneously
- AABB overlap detection repositions boxes to prevent collisions
- Player input is locked while any dialogue is displaying
- When personal information is detected, the scene lighting flickers

---

### Data Logging

Every AI interaction is logged locally in JSONL format using `system / user / assistant` triplet structure, across four separate files for dialogue, info extraction, addressing decisions, and character generation. Intended for fine-tuning and analysis.

---

## Configurable Parameters

| Parameter | Description |
|---|---|
| Dialogue model | Model ID used for character dialogue and addressing |
| Info model | Model ID used for information extraction |
| Temperature | Separate values for dialogue and character generation |
| Max dialogue tokens | Token cap per character response (default: 150) |
| Max addressing tokens | Token cap for routing decisions (default: 10) |
| Info extractor backend | Toggle between Qwen3 4B (FastAPI) and 0.6B (Gradio) |
