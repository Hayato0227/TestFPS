using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class PlayerController : NetworkBehaviour
{
    //コンポーネント
    [SerializeField] private Rigidbody rig;
    public Animator animator;

    //アウトレット接続用
    [SerializeField] private GameObject camBase;
    public Camera cam;
    [SerializeField] public GameObject trionPrefab;
    public GameObject rightHand;
    public GameObject leftHand;
    [SerializeField] private GameObject glassHopperPrefab;

    //防御
    [SerializeField] private GameObject barrierGameObject;
    [SerializeField] private GameObject shieldGameObject;
    [SerializeField] private GameObject escudoPrefab;
    public GameObject previewEscudoPrefab;

    //変数(固定)
    private float sprintSpeed = 7.5f;
    private float normalSpeed = 5f;
    private float airSpeed = 10f;
    private float maxSpeed = 50f;

    private float maxJumpPower = 10f;
    
    private float mouseSensitive = 0.5f;
    private float trionHealPower = 5f;

    private float glassHopperTrion = 10f;

    //変数(変わる数値)
    private float jumpLongPress = 0f;
    private float unplayableTime = 0f;
    private float playerSpeed = 10f;
    private float previousHitPoint = 100f;

    //武器の数値
    private int rightWeaponNum = 2;
    private int leftWeaponNum = 2;

    //右手左手に溜めているトリオンのリスト
    public List<GameObject> rightChargedTrion = new();
    public List<GameObject> leftChargedTrion = new();

    //プレイヤーステータス
    public float trionPoint = 100f;
    public NetworkVariable<float> hitPoint = new(100f);
    public int trionPower = 0;

    //チーム用
    public enum Team
    {
        Alpha,
        Beta,
        Gamma,
        Delta,
        None
    }
    public NetworkVariable<Team> team = new();

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
        if (IsOwner)
        {
            camBase.SetActive(true);
        }

        if (IsServer)
        {
            //チームなしに設定
            team.Value = Team.None;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //自分のみ操作
        if (!IsOwner) return;

        //良く使う変数は予め取得しておく
        float deltaTime = Time.deltaTime;

        //HPを反映
        UIManager.instance.ChangeHP(hitPoint.Value);
        if(previousHitPoint > 0f && hitPoint.Value <= 0f)
        {
            //死
            Dead();
        }
        previousHitPoint = hitPoint.Value;
        

        //トリオン回復
        trionPoint += trionHealPower * deltaTime;
        if (trionPoint > 100f) trionPoint = 100f;
        //トリオンを表示
        UIManager.instance.ChangeTrionPointGUI(trionPoint);

        if (Input.GetKeyDown(KeyCode.K)) Dead();

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
        Vector3 camPos = cam.transform.position - transform.position;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, camPos, 5f);
        if(hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.isTrigger)
                {
                    cam.transform.position = hit.point;
                }
            }
        } else
        {
            cam.transform.position = transform.position + camPos.normalized * 5f;
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
            if (Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, Quaternion.identity, 1.1f))
            {
                //ダッシュ
                if (Input.GetButton("Sprint"))
                {
                    playerSpeed = sprintSpeed;
                }
                else  //普通
                {
                    playerSpeed = normalSpeed;
                }

                //移動
                Vector3 tmpVelocity = GetMoveVector() * playerSpeed;
                tmpVelocity.y = rig.velocity.y;
                rig.velocity = tmpVelocity;

                //前に段差があるかチェック
                StepClimb();

                //ジャンプ
                if (Input.GetButtonUp("Jump"))
                {

                    Jump();
                    unplayableTime += 0.3f; //操作できない時間を追加

                    animator.SetBool("IsFlying", true);
                } else
                {
                    //アニメーションを設定
                    animator.SetBool("IsFlying", false);
                    animator.SetFloat("Speed", rig.velocity.magnitude);
                }
            }
            else  //ジャンプ中は前後に少しだけ移動できる
            {
                //ジャンプアニメーション
                animator.SetBool("IsFlying", true);

                Vector3 tmpVelocity = GetMoveVector() * airSpeed * deltaTime;
                rig.velocity += tmpVelocity;

                //グラスホッパーを起動
                if(Input.GetButtonDown("Jump"))
                {
                    if(UseTrion(glassHopperTrion))
                    {
                        Vector3 moveVec = GetMoveVector();
                        if (moveVec.magnitude == 0f)
                        {
                            //上向きに生成
                            TryToGenerateGlassHopper(Quaternion.Euler(0f, 0f, 0f));
                        }
                        else
                        {
                            //進む向き*70度の向きで生成
                            TryToGenerateGlassHopper(Quaternion.LookRotation(moveVec) * Quaternion.Euler(70f, 0f, 0f));
                        }
                    }
                }
                else if(Input.GetButtonDown("Crouch"))
                {
                    if(UseTrion(glassHopperTrion))
                    {
                        Vector3 moveVec = GetMoveVector();
                        if (moveVec.magnitude == 0f)
                        {
                            //下向きに生成
                            TryToGenerateGlassHopper(Quaternion.Euler(180f, 0f, 0f));
                        }
                        else
                        {
                            //進む向き*110度の向きで生成
                            TryToGenerateGlassHopper(Quaternion.LookRotation(moveVec) * Quaternion.Euler(110f, 0f, 0f));
                        }
                    }
                }
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
    private void Jump()
    {
        float jumpPower = jumpLongPress > 1f ? 1f : jumpLongPress;
        rig.AddForce(Vector3.up * jumpPower * maxJumpPower, ForceMode.VelocityChange);
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
    private void TryToGenerateGlassHopper(Quaternion rot)
    {
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
        rightWeaponNum = (rightWeaponNum + 1) % 3;

        switch(rightWeaponNum)
        {
            case 0:  //アステロイド
                Destroy(rightHand.GetComponent<HaundController>());
                rightHand.AddComponent<AsteroidController>().Initialize(this, Place.Right, Vector3.one * 0.1f);
                break;

            case 1:  //バイパー
                Destroy(rightHand.GetComponent<AsteroidController>());
                rightHand.AddComponent<ViperController>().Initialize(this, Place.Right, Vector3.one * 0.1f);
                break;

            case 2:  //ハウンド
                Destroy(rightHand.GetComponent<ViperController>());
                rightHand.AddComponent<HaundController>().Initialize(this, Place.Right, Vector3.one * 0.1f);
                break;
        }
        WeaponUIManager.instance.ChangeWeapon(rightWeaponNum, Place.Right);
    }

    //左手の武器を変える
    private void NextLeftWeapon()
    {
        leftWeaponNum = (leftWeaponNum + 1) % 3;

        switch(leftWeaponNum)
        {
            case 0: //シールド
                Destroy(leftHand.GetComponent<EscudoController>());
                leftHand.AddComponent<ShieldController>().Initialize(this, Place.Left, Vector3.one);
                break;

            case 1: //バリアー
                Destroy(leftHand.GetComponent<ShieldController>());
                leftHand.AddComponent<BarrierController>().Initialize(this, Place.Left, Vector3.one);
                break;

            case 2: //エスクード
                Destroy(leftHand.GetComponent<BarrierController>());
                leftHand.AddComponent<EscudoController>().Initialize(this, Place.Left, Vector3.one);
                break;
        }
        WeaponUIManager.instance.ChangeWeapon(leftWeaponNum, Place.Left);
    }

    private bool UseTrion(float useTrion)
    {
        if(trionPoint >= useTrion)
        {
            trionPoint -= useTrion;
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)] public void DamageServerRpc(float damagePoint)
    {
        hitPoint.Value -= damagePoint;
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

    //死ぬ関数
    public void Dead()
    {
        float deadTime = 5f;
        PlayerFadeOut();
        unplayableTime = deadTime;
        Invoke("PlayerFadeIn", deadTime);
        Invoke("Respawn", deadTime);
    }

    //消える関数
    private void PlayerFadeOut()
    {
        transform.Find("Rob/Model").GetComponent<Renderer>().material.DOFade(0f, 1f);
    }

    //出てくる関数
    private void PlayerFadeIn()
    {
        transform.Find("Rob/Model").GetComponent<Renderer>().material.DOFade(1f, 1f);
    }

    //リスポーン関数
    public void Respawn()
    {
        GameManager.instance.Respawn(this.gameObject, team.Value);
        ResetHPServerRpc();
    }

    //HP更新関数
    [ServerRpc(RequireOwnership = false)] private void ResetHPServerRpc()
    {
        hitPoint.Value = 100f;
    }
} 
 