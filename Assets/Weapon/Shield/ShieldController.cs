using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : WeaponController
{    private bool isUsing = false;
    void Update()
    {
        //�V�[���h�W�J��
        if(isUsing)
        {
            //�g���I��������
            if(!UseTrion((trionPointForGeneration * 2.5f) * Time.deltaTime))
            {
                Change(false);
            }
        }

        //�V�[���h�I��
        if(Input.GetButtonDown(weaponKey))
        {
            //�傫������
            trionPointForGeneration = 1f + playerController.trionPower / 5f;
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
        playerController.ChangeLeftWeaponStatusServerRpc(size, 0, flag);
        isUsing = flag;
    }
}
