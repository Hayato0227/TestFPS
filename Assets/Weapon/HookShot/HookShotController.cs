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
                        if(UseTrion(trionPointForGeneration))
                        {
                            //�t�b�N�V���b�g�`��J�n
                            playerController.HookShotServerRpc(hit.point, weaponPlace);

                            //�t�b�N�V���b�g�̉���炷
                            playerController.audioSource.PlayAudio(8);

                            //�X�v�����O�W���C���g��ݒ�
                            springJoint = transform.root.gameObject.AddComponent<SpringJoint>();

                            //��������ݒ�
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
            //�������v�Z
            springJoint.maxDistance = (playerController.trionPower + 1) * 5f;
        }
    }

    private void OnDestroy()
    {
        playerController.HookShotServerRpc(Vector3.zero, weaponPlace);
        Destroy(springJoint);
    }
}
