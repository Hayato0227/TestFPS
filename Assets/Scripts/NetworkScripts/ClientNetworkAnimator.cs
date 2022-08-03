using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientNetworkAnimator : NetworkBehaviour
{
    private Animator m_Animator;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void SetFloat(string name, float value)
    {
        m_Animator.SetFloat(name, value);
        SetFloatServerRpc(name, value);
    }
    public void SetInteger(string name, int value)
    {
        m_Animator.SetInteger(name, value);
        SetIntegerServerRpc(name, value);
    }
    public void SetBool(string name, bool value)
    {
        m_Animator.SetBool(name, value);
        SetBoolServerRpc(name, value);
    }
    public void SetTrigger(string name)
    {
        m_Animator.SetTrigger(name);
        SetTriggerServerRpc(name);
    }

    [ServerRpc(RequireOwnership = false)] private void SetFloatServerRpc(string name, float value)
    {
        SetFloatClientRpc(name, value);
    }
    [ClientRpc] private void SetFloatClientRpc(string name, float value)
    {
        if (IsOwner) return;
        m_Animator.SetFloat(name, value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIntegerServerRpc(string name, int value)
    {
        SetIntegerClientRpc(name, value);
    }
    [ClientRpc]
    private void SetIntegerClientRpc(string name, int value)
    {
        if (IsOwner) return;
        m_Animator.SetInteger(name, value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBoolServerRpc(string name, bool value)
    {
        SetBoolClientRpc(name, value);
    }
    [ClientRpc]
    private void SetBoolClientRpc(string name, bool value)
    {
        if (IsOwner) return;
        m_Animator.SetBool(name, value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTriggerServerRpc(string name)
    {
        SetTriggerClientRpc(name);
    }
    [ClientRpc]
    private void SetTriggerClientRpc(string name)
    {
        if (IsOwner) return;
        m_Animator.SetTrigger(name);
    }
}
