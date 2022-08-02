using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class GlassHopperManager : MonoBehaviour
{
    private static float glassHopperPower = 20f;
    private static float glassHopperDuration = 5f;

    private void Start()
    {
        StartCoroutine(glassHopperCoroutine());
    }

    private IEnumerator glassHopperCoroutine()
    {
        yield return transform.DOScale(new Vector3(1f, 0.1f, 1f), 0.5f).WaitForCompletion();

        yield return new WaitForSeconds(glassHopperDuration);

        yield return transform.DOScale(Vector3.zero, 0.5f).WaitForCompletion();

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //プレイヤーオブジェクトのみに処理
        if (!other.CompareTag("Player")) return;

        other.transform.root.GetComponent<Rigidbody>().velocity = transform.up * glassHopperPower;
    }
}
