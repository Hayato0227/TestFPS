using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierController : WeaponController
{
    private bool isUsing = false;
    void Update()
    {
        //シールド展開中
        if(isUsing)
        {
            //トリオンを消費
            if(!UseTrion((trionPointForGeneration + 1f) * Time.deltaTime))
            {
                Change(false);
            }
        }

        //バリアーオン
        if(Input.GetButtonDown(weaponKey))
        {
            //大きさ調整
            trionPointForGeneration = (1f + playerController.trionPower / 10f) * 5f;
            playerController.audioSource.PlayAudio(10);
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
        playerController.ChangeLeftWeaponStatusServerRpc(size, 1, flag);
        isUsing = flag;
    }
}
