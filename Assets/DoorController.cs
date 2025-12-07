using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public SpriteRenderer roomSprite;
    public List<Sprite> doorSprites;

    public bool doorOpen = false;

    void Start()
    {
        roomSprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (doorOpen)
        {
            roomSprite.sprite = doorSprites[1];
        }
        else
        {
            roomSprite.sprite = doorSprites[0];
        }
    }
}
