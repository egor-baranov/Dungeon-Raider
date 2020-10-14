using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour {
    public GameObject bulletPrefab;

    public float bulletSpeed = 3;

    public float rechargeTime = 0.4F;

    public int bulletCount = 5;

    public float spreadAngle = 30F;

    public bool autoShoot = false;

    public float delayBetweenBullets = 0F;

    public int damage = 1;

    private PhotonView _photonView;

    public void Shoot() {
        UiManager.Singleton.UseBullets(bulletCount);
        StartCoroutine(ShootCoroutine());
    }

    public Transform GetOwner() {
        var owner = transform.parent;
        
        while (owner != null) {
            if (owner.GetComponent<Enemy>() || owner.GetComponent<Player>()) {
                return owner;
            }

            owner = owner.parent;
        }

        return null;
    }

    private IEnumerator ShootCoroutine() {
        if (GetComponent<Animator>()) {
            GetComponent<Animator>().Play("Shoot");
        }

        if (GetComponent<AudioSource>()) {
            var audioSource = GetComponent<AudioSource>();
            audioSource.pitch = Random.Range(0.9F, 1.1F);
            audioSource.Play();
        }

        if (!transform.parent.parent.parent.GetComponent<Player>().IsMine()) yield break;

        for (var i = 0; i < bulletCount; ++i) {
            var randZ = Random.Range(-spreadAngle, spreadAngle);

            var deviation = Quaternion.Euler(0, 0, randZ);

            var diff = transform.GetChild(0).transform.position - transform.position;

            // TODO: FIX
            var newBullet = PhotonNetwork.Instantiate(
                bulletPrefab.name,
                transform.GetChild(0).position,
                Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(diff.y, diff.x) + randZ));

            newBullet.GetComponent<Rigidbody2D>().velocity =
                deviation * diff.normalized * (bulletSpeed * 50);

            newBullet.GetComponent<Bullet>().damage = damage;

            newBullet.GetComponent<Bullet>().owner = transform.parent.parent.parent.gameObject;

            if (Math.Abs(delayBetweenBullets) > 0) yield return new WaitForSeconds(delayBetweenBullets);
        }
    }
}