using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public int damage = 1;

    public GameObject owner;

    void Update() { }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy") || other.CompareTag("Player") || other.CompareTag("Level")) {
            if (owner == null) return;
            // GetComponent<SpriteRenderer>().enabled = false;

            if (other.GetComponent<Enemy>()) {
                other.GetComponent<Enemy>().ReceiveDamage(damage, GetComponent<Rigidbody2D>().velocity);
            }
            
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Animator>().Play("Die");
        }
        
        if (other.GetComponent<StaticObject>()) {
            other.GetComponent<StaticObject>().ReceiveDamage(damage, GetComponent<Rigidbody2D>().velocity);
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Animator>().Play("Die");
        }
    }

    public void Die() => PhotonNetwork.Destroy(gameObject);
}