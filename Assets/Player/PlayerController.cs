using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine.Events;
using TMPro;
using Unity.Collections;

public class PlayerController : NetworkBehaviour
{
    //コンポーネント
    [SerializeField] private Rigidbody rig;
    public Outline outline;
    public ClientNetworkAnimator animator;
    public ClientNetworkAudioSource audioSource;

    //アウトレット接続用
    [SerializeField] private GameObject camBase;
    public Camera cam;
    [SerializeField] public GameObject trionPrefab;
    public GameObject rightHand;
    public GameObject leftHand;
    [SerializeField] private GameObject glassHopperPrefab;

    //FPS時に自分を非表示
    [SerializeField] private GameObject model;


    //防御
    [SerializeField] private GameObject barrierGameObject;
    [SerializeField] private GameObject shieldGameObject;
    [SerializeField] private GameObject escudoPrefab;
    public GameObject previewEscudoPrefab;

    //変数(固定)
    private float sprintSpeed = 7.5f;
    private float normalSpeed = 5f;
    private float airSpeed = 10f;
    private float maxSpeed = 15f;

    private float maxJumpPower = 10f;
    
    private float mouseSensitive = 0.5f;
    private float trionHealPower = 5f;


    //変数(変わる数値)
    private float jumpLongPress = 0f;
    private float unplayableTime = 0f;
    private float playerSpeed = 5f;
    private float previousHitPoint = 100f;
    private bool isFPS = false;
    public bool isGround = false;

    //フックショット用
    public NetworkVariable<bool> rightHookShot = new(false);
    public NetworkVariable<bool> leftHookShot = new NetworkVariable<bool>(true);
    [SerializeField] private LineRenderer rightLineRenderer;
    [SerializeField] private LineRenderer leftLineRenderer;
    public Vector3 rightHookShotPos;
    public Vector3 leftHookShotPos;

    //武器の数値
    private int rightWeaponNum = 2;
    private int leftWeaponNum = 2;
    public string[] rightTriggerName = new string[3];
    public string[] leftTriggerName = new string[3];


    //右手左手に溜めているトリオンのリスト
    public List<GameObject> rightChargedTrion = new();
    public List<GameObject> leftChargedTrion = new();

    //プレイヤーステータス
    public float trionPoint = 100f;
    public NetworkVariable<float> hitPoint = new(100f);
    public int trionPower = 0;

    //チーム用
    public NetworkVariable<BattleManager.Team> team = new(BattleManager.Team.None);
    public NetworkVariable<FixedString32Bytes> playerName = new();
    [SerializeField] private TMP_Text playerTag;

    public enum Place
    {
        Right,
        Left,
        Both
    }

    //段差乗り越え用
    [SerializeField] private GameObject legObject;
    private static float stepHeight = 0.3f;
    private static float stepDistance = 0.1f;
    private static float stepHalfWidth = 0.4f;


    // Start is called before the first frame update
    void Start()
    {
        playerName.OnValueChanged += ChangePlayerNameCallBack;

        if (IsOwner)
        {
            camBase.SetActive(true);
            StageManager.Singleton.Respawn(gameObject, team.Value);

            //プレイヤーの名前を設定
            string playerName = TitleManager.instance.playerName;
            if (playerName.Length > 10) playerName = playerName.Substring(0, 10);
            ChangePlayerNameServerRpc(playerName);
        }

        if (IsServer)
        {
            //チームなしに設定
            team.Value = BattleManager.Team.None;
        }
    }
    private void OnDestroy()
    {
        playerName.OnValueChanged -= ChangePlayerNameCallBack;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(playerName.Value != "" && !IsOwner)
        {
            playerTag.text = playerName.Value.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //アウトライン変更
        switch(team.Value)
        {
            case BattleManager.Team.Blue:
                outline.OutlineColor = Color.blue;
                break;

            case BattleManager.Team.Red:
                outline.OutlineColor = Color.red;
                break;

            default:
                outline.OutlineColor = Color.white;
                break;
        }

        //全員、フックショットを表示
        if (rightHookShot.Value)
        {
            rightLineRenderer.enabled = true;
            rightLineRenderer.SetPosition(0, rightHand.transform.position);
            rightLineRenderer.SetPosition(1, rightHookShotPos);
        } 
        else
        {
            rightLineRenderer.enabled = false;
        }
        if (leftHookShot.Value)
        {
            leftLineRenderer.enabled = true;
            leftLineRenderer.SetPosition(0, leftHand.transform.position);
            leftLineRenderer.SetPosition(1, leftHookShotPos);
        }
        else
        {
            leftLineRenderer.enabled = false;
        }

        //地面判定
        isGround = Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, Quaternion.identity, 0.8f);

        //自分のみ操作
        if (!IsOwner)
        {
            return;
        }

        //良く使う変数は予め取得しておく
        float deltaTime = Time.deltaTime;

        //FPS、TPSモード切り替え
        if(Input.GetButtonDown("Camera"))
        {
            isFPS = !isFPS;
            model.SetActive(!model.activeSelf);
        }

        //HP.を反映
        UIManager.instance.ChangeHP(hitPoint.Value);

        //トリオン回復
        trionPoint += trionHealPower * deltaTime;
        if (trionPoint > 100f) trionPoint = 100f;
        //トリオンを表示
        UIManager.instance.ChangeTrionPointGUI(trionPoint);

        //ボタンチェック
        RaycastHit buttonHit;
        if(Physics.Raycast(cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0.1f)), out buttonHit, isFPS ? 2f : 10f))
        {
            if (buttonHit.collider.CompareTag("Button"))
            {
                if(Input.GetButtonDown("RightTrigger"))
                {
                    buttonHit.collider.GetComponent<EventHolder>().Invoke();
                }
                else
                {
                    buttonHit.collider.GetComponent<EventHolder>().OutlineOn();
                }
            }
        }

        //マウス操作
        transform.Rotate(0f, Input.GetAxis("Mouse X") * mouseSensitive, 0f);
        float yMove = Input.GetAxis("Mouse Y") * -mouseSensitive;
        float currentYAngle = camBase.transform.eulerAngles.x;
        if (currentYAngle > 180) currentYAngle -= 360;
        //指定角より小さい場合は動かす
        if((currentYAngle < 80 && yMove > 0) || (currentYAngle > -80 && yMove < 0))
        {
            camBase.transform.Rotate(yMove, 0f, 0f);
        }

        //カメラ位置計算
        if(isFPS)
        {
            cam.transform.position = camBase.transform.position + new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            Vector3 camPos = camBase.transform.TransformDirection(new Vector3(1, 1, -5));
            RaycastHit[] hits = Physics.RaycastAll(camBase.transform.position, camPos, 5f);
            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (!hit.collider.isTrigger)
                    {
                        cam.transform.position = camBase.transform.position + camPos.normalized * hit.distance * 0.9f;
                    }
                }
            }
            else
            {
                cam.transform.position = camBase.transform.position + camPos.normalized * 5f;
            }
        }     

        //トリオン使用量調整
        if(Input.mouseScrollDelta.y > 0f)
        {
            trionPower = trionPower < 10 ? trionPower + 1 : 10;
            UIManager.instance.ChangeTrionPowerGUI(trionPower);
        } else if(Input.mouseScrollDelta.y < 0f)
        {
            trionPower = trionPower > 0 ? trionPower - 1 : 0;
            UIManager.instance.ChangeTrionPowerGUI(trionPower);
        }
        
        //使用する武器を変更（右手）
        if(Input.GetButtonDown("RightChange")) {
            NextRightWeapon();
        }

        //使用する防具を変更（左手）
        if(Input.GetButtonDown("LeftChange"))
        {
            NextLeftWeapon();
        }

        //キー操作できない時間は操作させない
        if (unplayableTime > 0f)
        {
            unplayableTime -= deltaTime;
        } else
        {
            //地面に着いているか判定
            if (isGround)
            {
                //ダッシュ
                if (Input.GetButtonDown("Sprint"))
                {
                    playerSpeed = playerSpeed == sprintSpeed ? normalSpeed : sprintSpeed;
                }

                //移動
                Vector3 tmpVelocity = GetMoveVector() * playerSpeed;
                if (tmpVelocity.magnitude == 0f) playerSpeed = normalSpeed;
                tmpVelocity.y = rig.velocity.y;
                rig.velocity = tmpVelocity;

                //前に段差があるかチェック
                StepClimb();

                //ジャンプ
                if (Input.GetButtonUp("Jump"))
                {
                    Jump(jumpLongPress);

                    animator.SetBool("IsFlying", true);
                }
                //スライディング
                else if (Input.GetButtonDown("Crouch")) {
                    animator.SetTrigger("Sliding");
                } 
                else
                {
                    //アニメーションを設定
                    animator.SetBool("IsFlying", false);
                    animator.SetFloat("Speed", rig.velocity.magnitude);
                }
            }
            else  //ジャンプ中は前後に少しだけ移動できる
            {
                //もし前に壁があるなら壁キック
                if(Input.GetButtonDown("Jump"))
                {
                    if (Physics.Raycast(transform.position, transform.forward, 0.5f))
                    {
                        rig.velocity = -transform.forward * playerSpeed;
                        animator.SetTrigger("WallJump");
                        Jump(maxJumpPower);
                    }
                }

                //ジャンプアニメーション
                animator.SetBool("IsFlying", true);

                //移動
                Vector3 tmpVelocity = GetMoveVector() * airSpeed * deltaTime;
                rig.velocity += tmpVelocity + Vector3.down * deltaTime * 10f;
            }
        }

        //ジャンプの長押し判定
        if (Input.GetButton("Jump")) jumpLongPress += deltaTime;
        else jumpLongPress = 0.3f;

        //早さを制限
        if(rig.velocity.magnitude > maxSpeed)
        {
            rig.velocity = rig.velocity.normalized * maxSpeed;
        }
    }

    //バリアー、シールドのオンオフ関数
    [ServerRpc(RequireOwnership = false)] public void ChangeLeftWeaponStatusServerRpc(float vec, int num, bool flag)
    {
        ChangeLeftWeaponStatusClientRpc(vec, num, flag);
    }
    [ClientRpc]
    public void ChangeLeftWeaponStatusClientRpc(float size, int num, bool flag)
    {
        switch (num)
        {
            case 0:
                shieldGameObject.SetActive(flag);
                shieldGameObject.transform.localScale = new Vector3(size, size, size);
                shieldGameObject.transform.localPosition = Quaternion.AngleAxis(camBase.transform.rotation.eulerAngles.x, Vector3.right) * Vector3.forward + new Vector3(0f, 0.5f, 0f);
                shieldGameObject.transform.LookAt(transform);
                shieldGameObject.transform.Rotate(90f, 0f, 0f);
                break;
                
            case 1:
                barrierGameObject.SetActive(flag);
                barrierGameObject.transform.localScale = new Vector3(size, size, size);
                break;
        }
    }

    //操作方向のベクトルの取得関数
    private Vector3 GetMoveVector()
    {
        Vector3 returnVec = Vector3.zero;
        if(Input.GetButton("Forward"))
        {
            returnVec += transform.forward;
        } else if(Input.GetButton("Back"))
        {
            returnVec -= transform.forward;
        }

        if(Input.GetButton("Right"))
        {
            returnVec += transform.right;
        }
        if (Input.GetButton("Left"))
        {
            returnVec -= transform.right;
        }
        return returnVec.normalized;
    }

    //ジャンプ用関数
    private void Jump(float power)
    {
        float jumpPower = power > 1f ? 1f : power;
        rig.AddForce(Vector3.up * jumpPower * maxJumpPower, ForceMode.VelocityChange);
        unplayableTime += 0.2f;
    }

    //クライアント側トリオン生成関数
    public void GenerateTrion(Vector3 pos, Quaternion rot, Vector3 scale, Place place)
    {
        GenerateTrionServerRpc(pos, rot, scale, place, NetworkManager.LocalClientId);
    }

    //トリオン生成依頼関数
    [ServerRpc(RequireOwnership =false)] private void GenerateTrionServerRpc(Vector3 pos, Quaternion rot, Vector3 scale, Place place, ulong id)
    {
        //初期設定
        GameObject tmpTrion = Instantiate(trionPrefab, pos, rot);
        tmpTrion.transform.localScale = scale;
        tmpTrion.GetComponent<TrionController>().place.Value = place;
        //権限を与えながら生成
        tmpTrion.GetComponent<NetworkObject>().SpawnWithOwnership(id);
    }

    //クライアント側トリオン状態変更変数
    public void ChangeTrionMode(Quaternion rot, Place place, TrionController.Mode mode, int trionPower = 5, GameObject targetObj = null)
    {
        List<GameObject> tmpList = place == Place.Right ? rightChargedTrion : leftChargedTrion;
        foreach (GameObject trion in tmpList)
        {
            //トリオンのコンポーネントを取得
            TrionController tmp = trion.GetComponent<TrionController>();
            if(tmp != null)
            {
                //トリオンの状態を変更
                tmp.ChangeMode(mode, trionPower);

                //ターゲットを設定
                tmp.targetObj = targetObj;
            }
            //トリオンの向きを変更
            trion.transform.rotation = GetLookingRotaion();
        }

        tmpList.Clear();
    }

    //グラスホッパー生成関数
    public void TryToGenerateGlassHopper( bool flag)
    {
        Quaternion rot = Quaternion.identity;
        Vector3 moveVec = GetMoveVector();

        if(moveVec.magnitude == 0f)
        {
            rot = flag ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(180f, 0f, 0f);
        } else
        {
            rot = flag ? Quaternion.LookRotation(moveVec) * Quaternion.Euler(70f, 0f, 0f) : Quaternion.LookRotation(moveVec) * Quaternion.Euler(110f, 0f, 0f);
        }

        //生成座標計算
        Vector3 pos = transform.position - new Vector3(0f, -0.3f, 0f);

        //自身の場所に生成
        Instantiate(glassHopperPrefab, pos, rot);

        //サーバー側に依頼
        TryToGenerateGlassHopperServerRpc(pos, rot, NetworkManager.Singleton.LocalClientId);
    }


    //サーバー側グラスホッパー生成関数
    [ServerRpc(RequireOwnership = false)] private void TryToGenerateGlassHopperServerRpc(Vector3 pos, Quaternion rot, ulong id)
    {
        TryToGenerateGlassHopperClientRpc(pos, rot, id);
    }
    //クライアント側グラスホッパー生成関数
    [ClientRpc] private void TryToGenerateGlassHopperClientRpc(Vector3 pos, Quaternion rot, ulong exceptId)
    {
        //自身は何もしない
        if (exceptId == NetworkManager.Singleton.LocalClientId) return;

        //指定の場所に生成
        Instantiate(glassHopperPrefab, pos, rot);
    }

    //サーバー側エスクード生成関数
    [ServerRpc(RequireOwnership = false)] public void GenerateEscudoServerRpc(Vector3 pos, Quaternion rot, float size, ulong id)
    {
        GameObject tmpEscudo = Instantiate(escudoPrefab, pos, rot);
        tmpEscudo.transform.localScale = new Vector3(size, 0f, size);
        tmpEscudo.GetComponent<NetworkObject>().SpawnWithOwnership(id);
    }

    //視点の先に居るモノを取得
    public RaycastHit[] GetRayHits()
    {
        return Physics.RaycastAll(cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0.1f)));
    }

    //プレイヤーの向きを取得
    public Quaternion GetLookingRotaion()
    {
        return Quaternion.Euler(cam.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y, 0f);
    }

    //右手の武器を変更
    private void NextRightWeapon()
    {
        if (rightTriggerName[rightWeaponNum] == "") return;

        //一つ前を消す
        Destroy(rightHand.GetComponent(Type.GetType(rightTriggerName[rightWeaponNum] + "Controller")));
        //一つ前にする
        rightWeaponNum = (rightWeaponNum + 1) % 3;

        //コントローラーを設定
        WeaponController weaponCon = (WeaponController)rightHand.AddComponent(Type.GetType(rightTriggerName[rightWeaponNum] + "Controller"));
        weaponCon.Initialize(this, Place.Right);

        WeaponUIManager.instance.ChangeWeapon(rightWeaponNum, Place.Right);
    }

    //左手の武器を変える
    private void NextLeftWeapon()
    {
        if (leftTriggerName[leftWeaponNum] == "") return;

        //一つ前を消す
        Destroy(leftHand.GetComponent(Type.GetType(leftTriggerName[leftWeaponNum] + "Controller")));
        //一つ前にする
        leftWeaponNum = (leftWeaponNum + 1) % 3;

        //コントローラーを設定
        WeaponController weaponCon = (WeaponController)leftHand.AddComponent(Type.GetType(leftTriggerName[leftWeaponNum] + "Controller"));
        weaponCon.Initialize(this, Place.Left);

        WeaponUIManager.instance.ChangeWeapon(leftWeaponNum, Place.Left);
    }

    [ServerRpc(RequireOwnership = false)] public void DamageServerRpc(float damagePoint, ulong id)
    {
        //同じチームの時は何もしない
        if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerController>().team.Value == team.Value) return;

        hitPoint.Value -= damagePoint;
        if(hitPoint.Value <= 0f)
        {
            BattleManager.Singleton.Kill(id, GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void StepClimb()
    {
        for(int i = 0; i < 3; i++)
        {
            float width = stepHalfWidth * (i - 1);
            Vector3 posVec = legObject.transform.TransformPoint(width, 0f, 0f);

            //前に壁があるかチェック
            if (Physics.Raycast(posVec, legObject.transform.forward, stepDistance))
            {
                //前の壁が登れるかチェック
                if (!Physics.Raycast(posVec + new Vector3(0f, stepHeight, 0f), legObject.transform.forward, stepDistance))
                {
                    //下にレイキャストを飛ばし高さを調べる
                    RaycastHit hit;
                    if (Physics.Raycast(legObject.transform.TransformPoint(width, stepHeight, stepDistance), -legObject.transform.up, out hit, stepHeight))
                    {
                        transform.position = new Vector3(transform.position.x, hit.point.y + 0.8f, transform.position.z);
                        break;
                    }
                }
            }

        }
    }

    [ClientRpc] public void RespawnClientRpc()
    {
        if (IsOwner) StageManager.Singleton.Respawn(gameObject, team.Value);
    }

    //フックショット反映関数
    [ServerRpc(RequireOwnership = false)] public void HookShotServerRpc(Place place, bool flag)
    {
        //右手
        if(place == Place.Right)
        {
            rightHookShot.Value = flag;
        } else
        {
            leftHookShot.Value = flag;
        }
    }

    private void ChangeOutlineColor()
    {

    }

    [ServerRpc(RequireOwnership = false)] public void ChangePlayerNameServerRpc(string name)
    {
        playerName.Value = name;
    }
    private void ChangePlayerNameCallBack(FixedString32Bytes pre, FixedString32Bytes next)
    {
        playerTag.text = next.ToString();
    }
} 
 