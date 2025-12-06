using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextOverlayController : MonoBehaviour
{
    public List<dialogueDisplay> TextDisplays;
    // public float overlapMargin = 10f; // Increased margin

    void Start()
    {
        if (TextDisplays == null)
        {
            TextDisplays = new List<dialogueDisplay>();
        }
    }

    void LateUpdate()
    {
        if (TextDisplays.Count > 1) // Only check if we have 2+ dialogues
        {
            if (TextDisplays[0].myBackgroundImage != null)
            {
                if (TextDisplays[1].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[0].myBackgroundImage.rectTransform, TextDisplays[1].myBackgroundImage.rectTransform))
                    {
                        //     Debug.Log($"🔍 {TextDisplays[0].gameObject.name} and {TextDisplays[1].gameObject.name} are overlapping, MOVING THE FIRST ONE");
                        MoveToNextPosition(TextDisplays[0]);
                    }
                }
                if (TextDisplays[2].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[0].myBackgroundImage.rectTransform, TextDisplays[2].myBackgroundImage.rectTransform))
                    {
                        //     Debug.Log($"🔍 {TextDisplays[0].gameObject.name} and {TextDisplays[1].gameObject.name} are overlapping, MOVING THE FIRST ONE");
                        MoveToNextPosition(TextDisplays[0]);
                    }
                }
            }
            if (TextDisplays[1].myBackgroundImage != null)
            {
                if (TextDisplays[0].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[1].myBackgroundImage.rectTransform, TextDisplays[0].myBackgroundImage.rectTransform))
                    {
                        //     Debug.Log($"🔍 {TextDisplays[0].gameObject.name} and {TextDisplays[1].gameObject.name} are overlapping, MOVING THE FIRST ONE");
                        MoveToNextPosition(TextDisplays[1]);
                    }
                }
                if (TextDisplays[2].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[1].myBackgroundImage.rectTransform, TextDisplays[2].myBackgroundImage.rectTransform))
                    {
                        //     Debug.Log($"🔍 {TextDisplays[0].gameObject.name} and {TextDisplays[1].gameObject.name} are overlapping, MOVING THE FIRST ONE");
                        MoveToNextPosition(TextDisplays[1]);
                    }
                }
            }

             if (TextDisplays[2].myBackgroundImage != null)
            {
                if (TextDisplays[0].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[2].myBackgroundImage.rectTransform, TextDisplays[0].myBackgroundImage.rectTransform))
                    {
                        //     Debug.Log($"🔍 {TextDisplays[0].gameObject.name} and {TextDisplays[1].gameObject.name} are overlapping, MOVING THE FIRST ONE");
                        MoveToNextPosition(TextDisplays[2]);
                    }
                }
                if (TextDisplays[1].myBackgroundImage != null)
                {
                    if (RectTransformsOverlap(TextDisplays[2].myBackgroundImage.rectTransform, TextDisplays[1].myBackgroundImage.rectTransform))
                    {
                        //     Debug.Log($"🔍 {TextDisplays[0].gameObject.name} and {TextDisplays[1].gameObject.name} are overlapping, MOVING THE FIRST ONE");
                        MoveToNextPosition(TextDisplays[2]);
                    }
                }
            }





            //  HashSet<dialogueDisplay> movedThisFrame = new HashSet<dialogueDisplay>();

            // for (int i = 0; i < TextDisplays.Count; i++)
            // {
            //     dialogueDisplay display = TextDisplays[i];

            //    if (display == null || display.myBackgroundImage == null) continue;

            // Skip if already moved this frame
            //    if (movedThisFrame.Contains(display)) continue;

            // List<dialogueDisplay> overlaps = GetOverlappingDialogues(display);

            //     if (overlaps.Count > 0)
            //     {
            //         Debug.Log($"🔍 {display.gameObject.name} has {overlaps.Count} overlaps, MOVING IT");
            //         // Move THIS box (the first one in the list that has an overlap)
            //         MoveToNextPosition(display);
            //  //       movedThisFrame.Add(display);
            //    }
            //     else
            //     {
            //         Debug.Log($"✅ {display.gameObject.name} has no overlaps");
            //     }
            // }
        }
    }

    public void addImage(dialogueDisplay newDisplay)
    {
        if (newDisplay != null && !TextDisplays.Contains(newDisplay))
        {
            TextDisplays.Add(newDisplay);
            Debug.Log($"Added dialogue to list. Total: {TextDisplays.Count}");
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

        Debug.Log($"Rect1: X({minX1:F1} to {maxX1:F1}) Y({minY1:F1} to {maxY1:F1})");
        Debug.Log($"Rect2: X({minX2:F1} to {maxX2:F1}) Y({minY2:F1} to {maxY2:F1})");
        Debug.Log($"OverlapX: {overlapX}, OverlapY: {overlapY}, RESULT: {overlaps}");

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

        Debug.Log($"🔄 {display.gameObject.name} (parent: {display.transform.parent.parent.name}) moved to position {controller.curentPos}");
    }
}

