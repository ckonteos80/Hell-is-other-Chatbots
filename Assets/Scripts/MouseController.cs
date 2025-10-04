using UnityEngine;

public class MouseController : MonoBehaviour
{


    public Sprite floorCursor;        // Assign in Inspector
    public Sprite overrideCursor;     // Assign in Inspector
    public Sprite defaultCursor;      // Assign in Inspector (for when not hovering over anything)

    public SpriteRenderer cursorRenderer; // SpriteRenderer for cursor
    private int floorLayer;
    private int overrideLayer;

    void Start()
    {
        floorLayer = LayerMask.NameToLayer("Floor");
        overrideLayer = LayerMask.NameToLayer("OverrideCollider");

        Cursor.visible = false; // Hide the system cursor
        SetCursor(defaultCursor, Color.yellow); // Start with default cursor
    }

    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition; // Move the cursor sprite to the mouse position

        // Use RaycastAll to detect all colliders hit by the ray
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        bool hitOverrideCollider = false; // Flag to track if we hit an OverrideCollider

        // Loop through all the hits to check if any is from the OverrideCollider layer
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.layer == overrideLayer)
            {
                SetCursor(overrideCursor, Color.blue); // Set to override cursor if hit
                hitOverrideCollider = true; // Mark that we've hit an OverrideCollider
                break; // Stop the loop since we only need one hit for the override cursor
            }
        }

        // If no override collider was hit, check for floor collider
        if (!hitOverrideCollider)
        {
            // If no collider is hit, use default cursor
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Floor"));
            if (hit.collider != null)
            {
                SetCursor(floorCursor, Color.red);
            }
            else
            {
                SetCursor(defaultCursor, Color.yellow); // No collider hit, use default cursor
            }
        }
    }

    void SetCursor(Sprite newCursor, Color color)
    {
        if (cursorRenderer.sprite != newCursor)
        {
            cursorRenderer.sprite = newCursor;
        }


        if (cursorRenderer.color != color)
        {
            cursorRenderer.color = color;
        }
    }
}
