using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {
    private GameObject _player;

    private void Update() {
        foreach (var p in FindObjectOfType<MapController>().players) {
            if (_player == null) {
                _player = p;
                continue;
            }

            if (Vector2.Distance(_player.transform.position, transform.position) >
                Vector2.Distance(p.transform.position, transform.position)) {
                _player = p;
            }
        }

        if (_player != null) {
            var dist = Vector2.Distance(_player.transform.position, transform.position);
            if (dist <= 1) {
                GetComponent<Rigidbody2D>().velocity = (_player.transform.position - transform.position);
            }
        }
        else {
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }
}