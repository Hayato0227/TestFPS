using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Door : NetworkBehaviour
{
    public NetworkVariable<bool> State = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        State.OnValueChanged += OnStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        State.OnValueChanged -= OnStateChanged;
    }

    public void OnStateChanged(bool previous, bool current)
    {
        if(State.Value)
        {
            Debug.Log("Door Opened");
        }
        else
        {
            Debug.Log("Door Closed");
        }
    }

    [ServerRpc(RequireOwnership =  false)] public void ToggleServerRpc()
    {
        State.Value = !State.Value;
    }

}
