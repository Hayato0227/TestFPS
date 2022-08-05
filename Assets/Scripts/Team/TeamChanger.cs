using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamChanger : MonoBehaviour
{
    [SerializeField] private BattleManager.Team team;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.GetComponent<PlayerController>() != null)
        {
            BattleManager.Singleton.ChangeTeam(other.transform.root.gameObject, team);
        }
    }
}
