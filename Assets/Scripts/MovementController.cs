using UnityEngine;
using System.Collections.Generic;

public class MovementController : MonoBehaviour
{
    public List<Vector2> MoveToPos;
    public float Xvelocity;

    public float Yvelocity;

    public float leniance;

    // public BoxColliderSideCheck TopTouch;
    // public BoxColliderSideCheck BottomTouch;
    // public BoxColliderSideCheck leftTouch;
    // public BoxColliderSideCheck RightTouch;

    public int contactSide;

    // public bool top;
    // public bool Bottom;
    // public bool left;
    // public bool right;

    //  public ObjectBlocking blockedByObject;

    public float distance;

    public Rigidbody2D myRB;

    public int ActionToPerform;

    public bool sitting;

    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
    }
    void Update()
    {

        // top = TopTouch.touching;
        // Bottom = BottomTouch.touching;
        // left = leftTouch.touching;
        // right = RightTouch.touching;


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

            move();
      //      myRB.linearVelocity = new Vector2(Xvelocity, Yvelocity);



        }
        else
        {
            distance = 0;

            if (ActionToPerform == 1)
            {
                ActionSit();
            }
        }


    }

    public void ActionSit()
    {
        SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        mySprite.color = Color.red;
        sitting = true;
        ActionToPerform = 0;
    }

    public void actionStand()
    {
        SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        mySprite.color = Color.white;
        sitting = false;
    }

    public void move()
    {
        myRB.linearVelocity = new Vector2(Xvelocity, Yvelocity);
    }

    public void clearHitObject()
    {
        
    }




}
