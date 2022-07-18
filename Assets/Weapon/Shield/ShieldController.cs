using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : WeaponController
{    private bool isUsing = false;
    void Update()
    {
        //シールド展開中
        if(isUsing)
        {
            //トリオンを消費
            if(!UseTrion((trionPointForGeneration * 2.5f) * Time.deltaTime))
            {
                Change(false);
            }
        }

        //シールドオン
        if(Input.GetButtonDown(weaponKey))
        {
            //大きさ調整
            trionPointForGeneration = 1f + playerController.trionPower / 5f;
            Change(true, trionPointForGeneration);
        }
        //シールドオフ
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
