using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViperController : WeaponController
{

    //�ύX�萔
    private int trionNum = 0;
    private float trionDuration;

    // Start is called before the first frame update
    public ViperController()
    {
        maxTrionNum = 5;
        trionPointForGeneration = 20f;
        trionNum = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //�L�[�������ꂽ�甭��
        if (!Input.GetButton(weaponKey))
        {
            //�A�j���[�V������ݒ�
            playerController.animator.SetBool("RightHandOver", false);

            //�����Ă�������ɔ���
            playerController.ChangeTrionMode(playerController.GetLookingRotaion(), weaponPlace, TrionController.Mode.Controll, playerController.trionPower);
            if (Input.GetButtonUp(weaponKey)) playerController.audioSource.PlayAudio(6);

            trionDuration = 0f;
            trionNum = 0;
        }
        //�L�[�������ꑱ���Ă���Ƃ��͎��Ԃ����Z
        else
        {
            //�A�j���[�V������ݒ�
            playerController.animator.SetBool("RightHandOver", true);

            trionDuration += Time.deltaTime;

            if (trionDuration > trionNum && trionNum < maxTrionNum)
            {
                //���Ă�Ƃ��͌���
                if(UseTrion(trionPointForGeneration))
                {
                    //����ʒu�ɐ���
                    playerController.GenerateTrion(transform.position + new Vector3(0f, 0.2f * trionNum, 0.2f), weaponPlace);
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
