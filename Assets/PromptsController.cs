using UnityEngine;
using System.Collections.Generic;

public class PromptsController : MonoBehaviour
{
    [TextArea(3, 10)]
    //  public string characterUser;
    public string mainGameSystemPrompt;
    //[TextArea(3, 10)]    // Prompts and summaries.
    //public string baseSetup;
    [TextArea(3, 10)]
    public string characterSetupSystemPrompt;
    [TextArea(3, 10)]
    public string characterSetupUserPrompt;
    // [TextArea(3, 10)]
    // //  public string characterUser;
    // public string characterNameSystemPrompt;
    [TextArea(3, 10)]
    public string characterNameUserPrompt; // Used when generating a name.
    [TextArea(3, 10)]
    public string characterDialogueSystemPrompt; // Used when generating a name.
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

    


    public List<string> systemPrompts; // One per character.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
