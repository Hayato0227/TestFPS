using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookShotController : WeaponController
{
    private SpringJoint springJoint;

    // Update is called once per frame
    void Update()
    {
        //フックショット開始
        if(Input.GetButtonDown(weaponKey))
        {
            //フックショットを当てる位置を設定
            Ray ray = playerController.cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            if (hits.Length > 0)
            {
                foreach(var hit in hits)
                {
                    if(!hit.transform.root.CompareTag("Player"))
                    {
                        //フックショット描画開始
                        playerController.HookShotServerRpc(weaponPlace, true);

                        //スプリングジョイントを設定
                        springJoint = transform.root.gameObject.AddComponent<SpringJoint>();

                        //距離を計算
                        springJoint.maxDistance = (playerController.trionPower + 1) * 5f;

                        //くっつきを設定
                        springJoint.autoConfigureConnectedAnchor = false;
                        springJoint.massScale = playerController.GetComponent<Rigidbody>().mass;
                        springJoint.connectedAnchor = hit.point;

                        //線の描画の位置を設定
                        if (weaponPlace == PlayerController.Place.Right)
                        {
                            playerController.rightHookShotPos = hit.point;
                        }
                        else
                        {
                            playerController.leftHookShotPos = hit.point;
                        }
                        break;
                    }
                }
            }
        }
        else if(Input.GetButtonUp(weaponKey))
        {
            playerController.HookShotServerRpc(weaponPlace, false);
            Destroy(springJoint);
        }
    }

    private void OnDestroy()
    {
        playerController.HookShotServerRpc(weaponPlace, false);
        Destroy(springJoint);
    }
}
