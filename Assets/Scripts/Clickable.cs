using UnityEngine;

public class Clickable : MonoBehaviour
{
    private PolygonCollider2D polyCollider;
    public Vector2 clickedPos;

    Master myMaster;

    public bool isObject;

    public bool objectClicked;
    ObjectMovePlayer myObjectMove;

    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        polyCollider = GetComponent<PolygonCollider2D>(); // Get the Polygon Collider
        myObjectMove = GetComponent<ObjectMovePlayer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickedPos = mousePos;

            // First, check if an object in "OverrideCollider" is clicked
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("OverrideCollider"));

            if (hit.collider != null) // Override Collider detected
            {
                if (hit.collider.gameObject == this.gameObject) // Ensure it belongs to THIS GameObject
                {
                    // objectClicked = true;
                    Debug.Log("Clicked inside Override Collider!");

                    if (isObject)
                    {
                        if (myMaster.theMovementController.sitting)
                        {
                            myMaster.theMovementController.actionStand();
                        }
                        objectClicked = true;
                        myObjectMove.ClickedObject();
                    }
                }




                return; // Exit early, preventing Floor Collider processing
            }

            // If no OverrideCollider was hit, process Floor Collider
            hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Floor"));

            if (hit.collider != null && !objectClicked) // Floor Collider detected
            {
                Debug.Log("Clicked inside Floor Collider!");

                if (isObject)
                {
                    objectClicked = true;
                }
                else
                {
                    if (myMaster.theMovementController.sitting)
                    {
                        myMaster.theMovementController.actionStand();
                    }
                    myMaster.theWaypointController.SetTarget(clickedPos);
                    myMaster.theMovementController.ActionToPerform = 0;

                    // for main waypoints
                    // myMaster.thePlayerMovement.MoveToPos.Clear();
                    // myMaster.thePlayerMovement.ActionToPerform = 0;
                    // myMaster.thePlayerMovement.MoveToPos.Add(clickedPos);
                }
            }
        }


    }
}
