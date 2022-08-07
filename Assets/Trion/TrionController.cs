using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations;

public class TrionController : NetworkBehaviour
{
    //�g���I���X�s�[�h
    public enum Speed
    {
        NormalSpeed,
        SlowSpeed,
        StopSpeed
    }

    //�g���I�����
    public enum Mode
    {
        Straight,
        Chase,
        Controll,
        Equip
    }

    //�g���I�������ʒu
    public NetworkVariable<PlayerController.Place> place = new();

    //�R���|�[�l���g
    private ParentConstraint positionConstraint;

    //������ԁF����
    public Mode mode = Mode.Equip;
    public int trionPower = 5;

    //�X�s�[�h
    private float normalSpeed = 30f;
    private static float speedRatio = 0.001f;

    //�`�F�C�X�p�g�����X�t�H�[��
    public GameObject targetObj = null;
    
    //���̑��ڍוϐ�
    private float lifeTime = 200.0f;
    private float duration = 0f;
    public float speed = 0f;
    private float slowDuration = 0f;

    private void Start()
    {
        if(IsOwner)
        {
            //�v���C���[��transform�擾
            Transform tmpTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

            //�v���C���[�̉E��or����ɒǉ�
            if (place.Value == PlayerController.Place.Right)
            {
                tmpTransform.GetComponent<PlayerController>().rightChargedTrion.Add(this.gameObject);
            }
            else
            {
                tmpTransform.GetComponent<PlayerController>().leftChargedTrion.Add(this.gameObject);
            }

            positionConstraint = GetComponent<ParentConstraint>();
            Vector3 tmpVec = Quaternion.Inverse(tmpTransform.rotation) * (transform.position - tmpTransform.position);

            ConstraintSource my_source = new ConstraintSource();
            my_source.sourceTransform = tmpTransform;

            my_source.weight = 1f;
            positionConstraint.AddSource(my_source);
            positionConstraint.SetTranslationOffset(0, tmpVec);
            positionConstraint.constraintActive = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (mode != Mode.Equip)
        {
            //�������Ԃ����Z�A���Ԃ��߂���Ώ���
            duration += deltaTime * speed;
            if (duration > lifeTime)
            {
                DestroyServerRpc();
            }
        }

        //�N���C�A���g�͐���
        if (IsOwner)
        {
            if(slowDuration > 0f)
            {
                slowDuration -= Time.deltaTime;
                if (slowDuration <= 0f) SetTrionSpeed(Speed.NormalSpeed);
            }

            //���[�h�ł��ꂼ��̏�����ύX
            if(mode == Mode.Chase && targetObj != null)
            {
                //�G�̕����Ɍ���
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetObj.transform.position - transform.position), deltaTime * (trionPower + 1));
            } else if(mode == Mode.Controll)
            {
                //�}�E�X�ړ��ňړ�����
                float xMove = Input.GetAxis("Mouse X") * (trionPower + 1) * 30;
                float yMove = Input.GetAxis("Mouse Y") * -(trionPower + 1) * 30;

                transform.Rotate(yMove * deltaTime, xMove * deltaTime, 0f);
            }

            //������ԈȊO�Ȃ�O�ɐi��
            if(mode != Mode.Equip)
            {
                positionConstraint.constraintActive = false;
                transform.position += transform.forward * speed * deltaTime;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CheckMyBarrier(other.transform)) return;

        //�o���A�ɓ��������猸��
        if (other.CompareTag("Barrier")) {
            SetTrionSpeed(Speed.SlowSpeed);
        } else if(other.transform.root.CompareTag("Player"))
        {
            //��������Ȃ����`�F�b�N
            if (other.transform.root.gameObject == NetworkManager.Singleton.LocalClient.PlayerObject.gameObject) return;

            //�G�̃_���[�W�֐����Ă�
            other.transform.root.GetComponent<PlayerController>().DamageServerRpc(transform.localScale.x * 100f, NetworkManager.Singleton.LocalClientId);

            //�A�^�b�N����炷
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.seClips[2], 1f);

            //���g������
            DestroyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)] private void DestroyServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
    private void OnTriggerExit(Collider other)
    {
        if (!CheckMyBarrier(other.transform)) return;

        //�o���A�𔲂��������
        if (other.CompareTag("Barrier"))
        {
            SetTrionSpeed(Speed.NormalSpeed);

        }
    }

    private bool CheckMyBarrier(Transform transform)
    {
        //�����ȊO�͓������Ȃ�
        if (!IsOwner) return false;

        //�����̃o���A�͕ς��Ȃ�
        if (NetworkManager.Singleton.LocalClientId == transform.root.GetComponent<NetworkObject>()?.OwnerClientId) return false;

        return true;
    }

    //�X�s�[�h�ݒ�֐�
    public void SetSpeedValue(float value)
    {
        normalSpeed = value;
    }

    //�g���I�����[�h�ύX�֐�
    public void ChangeMode(Mode modeValue, int powerValue)
    {
        mode = modeValue;
        trionPower = powerValue;

        //�X�g���[�g�̏ꍇ�̓X�s�[�h��ς���
        if(modeValue == Mode.Straight)
        {
            SetSpeedValue(normalSpeed * (1f + (float)(trionPower - 5) / 6f));
        }

        if(modeValue != Mode.Equip)
        {
            SetTrionSpeed(Speed.NormalSpeed);
        }
    }


    //�g���I���X�s�[�h�ύX�֐�
    public void SetTrionSpeed(Speed speed)
    {
        switch (speed)
        {
            case Speed.NormalSpeed:
                this.speed = normalSpeed;
                break;

            case Speed.SlowSpeed:
                this.speed = normalSpeed * speedRatio;
                break;

            case Speed.StopSpeed:
                this.speed = 0f;
                break;
        }
        slowDuration = 0.1f;
    }
}
