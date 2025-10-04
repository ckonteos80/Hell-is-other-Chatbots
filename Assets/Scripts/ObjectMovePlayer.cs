using UnityEngine;

public class ObjectMovePlayer : MonoBehaviour
{
    public Master myMaster;
    public Transform objectActionPos;

    public int actionNo;
    //1 sit chair
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMaster = GetComponentInParent<Master>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClickedObject()
    {
        // if (myMaster.theMovementController.MoveToPos.Count > 0)
        // {
        myMaster.theMovementController.MoveToPos.Clear();
        myMaster.theWaypointController.SetTarget(objectActionPos.position);
        // myMaster.theMovementController.MoveToPos.Add(objectActionPos.position);
        myMaster.theMovementController.ActionToPerform = actionNo;
        // }

    }
}
