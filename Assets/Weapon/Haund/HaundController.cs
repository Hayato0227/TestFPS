using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class HaundController : WeaponController
{
    //�ύX�萔
    private float trionDuration;
    private int trionNum = 0;
    private GameObject targetObj;

    public HaundController()
    {
        maxTrionNum = 5;
        trionPointForGeneration = 30f;
        trionNum = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //���_�̐�ɓG�����邩���`�F�b�N
        foreach(RaycastHit hit in playerController.GetRayHits()) {
            Transform tmpTransform = hit.transform.root;
            if(tmpTransform.CompareTag("Player"))
            {
                if (tmpTransform.gameObject != NetworkManager.Singleton.LocalClient.PlayerObject.gameObject)
                {

                    targetObj = tmpTransform.gameObject;
                }
            }
        }
        
        //�L�[�������ꂽ�甭��
        if (!Input.GetButton(weaponKey))
        {
            //�A�j���[�V������ݒ�
            playerController.animator.SetBool("RightHandOver", false);

            //�����Ă�������ɔ��� & �^�[�Q�b�g�w��
            playerController.ChangeTrionMode(playerController.GetLookingRotaion(), weaponPlace, TrionController.Mode.Chase, playerController.trionPower, targetObj);

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
                //���Ă�Ƃ��͑ł�
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

        //��ʊO���ǂ����\�����Ċm���߂�
        if (!UIManager.instance.ShowTarget(targetObj))
        {
            targetObj = null;
        }
    }

    private void OnDestroy()
    {
        UIManager.instance.ShowTarget(null);
    }
}
