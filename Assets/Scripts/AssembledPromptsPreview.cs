using UnityEngine;

public class AssembledPromptsPreview : MonoBehaviour
{
    public PromptsController promptsController;

    [Header("Name Generation\nSYS: characterSetupSystemPrompt\nUSR: characterNameUserPrompt + characterNameGenderSuffix([GENDER])")]
    [TextArea(5, 15)] public string nameGeneration;

    [Header("Life Call\nSYS: characterSetupSystemPrompt\nUSR: characterLifeUserPrompt ({NAME},{AGE},{GENDER}) + characterLifeAntiDuplicationBlock for Char 2")]
    [TextArea(5, 20)] public string lifeCall;

    [Header("Sin Call\nSYS: characterSetupSystemPrompt\nUSR: characterSinUserPrompt (Life fields substituted) + characterSinAntiDuplicationBlock for Char 2")]
    [TextArea(5, 20)] public string sinCall;

    [Header("Stance Call\nSYS: characterSetupSystemPrompt\nUSR: characterStanceUserPrompt (Life+Sin fields substituted) + characterStanceAntiDuplicationBlock for Char 2")]
    [TextArea(5, 20)] public string stanceCall;

    [Header("Dialogue\nSYS: dialogueSystemPromptTemplate ({CHARACTER_DESCRIPTION} substituted)\nUSR: ## What you know... / ## Recent dialogue... / [<other_N> is speaking to you] / [USER_MESSAGE]")]
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
            "SYSTEM:\n" + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterNameUserPrompt + nameSuffix;

        lifeCall =
            "SYSTEM:\n" + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterLifeUserPrompt
                .Replace("{NAME}", "[CHARACTER_NAME]")
                .Replace("{AGE}", "[AGE]")
                .Replace("{GENDER}", "[GENDER]") +
            "\n\n(For Character 2, the following is appended:)\n" +
            p.characterLifeAntiDuplicationBlock
                .Replace("{OTHER_OCCUPATION}", "[OTHER_OCCUPATION]")
                .Replace("{OTHER_CAUSE_OF_DEATH}", "[OTHER_CAUSE_OF_DEATH]");

        sinCall =
            "SYSTEM:\n" + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterSinUserPrompt
                .Replace("{NAME}", "[CHARACTER_NAME]")
                .Replace("{AGE}", "[AGE]")
                .Replace("{GENDER}", "[GENDER]")
                .Replace("{OCCUPATION}", "[OCCUPATION]")
                .Replace("{CAUSE_OF_DEATH}", "[CAUSE_OF_DEATH]")
                .Replace("{PROSE_BODY}", "[PROSE_BODY]") +
            "\n\n(For Character 2, the following is appended:)\n" +
            p.characterSinAntiDuplicationBlock
                .Replace("{OTHER_REASON_TRUE}", "[OTHER_REASON_TRUE]");

        stanceCall =
            "SYSTEM:\n" + p.characterSetupSystemPrompt +
            "\n\nUSER:\n" + p.characterStanceUserPrompt
                .Replace("{NAME}", "[CHARACTER_NAME]")
                .Replace("{AGE}", "[AGE]")
                .Replace("{GENDER}", "[GENDER]")
                .Replace("{OCCUPATION}", "[OCCUPATION]")
                .Replace("{CAUSE_OF_DEATH}", "[CAUSE_OF_DEATH]")
                .Replace("{REASON_TRUE}", "[REASON_TRUE]")
                .Replace("{REASON_SELF_TOLD}", "[REASON_SELF_TOLD]")
                .Replace("{REFUSE_TO_ADMIT}", "[REFUSE_TO_ADMIT]")
                .Replace("{PROSE_BODY}", "[PROSE_BODY]") +
            "\n\n(For Character 2, the following is appended:)\n" +
            p.characterStanceAntiDuplicationBlock
                .Replace("{OTHER_WANT}", "[OTHER_WANT]");

        dialogue =
            "SYSTEM:\n" + p.dialogueSystemPromptTemplate.Replace("{CHARACTER_DESCRIPTION}", "[CHARACTER_DESCRIPTION]") +
            "\n\nUSER:\n" +
            "## What you know about the others in the room\n\n" +
            "<other_1>:\n[INFO_OTHER_1]\n\n<other_2>:\n[INFO_OTHER_2]\n\n" +
            "## Recent dialogue in the room\n[RECENT_DIALOGUE]\n\n" +
            "---\n\n[<other_N> is speaking to you]\n[USER_MESSAGE]";

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
