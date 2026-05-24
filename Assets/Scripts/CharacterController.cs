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
    public string latestDialoguesContext; // Stores the latest 5 dialogues for prompt context

    // Character response queue
    private List<int> respondingCharacterQueue = new List<int>();
    private bool isProcessingResponses = false;

    //   public List<string> systemPrompts; // One per character.

    // Model parameters.
    public float temp;         // For dialogue requests.
    public float charaterTemp; // For generating names/descriptions.
    public int maxDialogueTokens = 150;  // Token limit for character dialogue responses
    public int maxAddressingTokens = 10; // Token limit for routing/addressing decisions

    saveController mySaveController;

    public string modelDialogue;
  /// <summary>
  ///  public string modelInfo;
  /// </summary>
 //   public ModelNamesController modelNames;
    
    // API Selection: switch between custom FastAPI and Hugging Face Space
    public bool useQween0_6; //if true uses the small Qween 0.6 model hosted on FastAPI, if false uses the Qween 4B fast api
 //   public bool useHuggingFaceProvider; // if true routes dialogue requests to Hugging Face; if false uses OpenAI

    void Start()
    {
        // Set which API the InfoExtractorHandler should use
        InfoExtractorHandler.useQween0_6 = useQween0_6;
        APIRequestHandler.useHuggingFaceProvider = GeneratedCharacters.Instance.useHuggingFaceProvider;
        
        myMaster = GetComponentInParent<Master>();
        mySaveController = GetComponent<saveController>();

        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
        if (events == null)
            events = new List<string>();

        // Load pre-generated character data from intro scene.
        if (myMaster.thePromptsController.systemPrompts == null)
            myMaster.thePromptsController.systemPrompts = new List<string> { "", "", "", "" };

        Characters[1].myName = GeneratedCharacters.Instance.characters[1].name;
        Characters[2].myName = GeneratedCharacters.Instance.characters[2].name;
        Characters[1].myCharacter = GeneratedCharacters.Instance.characters[1].description;
        Characters[2].myCharacter = GeneratedCharacters.Instance.characters[2].description;
        Characters[1].gender = GeneratedCharacters.Instance.characters[1].gender;
        Characters[2].gender = GeneratedCharacters.Instance.characters[2].gender;
        Characters[1].age = GeneratedCharacters.Instance.characters[1].age;
        Characters[2].age = GeneratedCharacters.Instance.characters[2].age;
        myMaster.thePromptsController.systemPrompts[1] =
            myMaster.thePromptsController.dialogueSystemPromptTemplate
                .Replace("{CHARACTER_DESCRIPTION}", Characters[1].myCharacter);
        myMaster.thePromptsController.systemPrompts[2] =
            myMaster.thePromptsController.dialogueSystemPromptTemplate
                .Replace("{CHARACTER_DESCRIPTION}", Characters[2].myCharacter);
    }

    // Called when the user sends a message.
    public void ParsedText(string userInput, int characterNo)
    {
        Debug.Log("Received input: " + userInput);
        string savedName = "Character " + characterNo;
        // Log user dialogue. Use "Character X" format for consistency
        DialogueEntry entry = new DialogueEntry(characterNo, savedName, userInput);
        dialogueEntries.Add(entry);
        events.Add(savedName + ": " + userInput);

        // Send dialogue requests for each AI character.
        // StartCoroutine(SendRequestForCharacter(userInput, 0, 1));
        // StartCoroutine(SendRequestForCharacter(userInput, 0, 2));

        StartCoroutine(CheckPersonalInfo(userInput, 0));

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
            ? "Character 1 information known: \n" + GetCharacterInfoString(1) + "\n "
            : "Nothing known for Character 1";

        string character2info = Characters[2].infoShared.Count != 0
            ? "Character 2 information known: \n" + GetCharacterInfoString(2) + "\n "
            : "Nothing known for Character 2";


        string AdressSystemPromptComplete = myMaster.thePromptsController.adressingSystemPromptIntro + "\n " + character1info + "\n " + character2info + "\n "+ myMaster.thePromptsController.adressingSystemPromptContext +"\n " + latestDialoguesContext +"\n " + myMaster.thePromptsController.adressingSystemPromptActions;

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(AdressSystemPromptComplete, userMessage, characterSpeakingNo, temp, GeneratedCharacters.Instance.modelNames.modelAddressing, maxAddressingTokens, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            string reply = assistantMessage.content;

            Debug.Log("checked adressing got this reply: " + reply);

            // Validate response contains valid addressing (0, 1, or 2)
            bool isValidResponse = reply.Contains("0") || reply.Contains("1") || reply.Contains("2");

            if (!isValidResponse)
            {
                if (retryAttempts < 3)
                {
                    // Invalid response, retry
                    Debug.LogWarning($"Invalid addressing response (attempt {retryAttempts + 1}/3): '{reply}'. Retrying...");
                    StartCoroutine(SendRequestForAdress(userMessage, characterSpeakingNo, retryAttempts + 1));
                    return;
                }
                else
                {
                    // Max retries reached, fallback to "0" (both characters respond)
                    Debug.LogError($"Invalid addressing response after 3 attempts: '{reply}'. Defaulting to both characters responding.");
                    reply = "0";
                }
            }

            // Populate response queue based on addressing detection
            if (reply.Contains("0"))
            {
                respondingCharacterQueue.Add(1);
                respondingCharacterQueue.Add(2);
            }
            else if (reply.Contains("1"))
            {
                respondingCharacterQueue.Add(1);
            }
            else if (reply.Contains("2"))
            {
                respondingCharacterQueue.Add(2);
            }

            // Start processing the queue if not already processing
            if (!isProcessingResponses && respondingCharacterQueue.Count > 0)
            {
                StartCoroutine(ProcessCharacterResponseQueue(userMessage));
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
        var (person1Index, person2Index) = GetPersonMapping(characterReplyingNo);
        int speakerPersonNo = GetPersonNumberFor(characterReplyingNo, characterSpeakingNo);

        string userPrompt =
            "## What you know about the others in the room\n\n" +
            "<other_1>:\n" + BuildInfoBlock(person1Index) + "\n\n" +
            "<other_2>:\n" + BuildInfoBlock(person2Index) + "\n\n" +
            "## Recent dialogue in the room\n" + FormatDialogueForCharacter(characterReplyingNo) + "\n\n" +
            "---\n\n" +
            "[<other_" + speakerPersonNo + "> is speaking to you]\n" +
            userMessage;

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(myMaster.thePromptsController.systemPrompts[characterReplyingNo], userPrompt, characterReplyingNo, temp, GeneratedCharacters.Instance.modelNames.modelDialogue, maxDialogueTokens, (response) =>
        {
            Message assistantMessage = response.choices[0].message;
            // string reply = StripCharacterNamePrefix(assistantMessage.content, Characters[characterReplyingNo].myName);
            string reply = assistantMessage.content;

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
        string characterType = characterNo == 0 ? "human player" : $"character {characterNo}";
        Debug.Log($"Checking personal info for {characterType}");

        InfoExtractorHandler.ExtractInfo(reply, myMaster.thePromptsController.infoExtractionSystemPrompt, (result) =>
        {
            Debug.Log($"Extracted info for {characterType}: {result}");

            if (!result.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                addPersonalInfo(characterNo, result);
                myMaster.StartFlicker(); 
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

    private (int person1Index, int person2Index) GetPersonMapping(int replyingChar)
    {
        int otherChar = (replyingChar == 1) ? 2 : 1;
        return (otherChar, 0);
    }

    private int GetPersonNumberFor(int replyingChar, int speakerIndex)
    {
        var (p1, p2) = GetPersonMapping(replyingChar);
        if (speakerIndex == p1) return 1;
        if (speakerIndex == p2) return 2;
        Debug.LogWarning($"Unexpected speakerIndex {speakerIndex} for replyingChar {replyingChar}");
        return 2;
    }

    private string BuildInfoBlock(int characterNo)
    {
        if (Characters[characterNo].infoShared.Count == 0)
            return "Nothing has been revealed yet.";

        var sb = new StringBuilder();
        foreach (string info in Characters[characterNo].infoShared)
            if (!string.IsNullOrWhiteSpace(info))
                sb.AppendLine(info);
        return sb.ToString().TrimEnd();
    }

    private string FormatDialogueForCharacter(int currentCharacter)
    {
        if (dialogueEntries.Count == 0) return "No one has spoken yet.";

        var sb = new StringBuilder();
        int startIndex = Mathf.Max(0, dialogueEntries.Count - 5);
        for (int i = startIndex; i < dialogueEntries.Count; i++)
        {
            int speakerId = dialogueEntries[i].characterId;
            string label = (speakerId == currentCharacter)
                ? "You"
                : "<other_" + GetPersonNumberFor(currentCharacter, speakerId) + ">";
            sb.Append(label).Append(": ").Append(dialogueEntries[i].dialogueText).Append("\n");
        }
        return sb.ToString().TrimEnd();
    }

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

        // Use "Character X" format instead of LLM-generated names to avoid confusing the LLM
        string characterLabel = "Character " + characterNo;
        DialogueEntry entry = new DialogueEntry(characterNo, characterLabel, dialogue);
        dialogueEntries.Add(entry);
        events.Add($"{characterLabel}: {dialogue}");
        UpdateLatestDialoguesContext();
    }

    private void SaveDialogueIfPossible(string systemPrompt, string userPrompt, string reply)
    {
        if (mySaveController != null)
        {
            mySaveController.NewEntryDialogue(systemPrompt, userPrompt, reply);
        }
    }

    private void UpdateLatestDialoguesContext()
    {
        StringBuilder sb = new StringBuilder();
        int startIndex = Mathf.Max(0, dialogueEntries.Count - 5);
        
        for (int i = startIndex; i < dialogueEntries.Count; i++)
        {
            // Use character index format for consistency in prompts
            string characterLabel = "Character " + dialogueEntries[i].characterId;
            sb.Append(characterLabel).Append(": ")
              .Append(dialogueEntries[i].dialogueText).Append("\n");
        }
        
        latestDialoguesContext = sb.ToString();
    }

    private IEnumerator ProcessCharacterResponseQueue(string userMessage)
    {
        isProcessingResponses = true;
        
        while (respondingCharacterQueue.Count > 0)
        {
            // Get first character in queue
            int characterNo = respondingCharacterQueue[0];
            respondingCharacterQueue.RemoveAt(0);
            
            Debug.Log($"Processing response from Character {characterNo}");
            
            // Wait for this character to respond
            yield return StartCoroutine(SendRequestForCharacter(userMessage, 0, characterNo));
            
            // Optional: Add small delay between responses for readability
            if (respondingCharacterQueue.Count > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        // Queue is empty, reset flag
        isProcessingResponses = false;
        Debug.Log("All characters have responded. Queue cleared.");
    }

    #endregion

    // Moved to intro scene (CharacterGenerator). Kept for reference.
    // private IEnumerator GenerateCharacterName(int characterNo)
    // {
    //     yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(myMaster.thePromptsController.mainGameSystemPrompt + " " + myMaster.thePromptsController.characterSetupSystemPrompt, myMaster.thePromptsController.characterNameUserPrompt, characterNo, charaterTemp, modelDialogue, 0, (response) =>
    //     {
    //         Debug.Log("Started");
    //         string generatedName = response.choices[0].message.content;
    //         Characters[characterNo].myName = generatedName;
    //         StartCoroutine(GenerateCharacterDescription(characterNo, generatedName));
    //     }, this));
    // }

    // private IEnumerator GenerateCharacterDescription(int characterNo, string characterName)
    // {
    //     string prompt = myMaster.thePromptsController.mainGameSystemPrompt + " " + myMaster.thePromptsController.characterSetupSystemPrompt;
    //     yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(prompt, myMaster.thePromptsController.characterSetupUserPrompt + " " + characterName, characterNo, charaterTemp, modelDialogue, 0, (response) =>
    //     {
    //         string description = response.choices[0].message.content;
    //         Characters[characterNo].myCharacter = description;
    //         myMaster.thePromptsController.systemPrompts[characterNo] = myMaster.thePromptsController.characterDialogueSystemPrompt + "\n " + description + "\n " + myMaster.thePromptsController.characterDialogueSystemPromptEnding;
    //         if (mySaveController != null)
    //             mySaveController.NewCharacterInfo(prompt, myMaster.thePromptsController.characterSetupUserPrompt, description);
    //         else
    //             Debug.LogError("mySaveController is not assigned.");
    //     }, this));
    // }

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
            GeneratedCharacters.Instance.modelNames.modelDialogue,
            0,
            (response) =>
            {
                Message assistantMessage = response.choices[0].message;
                // string reply = StripCharacterNamePrefix(assistantMessage.content, Characters[3].myName);
                string reply = assistantMessage.content;

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
            myMaster.thePromptsController.characterQuestionSystemPromptSuffix;

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
            GeneratedCharacters.Instance.modelNames.modelDialogue,
            0,
            (response) =>
            {
                Message assistantMessage = response.choices[0].message;
                // string question = StripCharacterNamePrefix(assistantMessage.content, Characters[characterNo].myName);
                string question = assistantMessage.content;

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
