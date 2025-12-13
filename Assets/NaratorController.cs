using UnityEngine;

public class NaratorController : MonoBehaviour
{

    Master myMaster;
    SpriteRenderer mySpriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMaster = GetComponentInParent<Master>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myMaster.theDoorController.doorOpen)
        {
            if (mySpriteRenderer.enabled == false)
            {

                mySpriteRenderer.enabled = true;
            }
        }
        else
        {
            if (mySpriteRenderer.enabled == true)
            {

                mySpriteRenderer.enabled = false;
            }
        }

    }
}
