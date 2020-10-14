using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class StaticObject : MonoBehaviour {
    public List<Sprite> states;

    private int _hp;
    public int maxHp;

    public GameObject particle;

    private void Awake() {
        _hp = maxHp;
    }

    public void ReceiveDamage(int amount) => ReceiveDamage(amount, Vector2.zero);

    public void ReceiveDamage(int amount, Vector2 direction) {
        if (_hp <= 0) return;

        _hp = Mathf.Max(0, _hp - amount);

        foreach (var i in Enumerable.Range(0, amount * 10)) {
            var force = Quaternion.AngleAxis(Random.Range(-80, 80), new Vector3(0, 0, 1)) *
                        direction.normalized * Random.Range(0.3F, 5F * Mathf.Sqrt(amount));
            var position =
                transform.position + new Vector3(Random.Range(-0.3F, 0.3F), Random.Range(-0.3F, 0.3F), 0);

            var newParticle = Instantiate(particle, position, quaternion.identity);
            newParticle.GetComponent<Rigidbody2D>().AddForce(force * Mathf.Sqrt(amount), ForceMode2D.Impulse);
            newParticle.GetComponent<SpriteRenderer>().color = new Color(92F / 255, 80F / 255, 73F / 255, 1);
            // if (Random.Range(0, 5) != 0) Destroy(newParticle, Random.Range(10F, 40F));
        }

        if (_hp <= 0) {
            // DropGold(10, direction);
            Die();
            return;
        }

        GetComponent<SpriteRenderer>().sprite = states[(int) ((float) _hp / maxHp * states.Count)];
        // GetComponent<PhotonView>().RPC("ReceiveDamageRpc", RpcTarget.All, amount, direction);
    }

    [PunRPC]
    private void ReceiveDamageRpc(int amount, Vector2 direction) { }

    private void Die() {
        GetComponent<BoxCollider2D>().enabled = false;
        // Destroy(gameObject);
    }
}