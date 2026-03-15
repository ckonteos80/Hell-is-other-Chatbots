using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class dialogueDisplay : MonoBehaviour
{
    public TextMeshProUGUI myText;
    public Image myBackgroundImage;
    private RectTransform boxRectTransform;
    private RectTransform textRectTransform;

    [Header("Box Settings")]
    public float padding = 20f;
    public float maxWidth = 400f;  // ✅ Important: Limits width so text wraps
    public float minHeight = 60f;

    [Header("Overlap Detection")]
    public bool checkOverlap = true;

    private Canvas parentCanvas;
    private Transform canvasTransform;

    public int ActionClose;
    // private bool hasText = false;
    // private static int instanceCounter = 0; // Static counter for priority
    // private int instanceID; // This instance's ID for priority


    public TextPositionController myTextPositionController;




    private int currentPositionIndex = 0; // Track which position we're using

    [Header("Typewriter Settings")]
    public float wordDelay = 0.08f;
    public bool isRevealing = false;
    private Coroutine revealCoroutine;
    private bool revealOnEnable = false;

    void Awake()  // ✅ Use Awake instead of Start (runs earlier)
    {
        myBackgroundImage = GetComponent<Image>();
        //   instanceID = instanceCounter++;


     //   Debug.Log($"📦 {gameObject.name} initialized - Controller: {(myTextPositionController != null ? myTextPositionController.gameObject.name : "NULL")}, Parent Canvas: {transform.parent.parent.name}");

        myText = GetComponentInChildren<TextMeshProUGUI>();
        boxRectTransform = GetComponent<RectTransform>();

        if (myText == null)
        {
            Debug.LogError("dialogueDisplay: No TextMeshProUGUI found!");
        }
        else
        {
            textRectTransform = myText.GetComponent<RectTransform>();
        }

        // Get parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasTransform = parentCanvas.transform;
        }


    //    //  Debug.Log($"DialogueDisplay {instanceID} initialized on {gameObject.name}");
    }

    void Start()
    {
        myTextPositionController = transform.parent.GetComponent<TextPositionController>();
    }

    public void showMessage(string message, int actionOnClose)
    {

        // Set the text
        myText.text = message;

        // ✅ Force TextMeshPro to calculate size immediately
        myText.ForceMeshUpdate();

        // ✅ Calculate size with width constraint (so text wraps)
        Vector2 textSize = myText.GetPreferredValues(
            message,
            maxWidth - (padding * 2),  // Account for padding on both sides
            0                    // No height limit
        );

        // Calculate box size with padding
        float boxWidth = Mathf.Min(textSize.x + (padding * 2), maxWidth);
        float boxHeight = Mathf.Max(textSize.y + (padding * 2), minHeight);

        // ✅ Set the box size
        boxRectTransform.sizeDelta = new Vector2(boxWidth, boxHeight);

        // ✅ Update text RectTransform to fit inside box with padding
        // Set anchors to stretch (fill parent)
        textRectTransform.anchorMin = Vector2.zero;        // (0, 0) bottom-left
        textRectTransform.anchorMax = Vector2.one;         // (1, 1) top-right

        // Set offsets to create padding
        textRectTransform.offsetMin = new Vector2(padding, padding);      // Left, Bottom padding
        textRectTransform.offsetMax = new Vector2(-padding, -padding);    // Right, Top padding (negative!)

        ///     Debug.Log($"[ID:{instanceID}] Message: '{message}' | Canvas pos: {canvasTransform.localPosition} | Position index: {currentPositionIndex}");
        ActionClose = actionOnClose;

        myText.maxVisibleWords = 0;
        isRevealing = true;
        revealOnEnable = true;

    }

    void OnEnable()
    {
        if (revealOnEnable)
        {
            revealOnEnable = false;
            if (revealCoroutine != null) StopCoroutine(revealCoroutine);
            revealCoroutine = StartCoroutine(RevealWords());
        }
    }

    private IEnumerator RevealWords()
    {
        myText.ForceMeshUpdate();
        int totalWords = myText.textInfo.wordCount;
        myText.maxVisibleWords = 0;

        for (int i = 1; i <= totalWords; i++)
        {
            myText.maxVisibleWords = i;
            yield return new WaitForSeconds(wordDelay);
        }

        myText.maxVisibleWords = int.MaxValue;
        isRevealing = false;
    }




    // ✅ Clear text and stop checking overlaps
    public void ClearMessage()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }
        if (myText != null)
        {
            myText.text = "";
            myText.maxVisibleWords = int.MaxValue;
        }
        isRevealing = false;
        revealOnEnable = false;
        currentPositionIndex = 0;
    }


    void OnDestroy()
    {
        if (ActionClose == 1)
        {
            Master myMaster = GetComponentInParent<Master>();
            if (myMaster != null)
            {
                myMaster.theDoorController.doorOpen = false;
                AiEventController newEventController = myMaster.theCharacterController.Characters[1].GetComponent<AiEventController>();
                newEventController.addEventTime(5f, 1);
            }
        }
    }
}
