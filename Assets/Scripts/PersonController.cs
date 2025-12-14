using System.Collections.Generic;
using UnityEngine;

public class PersonController : MonoBehaviour
{
    public string myName;
    public string myCharacter;
    public Transform dialogueHolder;

    public AiEventController myAiEventController;



    // public string myCharacter;

    public List<string> infoShared;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myAiEventController = GetComponent<AiEventController>();
    }

    // Update is called once per frame
    void Update()
    {

    }


}
