using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

public static class InfoExtractorHandler
{
    private static readonly string API_URL = "https://jejunepixels-qwen3-info-extractor-fastapi.hf.space/extract";
    
    // Default system prompt
    private static readonly string DEFAULT_SYSTEM_PROMPT = 
        "You are a strict extractor of personal information. Analyze the text and extract personal details (name, age, location, occupation, hobbies, etc.). If no personal information is found, output exactly:\nnone";

    /// <summary>
    /// Extracts personal information from text using FastAPI with fine-tuned Qwen3
    /// </summary>
    /// <param name="text">The text to extract information from</param>
    /// <param name="callback">Callback function when request completes, returns extracted info</param>
    /// <param name="caller">MonoBehaviour calling this (needed for coroutine)</param>
    public static void ExtractInfo(string text, Action<string> callback, MonoBehaviour caller)
    {
        ExtractInfo(text, DEFAULT_SYSTEM_PROMPT, callback, caller);
    }

    /// <summary>
    /// Extracts personal information from text with custom system prompt
    /// </summary>
    /// <param name="text">The text to extract information from</param>
    /// <param name="systemPrompt">Custom system prompt for the extraction</param>
    /// <param name="callback">Callback function when request completes, returns extracted info</param>
    /// <param name="caller">MonoBehaviour calling this (needed for coroutine)</param>
    public static void ExtractInfo(string text, string systemPrompt, Action<string> callback, MonoBehaviour caller)
    {
        caller.StartCoroutine(ExtractInfoCoroutine(text, systemPrompt, callback));
    }

    private static IEnumerator ExtractInfoCoroutine(string text, string systemPrompt, Action<string> callback)
    {
        var request = new ExtractionRequest
        {
            text = text,
            system_prompt = systemPrompt
        };

        string jsonPayload = JsonUtility.ToJson(request);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 180; // 3 minute timeout

            Debug.Log($"[InfoExtractor] Calling API for: {text.Substring(0, Mathf.Min(50, text.Length))}...");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[InfoExtractor] Error: {webRequest.error}");
                Debug.LogError($"[InfoExtractor] Response: {webRequest.downloadHandler.text}");
                callback?.Invoke("none");
                yield break;
            }

            try
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"[InfoExtractor] Response: {responseText}");

                var response = JsonUtility.FromJson<ExtractionResponse>(responseText);

                if (response != null && !string.IsNullOrEmpty(response.result))
                {
                    string result = response.result.Trim();

                    if (result.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning("[InfoExtractor] No personal information found");
                        callback?.Invoke("none");
                    }
                    else
                    {
                        Debug.Log($"[InfoExtractor] Extraction successful: {result}");
                        callback?.Invoke(result);
                    }
                }
                else
                {
                    Debug.LogWarning("[InfoExtractor] Empty response");
                    callback?.Invoke("none");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InfoExtractor] Parse error: {ex.Message}");
                callback?.Invoke("none");
            }
        }
    }
}

[System.Serializable]
public class ExtractionRequest
{
    public string text;
    public string system_prompt;
}

[System.Serializable]
public class ExtractionResponse
{
    public string result;
}
