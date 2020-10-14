using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class MapController : MonoBehaviour, IOnEventCallback
{
    // MapController should generate a seed, call Generator to
    // make a level and translate a seed to another player using Photon.

    // Codes of events
    private const int AttackCode = 23;
    private const int PlayerAnimationCode = 24;

    public List<GameObject> players = new List<GameObject>();

    private int seed;

    public int PlayerCount()
    {
        return players.Count;
    }
    
    public void AddPlayer(GameObject player)
    {
        players.Add(player);
    }

    private void Start()
    {
        seed = Random.Range(0, 9999);
    }


    private static Player GetPlayerById(int id) =>
        FindObjectsOfType<Player>().FirstOrDefault(player => player.GetId() == id);


    // method that should be called when someone is performing an attack

    // TODO: to RPC function calls
    
    
    public static void PerformAttack(int id)
    {
        var options = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
        var sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(AttackCode, id, options, sendOptions);
    }

    // TODO: Tuple.Create(id, animId)
    public static void PerformPlayerAnimation(int id)
    {

        var options = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
        var sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(PlayerAnimationCode, id, options, sendOptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case AttackCode:
                GetPlayerById((int) photonEvent.CustomData).Attack();
                break;

            case PlayerAnimationCode:
                GetPlayerById((int) photonEvent.CustomData).IsMoving =
                    !GetPlayerById((int) photonEvent.CustomData).IsMoving;
                break;
        }
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // public static object SerializeTupleIS(byte[] data)
    // {
    //     Tuple<int, string> result;
    //     
    //     result.Item1 = 
    //     
    //     return (object) result;
    // }
    //
    // public static byte[] DeserializeTupleIS(object data)
    // {
    //     
    // }
}