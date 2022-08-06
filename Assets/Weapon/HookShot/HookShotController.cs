using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookShotController : WeaponController
{
    public HookShotController()
    {
        trionPointForGeneration = 15f;
    }

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
                        if(UseTrion(trionPointForGeneration))
                        {
                            //フックショット描画開始
                            playerController.HookShotServerRpc(hit.point, weaponPlace);

                            //フックショットの音を鳴らす
                            playerController.audioSource.PlayAudio(8);

                            //スプリングジョイントを設定
                            springJoint = transform.root.gameObject.AddComponent<SpringJoint>();

                            //くっつきを設定
                            springJoint.autoConfigureConnectedAnchor = false;
                            springJoint.massScale = playerController.GetComponent<Rigidbody>().mass / 2f;
                            springJoint.connectedAnchor = hit.point;

                        }
                        break;
                    }
                }
            }
        }
        else if(Input.GetButtonUp(weaponKey))
        {
            playerController.HookShotServerRpc(Vector3.zero, weaponPlace);
            Destroy(springJoint);
        }

        if (springJoint != null)
        {
            //距離を計算
            springJoint.maxDistance = (playerController.trionPower + 1) * 5f;
        }
    }

    private void OnDestroy()
    {
        playerController.HookShotServerRpc(Vector3.zero, weaponPlace);
        Destroy(springJoint);
    }
}
