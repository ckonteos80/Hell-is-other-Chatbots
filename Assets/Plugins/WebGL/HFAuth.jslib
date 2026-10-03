mergeInto(LibraryManager.library, {
  // Returns the Hugging Face access token stored by the Space's landing page
  // (index.html, key "noexit_hf_oauth"), or "" if missing or expired.
  HFAuth_GetAccessToken: function () {
    var token = "";
    try {
      var stored = JSON.parse(window.localStorage.getItem("noexit_hf_oauth"));
      if (stored && stored.accessToken &&
          new Date(stored.accessTokenExpiresAt).getTime() > Date.now()) {
        token = stored.accessToken;
      }
    } catch (e) {}

    var size = lengthBytesUTF8(token) + 1;
    var buffer = _malloc(size);
    stringToUTF8(token, buffer, size);
    return buffer;
  },

  // Forgets a stored token that Hugging Face rejected, so the player is asked to sign in again.
  HFAuth_ClearToken: function () {
    try {
      window.localStorage.removeItem("noexit_hf_oauth");
    } catch (e) {}
  },

  // Shows / hides the sign-in prompt defined in the WebGL template (index.html).
  HFAuth_ShowSignInPrompt: function (messagePtr) {
    var prompt = document.getElementById("hf-signin-prompt");
    if (!prompt) return;
    document.getElementById("hf-signin-message").textContent = UTF8ToString(messagePtr);
    prompt.style.display = "flex";
  },

  HFAuth_HideSignInPrompt: function () {
    var prompt = document.getElementById("hf-signin-prompt");
    if (prompt) prompt.style.display = "none";
  }
});
