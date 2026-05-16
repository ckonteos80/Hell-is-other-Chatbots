using UnityEngine;
using System.Collections.Generic;

public class PromptsController : MonoBehaviour
{
    [TextArea(3, 10)]
    public string mainGameSystemPrompt;
    [TextArea(3, 10)]
    public string characterSetupSystemPrompt;
    [TextArea(3, 10)]
    public string characterSetupUserPrompt;
    [TextArea(3, 10)]
    public string characterNameUserPrompt;
    [TextArea(3, 10)]
    public string characterDialogueSystemPrompt;
    [TextArea(3, 10)]
    public string characterDialogueSystemPromptEnding;
    [TextArea(3, 10)]
    public string characterDialogueUserPromptEnding;
    [TextArea(3, 10)]
    public string characterDialogueUserPromptEnding2;
    [TextArea(3, 10)]
    public string infoExtractionSystemPrompt;
    [TextArea(3, 10)]
    public string adressingSystemPrompt;
    [TextArea(3, 10)]
    public string adressingSystemPromptIntro;
    [TextArea(3, 10)]
    public string adressingSystemPromptContext;
    [TextArea(3, 10)]
    public string adressingSystemPromptActions;
    [TextArea(3, 10)]
    public string characterQuestionSystemPromptSuffix;

    public List<string> systemPrompts; // One per character.

    void Awake()
    {
        mainGameSystemPrompt = "We are playing a game based on the stage play 'No Exit' by Jean-Paul Sartre, which delves into existential themes and the human condition.";

        characterSetupSystemPrompt = "You are a script writer creating a character for the play.";

        characterSetupUserPrompt =
            "Generate a unique character description for a character in this game/play.\n" +
            "Please include the following details:\n- Name\n- Age\n- Place of Birth\n- Occupation\n- Cause of Death\n\n" +
            "Ensure the character's background and personality reflect the dark, existential atmosphere of 'No Exit' " +
            "while remaining original and fitting within the narrative of our game. The name of the character you are creating is:";

        characterNameUserPrompt =
            "Generate a unique and creative character name. Your response must consist solely of two words—" +
            "a first name and a last name—separated by a single space. Do not include any additional text, punctuation, or explanation.";

        characterDialogueSystemPrompt =
            "You are an actor portraying a character in the stage play *No Exit* by Jean-Paul Sartre. " +
            "You must remain entirely in character for every response. Do not offer extra explanations or break character; " +
            "keep your answers brief and strictly from your character's perspective.\n\nBelow is your character description:";

        characterDialogueSystemPromptEnding =
            "Context:\nWhen the latest dialogue is appended after this message, continue the conversation " +
            "solely in character as the described persona in your character description. Do not include any out-of-character commentary, " +
            "meta descriptions, or notes about your acting—simply reply as the character you are playing would.";

        characterDialogueUserPromptEnding = "Context:\nTake into account the information you are aware about the user when replying:";

        characterDialogueUserPromptEnding2 =
            "Rules:\nWhen user details or any known information are provided in the context or appended by the user, " +
            "do not ask for or repeat that information. Simply acknowledge and incorporate it naturally into your in-character responses. Give brief replies.";

        infoExtractionSystemPrompt =
            "You are a personal information extractor. Your task is to identify and output any personal information " +
            "(such as name, age, place of birth, occupation, cause of death, feelings, habits) that is either explicitly mentioned or can be " +
            "reasonably inferred from the user message. Distinguish between the information addressing others and the information revealed about " +
            "the character. Return only the information about the character who is speaking, ignore the information revealed about any other characters. " +
            "Do not include any additional text, greetings, or commentary.";

        adressingSystemPrompt =
            "You are an assistant that determines if the user is addressing characters in a play. You will be provided with " +
            "details about two known characters (in the system prompt), the latest dialogye and a user message (in the user prompt). Your task is to " +
            "check if the user message explicitly refers to any known aspects of either character or continues the conversation with a specific character.\n\n" +
            "- If the user message addresses both characters, reply with \"0\".\n" +
            "- If the user message addresses character 1, reply with \"1\".\n" +
            "- If the user message addresses character 2, reply with \"2\".\n" +
            "- If the user message addresses character 3, reply with \"3\".\n\n" +
            "Do not include any additional text, commentary, or explanations.";

        adressingSystemPromptIntro =
            "You are an assistant that determines if the user is addressing characters in a game. " +
            "Here are the information that are known about the characters:";

        adressingSystemPromptContext = "This is the latest dialogue between the characters for context:";

        adressingSystemPromptActions =
            "- If the user message addresses both characters, reply with \"0\".\n" +
            "- If the user message addresses character 1, reply with \"1\".\n" +
            "- If the user message addresses character 2, reply with \"2\".\n" +
            "- If the user message addresses character 3, reply with \"3\".\n\n" +
            "Do not include any additional text, commentary, or explanations.";

        characterQuestionSystemPromptSuffix =
            "\n\nYou must ask the player a single, direct question. " +
            "Make it conversational and relevant to what you know about them or the situation.";
    }
}
