using Unity.VisualScripting;
using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    MovementController myMovementController;
    WaypointController myWaypointController;
    Master myMaster;

    public bool usingKeys;

    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        myMovementController = GetComponent<MovementController>();
        myWaypointController = GetComponent<WaypointController>();
    }

    void Update()
    {
        // Check if any arrow key is pressed (up, down, left, or right)
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (myMaster.theParseController.inputField.isFocused)
            {
                myMaster.theParseController.inputField.DeactivateInputField();  // Unfocus the input field
            }
            //    Debug.Log("An arrow key is pressed.");
            if (!usingKeys)
            {
                myMovementController.Xvelocity = 0;
                myMovementController.Yvelocity = 0;
                myMovementController.move();
            }
            usingKeys = true;
            OverrideKeyboard();
            myMovementController.move();

            if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                myMovementController.Xvelocity = 0;
                if (!Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow))
                {
                    myMovementController.Yvelocity = 0;
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                if (!Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow))
                {
                    myMovementController.Yvelocity = 0;
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    myMovementController.Xvelocity = -1;
                }
                else
                {
                    if (!Input.GetKey(KeyCode.RightArrow))
                    {
                        myMovementController.Xvelocity = 0;
                    }
                }

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    myMovementController.Xvelocity = 1;
                }
                else
                {
                    if (!Input.GetKey(KeyCode.LeftArrow))
                    {
                        myMovementController.Xvelocity = 0;
                    }
                }
            }

            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
            {
                myMovementController.Yvelocity = 0;
                if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                {
                    myMovementController.Xvelocity = 0;
                }

            }
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            {
                if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                {
                    myMovementController.Xvelocity = 0;
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    myMovementController.Yvelocity = 1;
                }
                else
                {
                    if (!Input.GetKey(KeyCode.DownArrow))
                    {
                        myMovementController.Yvelocity = 0;
                    }
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    myMovementController.Yvelocity = -1;
                }
                else
                {
                    if (!Input.GetKey(KeyCode.UpArrow))
                    {
                        myMovementController.Yvelocity = 0;
                    }
                }
            }
        }
        else
        {
            if (!myMaster.theParseController.inputField.isFocused)
            {
                myMaster.theParseController.inputField.ActivateInputField();  // Unfocus the input field
            }
            if (usingKeys)
            {
                myMovementController.Xvelocity = 0;
                myMovementController.Yvelocity = 0;
                myMovementController.move();
                usingKeys = false;
            }
        }
    }
    // void Update()
    // {
    //     if (!myMovementController.sitting)
    //     {
    //        

    //         if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.UpArrow))
    //         {
    //             // Check for arrow key inputs
    //             if (Input.GetKeyDown(KeyCode.UpArrow))
    //             {
    //                 usingKeys = true;
    //                 myMovementController.Yvelocity = 1;
    //                 myMovementController.move();
    //                 OverrideKeyboard();
    //                 Debug.Log("Up Arrow Key Pressed");
    //             }
    //             else
    //             {
    //                 if (usingKeys)
    //                 {
    //                     myMovementController.Yvelocity = 0;
    //                     myMovementController.move();
    //                 }
    //             }

    //             if (Input.GetKeyDown(KeyCode.DownArrow))
    //             {
    //                 usingKeys = true;
    //                 myMovementController.Yvelocity = -1;
    //                 myMovementController.move();
    //                 OverrideKeyboard();
    //                 Debug.Log("Down Arrow Key Pressed");
    //             }
    //             else
    //             {
    //                 if (usingKeys)
    //                 {
    //                     myMovementController.Yvelocity = 0;
    //                     myMovementController.move();
    //                 }
    //             }
    //         }


    //         if (Input.GetKeyDown(KeyCode.LeftArrow))
    //         {
    //             usingKeys = true;
    //             myMovementController.Xvelocity = -1;
    //             myMovementController.move();
    //             OverrideKeyboard();
    //             Debug.Log("Left Arrow Key Pressed");
    //         }
    //         else
    //         {
    //             if (usingKeys)
    //             {
    //                 myMovementController.Xvelocity = 0;
    //                 myMovementController.move();
    //             }
    //         }

    //         if (Input.GetKeyDown(KeyCode.RightArrow))
    //         {
    //             usingKeys = true;
    //             myMovementController.Xvelocity = 1;
    //             myMovementController.move();
    //             OverrideKeyboard();
    //             Debug.Log("Right Arrow Key Pressed");
    //         }
    //         else
    //         {
    //             if (usingKeys)
    //             {
    //                 myMovementController.Xvelocity = 0;
    //                 myMovementController.move();
    //             }
    //         }
    //     }

    //     if (usingKeys)
    //     {
    //         if (!Input.GetKey(KeyCode.UpArrow) &&
    //            !Input.GetKey(KeyCode.DownArrow) &&
    //            !Input.GetKey(KeyCode.LeftArrow) &&
    //            !Input.GetKey(KeyCode.RightArrow))
    //         {
    //             usingKeys = false;
    //             myMovementController.Xvelocity = 0;
    //             myMovementController.Yvelocity = 0;
    //             myMovementController.move();
    //         }
    //     }
    // }

    void OverrideKeyboard()
    {
        if (myMovementController.MoveToPos.Count > 0)
        {
            myMovementController.MoveToPos.Clear();
        }
        if (myMovementController.ActionToPerform != 0)
        {
            myMovementController.ActionToPerform = 0;

        }
        if (myWaypointController.hitObject != null)
        {
            myWaypointController.hitObject = null;
        }
    }
}
