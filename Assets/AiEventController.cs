using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class AiEventController : MonoBehaviour
{
    public float nextEventTime;
    public int nextEventType;

    Master myMaster;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMaster = GetComponentInParent<Master>();
    }

    // Update is called once per frame
    void Update()
    {

        if (nextEventTime != 0)
        {
            if (myMaster.timeFromStart > nextEventTime && myMaster.theOverlayController.TextDisplays.Count == 0)
            {
                if (nextEventType == 1)
                {
                    myMaster.theCharacterController.RequestNarratorDialogue("welcome player");
                    nextEventTime = 0;
                }



            }
        }
    }

    public void addEventTime(float addTime, int EventType)
    {
        nextEventTime += myMaster.timeFromStart + addTime;
        nextEventType = EventType;
    }
}
