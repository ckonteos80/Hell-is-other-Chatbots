using UnityEngine;
using System.Collections.Generic;

public class PromptsController : MonoBehaviour
{
    [TextArea(3, 10)]
    public string mainGameSystemPrompt;           // Sets the game's universe; used as the base for name and description generation in the intro scene.
    [TextArea(3, 10)]
    public string characterSetupSystemPrompt;     // Gives the model a script-writer role; combined with mainGameSystemPrompt for name and description generation.
    [TextArea(3, 10)]
    public string characterSetupUserPrompt;       // Requests the full character bio; the second call in character generation, with the generated name appended.
    [TextArea(3, 10)]
    public string characterNameUserPrompt;        // Requests a two-word name only; the first call in character generation.
    [TextArea(3, 10)]
    public string characterDialogueSystemPrompt;  // Opens the character's permanent system prompt by putting the model in-character as an actor; stored once after generation.
    [TextArea(3, 10)]
    public string characterDialogueSystemPromptEnding;  // Closes the character's permanent system prompt with rules for staying in character; appended after the character description.
    [TextArea(3, 10)]
    public string characterDialogueUserPromptEnding;    // Signals that known player info follows; injected into the user prompt when infoShared is not empty.
    [TextArea(3, 10)]
    public string characterDialogueUserPromptEnding2;   // Prevents the model from repeating or re-asking known info; appended after the player info block.
    [TextArea(3, 10)]
    public string infoExtractionSystemPrompt;     // Defines the info-extractor role; used after every player message and character reply to build up infoShared.
    [TextArea(3, 10)]
    public string adressingSystemPrompt;          // Full standalone addressing prompt (legacy/unused — split into Intro/Context/Actions parts below).
    [TextArea(3, 10)]
    public string adressingSystemPromptIntro;     // Opens the addressing call with known character info; only triggered once any character has info.
    [TextArea(3, 10)]
    public string adressingSystemPromptContext;   // Labels the recent dialogue block inside the addressing prompt.
    [TextArea(3, 10)]
    public string adressingSystemPromptActions;   // Defines the 0/1/2/3 reply rules for the addressing model; keeps the response to a single token.
    [TextArea(3, 10)]
    public string characterQuestionSystemPromptSuffix; // Appended to a character's system prompt when RequestCharacterQuestion is called, forcing a single direct question.
    [TextArea(3, 10)]
    public string characterNameGenderSuffix; // Appended to the name prompt; {0} is replaced with the gender word (male/female).
    [TextArea(3, 10)]
    public string characterDescriptionGenderAgeSuffix; // Appended to the description prompt; {0} = gender, {1} = age.

    public List<string> systemPrompts; // One per character.

    void OnValidate() => Init();
    void Awake() => Init();

    void Init()
    {
        mainGameSystemPrompt = "We are playing a game based on a theater play, which delves into existential themes and the human condition.";

        characterSetupSystemPrompt = "You are a script writer creating a character for the play.";

        characterSetupUserPrompt =
            "Generate a unique character description for a character in this game/play.\n" +
            "Please include the following details:\n- Name\n- Age\n- Place of Birth\n- Occupation\n- Cause of Death\n\n" +
            "Ensure the character's background and personality reflect a dark, existential atmosphere " +
            "while remaining original and fitting within the narrative of our game. The name of the character you are creating is:";

        characterNameUserPrompt =
            "Generate a unique and creative character name. Your response must consist solely of two words—" +
            "a first name and a last name—separated by a single space. Do not include any additional text, punctuation, or explanation.";

        characterDialogueSystemPrompt =
            "You are an actor portraying a character in a theater play. The stage is a living room but you are in hell after you have died" +
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

        characterNameGenderSuffix = "The character must be {0}.";

        characterDescriptionGenderAgeSuffix = "The character is {0}, age {1}.";
    }
}
