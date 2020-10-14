using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helmet : Item {
    [SerializeField] public int defense = 1;
    [SerializeField] public int weight = 1;
    [SerializeField] public int durability = 1;

    public List<Sprite> helmetSprites;

    private void Awake() {
        GetComponent<SpriteRenderer>().sprite =
            Random.Range(0, 5) == 0 ? null : helmetSprites[Random.Range(0, helmetSprites.Count)];
    }

    // blocks part of damage using durability 
    public int BlockDamage(int amount) {
        if (durability == 0) {
            return amount;
        }

        var blockedDamage = Mathf.Min(amount - 1, Mathf.Min(defense, durability));
        durability -= blockedDamage;
        return amount - blockedDamage;
    }
}