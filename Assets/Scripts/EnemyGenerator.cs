using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject enemyPrefab;

    private GameObject _currentEnemy;

    void Start()
    {
        Spawn();
    }

    public void Spawn(int count = 1)
    {
        StartCoroutine(SpawnCoroutine(count));
    }

    private IEnumerator SpawnCoroutine(int count = 1)
    {
        for (var i = 0; i < count; ++i)
        {
            _currentEnemy = PhotonNetwork.Instantiate(enemyPrefab.name, transform.position, Quaternion.identity);
            _currentEnemy.transform.parent = transform;
            
            yield return new WaitForSeconds(3);
        }
    }
}