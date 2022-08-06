using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class Escudo : NetworkBehaviour
{
    private static float escudoJumpPower = 20f;
    private bool isPush = false;
    void Start()
    {
        StartCoroutine(escudoCoroutine());
    }

    private IEnumerator escudoCoroutine()
    {
        //大きくする
        if (IsServer)
        {
            //サイズを変更する
            transform.DOScaleY(transform.localScale.x, 1.5f).SetEase(Ease.InExpo);
        }

        yield return new WaitForSeconds(1.25f);
        isPush = true;

        yield return new WaitForSeconds(0.25f);
        isPush = false;

        yield return new WaitForSeconds(30f);
        if (IsServer) GetComponent<NetworkObject>().Despawn();
    }

    private void OnCollisionStay(Collision collision)
    {
        //プレイヤーを飛ばす
        if(isPush)
        {
            //自分だけ飛ばす
            if (collision.transform.root.gameObject == NetworkManager.Singleton.LocalClient.PlayerObject.gameObject)
            {
                collision.rigidbody.AddForce(transform.up * escudoJumpPower, ForceMode.VelocityChange);
                isPush = false;
            }
        }
    }
}
