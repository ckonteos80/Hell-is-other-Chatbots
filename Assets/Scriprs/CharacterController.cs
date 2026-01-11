using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;


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

    //  public PersonController NarratorCharacter;

    // Dialogue and event tracking.
    public List<DialogueEntry> dialogueEntries;
    public List<string> events;




    //   public List<string> systemPrompts; // One per character.

    // Model parameters.
    public float temp;         // For dialogue requests.
    public float charaterTemp; // For generating names/descriptions.

    saveController mySaveController;

    public string modelDialogue;
    public string modelInfo;

    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        mySaveController = GetComponent<saveController>();

        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
        if (events == null)
            events = new List<string>();

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
            if (Characters[2].gameObject.activeSelf)
            {
                StartCoroutine(SendRequestForCharacter(userInput, 0, 2));
            }
            //   StartCoroutine(SendRequestForCharacter(userInput, 0, 2));
        }
        if (Characters[1].infoShared.Count != 0 || Characters[2].infoShared.Count != 0)
        {
            StartCoroutine(SendRequestForAdress(userInput, 0));
        }
    }

    public IEnumerator SendRequestForAdress(string userMessage, int characterSpeakingNo, int retryAttempts = 0)
    {
        // Build character info strings efficiently
        string character1info = Characters[1].infoShared.Count != 0
            ? "Character 1 information know: \n" + GetCharacterInfoString(1) + "\n "
            : "Nothing known for Character 1";

        string character2info = Characters[2].infoShared.Count != 0
            ? "Character 2 information know: \n" + GetCharacterInfoString(2) + "\n "
            : "Nothing known for Character 2";


        string AdressSystemPromptComplete = myMaster.thePromptsController.adressingSystemPrompt + "\n " + character1info + "\n " + character2info;

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(AdressSystemPromptComplete, userMessage, characterSpeakingNo, temp, modelDialogue, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            string reply = assistantMessage.content;

            // If the reply starts with the character's name followed by a colon or similar delimiter,
            // remove that prefix.
            Debug.Log("checked adressing got this reply: " + reply);

            if (reply.Contains("0"))
            {
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 1));
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 2));
            }
            if (reply.Contains("1"))
            {
                StartCoroutine(SendRequestForCharacter(userMessage, 0, 1));
            }
            if (reply.Contains("2"))
            {
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
        }, this));
    }


    public IEnumerator SendRequestForCharacter(string userMessage, int characterSpeakingNo, int characterReplyingNo, int retryAttempts = 0)
    {
        // Build user prompt with character info if available
        string userPrompt;
        if (Characters[characterSpeakingNo].infoShared.Count != 0)
        {
            string allInfo = GetCharacterInfoString(characterSpeakingNo);
            userPrompt = userMessage + "\n " + myMaster.thePromptsController.characterDialogueUserPromptEnding + "\n " + allInfo + "\n " + myMaster.thePromptsController.characterDialogueUserPromptEnding2;
        }
        else
        {
            userPrompt = userMessage;
        }

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(myMaster.thePromptsController.systemPrompts[characterReplyingNo], userPrompt, characterReplyingNo, temp, modelDialogue, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            string reply = StripCharacterNamePrefix(assistantMessage.content, Characters[characterReplyingNo].myName);

            // Log dialogue entry and event
            LogDialogueEntry(characterReplyingNo, reply, "said");

            // Check for personal info
            StartCoroutine(CheckPersonalInfo(reply, characterReplyingNo));

            // Display the dialogue
            ShowDialog(reply, characterReplyingNo, 0);

            // Save dialogue
            SaveDialogueIfPossible(myMaster.thePromptsController.systemPrompts[characterReplyingNo], userPrompt, reply);
        }, this));
    }

    public IEnumerator CheckPersonalInfo(string reply, int characterNo)
    {
        Debug.Log("Checking personal info for character " + characterNo);

        InfoExtractorHandler.ExtractInfo(reply, myMaster.thePromptsController.infoExtractionSystemPrompt, (result) =>
        {
            Debug.Log($"Extracted info for character {characterNo}: {result}");

            if (!result.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                addPersonalInfo(characterNo, result);
            }

            if (mySaveController != null)
            {
                mySaveController.NewEntryInfo(myMaster.thePromptsController.infoExtractionSystemPrompt, reply, result);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }, this);

        yield return null;
    }
    public IEnumerator CheckHumanPersonalInfo(string reply, int characterNo)
    {
        Debug.Log("Checking personal info for human player");

        InfoExtractorHandler.ExtractInfo(reply, myMaster.thePromptsController.infoExtractionSystemPrompt, (result) =>
        {
            Debug.Log($"Extracted info for human player: {result}");

            if (!result.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                addPersonalInfo(characterNo, result);
            }

            if (mySaveController != null)
            {
                mySaveController.NewEntryInfo(myMaster.thePromptsController.infoExtractionSystemPrompt, reply, result);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }, this);

        yield return null;
    }




    void addPersonalInfo(int characterNo, string response)
    {
        if (response != "none")
        {
            Characters[characterNo].infoShared.Add(response);
        }
    }

    #region Helper Methods

    private string GetCharacterInfoString(int characterNo)
    {
        if (Characters[characterNo].infoShared.Count == 0)
            return "";

        StringBuilder sb = new StringBuilder();
        foreach (string info in Characters[characterNo].infoShared)
        {
            sb.Append("\"").Append(info).Append("\"\n");
        }
        return sb.ToString();
    }

    private string StripCharacterNamePrefix(string text, string characterName)
    {
        text = text.Trim();
        if (!text.StartsWith(characterName))
            return text;

        int colonIndex = text.IndexOf(":");
        if (colonIndex > 0 && colonIndex < text.Length - 1)
        {
            return text.Substring(colonIndex + 1).Trim();
        }
        return text.Substring(characterName.Length).Trim();
    }

    private void LogDialogueEntry(int characterNo, string dialogue, string eventType = "said")
    {
        if (characterNo < 0 || characterNo >= Characters.Count)
        {
            Debug.LogError($"LogDialogueEntry: Character index {characterNo} out of range!");
            return;
        }

        if (Characters[characterNo] == null)
        {
            Debug.LogError($"LogDialogueEntry: Character at index {characterNo} is null!");
            return;
        }

        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
        if (events == null)
            events = new List<string>();

        DialogueEntry entry = new DialogueEntry(characterNo, Characters[characterNo].name, dialogue);
        dialogueEntries.Add(entry);
        events.Add($"{Characters[characterNo].myName} {eventType}: {dialogue}");
    }

    private void SaveDialogueIfPossible(string systemPrompt, string userPrompt, string reply)
    {
        if (mySaveController != null)
        {
            mySaveController.NewEntryDialogue(systemPrompt, userPrompt, reply);
        }
    }

    #endregion

    // Uses the generic request to generate a character name.
    private IEnumerator GenerateCharacterName(int characterNo)
    {

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(myMaster.thePromptsController.mainGameSystemPrompt + " " + myMaster.thePromptsController.characterSetupSystemPrompt, myMaster.thePromptsController.characterNameUserPrompt, characterNo, charaterTemp, modelDialogue, (response) =>
        {
            Debug.Log("Started");
            string generatedName = response.choices[0].message.content;
            // Debug.Log("Generated name for Character " + characterNo + ": " + generatedName);
            Characters[characterNo].myName = generatedName;
            // After name generation, request a character description.
            StartCoroutine(GenerateCharacterDescription(characterNo, generatedName));
        }, this));
    }

    // Uses the generic request to generate a character description.
    private IEnumerator GenerateCharacterDescription(int characterNo, string characterName)
    {
        string prompt = myMaster.thePromptsController.mainGameSystemPrompt + " " + myMaster.thePromptsController.characterSetupSystemPrompt;    //     ". Include: name, age, place born, job, cause of death.";
        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(prompt, myMaster.thePromptsController.characterSetupUserPrompt + " " + characterName, characterNo, charaterTemp, modelDialogue, (response) =>
        {
            string description = response.choices[0].message.content;
            // Debug.Log("Generated description for Character " + characterNo + ": " + description);
            Characters[characterNo].myCharacter = description;
            myMaster.thePromptsController.systemPrompts[characterNo] = myMaster.thePromptsController.characterDialogueSystemPrompt + "\n " + description + "\n " + myMaster.thePromptsController.characterDialogueSystemPromptEnding;


            if (mySaveController != null)
            {
                mySaveController.NewCharacterInfo(prompt, myMaster.thePromptsController.characterSetupUserPrompt, description);
            }
            else
            {
                Debug.LogError("mySaveController is not assigned.");
            }
        }, this));
    }

    // Displays dialogue in the specified character's dialogue holder.
    public void ShowDialog(string message, int characterNo, int actionAfterClose)
    {
        GameObject newObject = Instantiate(dialoguePrefab, Characters[0].dialogueHolder);
        newObject.transform.SetParent(Characters[characterNo].dialogueHolder);
        newObject.name = "character" + characterNo.ToString();
        newObject.transform.localPosition = Vector3.zero;
        // Debug.Log("Displaying for Character " + characterNo + ": " + message);
        dialogueDisplay myDialogueDisplay = newObject.GetComponent<dialogueDisplay>();
        myDialogueDisplay.showMessage(message, actionAfterClose);
        myMaster.theOverlayController.addImage(myDialogueDisplay);
    }

    public void RequestNarratorDialogue(string context = "")
    {
        StartCoroutine(GenerateNarratorDialogue(context));
    }

    private IEnumerator GenerateNarratorDialogue(string context)
    {
        string systemPrompt = myMaster.thePromptsController.systemPrompts[3];
        string userPrompt = context;

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
            systemPrompt,
            userPrompt,
            3,
            temp,
            modelDialogue,
            (response) =>
            {
                Message assistantMessage = response.choices[0].message;
                string reply = StripCharacterNamePrefix(assistantMessage.content, Characters[3].myName);

                // Log dialogue entry
                LogDialogueEntry(3, reply, "said");

                // Display the dialogue
                ShowDialog(reply, 3, 1);

                // Save dialogue
                SaveDialogueIfPossible(systemPrompt, userPrompt, reply);
            }
        , this));
    }

    // Call this to make a character ask the player a question
    public void RequestCharacterQuestion(int characterNo, string questionContext = "")
    {
        StartCoroutine(GenerateCharacterQuestion(characterNo, questionContext));
    }

    private IEnumerator GenerateCharacterQuestion(int characterNo, string questionContext)
    {
        // Build system prompt for question generation
        string systemPrompt = myMaster.thePromptsController.systemPrompts[characterNo] +
            "\n\nYou must ask the player a single, direct question. " +
            "Make it conversational and relevant to what you know about them or the situation.";

        // Build context from recent conversation using StringBuilder
        StringBuilder recentContext = new StringBuilder();
        int contextCount = Mathf.Min(3, dialogueEntries.Count);
        for (int i = dialogueEntries.Count - contextCount; i < dialogueEntries.Count; i++)
        {
            recentContext.Append(dialogueEntries[i].characterName).Append(": ")
                        .Append(dialogueEntries[i].dialogueText).Append(" ");
        }

        string userPrompt = string.IsNullOrEmpty(questionContext)
            ? $"Based on the conversation: {recentContext}\nAsk the player a relevant question."
            : questionContext;

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
            systemPrompt,
            userPrompt,
            characterNo,
            temp,
            modelDialogue,
            (response) =>
            {
                Message assistantMessage = response.choices[0].message;
                string question = StripCharacterNamePrefix(assistantMessage.content, Characters[characterNo].myName);

                // Log dialogue entry
                LogDialogueEntry(characterNo, question, "asked");

                // Display the question
                ShowDialog(question, characterNo, 0);

                // Save dialogue
                SaveDialogueIfPossible(systemPrompt, userPrompt, question);
            }
        , this));
    }

}
