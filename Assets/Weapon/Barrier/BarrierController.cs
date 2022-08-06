using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierController : WeaponController
{
    private bool isUsing = false;
    void Update()
    {
        //�V�[���h�W�J��
        if(isUsing)
        {
            //�g���I��������
            if(!UseTrion((trionPointForGeneration + 1f) * Time.deltaTime))
            {
                Change(false);
            }
        }

        //�o���A�[�I��
        if(Input.GetButtonDown(weaponKey))
        {
            //�傫������
            trionPointForGeneration = (1f + playerController.trionPower / 10f) * 5f;
            playerController.audioSource.PlayAudio(10);
            Change(true, trionPointForGeneration);
        }
        //�V�[���h�I�t
        else if(Input.GetButtonUp(weaponKey))
        {
            Change(false);
        }
    }

    private void OnDestroy()
    {
        Change(false);
    }

    private void Change(bool flag, float size = 1f)
    {
        playerController.ChangeLeftWeaponStatusServerRpc(size, 1, flag);
        isUsing = flag;
    }
}
