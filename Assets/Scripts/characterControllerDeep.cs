using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;


[Serializable]
public class DeepSeekResponse
{
    public DeepSeekChoice[] choices;
}

[Serializable]
public class DeepSeekChoice
{
    public DeepSeekMessage message;
}

[Serializable]
public class DeepSeekMessage
{
    public string role;
    public string content;
}

[Serializable]
public class ConversationWrapperDeep
{
    public string model;
    public DeepSeekMessage[] messages;
    public float temperature;
    public bool stream; // Add this to match DeepSeek's API.
}

[Serializable]
public class DialogueEntryDeep
{
    public int characterId;       // e.g., 0 for user, 1 for Character1, etc.
    public string characterName;  // e.g., "Chris", "Alex", etc.
    public string dialogueText;   // The dialogue line itself

    public DialogueEntryDeep(int id, string name, string text)
    {
        characterId = id;
        characterName = name;
        dialogueText = text;
    }
}
public class characterControllerDeep : MonoBehaviour
{
    Master myMaster;
    public string apiKey;
    private string apiUrl = "https://api.deepseek.com/chat/completions";
    private float retryDelay = 2f;

    // Scene elements.
    public GameObject dialoguePrefab;
    public List<GameObject> Dialogues;
    public List<PersonController> Characters; // Assumes PersonController has fields: myName, myCharacter, infoShared, dialogueHolder, etc.

    // Dialogue and event tracking.
    public List<DialogueEntryDeep> dialogueEntries;
    public List<string> events;

    // Prompts and summaries.
    public string baseSetup;
    public string characterSetup;
    public string characterUser;
    public string characterName; // Used when generating a name.
    public List<string> systemPrompts; // One per character.

    // Model parameters.
    public float temp;         // For dialogue requests.
    public float charaterTemp; // For generating names/descriptions.

    saveController mySaveController;

    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        apiKey = myMaster.DeepKey; // Get the API key from your Master script.
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
        DialogueEntryDeep entry = new DialogueEntryDeep(0, Characters[0].name, userInput);
        dialogueEntries.Add(entry);
        events.Add(Characters[0].myName + " said " + userInput + ".");

        // Send dialogue requests for each AI character.
        StartCoroutine(SendRequestForCharacter(userInput, 0, 1));
        // "the human user said: " + 
        StartCoroutine(SendRequestForCharacter(userInput, 0, 2));
        StartCoroutine(CheckHumanPersonalInfo(userInput, 0));
    }

    // Generic coroutine that sends an OpenAI API request.
    public IEnumerator SendDeepRequest(
     string systemMessage,
     string userMessage,
     int characterNo,
     float temperature,
     Action<DeepSeekResponse> callback,
     int retryAttempts = 3) // Add retryAttempts parameter
    {
        int attempts = 0;
        while (attempts < retryAttempts)
        {
            // Build the messages list.
            List<DeepSeekMessage> messages = new List<DeepSeekMessage>
        {
            new DeepSeekMessage { role = "system", content = systemMessage },
            new DeepSeekMessage { role = "user", content = userMessage }
        };

            // Clean up message content.
            for (int i = 0; i < messages.Count; i++)
            {
                if (!string.IsNullOrEmpty(messages[i].content))
                    messages[i].content = messages[i].content.Trim().Replace("\n", " ");
            }

            // Create a conversation payload.
            ConversationWrapperDeep conversation = new ConversationWrapperDeep
            {
                model = "deepseek-chat", // Use the correct DeepSeek model name.
                temperature = temperature,
                messages = messages.ToArray(),
                stream = false // Add this to match DeepSeek's API.
            };

            // Serialize the request body to JSON.
            string jsonBody = JsonUtility.ToJson(conversation);
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonBody);

            // Create and configure the UnityWebRequest.
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json"); // Add this header.
                request.SetRequestHeader("Authorization", "Bearer " + apiKey);
                request.timeout = 30; // Increase timeout to 30 seconds.

                // Send the request.
                yield return request.SendWebRequest();

                // Handle the response.
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Request failed for Character {characterNo}: {request.error}");
                    Debug.LogError($"Response Code: {request.responseCode}");
                    Debug.LogError($"Response Body: {request.downloadHandler.text}");

                    attempts++;
                    if (attempts < retryAttempts)
                    {
                        Debug.Log($"Retrying... Attempt {attempts + 1} of {retryAttempts}");
                        yield return new WaitForSeconds(retryDelay); // Wait before retrying.
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"Max retry attempts ({retryAttempts}) reached.");
                        yield break;
                    }
                }
                else
                {
                    string response = request.downloadHandler.text;
                    Debug.Log($"Raw Response for Character {characterNo}: {response}");

                    // Parse the response.
                    DeepSeekResponse deepSeekResponse = JsonUtility.FromJson<DeepSeekResponse>(response);
                    if (deepSeekResponse == null || deepSeekResponse.choices == null || deepSeekResponse.choices.Length == 0)
                    {
                        Debug.LogError($"Response did not contain expected choices for Character {characterNo}");
                        yield break;
                    }

                    // Invoke the callback to handle the response.
                    callback?.Invoke(deepSeekResponse);
                    yield break; // Exit the loop on success.
                }
            }
        }
    }

    public IEnumerator SendRequestForCharacter(string userMessage, int characterSpeakingNo, int characterReplyingNo, int retryAttempts = 0)
    {
        // Build the final system prompt by appending current events.
        string finalSystemPrompt = systemPrompts[characterReplyingNo];
        string allInfo = "";
        foreach (string info in Characters[characterSpeakingNo].infoShared)
        {
            allInfo += "\"" + info + "\"\n";
        }
        finalSystemPrompt += "\n Context: \n  The character providing the user input has revealed the following about themselves: \n" +
            allInfo + "\n When the user input is appended after this message, respond as your character considering this context.";
        Debug.Log("system prompt for Character " + characterReplyingNo + ": " + finalSystemPrompt);

        yield return StartCoroutine(SendDeepRequest(finalSystemPrompt, userMessage, characterReplyingNo, temp, (response) =>
        {
            DeepSeekMessage assistantMessage = response.choices[0].message;
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

            Debug.Log("Sent system Prompt: " + finalSystemPrompt + "\n User message: \n" + userMessage + "\n and I got this reply: \n" + reply);

            // Debug.Log("Processed reply for Character " + characterReplyingNo + ": " + reply);


            // Add dialogue entry and event.
            DialogueEntryDeep entry = new DialogueEntryDeep(characterReplyingNo, Characters[characterReplyingNo].name, reply);
            dialogueEntries.Add(entry);
            events.Add(Characters[characterReplyingNo].myName + " said: " + reply + " .");

            // Optionally, check for personal info.
            StartCoroutine(CheckPersonalInfo(reply, characterReplyingNo));

            // Display the dialogue.
            ShowDialog(reply, characterReplyingNo);

            if (mySaveController != null)
            {
                mySaveController.NewEntryDialogue(finalSystemPrompt, userMessage, reply);
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
            "\nYou are a strict extractor of personal information about the character you are playing. " +
            "You are provided with the full character description (above) and a user message below. " +
            "Compare the user message with the character description and output exactly the details that are explicitly mentioned in the user message and that also appear in the character description. " +
            "Preserve the order, formatting, and punctuation exactly as they appear in the user message. " +
            "Do not add, remove, or modify any details. " +
            "If any matching details are found, output exactly one line containing only those details, separated by commas. " +
            "If no matching details are found, output exactly: none" +
            "\nDo not include any extra text, greetings, or commentary.";

        Debug.Log("Checking personal info for character " + characterNo + ": " + finalSystemPromptCharacterInfo);

        yield return StartCoroutine(SendDeepRequest(finalSystemPromptCharacterInfo, reply, characterNo, tempPersonalInfo, (response) =>
        {
            DeepSeekMessage assistantMessage = response.choices[0].message;
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

        yield return StartCoroutine(SendDeepRequest(finalSystemPromptHumanInfo, reply, characterNo, 0.0f, (response) =>
            {
                DeepSeekMessage assistantMessage = response.choices[0].message;
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

        yield return StartCoroutine(SendDeepRequest(characterSetup, characterName, characterNo, charaterTemp, (response) =>
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
        string prompt = "Generate a unique character description for a character named: " + characterName +
            " in the game/play. Include: name, age, place born, job, cause of death.";
        yield return StartCoroutine(SendDeepRequest(characterSetup, prompt, characterNo, charaterTemp, (response) =>
        {
            string description = response.choices[0].message.content;
            // Debug.Log("Generated description for Character " + characterNo + ": " + description);
            Characters[characterNo].myCharacter = description;
            systemPrompts[characterNo] = baseSetup + description;
        }));
    }

    // Displays dialogue in the specified character's dialogue holder.
    public void ShowDialog(string message, int characterNo)
    {
        GameObject newObject = Instantiate(dialoguePrefab, Characters[0].dialogueHolder);
        newObject.transform.SetParent(Characters[characterNo].dialogueHolder);
        newObject.transform.localPosition = Vector3.zero;
        // Debug.Log("Displaying for Character " + characterNo + ": " + message);
        dialogueDisplay myDialogueDisplay = newObject.GetComponent<dialogueDisplay>();
        myDialogueDisplay.showMessage(message);
        Dialogues.Add(newObject);
    }
}
