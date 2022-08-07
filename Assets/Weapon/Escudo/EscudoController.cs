using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EscudoController : WeaponController
{
    private GameObject previewObject;
    private float escudoDistance = 10f;

    // Update is called once per frame
    void Update()
    {
        //�v���r���[����
        if(Input.GetButtonDown(weaponKey))
        {
            previewObject = Instantiate(playerController.previewEscudoPrefab);
            SetPreviewPosition();
        } 
        //�v���r���[�̈ʒu�ɐ���
        else if(Input.GetButtonUp(weaponKey))
        {
            if(UseTrion(10f + playerController.trionPower * 2f))
            {
                playerController.GenerateEscudoServerRpc(previewObject.transform.position, previewObject.transform.rotation, previewObject.transform.localScale.x);
                playerController.audioSource.PlayAudio(5);
            }
            Destroy(previewObject);
        }
        //�v���r���[�̈ʒu���ړ�
        else if(Input.GetButton(weaponKey))
        {
            SetPreviewPosition();
        }
    }

    private void SetPreviewPosition()
    {
        //���W���擾
        RaycastHit[] hits = playerController.GetRayHits();

        //�����������̂̋߂����́i�����ȊO�j���擾
        foreach (RaycastHit hit in playerController.GetRayHits())
        {
            //�������g�ȊO������Ό���
            if(hit.transform.root.gameObject != NetworkManager.Singleton.LocalClient.PlayerObject.gameObject)
            {
                //�ʒu����
                previewObject.transform.position = hit.point;

                //���������
                previewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * playerController.transform.rotation;

                //�傫������
                previewObject.transform.localScale = Vector3.one * (1f + playerController.trionPower / 10f);
                break;
            }

            //���������ȏゾ�Ǝ��߂�
            if(hit.distance > escudoDistance)
            {
                break;
            }
        }
    }
}
