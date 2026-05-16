using UnityEngine;

public class AssembledPromptsPreview : MonoBehaviour
{
    public PromptsController promptsController;

    [Header("Name Generation")]
    [TextArea(5, 15)] public string nameGeneration;

    [Header("Description Generation")]
    [TextArea(5, 15)] public string descriptionGeneration;

    [Header("Dialogue")]
    [TextArea(5, 25)] public string dialogue;

    [Header("Addressing")]
    [TextArea(5, 15)] public string addressing;

    [Header("Info Extraction")]
    [TextArea(5, 15)] public string infoExtraction;

    [Header("Character Question")]
    [TextArea(5, 15)] public string characterQuestion;

    [ContextMenu("Refresh")]
    void Assemble()
    {
        if (promptsController == null) return;
        var p = promptsController;

        nameGeneration =
            "SYSTEM:\n" + p.mainGameSystemPrompt + " " + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterNameUserPrompt;

        descriptionGeneration =
            "SYSTEM:\n" + p.mainGameSystemPrompt + " " + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterSetupUserPrompt + " [CHARACTER_NAME]";

        dialogue =
            "SYSTEM:\n" + p.characterDialogueSystemPrompt + "\n [CHARACTER_DESCRIPTION]\n " + p.characterDialogueSystemPromptEnding +
            "\n\nUSER:\n" + "[USER_MESSAGE]\n " + p.characterDialogueUserPromptEnding + "\n [PLAYER_INFO]\n " + p.characterDialogueUserPromptEnding2;

        addressing =
            "SYSTEM:\n" + p.adressingSystemPromptIntro + "\n [CHAR_1_INFO]\n [CHAR_2_INFO]\n " +
            p.adressingSystemPromptContext + "\n [RECENT_DIALOGUE]\n " + p.adressingSystemPromptActions;

        infoExtraction =
            "SYSTEM:\n" + p.infoExtractionSystemPrompt;

        characterQuestion =
            "SYSTEM:\n" + "[CHARACTER_SYSTEM_PROMPT]" + p.characterQuestionSystemPromptSuffix;
    }

    void Start() => Assemble();
}
