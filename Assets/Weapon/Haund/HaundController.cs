using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class HaundController : WeaponController
{
    //変更定数
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
        //視点の先に敵がいるかをチェック
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
        
        //キーが離されたら発射
        if (!Input.GetButton(weaponKey))
        {
            //アニメーションを設定
            playerController.animator.SetBool("RightHandOver", false);

            //向いている向きに発射 & ターゲット指定
            playerController.ChangeTrionMode(playerController.GetLookingRotaion(), weaponPlace, TrionController.Mode.Chase, playerController.trionPower, targetObj);

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
                //撃てるときは打つ
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

        //画面外かどうか表示して確かめる
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
