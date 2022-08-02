using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookShotController : WeaponController
{
    private SpringJoint springJoint;

    // Update is called once per frame
    void Update()
    {
        //�t�b�N�V���b�g�J�n
        if(Input.GetButtonDown(weaponKey))
        {
            //�t�b�N�V���b�g�𓖂Ă�ʒu��ݒ�
            Ray ray = playerController.cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            if (hits.Length > 0)
            {
                foreach(var hit in hits)
                {
                    if(!hit.transform.root.CompareTag("Player"))
                    {
                        //�t�b�N�V���b�g�`��J�n
                        playerController.HookShotServerRpc(weaponPlace, true);

                        //�X�v�����O�W���C���g��ݒ�
                        springJoint = transform.root.gameObject.AddComponent<SpringJoint>();

                        //�������v�Z
                        springJoint.maxDistance = (playerController.trionPower + 1) * 5f;

                        //��������ݒ�
                        springJoint.autoConfigureConnectedAnchor = false;
                        springJoint.massScale = playerController.GetComponent<Rigidbody>().mass;
                        springJoint.connectedAnchor = hit.point;

                        //���̕`��̈ʒu��ݒ�
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
