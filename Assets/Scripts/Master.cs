using UnityEngine;

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

    /// <summary>
    ///  public string key;
    /// </summary>


    /// public string GrokKey;

    ///  public string DeepKey;


    /// <summary>
    /// public PlayerMovement thePlayerMovement;
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // theCharacterController = GetComponentInChildren<CharacterController>();
        ///    thePlayerMovement = GetComponentInChildren<PlayerMovement>();
        theMovementController = GetComponentInChildren<MovementController>();
        theWaypointController = GetComponentInChildren<WaypointController>();
        theParseController = GetComponentInChildren<ParseController>();

        theCharacterController.RequestNarratorDialogue("welcome the player to the game");

    }
    // Update is called once per frame
    void Update()
    {
        timeFromStart += Time.deltaTime;
    }


}

