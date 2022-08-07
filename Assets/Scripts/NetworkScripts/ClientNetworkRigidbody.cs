using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientNetworkRigidbody : NetworkBehaviour
{
    private Rigidbody m_Rigidbody;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (IsOwner) SendInformationServerRpc(m_Rigidbody.velocity);
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Unreliable)] private void SendInformationServerRpc(Vector3 vel)
    {
        ReceiveInformationClientRpc(vel);
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)] private void ReceiveInformationClientRpc(Vector3 vel)
    {
        if (IsOwner) return;
        m_Rigidbody.velocity = vel;
    }
}
