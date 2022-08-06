using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViperController : WeaponController
{

    //変更定数
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
        //キーが離されたら発射
        if (!Input.GetButton(weaponKey))
        {
            //アニメーションを設定
            playerController.animator.SetBool("RightHandOver", false);

            //向いている向きに発射
            playerController.ChangeTrionMode(playerController.GetLookingRotaion(), weaponPlace, TrionController.Mode.Controll, playerController.trionPower);
            if (Input.GetButtonUp(weaponKey)) playerController.audioSource.PlayAudio(6);

            trionDuration = 0f;
            trionNum = 0;
        }
        //キーが押され続けているときは時間を加算
        else
        {
            //アニメーションを設定
            playerController.animator.SetBool("RightHandOver", true);

            trionDuration += Time.deltaTime;

            if (trionDuration > trionNum && trionNum < maxTrionNum)
            {
                //撃てるときは撃つ
                if(UseTrion(trionPointForGeneration))
                {
                    //所定位置に生成
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
