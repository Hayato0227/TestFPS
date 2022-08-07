using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class Escudo : MonoBehaviour
{
    private static float escudoJumpPower = 20f;
    private bool isPush = false;
    void Start()
    {
        StartCoroutine(escudoCoroutine());
    }

    private IEnumerator escudoCoroutine()
    {
        //ëÂÇ´Ç≠Ç∑ÇÈ
        transform.DOScaleY(transform.localScale.x, 1.5f).SetEase(Ease.InExpo);

        yield return new WaitForSeconds(1.25f);
        isPush = true;

        yield return new WaitForSeconds(0.25f);
        isPush = false;

        yield return new WaitForSeconds(10f);
        yield return transform.DOScaleY(0f, 1.5f).WaitForCompletion();

        Destroy(gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        //ÉvÉåÉCÉÑÅ[ÇîÚÇŒÇ∑
        if(isPush)
        {
            //é©ï™ÇæÇØîÚÇŒÇ∑
            if (collision.transform.root.gameObject == NetworkManager.Singleton.LocalClient.PlayerObject.gameObject)
            {
                collision.rigidbody.AddForce(transform.up * escudoJumpPower, ForceMode.VelocityChange);
                isPush = false;
            }
        }
    }
}
