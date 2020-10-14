using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using Light2D = UnityEngine.Experimental.Rendering.Universal.Light2D;

public class Item : MonoBehaviour {
    public Rarity rarity = Rarity.Usual;

    public string itemName = "Unknown item";
    public string description = "An unknown item.";

    private void Awake() {
        if (!transform.GetChild(0)) return;
        
        var light2D = transform.GetChild(0).GetComponent<Light2D>();
        switch (rarity) {
            case Rarity.Usual:
                light2D.color = Color.white;
                break;
            case Rarity.Unusual:
                light2D.color = Color.green;
                break;
            case Rarity.Rare:
                light2D.color = Color.blue;
                break;
            case Rarity.VeryRare:
                light2D.color = new Color(1, 69F / 255, 0);
                break;
            case Rarity.Legendary:
                light2D.color = Color.magenta;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum Rarity {
    Usual, // white
    Unusual, // green
    Rare, // blue
    VeryRare, // orange
    Legendary // purple
}