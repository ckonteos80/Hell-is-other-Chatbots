using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public List<Vector2> MoveToPos;
    public float Xvelocity;

    public float Yvelocity;

    public float leniance;

    public BoxColliderSideCheck TopTouch;
    public BoxColliderSideCheck BottomTouch;
    public BoxColliderSideCheck leftTouch;
    public BoxColliderSideCheck RightTouch;

    public int contactSide;

    public bool top;
    public bool Bottom;
    public bool left;
    public bool right;

    public ObjectBlocking blockedByObject;

    public float distance;

    Rigidbody2D myRB;

    public int ActionToPerform;

    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
    }
    void Update()
    {

        top = TopTouch.touching;
        Bottom = BottomTouch.touching;
        left = leftTouch.touching;
        right = RightTouch.touching;


        if (MoveToPos.Count >= 1)
        {

            if (Mathf.Abs(gameObject.transform.position.x - MoveToPos[0].x) > leniance)
            {
                if (MoveToPos[0].x < gameObject.transform.position.x)
                {
                    // if (!leftTouch.touching)
                    // {
                    //     Xvelocity = -1;
                    // }
                    // else
                    // {
                    //     Xvelocity = 0;
                    // }

                    if (contactSide != 3)
                    {
                        Xvelocity = -1;
                    }
                    else
                    {
                        Xvelocity = 0;
                    }

                }
                else
                {
                    // if (!RightTouch.touching)
                    // {
                    //     Xvelocity = 1;
                    // }
                    // else
                    // {
                    //     Xvelocity = 0;
                    // }

                    if (contactSide != 2)
                    {
                        Xvelocity = 1;
                    }
                    else
                    {
                        Xvelocity = 0;
                    }

                }
            }
            else
            {
                Xvelocity = 0;
            }

            if (Mathf.Abs(gameObject.transform.position.y - MoveToPos[0].y) > leniance)
            {
                if (MoveToPos[0].y < gameObject.transform.position.y)
                {
                    // if (!BottomTouch.touching)
                    // {
                    //     Yvelocity = -1;
                    // }
                    // else
                    // {
                    //     Yvelocity = 0;
                    // }
                    if (contactSide != 1)
                    {
                        Yvelocity = -1;
                    }
                    else
                    {
                        Yvelocity = 0;
                    }
                }
                else
                {
                    // if (!TopTouch.touching)
                    // {
                    //     Yvelocity = 1;
                    // }
                    // else
                    // {
                    //     Yvelocity = 0;
                    // }
                    if (contactSide != 0)
                    {
                        Yvelocity = 1;
                    }
                    else
                    {
                        Yvelocity = 0;
                    }

                }
            }
            else
            {
                Yvelocity = 0;
            }

            distance = Vector2.Distance(gameObject.transform.position, MoveToPos[0]);
            if (distance < leniance + 0.05)
            {
                MoveToPos.RemoveAt(0);
                Yvelocity = 0;
                Xvelocity = 0;
            }

            if (distance > leniance + 0.1 && Yvelocity == 0 && Xvelocity == 0)
            {
                // if (TopTouch || BottomTouch)
                if (contactSide == 0 || contactSide == 1)
                {
                    // if (!leftTouch && !RightTouch)
                    // {
                    //  if (MoveToPos.Count == 1)
                    ///   {

                    if (blockedByObject != null)
                    {
                        MoveToPos.Insert(0, GetClosestTransform(blockedByObject.waypointsVertical).position);
                    }
                    //  }

                    // }
                }

                // if (leftTouch || RightTouch)
                if (contactSide == 2 || contactSide == 3)
                {
                    // if (!TopTouch && !BottomTouch)
                    // {
                    //  if (MoveToPos.Count == 1)
                    //   {
                    if (blockedByObject != null)
                    {
                        MoveToPos.Insert(0, GetClosestTransform(blockedByObject.waypointsHorizontal).position);
                    }
                    // /
                    // /  }
                    // }

                }


            }





            myRB.linearVelocity = new Vector2(Xvelocity, Yvelocity);



        }
        else
        {
            distance = 0;
        }


    }

    Transform GetClosestTransform(List<Transform> waypoints)
    {
        // Initialize the closest transform to null
        Transform closestTransform = null;

        // Initialize the smallest distance to a large number (e.g., Mathf.Infinity)
        float smallestDistance = Mathf.Infinity;

        // Loop through each item in the list
        foreach (Transform target in waypoints)
        {
            // Calculate the distance between this GameObject's position and the current target's position
            float distance = Vector3.Distance(transform.position, target.position);

            // If this distance is smaller than the current smallest distance, update it
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestTransform = target; // Update the closest transform
            }
        }

        // Return the closest transform
        return closestTransform;
    }
}
