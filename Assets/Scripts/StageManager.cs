using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class StageManager : NetworkBehaviour
{
    //シングルトン
    public static StageManager Singleton;

    //変数
    public NetworkVariable<int> stageNum = new(0);

    //アウトレット接続
    [SerializeField] private GameObject[] AlphaRespawnPoint;
    [SerializeField] private GameObject[] BetaRespawnPoint;
    [SerializeField] private GameObject NoneRespawn;

    [SerializeField] private Material[] securityCameraMaterial;
    [SerializeField] private MeshRenderer securityCameraMeshRender;

    void Start()
    {
        Singleton = this;
        stageNum.OnValueChanged += ChangeStageCallBack;
    }
    private void OnDestroy()
    {
        stageNum.OnValueChanged -= ChangeStageCallBack;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //ステージ変更関数
    [ServerRpc(RequireOwnership = false)] public void ChangeStageServerRpc()
    {
        stageNum.Value = (stageNum.Value + 1) % AlphaRespawnPoint.Length;
    }
    private void ChangeStageCallBack(int pre, int next)
    {
        securityCameraMeshRender.material = securityCameraMaterial[next];
    }

    //リスポーン関数
    public void Respawn(GameObject obj, BattleManager.Team team)
    {
        //ゲーム中でないときはスポーン地点を設定
        if (BattleManager.Singleton.battlePhase.Value == BattleManager.BattlePhase.Lobby) obj.transform.position = NoneRespawn.transform.position;
        //チームに合わせてリスポーン
        else
        {
            switch(team)
            {
                case BattleManager.Team.Red:
                    obj.transform.position = AlphaRespawnPoint[stageNum.Value].transform.position;
                    break;

                case BattleManager.Team.Blue:
                    obj.transform.position = BetaRespawnPoint[stageNum.Value].transform.position;
                    break;

                default:
                    obj.transform.position = NoneRespawn.transform.position;
                    break;
            }
        }
    }
    //サーバーからクライアントへの一斉リスポーン関数
    [ClientRpc] public void RespawnClientRpc()
    {
        GameObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        BattleManager.Team tmpTeam = playerObject.GetComponent<PlayerController>().team.Value;
        Respawn(playerObject, tmpTeam);
    }
}
