using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject _player;

    void Start()
    {
        _player = GameObject.Find("Player");
    }

    void Update()
    {
        if (_player == null) _player = GameObject.Find("Player");
        
        if (_player == null) return;
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position =
            Vector3.Lerp(
                transform.position,
                ((Vector2) _player.transform.position * 2F + mousePosition) / 3F,
                0.06F) - new Vector3(0, 0, 10);
    }
}