using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

public static class InfoExtractorHandler_HuggingFace
{
    private static readonly string BASE_URL = "https://jejunepixels-qwen3-0.6B-info-extractor-demo.hf.space";
    private static readonly string API_URL = BASE_URL + "/api/predict/";
    
    // Default inference parameters for Hugging Face Space
    private static readonly float DEFAULT_TEMPERATURE = 0.3f;
    private static readonly float DEFAULT_TOP_P = 0.8f;
    private static readonly int DEFAULT_MAX_TOKENS = 256;

    /// <summary>
    /// Extracts personal information from text using Hugging Face Space Gradio API
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
        // Build Gradio API request with data array format
        var request = new GradioRequest
        {
            data = new object[]
            {
                text,
                systemPrompt,
                DEFAULT_TEMPERATURE,
                DEFAULT_TOP_P,
                DEFAULT_MAX_TOKENS
            }
        };

        string jsonPayload = JsonUtility.ToJson(request);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 180;
            webRequest.certificateHandler = new BypassCertificateHandler();

            Debug.Log($"[InfoExtractor-HF] Calling Hugging Face Space API for: {text.Substring(0, Mathf.Min(50, text.Length))}...");
            Debug.Log($"[InfoExtractor-HF] URL: {API_URL}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[InfoExtractor-HF] Error: {webRequest.error}");
                Debug.LogError($"[InfoExtractor-HF] Response: {webRequest.downloadHandler.text}");
                callback?.Invoke("none");
                yield break;
            }

            try
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"[InfoExtractor-HF] Response: {responseText}");

                var response = JsonUtility.FromJson<GradioResponse>(responseText);

                if (response != null && response.data != null && response.data.Length > 0)
                {
                    string result = response.data[0].Trim();

                    if (result.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning("[InfoExtractor-HF] No personal information found");
                        callback?.Invoke("none");
                    }
                    else
                    {
                        Debug.Log($"[InfoExtractor-HF] Extraction successful: {result}");
                        callback?.Invoke(result);
                    }
                }
                else
                {
                    Debug.LogWarning("[InfoExtractor-HF] Empty response");
                    callback?.Invoke("none");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InfoExtractor-HF] Parse error: {ex.Message}");
                callback?.Invoke("none");
            }
        }
    }
}

[System.Serializable]
public class GradioRequest
{
    public object[] data;
}

[System.Serializable]
public class GradioResponse
{
    public string[] data;
}

/// <summary>
/// Custom certificate handler to bypass SSL certificate validation
/// Used for Hugging Face Space API which may have certificate issues
/// </summary>
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Accept all certificates (development only)
        return true;
    }
}
