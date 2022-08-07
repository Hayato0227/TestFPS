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
        //プレビュー生成
        if(Input.GetButtonDown(weaponKey))
        {
            previewObject = Instantiate(playerController.previewEscudoPrefab);
            SetPreviewPosition();
        } 
        //プレビューの位置に生成
        else if(Input.GetButtonUp(weaponKey))
        {
            if(UseTrion(10f + playerController.trionPower * 2f))
            {
                playerController.GenerateEscudoServerRpc(previewObject.transform.position, previewObject.transform.rotation, previewObject.transform.localScale.x);
                playerController.audioSource.PlayAudio(5);
            }
            Destroy(previewObject);
        }
        //プレビューの位置を移動
        else if(Input.GetButton(weaponKey))
        {
            SetPreviewPosition();
        }
    }

    private void SetPreviewPosition()
    {
        //座標を取得
        RaycastHit[] hits = playerController.GetRayHits();

        //当たったものの近いもの（自分以外）を取得
        foreach (RaycastHit hit in playerController.GetRayHits())
        {
            //自分自身以外があれば決定
            if(hit.transform.root.gameObject != NetworkManager.Singleton.LocalClient.PlayerObject.gameObject)
            {
                //位置調整
                previewObject.transform.position = hit.point;

                //上向き調整
                previewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * playerController.transform.rotation;

                //大きさ調整
                previewObject.transform.localScale = Vector3.one * (1f + playerController.trionPower / 10f);
                break;
            }

            //距離が一定以上だと辞める
            if(hit.distance > escudoDistance)
            {
                break;
            }
        }
    }
}
