using UnityEngine;

public class AssembledPromptsPreview : MonoBehaviour
{
    public PromptsController promptsController;

    [Header("Name Generation\nSYS: mainGameSystemPrompt + characterSetupSystemPrompt\nUSR: characterNameUserPrompt + characterNameGenderSuffix([GENDER])")]
    [TextArea(5, 15)] public string nameGeneration;

    [Header("Description Generation\nSYS: mainGameSystemPrompt + characterSetupSystemPrompt\nUSR: characterSetupUserPrompt + [CHARACTER_NAME] + characterDescriptionGenderAgeSuffix([GENDER],[AGE])")]
    [TextArea(5, 15)] public string descriptionGeneration;

    [Header("Dialogue\nSYS: characterDialogueSystemPrompt + [CHARACTER_DESCRIPTION] + characterDialogueSystemPromptEnding\nUSR: [USER_MESSAGE] + characterDialogueUserPromptEnding + [PLAYER_INFO] + characterDialogueUserPromptEnding2")]
    [TextArea(5, 25)] public string dialogue;

    [Header("Addressing\nSYS: adressingSystemPromptIntro + [CHAR_1_INFO] + [CHAR_2_INFO] + adressingSystemPromptContext + [RECENT_DIALOGUE] + adressingSystemPromptActions")]
    [TextArea(5, 15)] public string addressing;

    [Header("Info Extraction\nSYS: infoExtractionSystemPrompt")]
    [TextArea(5, 15)] public string infoExtraction;

    [Header("Character Question\nSYS: [CHARACTER_SYSTEM_PROMPT] + characterQuestionSystemPromptSuffix")]
    [TextArea(5, 15)] public string characterQuestion;

    [ContextMenu("Refresh")]
    void Assemble()
    {
        if (promptsController == null) return;
        var p = promptsController;

        string nameSuffix = !string.IsNullOrEmpty(p.characterNameGenderSuffix)
            ? " " + string.Format(p.characterNameGenderSuffix, "[GENDER]") : "";
        nameGeneration =
            "SYSTEM:\n" + p.mainGameSystemPrompt + " " + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterNameUserPrompt + nameSuffix;

        string descSuffix = !string.IsNullOrEmpty(p.characterDescriptionGenderAgeSuffix)
            ? " " + string.Format(p.characterDescriptionGenderAgeSuffix, "[GENDER]", "[AGE]") : "";
        descriptionGeneration =
            "SYSTEM:\n" + p.mainGameSystemPrompt + " " + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterSetupUserPrompt + " [CHARACTER_NAME]" + descSuffix;

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

    void OnValidate() => Assemble();
    void Start() => Assemble();
}
