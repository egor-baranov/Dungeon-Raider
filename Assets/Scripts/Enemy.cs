using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pathfinding;
using Photon.Pun;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Enemy : MonoBehaviour, IPunObservable {
    public GameObject dashShadowPrefab;

    public bool _faceRight = true;

    public bool isAlive = true;

    public bool isAttacking = false;
    public bool isDashing = false;

    public float dashSpeed = 200;

    public float attackCooldown = 3F;
    public float dashCooldown = 0;

    public float speed = 3F;
    public float requiredDistance = 1.5F;

    public Sprite hp75, hp50, hp25, dead1, dead2;

    public float maxSpeed = 3;
    public int maxHp = 10;
    public int hp;

    public GameObject hand;

    public GameObject particle;

    public GameObject coinPrefab;

    // TODO: use rechargeTime from used weapon
    public float rechargeTime = 2F;

    public Color bloodColor = new Color(152, 38, 55, 255);
    public Color fleshColor1 = new Color(104, 110, 72, 255);

    public GameObject stepSound;

    public GameObject body, helmet;

    public List<GameObject> inventory;

    // Wrapper property over isMoving Body animation parameter
    public bool IsMoving {
        get => transform.GetChild(0).GetComponent<Animator>().GetBool(Moving);
        set {
            if (value == IsMoving) return;
            transform.GetChild(0).GetComponent<Animator>().SetBool(Moving, value);
            // MapController.PerformPlayerAnimation(GetId());
            if (value) {
                // StartCoroutine(StepCoroutine());
            }
        }
    }

    public bool FaceRight {
        get => _faceRight;
        set {
            _faceRight = value;
            transform.rotation = new Quaternion(0, _faceRight ? 0 : 180, 0, 0);
        }
    }

    private Rigidbody2D _rigidbody2D;

    private GameObject _player;

    private PhotonView _photonView;
    private static readonly int Moving = Animator.StringToHash("isMoving");

    private void Awake() {
        body = transform.GetChild(0).gameObject;
        helmet = body.transform.GetChild(0).gameObject;

        if (Random.Range(0, 3) != 0) {
            helmet.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    private void Start() {
        hp = maxHp;

        _player = GameObject.Find("Player");
        if (_player != null) GetComponent<AIDestinationSetter>().target = _player.transform;
        _rigidbody2D = GetComponent<Rigidbody2D>();

        GetComponent<AIPath>().maxSpeed = maxSpeed * Random.Range(0.7F, 1F);
        GetComponent<AIPath>().pickNextWaypointDist = Random.Range(0.4F, 0.6F);

        dashCooldown = Random.Range(3F, 10F);
    }

    bool xor(bool a, bool b) {
        return (a || b) && !(a && b);
    }


    private void Update() {
        if (isDashing || !isAlive || hp <= 0) return;

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
            // TODO: replace with https://answers.unity.com/questions/575125/how-to-calculate-distance-based-on-a.html
            var distance = Vector2.Distance(_player.transform.position, transform.position);

            // TODO: fix errors
            if (distance >= 5F) {
                // GetComponent<AIDestinationSetter>().target = null;
                // IsMoving = false;
                // GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                // return;
            }

            IsMoving = true;
            GetComponent<AIDestinationSetter>().target = _player.transform;
        }
        else {
            GetComponent<AIDestinationSetter>().target = null;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            IsMoving = false;
            return;
        }

        // Look & Feel
        FaceRight = _player.transform.position.x > transform.position.x;

        Vector2 diff = _player.transform.position - transform.position;
        if (!isAttacking && !isDashing) {
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

        // Dashing

        dashCooldown = Mathf.Max(dashCooldown - 0.02F, 0);

        if (!isDashing && dashCooldown <= 0) {
            DashAttack();
        }

        if (isDashing) return;

        // Attacking 
        attackCooldown = Mathf.Max(attackCooldown - 0.02F, 0);

        // Movement

        if (Vector2.Distance(transform.position, _player.transform.position) <= requiredDistance) {
            if (!isAttacking && attackCooldown <= 0 && !isDashing) Attack();
        }

        float xMul = GetComponent<AIPath>().velocity.x, yMul = GetComponent<AIPath>().velocity.y;

        IsMoving = Mathf.Abs(xMul) > Mathf.Pow(10, -8) || Mathf.Abs(yMul) > Mathf.Pow(10, -8);
    }

    private void Attack() {
        if (isAttacking || attackCooldown != 0) return;

        isAttacking = true;

        // hand.transform.GetChild(0).GetChild(0).GetComponent<Gun>().Shoot();

        StartCoroutine(AttackCoroutine());
    }

    private void DashAttack() {
        return;
        if (isDashing || dashCooldown > 0) return;
        isDashing = true;
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine() {
        if (_player == null) {
            isDashing = false;
            yield break;
        }

        IsMoving = false;
        GetComponent<AIDestinationSetter>().enabled = false;
        GetComponent<AIPath>().enabled = false;
        _rigidbody2D.velocity = (_player.transform.position - transform.position).normalized * dashSpeed;


        var count = 8;
        for (var i = 0; i < count; ++i) {
            var ds = Instantiate(dashShadowPrefab, transform.position, transform.rotation);
            ds.GetComponent<SpriteRenderer>().sprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            Destroy(ds, 1);
            yield return new WaitForSeconds(0.36F / count);
        }

        GetComponent<AIDestinationSetter>().enabled = true;
        GetComponent<AIPath>().enabled = true;

        _rigidbody2D.velocity = Vector2.zero;
        isDashing = false;
        dashCooldown = Random.Range(3F, 10F);

        Debug.Log("Break Dash Coroutine");
    }

    private IEnumerator AttackCoroutine() {
        hand.GetComponent<Animator>().Play("AttackRight");
        yield return new WaitForSeconds(0.40F);
        isAttacking = false;
        attackCooldown = rechargeTime * Mathf.Sqrt((float) maxHp / hp) * Random.Range(0.7F, 1.2F);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        // isDashing = false;
        // Debug.Log("OnCollisionEnter2D");
    }

    public void ReceiveDamage(int amount) => ReceiveDamage(amount, Vector2.zero);

    public void ReceiveDamage(int amount, Vector2 direction) {
        GetComponent<PhotonView>().RPC("ReceiveDamageRpc", RpcTarget.All, amount, direction);
    }

    [PunRPC]
    private void ReceiveDamageRpc(int amount, Vector2 direction) {
        if (hp <= 0) return;

        amount = helmet.GetComponent<Helmet>().BlockDamage(amount);

        if (hp >= maxHp * 0.5F && hp - amount < maxHp * 0.5F) {
            var dropHelmet = Instantiate(helmet, transform.position, helmet.transform.rotation);
            dropHelmet.GetComponent<SpriteRenderer>().sprite = helmet.GetComponent<SpriteRenderer>().sprite;
            dropHelmet.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            dropHelmet.GetComponent<SpriteRenderer>().sortingOrder = 0;

            helmet.GetComponent<SpriteRenderer>().sprite = null;
        }

        hp = Mathf.Max(0, hp - amount);

        foreach (var i in Enumerable.Range(0, amount * 30)) {
            var force = Quaternion.AngleAxis(Random.Range(-40, 40), new Vector3(0, 0, 1)) *
                        direction.normalized * Random.Range(0.3F, 2.4F * Mathf.Sqrt(amount));
            var position =
                transform.position + new Vector3(Random.Range(-0.1F, 0.1F), Random.Range(-0.1F, 0.1F), 0);

            var newParticle = Instantiate(particle, position, quaternion.identity);
            newParticle.GetComponent<Rigidbody2D>().AddForce(force * Mathf.Sqrt(amount), ForceMode2D.Impulse);

            // if (Random.Range(0, 5) != 0) Destroy(newParticle, Random.Range(10F, 40F));
        }

        if (hp <= 0) {
            DropGold(10, direction);
            Die();
            return;
        }

        if (hp <= maxHp * 0.25F) {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hp25;
            GetComponent<AIPath>().maxSpeed = maxSpeed * 0.25F * Random.Range(0.7F, 1F);
        }
        else if (hp <= maxHp * 0.5F) {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hp50;
            GetComponent<AIPath>().maxSpeed = maxSpeed * 0.5F * Random.Range(0.7F, 1F);
        }
        else if (hp <= maxHp * 0.75F) {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hp75;
            GetComponent<AIPath>().maxSpeed = maxSpeed * 0.75F * Random.Range(0.7F, 1F);
        }

        StartCoroutine(Push(direction));
    }


    // function that drops gold when enemy dies
    public void DropGold(int amount, Vector2 direction) {
        foreach (var i in Enumerable.Range(0, amount)) {
            var force = Quaternion.AngleAxis(Random.Range(-180, 180), new Vector3(0, 0, 1)) *
                        direction.normalized;
            var position =
                transform.position + new Vector3(Random.Range(-0.1F, 0.1F), Random.Range(-0.1F, 0.1F), 0);

            var newParticle = Instantiate(coinPrefab, position, quaternion.identity);
            newParticle.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
    }

    private IEnumerator Push(Vector2 direction) {
        // todo: change to Gun parameter
        const float pushForce = 20F;

        GetComponent<AIPath>().enabled = false;
        GetComponent<Seeker>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        // GetComponent<Animator>().enabled = false;
        // transform.GetChild(0).GetComponent<Animator>().enabled = false;

        GetComponent<Rigidbody2D>().AddForce(direction.normalized * pushForce);
        yield return new WaitForSeconds(0.75F);

        if (!isAlive) {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            yield break;
        }

        GetComponent<AIPath>().enabled = true;
        GetComponent<Seeker>().enabled = true;
        GetComponent<Animator>().enabled = true;
        transform.GetChild(0).GetComponent<Animator>().enabled = true;
    }

    public IEnumerator DieWithDelay() {
        yield return new WaitForSeconds(0.5F);
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = dead2;
    }

    public void Die() {
        if (!isAlive) return;

        // transform.GetChild(0).GetComponent<Animator>().Play("Death");
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = dead1;
        // transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Background";
        // transform.GetChild(0).parent = null;

        GetComponent<AIDestinationSetter>().enabled = false;
        GetComponent<AIPath>().enabled = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
        GetComponent<Seeker>().enabled = false;
        GetComponent<Animator>().enabled = false;
        transform.GetChild(0).GetComponent<Animator>().enabled = false;
        transform.GetChild(0).GetComponent<ShadowCaster2D>().enabled = false;

        isAlive = false;
        Debug.Log("Enemy Died");
        // GetComponent<Animator>().Play("Death"); 

        // :^)
        transform.parent.GetComponent<EnemyGenerator>().Spawn(1);

        StartCoroutine(DieWithDelay());
        // PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        // TODO: implement
    }

    // public static void SetTextureImporterFormat(Texture2D texture, bool isReadable) {
    //     if (null == texture) return;

    //     var assetPath = AssetDatabase.GetAssetPath(texture);
    //     var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

    //     if (tImporter == null) return;

    //     tImporter.textureType = TextureImporterType.Default;

    //     tImporter.isReadable = isReadable;

    //     AssetDatabase.ImportAsset(assetPath);
    //     AssetDatabase.Refresh();
    // }

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
        var sound = stepSound.GetComponent<AudioSource>();
        sound.pitch = Random.Range(1F, 1.2F);
        sound.PlayOneShot(sound.clip);
    }
}