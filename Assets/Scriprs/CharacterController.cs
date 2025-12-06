using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;


// Global Models
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

[Serializable]
public class DialogueEntry
{
    public int characterId;       // e.g., 0 for user, 1 for Character1, etc.
    public string characterName;  // e.g., "Chris", "Alex", etc.
    public string dialogueText;   // The dialogue line itself

    public DialogueEntry(int id, string name, string text)
    {
        characterId = id;
        characterName = name;
        dialogueText = text;
    }
}

public class CharacterController : MonoBehaviour
{
    // References and API details.
    Master myMaster;

    // Scene elements.
    public GameObject dialoguePrefab;
    //public List<GameObject> Dialogues;



    public List<PersonController> Characters; // Assumes PersonController has fields: myName, myCharacter, infoShared, dialogueHolder, etc.

    // Dialogue and event tracking.
    public List<DialogueEntry> dialogueEntries;
    public List<string> events;

    [TextArea(3, 10)]
    //  public string characterUser;
    public string mainGameSystemPrompt;
    //[TextArea(3, 10)]    // Prompts and summaries.
    //public string baseSetup;
    [TextArea(3, 10)]
    public string characterSetupSystemPrompt;
    [TextArea(3, 10)]
    public string characterSetupUserPrompt;
    // [TextArea(3, 10)]
    // //  public string characterUser;
    // public string characterNameSystemPrompt;
    [TextArea(3, 10)]
    public string characterNameUserPrompt; // Used when generating a name.
    [TextArea(3, 10)]
    public string characterDialogueSystemPrompt; // Used when generating a name.
    [TextArea(3, 10)]
    public string characterDialogueSystemPromptEnding;

    [TextArea(3, 10)]
    public string characterDialogueUserPromptEnding;

    [TextArea(3, 10)]
    public string characterDialogueUserPromptEnding2;
    [TextArea(3, 10)]
    public string infoExtractionSystemPrompt;


    public List<string> systemPrompts; // One per character.

    // Model parameters.
    public float temp;         // For dialogue requests.
    public float charaterTemp; // For generating names/descriptions.

    saveController mySaveController;


    public string modelDialogue;
    public string modelInfo;
    [TextArea(3, 10)]
    public string adressingSystemPrompt;


    private string spaceUrl = "https://jejunepixels-openai-proxy.hf.space/chat";

    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        ///    apiKey = myMaster.key; // Get the API key from your Master script.
        mySaveController = GetComponent<saveController>();

        // Start generating names for the characters.
        StartCoroutine(GenerateCharacterName(1));
        StartCoroutine(GenerateCharacterName(2));

    }

    // Called when the user sends a message.
    public void ParsedText(string userInput)
    {
        Debug.Log("Received input: " + userInput);
        // Log user dialogue.
        DialogueEntry entry = new DialogueEntry(0, Characters[0].name, userInput);
        dialogueEntries.Add(entry);
        events.Add(Characters[0].myName + " said " + userInput + ".");

        // Send dialogue requests for each AI character.
        // StartCoroutine(SendRequestForCharacter(userInput, 0, 1));
        // StartCoroutine(SendRequestForCharacter(userInput, 0, 2));

        StartCoroutine(CheckHumanPersonalInfo(userInput, 0));

        if (Characters[1].infoShared.Count == 0 && Characters[2].infoShared.Count == 0)
        {
            StartCoroutine(SendRequestForCharacter(userInput, 0, 1));
            StartCoroutine(SendRequestForCharacter(userInput, 0, 2));
        }
        if (Characters[1].infoShared.Count != 0 || Characters[2].infoShared.Count != 0)
        {
            StartCoroutine(SendRequestForAdress(userInput, 0));
        }
    }

    // Generic coroutine that sends an OpenAI API request.
    public IEnumerator SendOpenAIRequest(string systemMessage, string userMessage, int characterNo, float temperature, string model, Action<OpenAIResponse> callback, int retryAttempts = 0)
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

        // // ✅ DETAILED LOGGING
        // Debug.Log("═══════════════════════════════════════");
        // Debug.Log($"🌐 Sending Request - Attempt {retryAttempts + 1}/3");
        // Debug.Log($"📍 URL: {spaceUrl}");
        // Debug.Log($"🎯 Character: {characterNo}");
        // Debug.Log($"🤖 Model: {model}");
        // Debug.Log($"🌡️ Temperature: {temperature}");
        // Debug.Log($"📤 JSON Body: {jsonBody}");
        // Debug.Log("═══════════════════════════════════════");

        // Create the web request
        UnityWebRequest request = new UnityWebRequest(spaceUrl, "POST");

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
            // Debug.LogError("═══════════════════════════════════════");
            // Debug.LogError($"❌ REQUEST FAILED - Attempt {retryAttempts + 1}/3");
            // Debug.LogError($"Character: {characterNo}");
            // Debug.LogError($"URL: {spaceUrl}");
            // Debug.LogError($"Error: {request.error}");
            // Debug.LogError($"Response Code: {request.responseCode}");
            // Debug.LogError($"Response Body: {request.downloadHandler.text}");

            // Log response headers for debugging
            if (request.GetResponseHeaders() != null)
            {
                Debug.LogError("Response Headers:");
                foreach (var header in request.GetResponseHeaders())
                {
                    Debug.LogError($"  {header.Key}: {header.Value}");
                }
            }
            Debug.LogError("═══════════════════════════════════════");

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
                yield return StartCoroutine(SendOpenAIRequest(systemMessage, userMessage, characterNo, temperature, model, callback, retryAttempts + 1));
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

            // Debug.Log("═══════════════════════════════════════");
            // Debug.Log($"✅ SUCCESS - Character {characterNo}");
            // Debug.Log($"📥 Raw Response: {response}");
            // Debug.Log("═══════════════════════════════════════");

            // Parse the JSON response
            OpenAIResponse openAIResponse = JsonUtility.FromJson<OpenAIResponse>(response);

            // Validate the response structure
            if (openAIResponse == null || openAIResponse.choices == null || openAIResponse.choices.Length == 0)
            {
                // Debug.LogError("═══════════════════════════════════════");
                // Debug.LogError("❌ Response parsing failed - Invalid JSON structure");
                // Debug.LogError($"Received response: {response}");
                // Debug.LogError("Expected structure: {\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"...\"}}]}");
                // Debug.LogError("═══════════════════════════════════════");
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
    public IEnumerator SendRequestForAdress(string userMessage, int characterSpeakingNo, int retryAttempts = 0)
    {
        // Build the final system prompt by appending current events.
        //        string finalSystemPrompt = characterDialogueSystemPrompt+ "\n"+;

        string character1info = "";
        if (Characters[1].infoShared.Count != 0)
        {
            string allInfo = "";
            foreach (string info in Characters[1].infoShared)
            {
                allInfo += "\"" + info + "\"\n";
            }
            character1info = "Character 1 information know: \n" + allInfo + "\n ";
            //    "User: "  +
        }
        else
        {
            character1info = "Nothing known for Character 1";
        }

        string character2info = "";
        if (Characters[2].infoShared.Count != 0)
        {
            string allInfo = "";
            foreach (string info in Characters[2].infoShared)
            {
                allInfo += "\"" + info + "\"\n";
            }
            character2info = "Character 2 information know: \n" + allInfo + "\n ";
            //    "User: "  +
        }
        else
        {
            character2info = "Nothing known for Character 2";
        }


        string AdressSystemPromptComplete = adressingSystemPrompt + "\n " + character1info + "\n " + character2info;

        yield return StartCoroutine(SendOpenAIRequest(AdressSystemPromptComplete, userMessage, characterSpeakingNo, temp, modelDialogue, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            string reply = assistantMessage.content;

            // If the reply starts with the character's name followed by a colon or similar delimiter,
            // remove that prefix.
            Debug.Log("checked adressing got this reply: " + reply);

            if (reply == "0")
            {
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 1));
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 2));
            }
            if (reply == "1")
            {
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 1));
                //  StartCoroutine(SendRequestForCharacter(userMessage, 0, 2));
            }
            if (reply == "2")
            {
                //    StartCoroutine(SendRequestForCharacter(userMessage, 0, 1));
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 2));
            }



            if (mySaveController != null)
            {
                mySaveController.NewAdressing(AdressSystemPromptComplete, userMessage, reply);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }));
    }


    public IEnumerator SendRequestForCharacter(string userMessage, int characterSpeakingNo, int characterReplyingNo, int retryAttempts = 0)
    {
        // Build the final system prompt by appending current events.
        //        string finalSystemPrompt = characterDialogueSystemPrompt+ "\n"+;
        string userPrompt = "";
        if (Characters[characterSpeakingNo].infoShared.Count != 0)
        {
            string allInfo = "";
            foreach (string info in Characters[characterSpeakingNo].infoShared)
            {
                allInfo += "\"" + info + "\"\n";
            }
            userPrompt = userMessage + "\n " + characterDialogueUserPromptEnding + "\n " + allInfo + "\n " + characterDialogueUserPromptEnding2;
            //    "User: "  +
        }
        else
        {
            userPrompt = userMessage;
            // Characters[characterSpeakingNo].myName + ": "

        }

        yield return StartCoroutine(SendOpenAIRequest(systemPrompts[characterReplyingNo], userPrompt, characterReplyingNo, temp, modelDialogue, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            string reply = assistantMessage.content.Trim();
            string charName = Characters[characterReplyingNo].myName.Trim();

            // If the reply starts with the character's name followed by a colon or similar delimiter,
            // remove that prefix.
            if (reply.StartsWith(charName))
            {
                // Try to find the colon separator
                int colonIndex = reply.IndexOf(":");
                if (colonIndex > 0 && colonIndex < reply.Length - 1)
                {
                    reply = reply.Substring(colonIndex + 1).Trim();
                }
                else
                {
                    // Otherwise, remove the character name from the beginning.
                    reply = reply.Substring(charName.Length).Trim();
                }
            }

            //    Debug.Log("Sent system Prompt: " + userPrompt + "\n User message: \n" + userMessage + "\n and I got this reply: \n" + reply);

            // Debug.Log("Processed reply for Character " + characterReplyingNo + ": " + reply);


            // Add dialogue entry and event.
            DialogueEntry entry = new DialogueEntry(characterReplyingNo, Characters[characterReplyingNo].name, reply);
            dialogueEntries.Add(entry);
            events.Add(Characters[characterReplyingNo].myName + " said: " + reply + " .");

            // Optionally, check for personal info.
            StartCoroutine(CheckPersonalInfo(reply, characterReplyingNo));

            // Display the dialogue.
            ShowDialog(reply, characterReplyingNo);

            if (mySaveController != null)
            {
                mySaveController.NewEntryDialogue(systemPrompts[characterReplyingNo], userPrompt, reply);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }));
    }

    public IEnumerator CheckPersonalInfo(string reply, int characterNo)
    {
        float tempPersonalInfo = 0.0f;
        // Build a system prompt that compares the user input with the character description.

        //  systemPrompts[characterNo] +
        string finalSystemPromptCharacterInfo = Characters[characterNo].myCharacter +
            "\n" + infoExtractionSystemPrompt;

        Debug.Log("Checking personal info for character " + characterNo + ": " + finalSystemPromptCharacterInfo);

        yield return StartCoroutine(SendOpenAIRequest(finalSystemPromptCharacterInfo, reply, characterNo, tempPersonalInfo, modelInfo, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            string result = assistantMessage.content.Trim();

            Debug.Log("Sent system Prompt: " + finalSystemPromptCharacterInfo + "\n User message: \n" + reply + "\n and I got this reply: \n" + result);


            if (result.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("No personal info revealed for character " + characterNo);
            }
            else
            {
                addPersonalInfo(characterNo, result);
            }

            // Check that mySaveController is not null before calling NewEntry.
            if (mySaveController != null)
            {
                mySaveController.NewEntryInfo(finalSystemPromptCharacterInfo, reply, assistantMessage.content);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }));

    }
    public IEnumerator CheckHumanPersonalInfo(string reply, int characterNo)
    {
        // {systemPrompts[characterNo] +
        // Build a system prompt that compares the user input with the character description.
        string finalSystemPromptHumanInfo =
       "\nYou are a strict extractor of personal information for the human player. " +
       "Analyze the text provided below and extract any personal details mentioned (such as name, age, location, background, occupation, hobbies, etc.). " +
       "If no personal information is found, output exactly:\n" +
       "none\n" +
       "Do not include any extra text, greetings, or commentary.";

        // Debug.Log("Checking personal info for character " + characterNo + ": " + finalSystemPromptCharacterInfo);

        yield return StartCoroutine(SendOpenAIRequest(finalSystemPromptHumanInfo, reply, characterNo, 0.0f, modelInfo, (response) =>
            {
                Message assistantMessage = response.choices[0].message;
                string result = assistantMessage.content.Trim();

                if (result.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("No personal info revealed for character " + characterNo);
                }
                else
                {
                    addPersonalInfo(characterNo, result);
                }

                if (mySaveController != null)
                {
                    mySaveController.NewEntryInfo(finalSystemPromptHumanInfo, reply, assistantMessage.content);
                }
                else
                {
                    Debug.LogError("mySaveController is not assigned.");
                }

                Debug.Log("Sent system Prompt for human: " + finalSystemPromptHumanInfo + "\n User message: \n" + reply + "\n and I got this reply: \n" + result);
            }));
    }




    void addPersonalInfo(int characterNo, string response)
    {
        if (response != "none")
        {
            Characters[characterNo].infoShared.Add(response);
        }
    }

    // Uses the generic request to generate a character name.
    private IEnumerator GenerateCharacterName(int characterNo)
    {

        yield return StartCoroutine(SendOpenAIRequest(mainGameSystemPrompt + " " + characterSetupSystemPrompt, characterNameUserPrompt, characterNo, charaterTemp, modelDialogue, (response) =>
        {
            Debug.Log("Started");
            string generatedName = response.choices[0].message.content;
            // Debug.Log("Generated name for Character " + characterNo + ": " + generatedName);
            Characters[characterNo].myName = generatedName;
            // After name generation, request a character description.
            StartCoroutine(GenerateCharacterDescription(characterNo, generatedName));
        }));
    }

    // Uses the generic request to generate a character description.
    private IEnumerator GenerateCharacterDescription(int characterNo, string characterName)
    {
        string prompt = mainGameSystemPrompt + " " + characterSetupSystemPrompt;    //     ". Include: name, age, place born, job, cause of death.";
        yield return StartCoroutine(SendOpenAIRequest(prompt, characterSetupUserPrompt + " " + characterName, characterNo, charaterTemp, modelDialogue, (response) =>
        {
            string description = response.choices[0].message.content;
            // Debug.Log("Generated description for Character " + characterNo + ": " + description);
            Characters[characterNo].myCharacter = description;
            systemPrompts[characterNo] = characterDialogueSystemPrompt + "\n " + description + "\n " + characterDialogueSystemPromptEnding;


            if (mySaveController != null)
            {
                mySaveController.NewCharacterInfo(prompt, characterSetupUserPrompt, description);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }));
    }

    // Displays dialogue in the specified character's dialogue holder.
    public void ShowDialog(string message, int characterNo)
    {
        GameObject newObject = Instantiate(dialoguePrefab, Characters[0].dialogueHolder);
        newObject.transform.SetParent(Characters[characterNo].dialogueHolder);
        newObject.name = "character" + characterNo.ToString();
        newObject.transform.localPosition = Vector3.zero;
        // Debug.Log("Displaying for Character " + characterNo + ": " + message);
        dialogueDisplay myDialogueDisplay = newObject.GetComponent<dialogueDisplay>();
        myDialogueDisplay.showMessage(message);
        myMaster.theOverlayController.addImage(myDialogueDisplay);
    }

}
