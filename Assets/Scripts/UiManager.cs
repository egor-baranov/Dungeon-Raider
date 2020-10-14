using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UiManager : MonoBehaviour {
    [SerializeField] private int displayedBullets = 24;
    private int _neededBullets = 24;

    [SerializeField] private int displayedRedAlpha = 0;
    private int _neededRedAlpha = 0;

    public Text bulletCounter;
    public GameObject redPanel;

    public float displayedHp = 10;
    public int maxHp = 10;

    public Sprite hpBar25, hpBar50, hpBar75, hpBar100;

    public GameObject hpMask, hpContainer, youDied;

    public static UiManager Singleton;

    private void Awake() {
        Singleton = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            // SetDisplayedBullets(Mathf.Max(displayedBullets - 5, 0));
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            SetDisplayedBullets(displayedBullets + 20);
        }
    }


    public void UpdateHp(int now) {
        MakeRedEffect();

        if (now <= maxHp * 0.25F) {
            hpContainer.GetComponent<Image>().sprite = hpBar25;
        }
        else if (now <= maxHp * 0.5F) {
            hpContainer.GetComponent<Image>().sprite = hpBar50;
        }
        else if (now <= maxHp * 0.75F) {
            hpContainer.GetComponent<Image>().sprite = hpBar75;
        }
        else {
            hpContainer.GetComponent<Image>().sprite = hpBar100;
        }

        // var pos = hpMask.GetComponent<RectTransform>().position;

        // -156 -228.6 72.6
        // hpMask.GetComponent<RectTransform>().position = new Vector2(pos.x, -228.6F + 72.6F * now / maxHp);

        // Debug.Log($"{now}, {-228.6F + 72.6F * now / maxHp}");

        // var size = hpMask.GetComponent<RectTransform>().sizeDelta;
        // hpMask.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, 0.3470959F * now / maxHp);


        // Debug.Log($"new PosY = {-0.173F + (0.1736F + 0.173F) * now / maxHp}");
        StartCoroutine(HPChangeCoroutine(now));

        if (now == 0) {
            youDied.SetActive(true);
        }
    }

    private IEnumerator HPChangeCoroutine(int now) {
        var pos = hpMask.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;

        while (Math.Abs(displayedHp - now) > 0.0001F) {
            yield return new WaitForSeconds(0.03F);
            displayedHp += displayedHp < now ? 0.1F : -0.1F;

            hpMask.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition =
                new Vector2(pos.x, -0.173F + (0.1736F + 0.173F) * displayedHp / maxHp);
        }
    }

    public void MakeRedEffect() {
        StartCoroutine(RedEffectCoroutine());
    }

    private IEnumerator RedEffectCoroutine() {
        _neededRedAlpha = Mathf.Max(_neededRedAlpha, 20);
        while (displayedRedAlpha != _neededRedAlpha) {
            yield return new WaitForSeconds(0.02F);
            displayedRedAlpha += _neededRedAlpha > displayedRedAlpha ? 1 : -1;
            // Debug.Log($"displayedRedAlpha = {displayedRedAlpha}");
            redPanel.GetComponent<Image>().color = new Color(1, 0, 0, displayedRedAlpha / 255F);
        }

        _neededRedAlpha = 0;
        while (displayedRedAlpha != _neededRedAlpha) {
            yield return new WaitForSeconds(0.02F);
            displayedRedAlpha += _neededRedAlpha > displayedRedAlpha ? 1 : -1;
            // Debug.Log($"displayedRedAlpha = {displayedRedAlpha}");
            redPanel.GetComponent<Image>().color = new Color(1, 0, 0, displayedRedAlpha / 255F);
        }
    }

    public void UseBullets(int count) {
        _neededBullets = Mathf.Max(_neededBullets - count, 0);
        StartCoroutine(BulletCoroutine());
    }

    public void SetDisplayedBullets(int count) {
        _neededBullets = count;
        StartCoroutine(BulletCoroutine());
    }

    private IEnumerator BulletCoroutine() {
        while (displayedBullets != _neededBullets) {
            yield return new WaitForSeconds(0.02F);
            displayedBullets += _neededBullets > displayedBullets ? 1 : -1;
            bulletCounter.text = displayedBullets.ToString();
        }
    }
}