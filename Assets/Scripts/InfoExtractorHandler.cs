using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

public static class InfoExtractorHandler
{
    // API URLs
    private static readonly string Qween4B = "https://jejunepixels-qwen3-4B-info-extractor-fastapi.hf.space/extract";
    private static readonly string Qween0_6 = "https://jejunepixels-qwen3-0-6b-info-extractor-api.hf.space/extract";
    
    // Public toggle - set from CharacterController
    public static bool useQween0_6;
  
    /// <summary>
    /// Extracts personal information from text with custom system prompt
    /// </summary>
    /// <param name="text">The text to extract information from</param>
    /// <param name="systemPrompt">Custom system prompt for the extraction</param>
    /// <param name="callback">Callback function when request completes, returns extracted info</param>
    /// <param name="caller">MonoBehaviour calling this (needed for coroutine)</param>
    public static void ExtractInfo(string text, string systemPrompt, Action<string> callback, MonoBehaviour caller, int maxTokens = 50, float temperature = 0f)
    {
        caller.StartCoroutine(ExtractInfoCoroutine(text, systemPrompt, callback, maxTokens, temperature));
    }

    private static IEnumerator ExtractInfoCoroutine(string text, string systemPrompt, Action<string> callback, int maxTokens = 50, float temperature = 0f)
    {
        // Select API URL based on bool
        string apiUrl = useQween0_6 ? Qween0_6 : Qween4B;
        string apiLabel = useQween0_6 ? "Qwen3-0.6B" : "Qwen3-4B";

        var request = new ExtractionRequest
        {
            text = text,
            system_prompt = systemPrompt,
            max_tokens = maxTokens,
            temperature = temperature
        };

        string jsonPayload = JsonUtility.ToJson(request);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 180; // 3 minute timeout
            webRequest.certificateHandler = new AcceptAllCertificatesHandler();

            Debug.Log($"[InfoExtractor] Using {apiLabel} API");
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
    public int max_tokens;
    public float temperature;
}

[System.Serializable]
public class ExtractionResponse
{
    public string result;
}
