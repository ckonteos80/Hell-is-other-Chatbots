using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#else
using System.IO;
#endif

/// <summary>
/// Supplies the Hugging Face access token sent with chat requests, so that
/// inference is billed to whoever is playing. With no token the proxy serves
/// the request from the visitor's free allowance instead.
/// WebGL build: the token the player got from "Sign in with Hugging Face" on the Space's landing page.
/// Editor: a personal token read from UserSettings/hf_token.txt (git-ignored), or the HF_TOKEN environment variable.
/// </summary>
public static class HFAuth
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string HFAuth_GetAccessToken();

    [DllImport("__Internal")]
    private static extern void HFAuth_ClearToken();

    [DllImport("__Internal")]
    private static extern void HFAuth_ShowSignInPrompt(string message);

    [DllImport("__Internal")]
    private static extern void HFAuth_HideSignInPrompt();

    // Read on every call: the player can sign in, or the token can expire, during a session.
    public static string GetToken()
    {
        return HFAuth_GetAccessToken();
    }

    public static void ClearToken()
    {
        HFAuth_ClearToken();
    }

    public static void ShowSignInPrompt(string message)
    {
        HFAuth_ShowSignInPrompt(message);
    }

    public static void HideSignInPrompt()
    {
        HFAuth_HideSignInPrompt();
    }
#else
    private static string cachedToken;

    public static string GetToken()
    {
        if (!string.IsNullOrEmpty(cachedToken))
            return cachedToken;

        string token = "";
        string path = Path.Combine(Application.dataPath, "..", "UserSettings", "hf_token.txt");
        if (File.Exists(path))
            token = File.ReadAllText(path).Trim();

        if (string.IsNullOrEmpty(token))
            token = (System.Environment.GetEnvironmentVariable("HF_TOKEN") ?? "").Trim();

        cachedToken = token;
        return token;
    }

    // The sign-in prompt lives in the web page; outside a WebGL build there is nothing to show.
    public static void ClearToken() { }
    public static void ShowSignInPrompt(string message) { }
    public static void HideSignInPrompt() { }
#endif
}
