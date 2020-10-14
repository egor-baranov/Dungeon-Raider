using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DashEffect : MonoBehaviour
{

    public void Die()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}