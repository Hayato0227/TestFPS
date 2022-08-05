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
    //�R���|�[�l���g
    [SerializeField] private Rigidbody rig;
    public Outline outline;
    public ClientNetworkAnimator animator;
    public ClientNetworkAudioSource audioSource;

    //�A�E�g���b�g�ڑ��p
    [SerializeField] private GameObject camBase;
    public Camera cam;
    [SerializeField] public GameObject trionPrefab;
    public GameObject rightHand;
    public GameObject leftHand;
    [SerializeField] private GameObject glassHopperPrefab;

    //FPS���Ɏ������\��
    [SerializeField] private GameObject model;


    //�h��
    [SerializeField] private GameObject barrierGameObject;
    [SerializeField] private GameObject shieldGameObject;
    [SerializeField] private GameObject escudoPrefab;
    public GameObject previewEscudoPrefab;

    //�ϐ�(�Œ�)
    private float sprintSpeed = 7.5f;
    private float normalSpeed = 5f;
    private float airSpeed = 10f;
    private float maxSpeed = 15f;

    private float maxJumpPower = 10f;
    
    private float mouseSensitive = 0.5f;
    private float trionHealPower = 5f;


    //�ϐ�(�ς�鐔�l)
    private float jumpLongPress = 0f;
    private float unplayableTime = 0f;
    private float playerSpeed = 5f;
    private float previousHitPoint = 100f;
    private bool isFPS = false;
    public bool isGround = false;

    //�t�b�N�V���b�g�p
    public NetworkVariable<bool> rightHookShot = new(false);
    public NetworkVariable<bool> leftHookShot = new NetworkVariable<bool>(true);
    [SerializeField] private LineRenderer rightLineRenderer;
    [SerializeField] private LineRenderer leftLineRenderer;
    public Vector3 rightHookShotPos;
    public Vector3 leftHookShotPos;

    //����̐��l
    private int rightWeaponNum = 2;
    private int leftWeaponNum = 2;
    public string[] rightTriggerName = new string[3];
    public string[] leftTriggerName = new string[3];


    //�E�荶��ɗ��߂Ă���g���I���̃��X�g
    public List<GameObject> rightChargedTrion = new();
    public List<GameObject> leftChargedTrion = new();

    //�v���C���[�X�e�[�^�X
    public float trionPoint = 100f;
    public NetworkVariable<float> hitPoint = new(100f);
    public int trionPower = 0;

    //�`�[���p
    public NetworkVariable<BattleManager.Team> team = new(BattleManager.Team.None);
    public NetworkVariable<FixedString32Bytes> playerName = new();
    [SerializeField] private TMP_Text playerTag;

    public enum Place
    {
        Right,
        Left,
        Both
    }

    //�i�����z���p
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

            //�v���C���[�̖��O��ݒ�
            string playerName = TitleManager.instance.playerName;
            if (playerName.Length > 10) playerName = playerName.Substring(0, 10);
            ChangePlayerNameServerRpc(playerName);
        }

        if (IsServer)
        {
            //�`�[���Ȃ��ɐݒ�
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
        //�A�E�g���C���ύX
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

        //�S���A�t�b�N�V���b�g��\��
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

        //�n�ʔ���
        isGround = Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, Quaternion.identity, 0.8f);

        //�����̂ݑ���
        if (!IsOwner)
        {
            return;
        }

        //�ǂ��g���ϐ��͗\�ߎ擾���Ă���
        float deltaTime = Time.deltaTime;

        //FPS�ATPS���[�h�؂�ւ�
        if(Input.GetButtonDown("Camera"))
        {
            isFPS = !isFPS;
            model.SetActive(!model.activeSelf);
        }

        //HP.�𔽉f
        UIManager.instance.ChangeHP(hitPoint.Value);

        //�g���I����
        trionPoint += trionHealPower * deltaTime;
        if (trionPoint > 100f) trionPoint = 100f;
        //�g���I����\��
        UIManager.instance.ChangeTrionPointGUI(trionPoint);

        //�{�^���`�F�b�N
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

        //�}�E�X����
        transform.Rotate(0f, Input.GetAxis("Mouse X") * mouseSensitive, 0f);
        float yMove = Input.GetAxis("Mouse Y") * -mouseSensitive;
        float currentYAngle = camBase.transform.eulerAngles.x;
        if (currentYAngle > 180) currentYAngle -= 360;
        //�w��p��菬�����ꍇ�͓�����
        if((currentYAngle < 80 && yMove > 0) || (currentYAngle > -80 && yMove < 0))
        {
            camBase.transform.Rotate(yMove, 0f, 0f);
        }

        //�J�����ʒu�v�Z
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

        //�g���I���g�p�ʒ���
        if(Input.mouseScrollDelta.y > 0f)
        {
            trionPower = trionPower < 10 ? trionPower + 1 : 10;
            UIManager.instance.ChangeTrionPowerGUI(trionPower);
        } else if(Input.mouseScrollDelta.y < 0f)
        {
            trionPower = trionPower > 0 ? trionPower - 1 : 0;
            UIManager.instance.ChangeTrionPowerGUI(trionPower);
        }
        
        //�g�p���镐���ύX�i�E��j
        if(Input.GetButtonDown("RightChange")) {
            NextRightWeapon();
        }

        //�g�p����h���ύX�i����j
        if(Input.GetButtonDown("LeftChange"))
        {
            NextLeftWeapon();
        }

        //�L�[����ł��Ȃ����Ԃ͑��삳���Ȃ�
        if (unplayableTime > 0f)
        {
            unplayableTime -= deltaTime;
        } else
        {
            //�n�ʂɒ����Ă��邩����
            if (isGround)
            {
                //�_�b�V��
                if (Input.GetButtonDown("Sprint"))
                {
                    playerSpeed = playerSpeed == sprintSpeed ? normalSpeed : sprintSpeed;
                }

                //�ړ�
                Vector3 tmpVelocity = GetMoveVector() * playerSpeed;
                if (tmpVelocity.magnitude == 0f) playerSpeed = normalSpeed;
                tmpVelocity.y = rig.velocity.y;
                rig.velocity = tmpVelocity;

                //�O�ɒi�������邩�`�F�b�N
                StepClimb();

                //�W�����v
                if (Input.GetButtonUp("Jump"))
                {
                    Jump(jumpLongPress);

                    animator.SetBool("IsFlying", true);
                }
                //�X���C�f�B���O
                else if (Input.GetButtonDown("Crouch")) {
                    animator.SetTrigger("Sliding");
                } 
                else
                {
                    //�A�j���[�V������ݒ�
                    animator.SetBool("IsFlying", false);
                    animator.SetFloat("Speed", rig.velocity.magnitude);
                }
            }
            else  //�W�����v���͑O��ɏ��������ړ��ł���
            {
                //�����O�ɕǂ�����Ȃ�ǃL�b�N
                if(Input.GetButtonDown("Jump"))
                {
                    if (Physics.Raycast(transform.position, transform.forward, 0.5f))
                    {
                        rig.velocity = -transform.forward * playerSpeed;
                        animator.SetTrigger("WallJump");
                        Jump(maxJumpPower);
                    }
                }

                //�W�����v�A�j���[�V����
                animator.SetBool("IsFlying", true);

                //�ړ�
                Vector3 tmpVelocity = GetMoveVector() * airSpeed * deltaTime;
                rig.velocity += tmpVelocity + Vector3.down * deltaTime * 10f;
            }
        }

        //�W�����v�̒���������
        if (Input.GetButton("Jump")) jumpLongPress += deltaTime;
        else jumpLongPress = 0.3f;

        //�����𐧌�
        if(rig.velocity.magnitude > maxSpeed)
        {
            rig.velocity = rig.velocity.normalized * maxSpeed;
        }
    }

    //�o���A�[�A�V�[���h�̃I���I�t�֐�
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

    //��������̃x�N�g���̎擾�֐�
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

    //�W�����v�p�֐�
    private void Jump(float power)
    {
        float jumpPower = power > 1f ? 1f : power;
        rig.AddForce(Vector3.up * jumpPower * maxJumpPower, ForceMode.VelocityChange);
        unplayableTime += 0.2f;
    }

    //�N���C�A���g���g���I�������֐�
    public void GenerateTrion(Vector3 pos, Quaternion rot, Vector3 scale, Place place)
    {
        GenerateTrionServerRpc(pos, rot, scale, place, NetworkManager.LocalClientId);
    }

    //�g���I�������˗��֐�
    [ServerRpc(RequireOwnership =false)] private void GenerateTrionServerRpc(Vector3 pos, Quaternion rot, Vector3 scale, Place place, ulong id)
    {
        //�����ݒ�
        GameObject tmpTrion = Instantiate(trionPrefab, pos, rot);
        tmpTrion.transform.localScale = scale;
        tmpTrion.GetComponent<TrionController>().place.Value = place;
        //������^���Ȃ��琶��
        tmpTrion.GetComponent<NetworkObject>().SpawnWithOwnership(id);
    }

    //�N���C�A���g���g���I����ԕύX�ϐ�
    public void ChangeTrionMode(Quaternion rot, Place place, TrionController.Mode mode, int trionPower = 5, GameObject targetObj = null)
    {
        List<GameObject> tmpList = place == Place.Right ? rightChargedTrion : leftChargedTrion;
        foreach (GameObject trion in tmpList)
        {
            //�g���I���̃R���|�[�l���g���擾
            TrionController tmp = trion.GetComponent<TrionController>();
            if(tmp != null)
            {
                //�g���I���̏�Ԃ�ύX
                tmp.ChangeMode(mode, trionPower);

                //�^�[�Q�b�g��ݒ�
                tmp.targetObj = targetObj;
            }
            //�g���I���̌�����ύX
            trion.transform.rotation = GetLookingRotaion();
        }

        tmpList.Clear();
    }

    //�O���X�z�b�p�[�����֐�
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

        //�������W�v�Z
        Vector3 pos = transform.position - new Vector3(0f, -0.3f, 0f);

        //���g�̏ꏊ�ɐ���
        Instantiate(glassHopperPrefab, pos, rot);

        //�T�[�o�[���Ɉ˗�
        TryToGenerateGlassHopperServerRpc(pos, rot, NetworkManager.Singleton.LocalClientId);
    }


    //�T�[�o�[���O���X�z�b�p�[�����֐�
    [ServerRpc(RequireOwnership = false)] private void TryToGenerateGlassHopperServerRpc(Vector3 pos, Quaternion rot, ulong id)
    {
        TryToGenerateGlassHopperClientRpc(pos, rot, id);
    }
    //�N���C�A���g���O���X�z�b�p�[�����֐�
    [ClientRpc] private void TryToGenerateGlassHopperClientRpc(Vector3 pos, Quaternion rot, ulong exceptId)
    {
        //���g�͉������Ȃ�
        if (exceptId == NetworkManager.Singleton.LocalClientId) return;

        //�w��̏ꏊ�ɐ���
        Instantiate(glassHopperPrefab, pos, rot);
    }

    //�T�[�o�[���G�X�N�[�h�����֐�
    [ServerRpc(RequireOwnership = false)] public void GenerateEscudoServerRpc(Vector3 pos, Quaternion rot, float size, ulong id)
    {
        GameObject tmpEscudo = Instantiate(escudoPrefab, pos, rot);
        tmpEscudo.transform.localScale = new Vector3(size, 0f, size);
        tmpEscudo.GetComponent<NetworkObject>().SpawnWithOwnership(id);
    }

    //���_�̐�ɋ��郂�m���擾
    public RaycastHit[] GetRayHits()
    {
        return Physics.RaycastAll(cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0.1f)));
    }

    //�v���C���[�̌������擾
    public Quaternion GetLookingRotaion()
    {
        return Quaternion.Euler(cam.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y, 0f);
    }

    //�E��̕����ύX
    private void NextRightWeapon()
    {
        if (rightTriggerName[rightWeaponNum] == "") return;

        //��O������
        Destroy(rightHand.GetComponent(Type.GetType(rightTriggerName[rightWeaponNum] + "Controller")));
        //��O�ɂ���
        rightWeaponNum = (rightWeaponNum + 1) % 3;

        //�R���g���[���[��ݒ�
        WeaponController weaponCon = (WeaponController)rightHand.AddComponent(Type.GetType(rightTriggerName[rightWeaponNum] + "Controller"));
        weaponCon.Initialize(this, Place.Right);

        WeaponUIManager.instance.ChangeWeapon(rightWeaponNum, Place.Right);
    }

    //����̕����ς���
    private void NextLeftWeapon()
    {
        if (leftTriggerName[leftWeaponNum] == "") return;

        //��O������
        Destroy(leftHand.GetComponent(Type.GetType(leftTriggerName[leftWeaponNum] + "Controller")));
        //��O�ɂ���
        leftWeaponNum = (leftWeaponNum + 1) % 3;

        //�R���g���[���[��ݒ�
        WeaponController weaponCon = (WeaponController)leftHand.AddComponent(Type.GetType(leftTriggerName[leftWeaponNum] + "Controller"));
        weaponCon.Initialize(this, Place.Left);

        WeaponUIManager.instance.ChangeWeapon(leftWeaponNum, Place.Left);
    }

    [ServerRpc(RequireOwnership = false)] public void DamageServerRpc(float damagePoint, ulong id)
    {
        //�����`�[���̎��͉������Ȃ�
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

            //�O�ɕǂ����邩�`�F�b�N
            if (Physics.Raycast(posVec, legObject.transform.forward, stepDistance))
            {
                //�O�̕ǂ��o��邩�`�F�b�N
                if (!Physics.Raycast(posVec + new Vector3(0f, stepHeight, 0f), legObject.transform.forward, stepDistance))
                {
                    //���Ƀ��C�L���X�g���΂������𒲂ׂ�
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

    //�t�b�N�V���b�g���f�֐�
    [ServerRpc(RequireOwnership = false)] public void HookShotServerRpc(Place place, bool flag)
    {
        //�E��
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
 