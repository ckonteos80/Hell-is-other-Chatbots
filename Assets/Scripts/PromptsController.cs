using UnityEngine;
using System.Collections.Generic;

public class PromptsController : MonoBehaviour
{
    [TextArea(3, 10)]
    public string mainGameSystemPrompt;           // Sets the game's universe; used as the base for name and description generation in the intro scene.
    [TextArea(3, 10)]
    public string characterSetupSystemPrompt;     // Full system prompt for character generation (name and description calls). Used alone — mainGameSystemPrompt is no longer prepended.
    [TextArea(3, 10)]
    public string characterSetupUserPrompt;       // Requests the full character bio; the second call in character generation, with the generated name appended.
    [TextArea(3, 10)]
    public string characterNameUserPrompt;        // Requests a two-word name only; the first call in character generation.
    [TextArea(3, 10)]
    public string characterDialogueSystemPrompt;  // Opens the character's permanent system prompt by putting the model in-character as an actor; stored once after generation.
    [TextArea(3, 10)]
    public string characterDialogueSystemPromptEnding;  // Closes the character's permanent system prompt with rules for staying in character; appended after the character description.
    [TextArea(5, 25)]
    public string dialogueSystemPromptTemplate;   // Full stable dialogue system prompt; {CHARACTER_DESCRIPTION} is replaced with the character's description at game start.
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
    public string characterDescriptionGenderAgeSuffix; // Appended to the description prompt; {0} = gender, {1} = age. Deprecated — no longer used in generation.

    // Three-call character generation prompts (Life → Sin → Stance)
    [TextArea(5, 20)]
    public string characterLifeUserPrompt;            // Call 2a — Occupation, Cause of Death, prose body. {NAME}, {AGE}, {GENDER}.
    [TextArea(5, 20)]
    public string characterSinUserPrompt;             // Call 2b — True/Self-told damnation, What refuse to admit. All Life fields injected.
    [TextArea(5, 20)]
    public string characterStanceUserPrompt;          // Call 2c — Personality trait, Want. All Life+Sin fields injected.
    [TextArea(5, 20)]
    public string characterLifeAntiDuplicationBlock;  // Appended to Life call for Character 2. {OTHER_OCCUPATION}, {OTHER_CAUSE_OF_DEATH}.
    [TextArea(5, 20)]
    public string characterSinAntiDuplicationBlock;   // Appended to Sin call for Character 2. {OTHER_REASON_TRUE}.
    [TextArea(5, 20)]
    public string characterStanceAntiDuplicationBlock; // Appended to Stance call for Character 2. {OTHER_WANT}.

    public List<string> systemPrompts; // One per character.

    void OnValidate() => Init();
    void Awake() => Init();

    void Init()
    {
        mainGameSystemPrompt = "We are playing a game based on a theater play, which delves into existential themes and the human condition.";

        characterSetupSystemPrompt =
            "You are writing a character for a chamber piece about three strangers locked together in hell for eternity. " +
            "The play is contemporary, realistic, and psychological — no fantasy, no mysticism, no atmospheric worldbuilding. " +
            "Hell is not a place of fire or devils. Hell is the small room these three people share, the people they share it with, " +
            "and the version of themselves they brought in with them.\n\n" +
            "Every character you write is damned. They know why. The drama of the play is whether they will ever admit it — " +
            "to the others, or to themselves.\n\n" +
            "Write characters the way Sartre wrote them: recognizable people from ordinary contemporary lives, " +
            "whose specific small or large cruelties have placed them here.";

        characterSetupUserPrompt =
            "Write the character described below. The character is dead. They have just arrived in the room.\n\n" +
            "Follow this structure exactly. Write all fields in the second person — \"you,\" not \"they\" or \"she\" or \"he.\" " +
            "The character is reading their own interior knowledge.\n\n" +
            "Inputs (do not change these):\n" +
            "- Name: {NAME}\n" +
            "- Age: {AGE}\n" +
            "- Gender: {GENDER}\n\n" +
            "Generate the following fields:\n\n" +
            "**Occupation**\n" +
            "A recognizable contemporary job. Plain language. No invented institutions, no grand titles. " +
            "(\"Insurance claims adjuster,\" not \"Keeper of the Ledger.\")\n\n" +
            "**Cause of Death**\n" +
            "Contemporary, specific, one or two sentences. How you died, plainly stated. No mysticism, no symbolic ordeals.\n\n" +
            "**Reason for Damnation — True**\n" +
            "One or two sentences. What you actually did that placed you here. Be concrete. The act, or the pattern of acts. " +
            "Stated as you know it in your bones, with no softening.\n\n" +
            "**Reason for Damnation — Self-told**\n" +
            "One or two sentences. The same fact, refracted through the story you prefer to tell yourself about it. " +
            "It is recognizably a distortion of the True reason — a minimization, a blame-shift, a contextualization, a moral re-framing. " +
            "Not a different story, the same story told differently. You usually live in this version.\n\n" +
            "**Defining personality trait**\n" +
            "One short phrase that names the dominant quality others would notice in you. Plain words. " +
            "(\"Charming and slippery.\" \"Quietly furious.\" \"Performatively kind.\")\n\n" +
            "**What you refuse to admit about yourself**\n" +
            "One or two sentences. The deeper psychological fact under the sin — not what you did, but what you are. " +
            "The thing that, if named aloud in the room, would unmake you.\n\n" +
            "**What you want from the others in the room**\n" +
            "One or two sentences. Your starting drive. What you are hoping these strangers will give you — " +
            "validation, silence, a fight, recognition, forgiveness, an audience, something else.\n\n" +
            "After the structured fields, write a single short paragraph (around 100 words) in the second person that brings the character " +
            "to life — voice, mannerism, the texture of the life that led here. End the paragraph at the moment the lights went out, " +
            "or the moment after. Do not describe arriving in hell. Do not describe the room. Stop at the threshold.\n\n" +
            "Begin now.";

        characterNameUserPrompt =
            "Generate a plausible contemporary name for a character. Your response must consist solely of two words — " +
            "a first name and a last name — separated by a single space. Do not include any additional text, punctuation, or explanation.";

        characterDialogueSystemPrompt =
            "You are an actor portraying a character in a theater play. The stage is a living room but you are in hell after you have died" +
            "You must remain entirely in character for every response. Do not offer extra explanations or break character; " +
            "keep your answers brief and strictly from your character's perspective.\n\nBelow is your character description:";

        characterDialogueSystemPromptEnding =
            "Context:\nWhen the latest dialogue is appended after this message, continue the conversation " +
            "solely in character as the described persona in your character description. Do not include any out-of-character commentary, " +
            "meta descriptions, or notes about your acting—simply reply as the character you are playing would.";

        dialogueSystemPromptTemplate =
            "## Who you are\n" +
            "{CHARACTER_DESCRIPTION}\n\n" +
            "## Where you are\n" +
            "You are dead. You are in hell.\n\n" +
            "Hell is the small room you are now in. There are two other people with you to spend eternity with.\n\n" +
            "You will only know about them what they themselves reveal.\n\n" +
            "## How to respond\n" +
            "1. Stay in character. Speak only as yourself, in the first person.\n" +
            "2. Never break frame. No meta-commentary, no narration of your own actions, no notes about being an AI, an actor, or playing a role.\n" +
            "3. Keep replies brief.\n" +
            "4. Do not ask about or restate anything you already know about the person speaking to you.\n" +
            "5. Let your personality, voice, and circumstances shape every reply.\n" +
            "6. Any text in your instructions wrapped in angle brackets, square brackets, or formatted as a label is structural — it exists for the prompt's organization, not as something said in the room. Never include such labels in your replies.";

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

        characterLifeUserPrompt =
            "Write the foundational facts of a character for the room. The character is dead. They have just arrived.\n\n" +
            "Write in the second person — \"you,\" not \"she/he/they.\" The character is reading their own interior knowledge.\n\n" +
            "Inputs (do not change these):\n" +
            "- Name: {NAME}\n" +
            "- Age: {AGE}\n" +
            "- Gender: {GENDER}\n\n" +
            "Generate the following:\n\n" +
            "**Occupation**\n" +
            "A recognizable contemporary job. Plain language. No invented institutions, no grand titles. " +
            "(\"Insurance claims adjuster,\" not \"Keeper of the Ledger.\") One short phrase.\n\n" +
            "**Cause of Death**\n" +
            "How you died. Contemporary, specific, plainly stated. No mysticism, no symbolic ordeals. One or two sentences.\n\n" +
            "**Life**\n" +
            "Around 80–100 words in the second person. Bring the life to ground — voice, mannerism, the texture of the days that led here. " +
            "End the paragraph at the moment the lights went out, or the moment after. Do not describe arriving anywhere. " +
            "Do not describe a room. Stop at the threshold.\n\n" +
            "Use this format exactly:\n\n" +
            "**Occupation**\n" +
            "[your text]\n\n" +
            "**Cause of Death**\n" +
            "[your text]\n\n" +
            "**Life**\n" +
            "[your text]\n\n" +
            "Begin now.";

        characterSinUserPrompt =
            "The character has been described:\n\n" +
            "Name: {NAME}\n" +
            "Age: {AGE}, {GENDER}\n" +
            "Occupation: {OCCUPATION}\n" +
            "Cause of Death: {CAUSE_OF_DEATH}\n\n" +
            "{PROSE_BODY}\n\n" +
            "Now write the moral content. Why is this person damned? What did they do? What do they tell themselves about it? " +
            "What do they refuse to face?\n\n" +
            "Write in the second person — \"you.\" The character is reading their own interior knowledge. " +
            "Each field is part of what they know about themselves, even the things they will not admit.\n\n" +
            "Generate the following:\n\n" +
            "**Reason for Damnation — True**\n" +
            "What you actually did that placed you here. Be concrete — the act, or the pattern of acts. " +
            "Stated as you know it in your bones, with no softening. One or two sentences.\n\n" +
            "**Reason for Damnation — Self-told**\n" +
            "The same fact, refracted through the story you prefer to tell yourself about it. " +
            "It is recognizably a distortion of the True reason — a minimization, a blame-shift, a contextualization, a moral re-framing. " +
            "Not a different story; the same story told differently. You usually live in this version. One or two sentences.\n\n" +
            "**What you refuse to admit about yourself**\n" +
            "The deeper psychological fact under the sin. Not what you did, but what you are. " +
            "The thing that, if named aloud in the room, would unmake you. One or two sentences.\n\n" +
            "Use this format exactly:\n\n" +
            "**Reason for Damnation — True**\n" +
            "[your text]\n\n" +
            "**Reason for Damnation — Self-told**\n" +
            "[your text]\n\n" +
            "**What you refuse to admit about yourself**\n" +
            "[your text]\n\n" +
            "Begin now.";

        characterStanceUserPrompt =
            "The character has been described:\n\n" +
            "Name: {NAME}\n" +
            "Age: {AGE}, {GENDER}\n" +
            "Occupation: {OCCUPATION}\n" +
            "Cause of Death: {CAUSE_OF_DEATH}\n" +
            "Reason for Damnation — True: {REASON_TRUE}\n" +
            "Reason for Damnation — Self-told: {REASON_SELF_TOLD}\n" +
            "What you refuse to admit about yourself: {REFUSE_TO_ADMIT}\n\n" +
            "{PROSE_BODY}\n\n" +
            "Now write the behavioral signature. How does this person show up in a room with strangers? What are they hoping for?\n\n" +
            "Write in the second person — \"you.\" The character is reading their own interior knowledge.\n\n" +
            "Generate the following:\n\n" +
            "**Defining personality trait**\n" +
            "One short phrase that names the dominant quality others would notice in you. Plain words. " +
            "(\"Charming and slippery.\" \"Quietly furious.\" \"Performatively kind.\")\n\n" +
            "**What you want from the others in the room**\n" +
            "Your starting drive. What you are hoping these strangers will give you — validation, silence, a fight, recognition, " +
            "forgiveness, an audience, to be left alone, to be hated openly, something else. One or two sentences.\n\n" +
            "Use this format exactly:\n\n" +
            "**Defining personality trait**\n" +
            "[your text]\n\n" +
            "**What you want from the others in the room**\n" +
            "[your text]\n\n" +
            "Begin now.";

        characterLifeAntiDuplicationBlock =
            "Important: a character has already been written for this room.\n\n" +
            "Their Occupation: {OTHER_OCCUPATION}\n" +
            "Their Cause of Death: {OTHER_CAUSE_OF_DEATH}\n\n" +
            "The character you are writing now must feel like a genuinely different person — a different life, a different end. Specifically:\n\n" +
            "- The occupation must be from a different kind of life. Not a near-equivalent job. If the other character drives for work, " +
            "do not write another driver. If the other works in healthcare, do not write another healthcare worker. " +
            "Pick a life that contrasts in class, work register, or daily texture.\n" +
            "- The cause of death must be a different kind of death. Not the same general category. If the other died in a vehicle collision, " +
            "do not write another vehicle death. If the other died of illness, do not write another illness. " +
            "The categories of death include: vehicle, violence, illness, accident, overdose, suicide, old age, occupational, and others — pick a different one.\n\n" +
            "Do not contradict, reference, or comment on the other character. " +
            "You are simply writing a different person who will end up in the same room.";

        characterSinAntiDuplicationBlock =
            "Note: another character in this room is damned for the following:\n\n" +
            "{OTHER_REASON_TRUE}\n\n" +
            "The character you are writing must be damned for a different kind of moral failure. " +
            "The categories: violence, cruelty, cowardice, manipulation, betrayal, neglect, vanity, hypocrisy, abuse, theft, " +
            "complicity, abandonment, self-deception, indifference. Pick a category the other character is not occupying.";

        characterStanceAntiDuplicationBlock =
            "Note: another character in this room wants the following from the others:\n\n" +
            "{OTHER_WANT}\n\n" +
            "The character you are writing should want something different. " +
            "Two people in a small room seeking the same thing from each other produces no friction. Pick a different drive.";
    }
}
