using System.Collections.Generic;
using UnityEngine;

public class TextPositionController : MonoBehaviour
{
    public List<Vector2> textPositions;
    public int curentPos;
    RectTransform rectTransform;
    public RectTransform boxRectTransform;

    public List<Vector2> anchorMinPositions; // Bottom-left anchor
    public List<Vector2> anchorMaxPositions; // Top-right anchor
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        setCanvasPosition(curentPos);

        if (transform.childCount > 0)
        {
            // Get first child's RectTransform
            boxRectTransform = transform.GetChild(0).GetComponent<RectTransform>();

            // if (boxRectTransform == null)
            // {
            //     Debug.LogError("TextPositionController: First child has no RectTransform!");
            // }
        }

    }

    public void setCanvasPosition(int position)
    {

        rectTransform.localPosition = textPositions[position];


        if (boxRectTransform != null)
        {
            boxRectTransform.anchorMin = anchorMinPositions[position];
            boxRectTransform.anchorMax = anchorMaxPositions[position];

            Vector2 pivot = (anchorMinPositions[position] + anchorMaxPositions[position]) / 2f;
            boxRectTransform.pivot = pivot;

            boxRectTransform.anchoredPosition = Vector2.zero;
        }

    }
}
