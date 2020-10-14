using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameConnectionManager : MonoBehaviourPunCallbacks {
    public GameObject PlayerPrefab;

    private void Start() {
        var newPlayer =
            PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(25F, 25F, 0), Quaternion.identity);
        Camera.main.GetComponent<Transform>().position = newPlayer.transform.position;
    }

    void Update() { }

    public void Leave() {
        PhotonNetwork.LeaveRoom();
    }

    public void RestartLevel() {
        SceneManager.LoadScene("Main");
    }

    public override void OnLeftRoom() {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        Debug.LogFormat("Player {0} entered room", newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        Debug.LogFormat("Player {0} entered room", otherPlayer.NickName);
    }
}