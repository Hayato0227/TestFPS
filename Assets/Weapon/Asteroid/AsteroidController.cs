using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidController : WeaponController
{
    //長押しの変数
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
        //キーが離されたら発射
        if(!Input.GetButton(weaponKey))
        {
            //アニメーションを設定
            playerController.animator.SetBool("RightHandOver", false);

            //向いている向きに発射
            playerController.ChangeTrionMode(playerController.GetLookingRotaion(), weaponPlace, TrionController.Mode.Straight, playerController.trionPower);

            trionDuration = 0f;
            trionNum = 0;
        }
        //キーが押され続けているときは時間を加算
        else
        {
            //アニメーションを設定
            playerController.animator.SetBool("RightHandOver", true);

            trionDuration += Time.deltaTime;

            if(trionDuration > trionNum && trionNum < maxTrionNum)
            {
                //撃てるときは撃つ
                if(UseTrion(trionPointForGeneration))
                {
                    //所定位置に生成
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