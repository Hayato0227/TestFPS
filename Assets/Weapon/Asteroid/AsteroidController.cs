using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidController : WeaponController
{
    //�������̕ϐ�
    private float trionDuration;
    private int trionNum;

    public AsteroidController()
    {
        maxTrionNum = 5;
        trionPointForGeneration = 10f;
        trionNum = 0;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
     void Update()
    {
        //�L�[�������ꂽ�甭��
        if(!Input.GetButton(weaponKey))
        {
            //�A�j���[�V������ݒ�
            playerController.animator.SetBool("RightHandOver", false);

            //�����Ă�������ɔ���
            playerController.ChangeTrionMode(playerController.GetLookingRotaion(), weaponPlace, TrionController.Mode.Straight, playerController.trionPower);

            trionDuration = 0f;
            trionNum = 0;
        }
        //�L�[�������ꑱ���Ă���Ƃ��͎��Ԃ����Z
        else
        {
            //�A�j���[�V������ݒ�
            playerController.animator.SetBool("RightHandOver", true);

            trionDuration += Time.deltaTime;

            if(trionDuration > trionNum && trionNum < maxTrionNum)
            {
                //���Ă�Ƃ��͌���
                if(UseTrion(trionPointForGeneration))
                {
                    //����ʒu�ɐ���
                    playerController.GenerateTrion(transform.position + new Vector3(0f, 0.2f * trionNum, 0.2f), Quaternion.identity, trionSize, weaponPlace);
                    playerController.audioSource.PlayAudio(trionNum);
                    trionNum++;
                } else
                {
                    trionDuration -= Time.deltaTime;
                }
            }
        }
    }
}