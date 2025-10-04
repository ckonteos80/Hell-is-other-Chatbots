using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

// Global Models
[Serializable]
public class GrokResponse
{
    public GrokChoice[] choices;
}

[Serializable]
public class GrokChoice
{
    public GrokMessage message;
}

[Serializable]
public class GrokMessage
{
    public string role;
    public string content;
}

[Serializable]
public class GrokConversationWrapper
{
    public string model;
    public float temperature;
    public bool stream;
    public GrokMessage[] messages;
}

[Serializable]
public class GrokDialogueEntry
{
    public int characterId;
    public string characterName;
    public string dialogueText;

    public GrokDialogueEntry(int id, string name, string text)
    {
        characterId = id;
        characterName = name;
        dialogueText = text;
    }
}

public class CharacterControllerGrok : MonoBehaviour
{
    Master myMaster;
    public string apiKeyGrok;
    private string apiUrl = "https://api.x.ai/v1/chat/completions"; // Update with actual xAI endpoint
    private float retryDelay = 2f;

    // Scene elements
    public GameObject dialoguePrefab;
    public List<GameObject> Dialogues;
    public List<PersonController> Characters;

    // Dialogue and event tracking
    public List<GrokDialogueEntry> dialogueEntries = new List<GrokDialogueEntry>(); // Updated type
    public List<string> events = new List<string>();

    // Prompts and summaries
    public string baseSetup;
    public string characterSetup;
    public string characterUser;
    public string characterName;
    public List<string> systemPrompts;

    // Model parameters
    public float temp;
    public float charaterTemp;

    saveController mySaveController;


    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        apiKeyGrok = myMaster.GrokKey;
        mySaveController = GetComponent<saveController>();

        StartCoroutine(GenerateCharacterName(1));
        StartCoroutine(GenerateCharacterName(2));
    }

    public void ParsedText(string userInput)
    {
        Debug.Log("Received input: " + userInput);
        GrokDialogueEntry entry = new GrokDialogueEntry(0, Characters[0].name, userInput);
        dialogueEntries.Add(entry);
        events.Add(Characters[0].myName + " said " + userInput + ".");
        // "the human user said: " +
        StartCoroutine(SendRequestForCharacter(userInput, 0, 1));
        StartCoroutine(SendRequestForCharacter(userInput, 0, 2));
        StartCoroutine(CheckHumanPersonalInfo(userInput, 0));
    }

    public IEnumerator SendGrokRequest(string systemMessage, string userMessage, int characterNo, float temperature, Action<GrokResponse> callback)
    {
        List<GrokMessage> messages = new List<GrokMessage>
    {
        new GrokMessage { role = "system", content = systemMessage },
        new GrokMessage { role = "user", content = userMessage }
    };

        for (int i = 0; i < messages.Count; i++)
        {
            if (!string.IsNullOrEmpty(messages[i].content))
                messages[i].content = messages[i].content.Trim().Replace("\n", " ");
        }

        GrokConversationWrapper conversation = new GrokConversationWrapper
        {
            model = "grok-2", // Set to Grok model, adjust if needed (e.g., "grok-2-latest")
            temperature = temperature,
            stream = false, // Added for Grok API compatibility, as seen in curl command
            messages = messages.ToArray()
        };

        string jsonBody = JsonUtility.ToJson(conversation);
        Debug.Log("Request JSON: " + jsonBody); // Log the request body for debugging

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKeyGrok);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed for Character " + characterNo + ": " + request.error);
            Debug.LogError("Response body: " + request.downloadHandler.text); // Log server response for debugging
            request.Dispose();
            yield break;
        }
        else
        {
            string response = request.downloadHandler.text;
            GrokResponse grokResponse = JsonUtility.FromJson<GrokResponse>(response);
            if (grokResponse == null || grokResponse.choices == null || grokResponse.choices.Length == 0)
            {
                Debug.LogError("Response did not contain expected choices for Character " + characterNo);
                request.Dispose();
                yield break;
            }
            callback?.Invoke(grokResponse);
        }
        request.Dispose();
        yield break;
    }

    public IEnumerator SendRequestForCharacter(string userMessage, int characterSpeakingNo, int characterReplyingNo, int retryAttempts = 0)
    {
        string finalSystemPrompt = systemPrompts[characterReplyingNo];
        string allInfo = "";
        foreach (string info in Characters[characterSpeakingNo].infoShared)
        {
            allInfo += "\"" + info + "\"\n";
        }
        finalSystemPrompt += "\n Context: \n  The character providing the user input has revealed the following about themselves: \n" +
            allInfo + "\n When the user input is appended after this message, respond as your character considering this context but without asking questions.";

        yield return StartCoroutine(SendGrokRequest(finalSystemPrompt, userMessage, characterReplyingNo, temp, (response) =>
        {
            GrokMessage assistantMessage = response.choices[0].message;
            string reply = assistantMessage.content.Trim();
            string charName = Characters[characterReplyingNo].myName.Trim();

            if (reply.StartsWith(charName))
            {
                int colonIndex = reply.IndexOf(":");
                if (colonIndex > 0 && colonIndex < reply.Length - 1)
                {
                    reply = reply.Substring(colonIndex + 1).Trim();
                }
                else
                {
                    reply = reply.Substring(charName.Length).Trim();
                }
            }

            GrokDialogueEntry entry = new GrokDialogueEntry(characterReplyingNo, Characters[characterReplyingNo].name, reply);
            dialogueEntries.Add(entry);
            events.Add(Characters[characterReplyingNo].myName + " said: " + reply + " .");

            StartCoroutine(CheckPersonalInfo(reply, characterReplyingNo));
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
        string finalSystemPromptCharacterInfo = Characters[characterNo].myCharacter +
            "\nYou are a strict extractor of personal information about the character you are playing. " +
            "You are provided with the full character description (above) and a user message below. " +
            "Compare the user message with the character description and output exactly the details that are explicitly mentioned in the user message and that also appear in the character description. " +
            "Preserve the order, formatting, and punctuation exactly as they appear in the user message. " +
            "Do not add, remove, or modify any details. " +
            "If any matching details are found, output exactly one line containing only those details, separated by commas. " +
            "If no matching details are found, output exactly: none" +
            "\nDo not include any extra text, greetings, or commentary.";

        yield return StartCoroutine(SendGrokRequest(finalSystemPromptCharacterInfo, reply, characterNo, tempPersonalInfo, (response) =>
        {
            GrokMessage assistantMessage = response.choices[0].message;
            string result = assistantMessage.content.Trim();

            if (result.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("No personal info revealed for character " + characterNo);
            }
            else
            {
                AddPersonalInfo(characterNo, result);
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
        string finalSystemPromptHumanInfo =
           "\nYou are a strict extractor of personal information for the human player. " +
       "Analyze the text provided below and extract any personal details mentioned (such as name, age, location, background, occupation, hobbies, etc.). " +
       "If no personal information is found, output exactly:\n" +
       "none\n" +
       "Do not include any extra text, greetings, or commentary.";

        yield return StartCoroutine(SendGrokRequest(finalSystemPromptHumanInfo, reply, characterNo, 0.0f, (response) =>
        {
            GrokMessage assistantMessage = response.choices[0].message;
            string result = assistantMessage.content.Trim();

            if (result.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("No personal info revealed for character " + characterNo);
            }
            else
            {
                AddPersonalInfo(characterNo, result);
            }

            if (mySaveController != null)
            {
                mySaveController.NewEntryInfo(finalSystemPromptHumanInfo, reply, assistantMessage.content);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }));
    }

    private IEnumerator GenerateCharacterName(int characterNo)
    {
        yield return StartCoroutine(SendGrokRequest(characterSetup, characterName, characterNo, charaterTemp, (response) =>
        {
            string generatedName = response.choices[0].message.content;
            Characters[characterNo].myName = generatedName;
            StartCoroutine(GenerateCharacterDescription(characterNo, generatedName));
        }));
    }

    private IEnumerator GenerateCharacterDescription(int characterNo, string characterName)
    {
        string prompt = "Generate a unique character description for a character named: " + characterName +
            " in the game/play. Include: name, age, place born, job, cause of death.";
        yield return StartCoroutine(SendGrokRequest(characterSetup, prompt, characterNo, charaterTemp, (response) =>
        {
            string description = response.choices[0].message.content;
            Characters[characterNo].myCharacter = description;
            systemPrompts[characterNo] = baseSetup + description;
        }));
    }

    public void ShowDialog(string message, int characterNo)
    {
        GameObject newObject = Instantiate(dialoguePrefab, Characters[0].dialogueHolder);
        newObject.transform.SetParent(Characters[characterNo].dialogueHolder);
        newObject.transform.localPosition = Vector3.zero;
        dialogueDisplay myDialogueDisplay = newObject.GetComponent<dialogueDisplay>();
        myDialogueDisplay.showMessage(message);
        Dialogues.Add(newObject);
    }

    private void AddPersonalInfo(int characterNo, string info)
    {
        if (!string.IsNullOrEmpty(info) && info != "none")
        {
            Characters[characterNo].infoShared.Add(info);
        }
    }
}