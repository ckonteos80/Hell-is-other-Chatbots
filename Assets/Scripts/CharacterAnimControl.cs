using UnityEngine;

public class CharacterAnimControl : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MovementController movementController;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private string currentState;

    // Animation state names
    public string IDLE = "idle_Clip";
    public string HORIZONTAL_WALK = "HorizontalWalk_Clip";
    public string WALK_UP = "WalkUp_Clip";
    public string WALK_DOWN = "WalkingDown_Clip";

    void Start()
    {
        // Get components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
        if (movementController == null)
            movementController = GetComponentInParent<MovementController>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (movementController == null || animator == null)
            return;

        // Get velocity from movement controller
        float xVel = movementController.Xvelocity;
        float yVel = movementController.Yvelocity;

        // Debug logging
    //    Debug.Log($"XVel: {xVel}, YVel: {yVel}, Current State: {currentState}");

        // Small threshold for floating point comparison
        float threshold = 0.01f;

        // Determine animation state with horizontal priority
        string newState = IDLE; // Default to idle

        // Horizontal movement takes priority
        if (Mathf.Abs(xVel) >= threshold)
        {
            newState = HORIZONTAL_WALK;
            // Flip sprite based on horizontal direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = xVel < 0;
            }
        }
        // Vertical movement only if no horizontal movement
        else if (yVel > threshold)
        {
            newState = WALK_UP;
        }
        else if (yVel < -threshold)
        {
            newState = WALK_DOWN;
        }
        // Otherwise stays IDLE

        changeState(newState);
    }

    void changeState(string newState)
    {
        if (newState != currentState)
        {
            animator.Play(newState, 0, 0f);
            currentState = newState;
            Debug.Log($"Changed animation to: {newState}");
        }
    }
}
