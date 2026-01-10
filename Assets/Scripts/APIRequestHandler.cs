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
}

/// <summary>
/// Static utility class for handling OpenAI API requests.
/// Use this from any MonoBehaviour by calling: StartCoroutine(APIRequestHandler.SendOpenAIRequest(..., this))
/// </summary>
public static class APIRequestHandler
{
    private static readonly string SPACE_URL = "https://jejunepixels-noexit-proxy.hf.space/chat";

    /// <summary>
    /// Sends a request to the OpenAI API with retry logic for cold starts.
    /// </summary>
    /// <param name="systemMessage">System prompt for the AI model</param>
    /// <param name="userMessage">User input message</param>
    /// <param name="characterNo">Character identifier for logging</param>
    /// <param name="temperature">AI temperature parameter (0-1)</param>
    /// <param name="model">Model name to use</param>
    /// <param name="callback">Callback function when request succeeds</param>
    /// <param name="caller">MonoBehaviour calling this (needed for coroutine)</param>
    /// <param name="retryAttempts">Internal retry counter</param>
    public static IEnumerator SendOpenAIRequest(string systemMessage, string userMessage, int characterNo, float temperature, string model, Action<OpenAIResponse> callback, MonoBehaviour caller, int retryAttempts = 0)
    {
        // Build the messages list.
        List<Message> messages = new List<Message>
        {
            new Message { role = "system", content = systemMessage },
            new Message { role = "user", content = userMessage }
        };

        // Clean up message content.
        for (int i = 0; i < messages.Count; i++)
        {
            if (!string.IsNullOrEmpty(messages[i].content))
                messages[i].content = messages[i].content.Trim().Replace("\n", " ");
        }

        // Create a conversation payload.
        ConversationWrapper conversation = new ConversationWrapper
        {
            model = model,
            temperature = temperature,
            messages = messages.ToArray()
        };

        string jsonBody = JsonUtility.ToJson(conversation);

        // Create the web request
        UnityWebRequest request = new UnityWebRequest(SPACE_URL, "POST");

        // Convert JSON string to bytes
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set headers - NO Authorization header (Hugging Face Space handles it)
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
            // Log response headers for debugging
            if (request.GetResponseHeaders() != null)
            {
                Debug.LogError("Response Headers:");
                foreach (var header in request.GetResponseHeaders())
                {
                    Debug.LogError($"  {header.Key}: {header.Value}");
                }
            }

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
                yield return caller.StartCoroutine(SendOpenAIRequest(systemMessage, userMessage, characterNo, temperature, model, callback, caller, retryAttempts + 1));
                yield break;
            }

            // Max retries reached or non-retryable error
            Debug.LogError($"❌ Request failed after {retryAttempts + 1} attempts. Giving up.");
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
            Debug.Log($"✨ Invoking callback with content: {openAIResponse.choices[0].message.content}");
            callback?.Invoke(openAIResponse);
        }

        // Clean up
        request.Dispose();
    }
}
