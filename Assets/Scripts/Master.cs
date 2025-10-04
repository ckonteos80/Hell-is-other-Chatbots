using UnityEngine;

public class Master : MonoBehaviour
{
    public MovementController theMovementController;
    public WaypointController theWaypointController;

    public CharacterController theCharacterController;

    
    public CharacterControllerGrok theCharacterControllerGrok;
    
    public characterControllerDeep theCharacterControllerDeep;

    public ParseController theParseController;

    public string key;

    
    public string GrokKey;

    public string DeepKey;


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
    }

    // Update is called once per frame
    void Update()
    {

    }

    
}
