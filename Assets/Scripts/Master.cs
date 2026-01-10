using UnityEngine;


using System.Collections;

public class Master : MonoBehaviour
{
    public MovementController theMovementController;
    public WaypointController theWaypointController;

    public CharacterController theCharacterController;


    public ParseController theParseController;

    public TextOverlayController theOverlayController;

    public DoorController theDoorController;

    public Transform thePlayerStartMovePos;

    //  public bool CanCLickMove;
    public bool blockClick;

    public float timeFromStart;

    public PromptsController thePromptsController;

    /// <summary>
    ///  public string key;
    /// </summary>


    /// public string GrokKey;

    ///  public string DeepKey;


    /// <summary>
    /// public PlayerMovement thePlayerMovement;
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // theCharacterController = GetComponentInChildren<CharacterController>();
        ///    thePlayerMovement = GetComponentInChildren<PlayerMovement>();
        theMovementController = GetComponentInChildren<MovementController>();
        theWaypointController = GetComponentInChildren<WaypointController>();
        theParseController = GetComponentInChildren<ParseController>();


        thePromptsController = GetComponent<PromptsController>();
    }
    void Start()
    {
        StartCoroutine(WelcomePlayer());
    }

    IEnumerator WelcomePlayer()
    {
        yield return null; // Wait one frame for CharacterController.Start() to run
        theCharacterController.RequestNarratorDialogue("welcome the player to the game");
    }


    // Update is called once per frame
    void Update()
    {
        timeFromStart += Time.deltaTime;

    }

    public void SetNextEvent()
    {
        // Null check theCharacterController
        if (theCharacterController == null)
        {
            Debug.LogWarning("SetNextEvent: theCharacterController is null");
            return;
        }
        
        // Null check Characters list and ensure it has characters
        if (theCharacterController.Characters == null || theCharacterController.Characters.Count == 0)
        {
            Debug.LogWarning("SetNextEvent: Characters list is null or empty");
            return;
        }
        
        int randomChar = Random.Range(0, theCharacterController.Characters.Count);
        
        // Null check the selected character
        if (theCharacterController.Characters[randomChar] == null)
        {
            Debug.LogWarning($"SetNextEvent: Characters[{randomChar}] is null");
            return;
        }
        
        // Null check myAiEventController
        if (theCharacterController.Characters[randomChar].myAiEventController == null)
        {
            Debug.LogWarning($"SetNextEvent: Characters[{randomChar}].myAiEventController is null - component may not be assigned in Inspector");
            return;
        }
        
        float eventDelay = Random.Range(5f, 10f);  
        theCharacterController.Characters[randomChar].myAiEventController.addEventTime(eventDelay, 1);
    }


}

