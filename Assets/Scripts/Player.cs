using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour, IPunObservable {
    public int GetId() {
        return _photonView.Owner.ActorNumber;
    }

    public GameObject dashShadowPrefab;

    public float movementSpeed = 1;
    public float dashSpeed = 3;

    private Rigidbody2D _rigidbody2D;

    public GameObject hand;

    public float attackCooldown = 0;
    public float dashCooldown = 0;

    public bool isAttacking = false;

    private bool _faceRight = true;

    private float _invulnerableTimer = 0;

    public bool IsInvulnerable => _invulnerableTimer > 0;

    public int HP = 10, coins = 0;

    private Vector3 _correctPlayerPosition = Vector3.zero;
    private Quaternion _correctPlayerRotation = Quaternion.identity;

    private Vector3 _correctHandPosition = Vector3.zero;
    private Quaternion _correctHandRotation = Quaternion.identity;

    private Sprite _weaponSprite;

    public bool isDashing = false;

    public GameObject stepSound;

    public bool IsMine() {
        return _photonView.IsMine;
    }

    // Wrapper property over isMoving Body animation parameter
    public bool IsMoving {
        get => transform.GetChild(0).GetComponent<Animator>().GetBool(Moving);
        set {
            if (value == IsMoving) return;
            transform.GetChild(0).GetComponent<Animator>().SetBool(Moving, value);
            MapController.PerformPlayerAnimation(GetId());
            if (value) {
                StartCoroutine(StepCoroutine());
            }
        }
    }

    private bool FaceRight {
        get => _faceRight;
        set {
            _faceRight = value;
            transform.rotation = new Quaternion(0, _faceRight ? 0 : 180, 0, 1);
        }
    }


    private PhotonView _photonView;
    private static readonly int Moving = Animator.StringToHash("isMoving");

    private void Start() {
        _photonView = GetComponent<PhotonView>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        transform.name = "Player";

        FindObjectOfType<MapController>().AddPlayer(gameObject);
    }

    bool xor(bool a, bool b) {
        return (a || b) && !(a && b);
    }

    private void Update() {
        // any player

        attackCooldown = Mathf.Max(attackCooldown - 0.02F, 0);
        dashCooldown = Mathf.Max(dashCooldown - 0.02F, 0);

        if (_invulnerableTimer >= 0 && _invulnerableTimer <= 0.02F) {
            transform.GetChild(0).GetComponent<Animator>().Play("Idle", 1);
        }

        _invulnerableTimer = Mathf.Max(_invulnerableTimer - 0.02F, 0);

        // another player
        if (!_photonView.IsMine) {
            transform.position = Vector3.Lerp(transform.position, _correctPlayerPosition, Time.deltaTime * 4);
            transform.rotation = _correctPlayerRotation;

            var handTransform = transform.GetChild(1);
            handTransform.position =
                Vector3.Lerp(handTransform.position, _correctHandPosition, Time.deltaTime * 0.1F);
            handTransform.rotation =
                Quaternion.Lerp(handTransform.rotation, _correctHandRotation, Time.deltaTime * 16);

            return;
        }


        // your player


        // Dashing 

        if (Input.GetKeyDown(KeyCode.Space)) Dash();

        if (isDashing) return;

        // Player's look and feel

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        FaceRight = mousePosition.x > transform.position.x;

        var diff = mousePosition - (Vector2) transform.position;
        if (!isAttacking) {
            var rotation = hand.transform.rotation;
            rotation =
                Quaternion.Lerp(
                    rotation,
                    Quaternion.Euler(
                        0,
                        FaceRight ? 0 : 180,
                        (FaceRight ? 1 : -1) * Mathf.Rad2Deg * Mathf.Atan2(diff.y, diff.x) + (FaceRight ? -90 : 90)),
                    // you can ask why 0.0668 but idk
                    0.0668F);
            hand.transform.rotation = rotation;
        }


        // Attacking 

        var gun = hand.transform.GetChild(0).GetChild(0).GetComponent<Gun>();

        if (Input.GetKeyDown(KeyCode.Mouse0) && !gun.autoShoot || Input.GetKey(KeyCode.Mouse0) && gun.autoShoot) {
            Attack();
        }

        // Movement

        float xMultiplier = 0, yMultiplier = 0;

        if (xor(Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D))) {
            xMultiplier = movementSpeed * (Input.GetKey(KeyCode.D) ? 1 : -1);
        }

        if (xor(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S))) {
            yMultiplier = movementSpeed * (Input.GetKey(KeyCode.S) ? -1 : 1);
        }

        // transform.Translate(Time.deltaTime * xMultiplier, Time.deltaTime * yMultiplier, 0);
        // _rigidbody2D.velocity = new Vector2(Mathf.Lerp(0, xMultiplier * speed, 0.8F),
        //     Mathf.Lerp(0, yMultiplier * speed, 0.8F));

        if (Mathf.Abs(xMultiplier * yMultiplier) >= 0.5F) {
            xMultiplier *= Mathf.Sqrt(2) / 2;
            yMultiplier *= Mathf.Sqrt(2) / 2;
        }

        // _rigidbody2D.velocity = new Vector2(Mathf.Lerp(0, xMultiplier * movementSpeed, 0.8F),
        //     Mathf.Lerp(0, yMultiplier * movementSpeed, 0.8F));

        _rigidbody2D.velocity = new Vector2(xMultiplier * movementSpeed, yMultiplier * movementSpeed);

        // _rigidbody2D.velocity = new Vector2(xMultiplier * speed, yMultiplier * speed);

        IsMoving = Mathf.Abs(xMultiplier) > Mathf.Pow(10, -8) || Mathf.Abs(yMultiplier) > Mathf.Pow(10, -8);
    }

    public void Attack() {
        if (isAttacking || attackCooldown > 0) return;

        isAttacking = true;
        hand.transform.GetChild(0).GetChild(0).GetComponent<Gun>().Shoot();
        StartCoroutine(AttackCoroutine());

        MapController.PerformAttack(GetId());
    }

    private IEnumerator AttackCoroutine() {
        hand.GetComponent<Animator>().Play("Shoot");
        yield return new WaitForSeconds(0.40F);
        isAttacking = false;
        attackCooldown = hand.transform.GetChild(0).GetChild(0).GetComponent<Gun>().rechargeTime;
    }

    private void Dash() {
        // GetComponent<Animator>().Play("Dash");
        if (isDashing || dashCooldown > 0 || GetComponent<Rigidbody2D>().velocity.magnitude < 0.001F) return;
        IsMoving = false;
        isDashing = true;
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine() {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        _rigidbody2D.velocity = (GetComponent<Rigidbody2D>().velocity).normalized * dashSpeed;

        GetComponent<PhotonView>().RPC("ProcessDashEffect", RpcTarget.All);
        yield return new WaitForSeconds(0.18F);

        _rigidbody2D.velocity = Vector2.zero;
        isDashing = false;
        dashCooldown = 2;
    }

    [PunRPC]
    public void ProcessDashEffect() {
        StartCoroutine(DashEffectCoroutine());
    }

    private IEnumerator DashEffectCoroutine() {
        var count = 4;
        for (var i = 0; i < count; ++i) {
            var ds = PhotonNetwork.Instantiate(dashShadowPrefab.name, transform.position, transform.rotation);
            ds.GetComponent<SpriteRenderer>().sprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            yield return new WaitForSeconds(0.18F / count);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Coin")) {
            var coinPickup = other.gameObject.GetComponent<AudioSource>();
            coinPickup.pitch = Random.Range(0.8F, 1.2F);
            coinPickup.Play();
            coins += 1;
            other.GetComponent<SpriteRenderer>().enabled = false;
            Destroy(other.gameObject, 0.4F);
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Weapon")) {
            var enemy = other.transform.parent.parent.parent.GetComponent<Enemy>();
            var player = other.transform.parent.parent.parent.GetComponent<Player>();
            if (enemy) {
                if (enemy.isAttacking) ReceiveDamage(1);
            }
            else if (player) {
                if (player.isAttacking) ReceiveDamage(1);
            }
        }
    }

    public void ReceiveDamage(int amount) {
        // Invulnerable when dashing 
        if (isDashing || IsInvulnerable) {
            return;
        }

        HP = Mathf.Max(0, HP - amount);

        UiManager.Singleton.UpdateHp(HP);

        if (HP == 0) {
            Die();
        }
        else {
            _invulnerableTimer = 3F;
            transform.GetChild(0).GetComponent<Animator>().Play("Invulnerable", 1);
        }
    }

    public void Die() {
        Debug.Log("Player1 Died");
        // GetComponent<Animator>().Play("Death"); 
        Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(FindObjectOfType<Player>().transform.position);

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            stream.SendNext(transform.GetChild(1).position);
            stream.SendNext(transform.GetChild(1).rotation);
        }
        else {
            // _myCorrectPosition = (Vector3) stream.ReceiveNext();

            _correctPlayerPosition = (Vector3) stream.ReceiveNext();
            _correctPlayerRotation = (Quaternion) stream.ReceiveNext();

            _correctHandPosition = (Vector3) stream.ReceiveNext();
            _correctHandRotation = (Quaternion) stream.ReceiveNext();
        }
    }


    private bool _isStepCoroutinePlaying = false;

    private IEnumerator StepCoroutine() {
        if (_isStepCoroutinePlaying) yield break;
        _isStepCoroutinePlaying = true;
        while (transform.GetChild(0).GetComponent<Animator>().GetBool(Moving)) {
            yield return new WaitForSeconds(7F / 60);
            if (!IsMoving) break;
            Step();
            yield return new WaitForSeconds(13F / 60);
            if (!IsMoving) break;
            Step();
            yield return new WaitForSeconds(17F / 60);
            if (!IsMoving) break;
            Step();
            yield return new WaitForSeconds(13F / 60);
            if (!IsMoving) break;
            Step();
            yield return new WaitForSeconds(1F / 6);
        }

        _isStepCoroutinePlaying = false;
    }

    private void Step() {
        return;
        var sound = stepSound.GetComponent<AudioSource>();
        sound.pitch = Random.Range(1F, 1.2F);
        sound.PlayOneShot(sound.clip);
    }
}

enum AttackType {
    Sword,
    Gun,
    Bomb,
    Dash
}