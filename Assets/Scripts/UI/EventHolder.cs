using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EventHolder : MonoBehaviour
{
    [SerializeField] private UnityEvent unityEvent;
    private Outline outline;

    private Vector3 scale;
    private bool outLineFlag = false;
    private float outlineDuration = 0f;

    private void Start()
    {
        scale = transform.localScale;

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.red;
        outline.OutlineWidth = 5f;
        outline.enabled = false;
    }

    private void Update()
    {
        if(outlineDuration > 0f)
        {
            outlineDuration -= Time.deltaTime;
            if (outlineDuration < 0f) outline.enabled = false;
        }
    }

    public void OutlineOn()
    {
        outline.enabled = true;
        outlineDuration = 0.5f;
    }

    public void Invoke()
    {
        unityEvent.Invoke();
        transform.DOScaleY(scale.y / 2f, 0.5f).OnComplete(() =>
        {
            transform.DOScaleY(scale.y, 0.5f);
        });
        GameManager.instance.PlayerSEClientRpc(1);
    }
}
