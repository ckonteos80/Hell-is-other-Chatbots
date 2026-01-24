using UnityEngine;
using TMPro;

public class ParseController : MonoBehaviour
{
    Master myMaster;
    public TMP_InputField inputField; // Reference to the TMP InputField

    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        inputField.ActivateInputField(); // Ensure it's active at start
        inputField.onSubmit.AddListener(HandleInput); // Listen for Enter key
    }

    void HandleInput(string input)
    {
        // Prevent handling input if it's already being processed

        if (inputField.interactable)
        {
            if (myMaster.theOverlayController.TextDisplays.Count == 0)
            {
                // Only process if the input is not empty
                if (!string.IsNullOrEmpty(input))
                {
                    Debug.Log("Input received: " + input);
                    myMaster.theCharacterController.ParsedText(input, 0);
                    //  myMaster.theCharacterControllerGrok.ParsedText(input);
                    //  myMaster.theCharacterControllerDeep.ParsedText(input);

                    myMaster.theCharacterController.ShowDialog(input, 0, 0);
                    ///  myMaster.theCharacterControllerGrok.ShowDialog(input, 0);
                  //  myMaster.theCharacterControllerDeep.ShowDialog(input, 0);
                    inputField.text = "";
                }
            }
        }
    }

    void Update()
    {
        if (!inputField.interactable)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (myMaster.theOverlayController.TextDisplays.Count > 0)
                {
                    // Destroy(myMaster.theOverlayController.TextDisplays[0].gameObject);
                    // myMaster.theOverlayController.TextDisplays.RemoveAt(0);
                    myMaster.theOverlayController.clearText();
                }

                // if (myMaster.theCharacterControllerGrok.Dialogues.Count > 0)
                // {
                //     Destroy(myMaster.theCharacterControllerGrok.Dialogues[0]);
                //     myMaster.theCharacterControllerGrok.Dialogues.RemoveAt(0);
                // }

                // if (myMaster.theCharacterControllerDeep.Dialogues.Count > 0)
                // {
                //     Destroy(myMaster.theCharacterControllerDeep.Dialogues[0]);
                //     myMaster.theCharacterControllerDeep.Dialogues.RemoveAt(0);
                // }
            }
        }
        // Check if the input field is selected and if arrow keys are pressed
        if (inputField.isFocused)
        {
            // Disable arrow key inputs
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            {
                // Prevent any arrow key from affecting the input field
                PreventArrowKeyInput();
            }
        }
        if (myMaster.theOverlayController.TextDisplays.Count == 0)
        //   if (myMaster.theCharacterControllerDeep.Dialogues.Count == 0)
        //  if (myMaster.theCharacterControllerGrok.Dialogues.Count == 0)
        {
            inputField.ActivateInputField(); // Keep it active
            inputField.interactable = true;
        }
        else
        {
            inputField.DeactivateInputField();
            inputField.interactable = false;
        }
    }

    private void PreventArrowKeyInput()
    {
        // Set up an event to prevent arrow key actions in the InputField
        Event currentEvent = Event.current;
        if (currentEvent != null &&
            (currentEvent.keyCode == KeyCode.LeftArrow ||
             currentEvent.keyCode == KeyCode.RightArrow ||
             currentEvent.keyCode == KeyCode.UpArrow ||
             currentEvent.keyCode == KeyCode.DownArrow))
        {
            // Stop propagation of the arrow key events
            currentEvent.Use();
        }
    }
}
