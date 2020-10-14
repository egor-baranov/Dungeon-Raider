using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject[] doors;

    public bool open;

    void Update()
    {
        if (open)
        {
            for(int i=0; i < doors.Length; i++)
            {
                if(doors[i]!= null)
                {
                    doors[i].GetComponent<DoorController>().openDoor();
                }
            }
        }
        else
        {
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null)
                {
                    doors[i].GetComponent<DoorController>().closeDoor();
                }
            }
        }
    }
}
