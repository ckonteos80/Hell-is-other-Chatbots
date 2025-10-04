using System.Collections.Generic;
using UnityEngine;

public class BoxColliderSideCheck : MonoBehaviour
{
    public bool touching;

    private BoxCollider2D boxCollider;

    PlayerMovement myPlayerMovement;



    void Start()
    {
        myPlayerMovement = GetComponentInParent<PlayerMovement>();
        boxCollider = GetComponent<BoxCollider2D>(); // Ensure the BoxCollider2D is assigned
    }

    // Called when a collision is ongoing (touching)
    void OnCollisionStay2D(Collision2D collision)
    {
        ObjectBlocking objectBlock = collision.gameObject.GetComponent<ObjectBlocking>();

        // If PlayerMovement script is found, set the 'touching' bool to true
        if (objectBlock != null)
        {
            touching = true;
            myPlayerMovement.blockedByObject = objectBlock;
        }

    }

    // Called when a collision stops (touch ends)
    void OnCollisionExit2D(Collision2D collision)
    {
        ObjectBlocking objectBlock = collision.gameObject.GetComponent<ObjectBlocking>();

        // If PlayerMovement script is found, set the 'touching' bool to true
        if (objectBlock != null)
        {
            touching = false;
            myPlayerMovement.blockedByObject = null;
        }

    }


    // Optionally, you can log the states for debugging
    // void Update()
    // {
    //     if (isTouchingLeft) Debug.Log("Touching Left side");
    //     if (isTouchingRight) Debug.Log("Touching Right side");
    //     if (isTouchingTop) Debug.Log("Touching Top side");
    //     if (isTouchingBottom) Debug.Log("Touching Bottom side");
    // }
}
