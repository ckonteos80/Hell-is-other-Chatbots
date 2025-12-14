
using UnityEngine;

public class AiEventController : MonoBehaviour
{
    public int characterNo;
    public float nextEventTime;
    public int nextEventType;
    ///1 ask player a question

    Master myMaster;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMaster = GetComponentInParent<Master>();
    }

    // Update is called once per frame
    void Update()
    {

        if (nextEventType != 0)
        {
            if (myMaster.timeFromStart > nextEventTime && myMaster.theOverlayController.TextDisplays.Count == 0)
            {
                if (nextEventType == 1)
                {
                    // Check if player has shared any info
                    if (myMaster.theCharacterController.Characters[0].infoShared.Count > 0)
                    {
                        // Pick a random info from what player shared
                        int randomIndex = Random.Range(0, myMaster.theCharacterController.Characters[0].infoShared.Count);
                        string sharedInfo = myMaster.theCharacterController.Characters[0].infoShared[randomIndex];

                        // Ask question about that specific info
                        string questionContext = $"The player revealed this about themselves: {sharedInfo}. Ask them a follow-up question about this information.";
                        myMaster.theCharacterController.RequestCharacterQuestion(characterNo, questionContext);
                    }
                    else
                    {
                        // No info shared yet, ask generic question
                        myMaster.theCharacterController.RequestCharacterQuestion(characterNo, "Ask the player a question to get to know them better.");
                    }

                    nextEventTime = 0;
                    nextEventType = 0;
                    myMaster.SetNextEvent();
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
