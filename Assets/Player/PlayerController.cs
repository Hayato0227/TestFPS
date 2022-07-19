using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class PlayerController : NetworkBehaviour
{
    //�R���|�[�l���g
    [SerializeField] private Rigidbody rig;
    public Animator animator;

    //�A�E�g���b�g�ڑ��p
    [SerializeField] private GameObject camBase;
    public Camera cam;
    [SerializeField] public GameObject trionPrefab;
    public GameObject rightHand;
    public GameObject leftHand;
    [SerializeField] private GameObject glassHopperPrefab;

    //�h��
    [SerializeField] private GameObject barrierGameObject;
    [SerializeField] private GameObject shieldGameObject;
    [SerializeField] private GameObject escudoPrefab;
    public GameObject previewEscudoPrefab;

    //�ϐ�(�Œ�)
    private float sprintSpeed = 7.5f;
    private float normalSpeed = 5f;
    private float airSpeed = 10f;
    private float maxSpeed = 50f;

    private float maxJumpPower = 10f;
    
    private float mouseSensitive = 0.5f;
    private float trionHealPower = 5f;

    private float glassHopperTrion = 10f;

    //�ϐ�(�ς�鐔�l)
    private float jumpLongPress = 0f;
    private float unplayableTime = 0f;
    private float playerSpeed = 10f;
    private float previousHitPoint = 100f;

    //����̐��l
    private int rightWeaponNum = 2;
    private int leftWeaponNum = 2;

    //�E�荶��ɗ��߂Ă���g���I���̃��X�g
    public List<GameObject> rightChargedTrion = new();
    public List<GameObject> leftChargedTrion = new();

    //�v���C���[�X�e�[�^�X
    public float trionPoint = 100f;
    public NetworkVariable<float> hitPoint = new(100f);
    public int trionPower = 0;

    //�`�[���p
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

    //�i�����z���p
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
            //�`�[���Ȃ��ɐݒ�
            team.Value = Team.None;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //�����̂ݑ���
        if (!IsOwner) return;

        //�ǂ��g���ϐ��͗\�ߎ擾���Ă���
        float deltaTime = Time.deltaTime;

        //HP�𔽉f
        UIManager.instance.ChangeHP(hitPoint.Value);
        if(previousHitPoint > 0f && hitPoint.Value <= 0f)
        {
            //��
            Dead();
        }
        previousHitPoint = hitPoint.Value;
        

        //�g���I����
        trionPoint += trionHealPower * deltaTime;
        if (trionPoint > 100f) trionPoint = 100f;
        //�g���I����\��
        UIManager.instance.ChangeTrionPointGUI(trionPoint);

        if (Input.GetKeyDown(KeyCode.K)) Dead();

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
            if (Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, Quaternion.identity, 1.1f))
            {
                //�_�b�V��
                if (Input.GetButton("Sprint"))
                {
                    playerSpeed = sprintSpeed;
                }
                else  //����
                {
                    playerSpeed = normalSpeed;
                }

                //�ړ�
                Vector3 tmpVelocity = GetMoveVector() * playerSpeed;
                tmpVelocity.y = rig.velocity.y;
                rig.velocity = tmpVelocity;

                //�O�ɒi�������邩�`�F�b�N
                StepClimb();

                //�W�����v
                if (Input.GetButtonUp("Jump"))
                {

                    Jump();
                    unplayableTime += 0.3f; //����ł��Ȃ����Ԃ�ǉ�

                    animator.SetBool("IsFlying", true);
                } else
                {
                    //�A�j���[�V������ݒ�
                    animator.SetBool("IsFlying", false);
                    animator.SetFloat("Speed", rig.velocity.magnitude);
                }
            }
            else  //�W�����v���͑O��ɏ��������ړ��ł���
            {
                //�W�����v�A�j���[�V����
                animator.SetBool("IsFlying", true);

                Vector3 tmpVelocity = GetMoveVector() * airSpeed * deltaTime;
                rig.velocity += tmpVelocity;

                //�O���X�z�b�p�[���N��
                if(Input.GetButtonDown("Jump"))
                {
                    if(UseTrion(glassHopperTrion))
                    {
                        Vector3 moveVec = GetMoveVector();
                        if (moveVec.magnitude == 0f)
                        {
                            //������ɐ���
                            TryToGenerateGlassHopper(Quaternion.Euler(0f, 0f, 0f));
                        }
                        else
                        {
                            //�i�ތ���*70�x�̌����Ő���
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
                            //�������ɐ���
                            TryToGenerateGlassHopper(Quaternion.Euler(180f, 0f, 0f));
                        }
                        else
                        {
                            //�i�ތ���*110�x�̌����Ő���
                            TryToGenerateGlassHopper(Quaternion.LookRotation(moveVec) * Quaternion.Euler(110f, 0f, 0f));
                        }
                    }
                }
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
    private void Jump()
    {
        float jumpPower = jumpLongPress > 1f ? 1f : jumpLongPress;
        rig.AddForce(Vector3.up * jumpPower * maxJumpPower, ForceMode.VelocityChange);
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
    private void TryToGenerateGlassHopper(Quaternion rot)
    {
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
        rightWeaponNum = (rightWeaponNum + 1) % 3;

        switch(rightWeaponNum)
        {
            case 0:  //�A�X�e���C�h
                Destroy(rightHand.GetComponent<HaundController>());
                rightHand.AddComponent<AsteroidController>().Initialize(this, Place.Right, Vector3.one * 0.1f);
                break;

            case 1:  //�o�C�p�[
                Destroy(rightHand.GetComponent<AsteroidController>());
                rightHand.AddComponent<ViperController>().Initialize(this, Place.Right, Vector3.one * 0.1f);
                break;

            case 2:  //�n�E���h
                Destroy(rightHand.GetComponent<ViperController>());
                rightHand.AddComponent<HaundController>().Initialize(this, Place.Right, Vector3.one * 0.1f);
                break;
        }
        WeaponUIManager.instance.ChangeWeapon(rightWeaponNum, Place.Right);
    }

    //����̕����ς���
    private void NextLeftWeapon()
    {
        leftWeaponNum = (leftWeaponNum + 1) % 3;

        switch(leftWeaponNum)
        {
            case 0: //�V�[���h
                Destroy(leftHand.GetComponent<EscudoController>());
                leftHand.AddComponent<ShieldController>().Initialize(this, Place.Left, Vector3.one);
                break;

            case 1: //�o���A�[
                Destroy(leftHand.GetComponent<ShieldController>());
                leftHand.AddComponent<BarrierController>().Initialize(this, Place.Left, Vector3.one);
                break;

            case 2: //�G�X�N�[�h
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

    //���ʊ֐�
    public void Dead()
    {
        float deadTime = 5f;
        PlayerFadeOut();
        unplayableTime = deadTime;
        Invoke("PlayerFadeIn", deadTime);
        Invoke("Respawn", deadTime);
    }

    //������֐�
    private void PlayerFadeOut()
    {
        transform.Find("Rob/Model").GetComponent<Renderer>().material.DOFade(0f, 1f);
    }

    //�o�Ă���֐�
    private void PlayerFadeIn()
    {
        transform.Find("Rob/Model").GetComponent<Renderer>().material.DOFade(1f, 1f);
    }

    //���X�|�[���֐�
    public void Respawn()
    {
        GameManager.instance.Respawn(this.gameObject, team.Value);
        ResetHPServerRpc();
    }

    //HP�X�V�֐�
    [ServerRpc(RequireOwnership = false)] private void ResetHPServerRpc()
    {
        hitPoint.Value = 100f;
    }
} 
 