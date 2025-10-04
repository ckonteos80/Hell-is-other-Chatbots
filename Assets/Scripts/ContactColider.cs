using UnityEngine;

public class ContactColider : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    PlayerMovement myPlayerMovement;

    MovementController myMovedController;

    bool touching;

    void Start()
    {
        // Get the BoxCollider2D component attached to the object
        boxCollider = GetComponent<BoxCollider2D>();
        ///  myPlayerMovement = GetComponentInParent<PlayerMovement>();
        myMovedController = GetComponentInParent<MovementController>();
    }

    // This function checks the relative position of a point to the BoxCollider
    public int GetSideOfPoint(Vector2 point)
    {
        // Get the center and size of the BoxCollider, accounting for its offset
        Vector2 boxCenter = boxCollider.bounds.center;
        Vector2 boxSize = boxCollider.bounds.size;

        // Get half the width and height of the collider for comparison
        float halfWidth = boxSize.x / 2f;
        float halfHeight = boxSize.y / 2f;

        // Check if the point is on the left, right, top, or bottom
        if (point.y > boxCenter.y + halfHeight)  // Top
        {
            return 0; // Top
        }
        else if (point.y < boxCenter.y - halfHeight)  // Bottom
        {
            return 1; // Bottom
        }
        else if (point.x > boxCenter.x + halfWidth)  // Right
        {
            return 2; // Right
        }
        else if (point.x < boxCenter.x - halfWidth)  // Left
        {
            return 3; // Left
        }

        return 4;  // If the point is inside the box
    }

    // Example: OnCollisionStay2D to check if the point collides with the sides of the BoxCollider
    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Debug.Log($"Contact Point: {contact.point}"); // For debugging
            // Debug.Log($"Collider Center: {boxCollider.bounds.center}"); // For debugging
            // Debug.Log($"Collider Size: {boxCollider.bounds.size}"); // For debugging

            int side = GetSideOfPoint(contact.point);
            ///   myPlayerMovement.contactSide = side;
            myMovedController.contactSide = side;
        }

        ObjectBlocking objectBlock = collision.gameObject.GetComponent<ObjectBlocking>();

        // If PlayerMovement script is found, set the 'touching' bool to true
        if (objectBlock != null)
        {
            touching = true;
          //  myPlayerMovement.blockedByObject = objectBlock;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
     ///   myPlayerMovement.contactSide = 5;
     ///   
     myMovedController.contactSide = 5;

        ObjectBlocking objectBlock = collision.gameObject.GetComponent<ObjectBlocking>();

        // If PlayerMovement script is found, set the 'touching' bool to true
        if (objectBlock != null)
        {
            touching = false;
          //  myPlayerMovement.blockedByObject = null;
        }
    }
}

