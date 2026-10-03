using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

// Serializable data classes for OpenAI API communication
[Serializable]
public class OpenAIResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ConversationWrapper
{
    public string model;
    public float temperature;
    public Message[] messages;
    public int max_tokens;
    public string provider;
}

/// <summary>
/// Static utility class for handling OpenAI API requests.
/// Use this from any MonoBehaviour by calling: StartCoroutine(APIRequestHandler.SendOpenAIRequest(..., this))
/// </summary>
public static class APIRequestHandler
{
    private static readonly string SPACE_URL = "https://jejunepixels-noexit-proxy.hf.space/chat";

    // Set from CharacterController — if true, routes requests to Hugging Face; otherwise OpenAI
    public static bool useHuggingFaceProvider;

    // Player-facing reason for the most recent failed request (null after a success).
    // Lets callers show why a request failed instead of only logging it.
    public static string lastError;

    /// <summary>
    /// Sends a request to the OpenAI API with retry logic for cold starts.
    /// </summary>
    /// <param name="systemMessage">System prompt for the AI model</param>
    /// <param name="userMessage">User input message</param>
    /// <param name="characterNo">Character identifier for logging</param>
    /// <param name="temperature">AI temperature parameter (0-1)</param>
    /// <param name="model">Model name to use</param>
    /// <param name="maxTokens">Maximum tokens to generate (0 for unlimited)</param>
    /// <param name="callback">Callback function when request succeeds</param>
    /// <param name="caller">MonoBehaviour calling this (needed for coroutine)</param>
    /// <param name="retryAttempts">Internal retry counter</param>
    public static IEnumerator SendOpenAIRequest(string systemMessage, string userMessage, int characterNo, float temperature, string model, int maxTokens, Action<OpenAIResponse> callback, MonoBehaviour caller, int retryAttempts = 0)
    {
        string provider = useHuggingFaceProvider ? "hf" : "openai";

        // Build the messages list.
        List<Message> messages = new List<Message>
        {
            new Message { role = "system", content = systemMessage },
            new Message { role = "user", content = userMessage }
        };

        // Normalize message content — trim whitespace and unify line endings to LF.
        for (int i = 0; i < messages.Count; i++)
        {
            if (!string.IsNullOrEmpty(messages[i].content))
                messages[i].content = messages[i].content.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
        }

        // Create a conversation payload.
        ConversationWrapper conversation = new ConversationWrapper
        {
            model = model,
            temperature = temperature,
            messages = messages.ToArray(),
            max_tokens = maxTokens,
            provider = provider
        };

        string jsonBody = JsonUtility.ToJson(conversation);

        // With a token, inference is billed to the signed-in player. Without one the
        // proxy serves the request from this visitor's free allowance, until it runs out.
        string token = HFAuth.GetToken();
        bool hasToken = !string.IsNullOrEmpty(token);

        // Create the web request
        UnityWebRequest request = new UnityWebRequest(SPACE_URL, "POST");

        // Convert JSON string to bytes
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set headers - the player's Hugging Face token (if signed in) is forwarded by the proxy Space
        if (hasToken)
            request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("User-Agent", "UnityPlayer");

        // Set timeout for Hugging Face Spaces (they can be slow on first request/cold start)
        request.timeout = 60;

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ HTTP {request.responseCode} — {request.error}");
            if (!string.IsNullOrEmpty(request.downloadHandler?.text))
                Debug.LogError($"Response body: {request.downloadHandler.text}");

            // ✅ RETRY LOGIC for cold starts (503/504), timeouts, or connection errors
            bool shouldRetry = (request.responseCode == 503 ||
                               request.responseCode == 504 ||
                               request.responseCode == 0 || // Connection failed
                               request.error.Contains("timeout") ||
                               request.error.Contains("Cannot connect") ||
                               request.error.Contains("Could not resolve host"));

            if (shouldRetry && retryAttempts < 2)
            {
                Debug.LogWarning($"⏳ Space might be starting up or connection issue. Retrying in 10 seconds...");
                request.Dispose();
                yield return new WaitForSeconds(10f);

                // Retry recursively
                yield return caller.StartCoroutine(SendOpenAIRequest(systemMessage, userMessage, characterNo, temperature, model, maxTokens, callback, caller, retryAttempts + 1));
                yield break;
            }

            // Sign-in needed: free play has run out (or is unavailable), or the stored
            // sign-in was rejected. Ask the player to sign in, wait until they have,
            // then send this same request again so the game carries on where it was.
            string errorBody = request.downloadHandler?.text ?? "";
            bool freeLimitReached = errorBody.Contains("free_limit_reached");
            bool tokenRejected = hasToken && request.responseCode == 401;

            if (freeLimitReached || errorBody.Contains("sign_in_required") || tokenRejected)
            {
                request.Dispose();

                string prompt = freeLimitReached
                    ? "Your free play has run out. Sign in with Hugging Face to keep playing on your own inference credits."
                    : tokenRejected
                        ? "Your Hugging Face sign-in has expired. Sign in again to keep playing."
                        : "Sign in with Hugging Face to play.";

#if UNITY_WEBGL && !UNITY_EDITOR
                if (tokenRejected)
                    HFAuth.ClearToken();

                Debug.LogWarning($"🔐 {prompt}");
                HFAuth.ShowSignInPrompt(prompt);
                while (string.IsNullOrEmpty(HFAuth.GetToken()))
                    yield return new WaitForSecondsRealtime(1f);
                HFAuth.HideSignInPrompt();

                yield return caller.StartCoroutine(SendOpenAIRequest(systemMessage, userMessage, characterNo, temperature, model, maxTokens, callback, caller, retryAttempts));
#else
                lastError = prompt + " In the editor, put a valid personal token in UserSettings/hf_token.txt.";
                Debug.LogError($"❌ {lastError}");
#endif
                yield break;
            }

            // Max retries reached or non-retryable error
            if (request.responseCode == 401 || request.responseCode == 403)
                lastError = "Your Hugging Face sign-in was not accepted. Go back to the start page and sign in again.";
            else if (request.responseCode == 402)
                lastError = "Your Hugging Face inference credits have run out. Add credits on huggingface.co to keep playing.";
            else
                lastError = "The request to the chat service failed. Please try again.";

            Debug.LogError($"❌ Request failed after {retryAttempts + 1} attempts. Giving up. {lastError}");
            request.Dispose();
            yield break;
        }
        else
        {
            // Request successful
            string response = request.downloadHandler.text;

            // Parse the JSON response
            OpenAIResponse openAIResponse = JsonUtility.FromJson<OpenAIResponse>(response);

            // Validate the response structure
            if (openAIResponse == null || openAIResponse.choices == null || openAIResponse.choices.Length == 0)
            {
                request.Dispose();
                yield break;
            }

            // Validate that we have message content
            if (openAIResponse.choices[0].message == null ||
                string.IsNullOrEmpty(openAIResponse.choices[0].message.content))
            {
                Debug.LogError("❌ Response message is null or empty");
                request.Dispose();
                yield break;
            }

            // Success! Invoke the callback with the parsed response
            lastError = null;
            Debug.Log($"✨ Invoking callback with content: {openAIResponse.choices[0].message.content}");
            callback?.Invoke(openAIResponse);
        }

        // Clean up
        request.Dispose();
    }
}
