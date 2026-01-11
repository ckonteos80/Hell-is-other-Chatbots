using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextOverlayController : MonoBehaviour
{
    public List<dialogueDisplay> TextDisplays;
    Master myMaster;
    // public float overlapMargin = 10f; // Increased margin

    void Start()
    {

        myMaster = GetComponentInParent<Master>();
        if (TextDisplays == null)
        {
            TextDisplays = new List<dialogueDisplay>();
        }
    }

    void LateUpdate()
    {
        ManageActiveDisplays();

        if (TextDisplays.Count > 1) // Only check if we have 2+ dialogues
        {
            if (TextDisplays[0].myBackgroundImage != null && TextDisplays[1].myBackgroundImage != null)
            {
                if (RectTransformsOverlap(TextDisplays[0].myBackgroundImage.rectTransform, TextDisplays[1].myBackgroundImage.rectTransform))
                {
                    MoveToNextPosition(TextDisplays[0]);
                }
            }

            if (TextDisplays.Count > 2) // Check for third dialogue
            {
                if (TextDisplays[0].myBackgroundImage != null && TextDisplays[2].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[0].myBackgroundImage.rectTransform, TextDisplays[2].myBackgroundImage.rectTransform))
                    {
                        MoveToNextPosition(TextDisplays[0]);
                    }
                }

                if (TextDisplays[1].myBackgroundImage != null && TextDisplays[2].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[1].myBackgroundImage.rectTransform, TextDisplays[2].myBackgroundImage.rectTransform))
                    {
                        MoveToNextPosition(TextDisplays[1]);
                    }
                }
            }
        }
    }

    public void clearText()
    {
        Destroy(TextDisplays[0].gameObject);
        TextDisplays.RemoveAt(0);

        if (myMaster.theCharacterController.Characters[1].myAiEventController.nextEventType == 0 && myMaster.theCharacterController.Characters[2].myAiEventController.nextEventType == 0)
        {
     //       myMaster.SetNextEvent();
        }
    }


    void ManageActiveDisplays()
    {
        for (int i = 0; i < TextDisplays.Count; i++)
        {
            if (TextDisplays[i] != null)
            {
                // Keep first 3 active, deactivate the rest
                TextDisplays[i].gameObject.SetActive(i < 3);
            }
        }
    }

    public void addImage(dialogueDisplay newDisplay)
    {
        if (newDisplay != null && !TextDisplays.Contains(newDisplay))
        {
            newDisplay.gameObject.SetActive(false);
            TextDisplays.Add(newDisplay);
            //            Debug.Log($"Added dialogue to list. Total: {TextDisplays.Count}");
        }
    }

    // List<dialogueDisplay> GetOverlappingDialogues(dialogueDisplay target)
    // {
    //     List<dialogueDisplay> overlapping = new List<dialogueDisplay>();

    //     RectTransform targetRect = target.myBackgroundImage.rectTransform;

    //     for (int i = 0; i < TextDisplays.Count; i++)
    //     {
    //         dialogueDisplay other = TextDisplays[i];

    //         // ✅ Skip self, null, and entries without images
    //         if (other == target || other == null || other.myBackgroundImage == null) continue;

    //         // ✅ Skip if same parent (same player)
    //         if (target.transform.parent == other.transform.parent) continue;

    //         RectTransform otherRect = other.myBackgroundImage.rectTransform;

    //         if (RectTransformsOverlap(targetRect, otherRect))
    //         {
    //             Debug.Log($"⚠️ OVERLAP FOUND: {target.gameObject.name} <-> {other.gameObject.name}");
    //             overlapping.Add(other);
    //         }
    //     }
    //     return overlapping;
    // }

    // ✅ Simple AABB overlap check with proper margin handling
    bool RectTransformsOverlap(RectTransform rect1, RectTransform rect2)
    {
        if (rect1 == null || rect2 == null) return false;

        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];

        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);

        // Get actual bounds
        float minX1 = Mathf.Min(corners1[0].x, corners1[1].x, corners1[2].x, corners1[3].x);
        float maxX1 = Mathf.Max(corners1[0].x, corners1[1].x, corners1[2].x, corners1[3].x);
        float minY1 = Mathf.Min(corners1[0].y, corners1[1].y, corners1[2].y, corners1[3].y);
        float maxY1 = Mathf.Max(corners1[0].y, corners1[1].y, corners1[2].y, corners1[3].y);

        float minX2 = Mathf.Min(corners2[0].x, corners2[1].x, corners2[2].x, corners2[3].x);
        float maxX2 = Mathf.Max(corners2[0].x, corners2[1].x, corners2[2].x, corners2[3].x);
        float minY2 = Mathf.Min(corners2[0].y, corners2[1].y, corners2[2].y, corners2[3].y);
        float maxY2 = Mathf.Max(corners2[0].y, corners2[1].y, corners2[2].y, corners2[3].y);

        // Standard AABB overlap test
        bool overlapX = minX1 < maxX2 && maxX1 > minX2;
        bool overlapY = minY1 < maxY2 && maxY1 > minY2;

        bool overlaps = overlapX && overlapY;

        // Debug.Log($"Rect1: X({minX1:F1} to {maxX1:F1}) Y({minY1:F1} to {maxY1:F1})");
        // Debug.Log($"Rect2: X({minX2:F1} to {maxX2:F1}) Y({minY2:F1} to {maxY2:F1})");
        // Debug.Log($"OverlapX: {overlapX}, OverlapY: {overlapY}, RESULT: {overlaps}");

        return overlaps;
    }

    void MoveToNextPosition(dialogueDisplay display)
    {
        TextPositionController controller = display.myTextPositionController;
        if (controller == null) return;

        int maxPositions = controller.textPositions.Count;
        if (maxPositions == 0) return;

        // Move to next position
        controller.curentPos = (controller.curentPos + 1) % maxPositions;

        // Force immediate position update
        controller.setCanvasPosition(controller.curentPos);

        //      Debug.Log($"🔄 {display.gameObject.name} (parent: {display.transform.parent.parent.name}) moved to position {controller.curentPos}");
    }
}

