using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Sprite openSprite;
    public Sprite closedSprite;

    public void closeDoor()
    {
        GetComponent<SpriteRenderer>().sprite = closedSprite;
    }
    public void openDoor()
    {
        GetComponent<SpriteRenderer>().sprite = openSprite;
    }
}
