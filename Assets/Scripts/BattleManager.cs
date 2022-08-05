using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Singleton;

    public enum BattlePhase
    {
        Lobby,
        Battle
    }
    public NetworkVariable<BattlePhase> battlePhase = new(BattlePhase.Lobby);

    public enum GameMode
    {
        Normal,
        King
    };
    public NetworkVariable<GameMode> gameMode = new(GameMode.Normal);

    public NetworkVariable<int> redTeamPoint = new(0);
    public NetworkVariable<int> blueTeamPoint = new(0);

    public enum Team
    {
        Red = 0,
        Blue = 1,
        None = 2
    }



    private void Awake()
    {
        Singleton = this;
        redTeamPoint.OnValueChanged += AddRedPointCallBack;
        blueTeamPoint.OnValueChanged += AddBluePointCallBack;
    }
    private void OnDestroy()
    {
        redTeamPoint.OnValueChanged -= AddRedPointCallBack;
        blueTeamPoint.OnValueChanged -= AddBluePointCallBack;
    }

    private void FixedUpdate()
    {
        if (battlePhase.Value == BattlePhase.Lobby) DisplaySetting();
    }

    //ゲーム開始
    public NetworkVariable<int> battleTime;
    [ServerRpc(RequireOwnership = false)] public void StartGameServerRpc()
    {
        //バトル開始
        if(battlePhase.Value != BattlePhase.Battle)
        {
            StartCoroutine(GameCoroutine());
        }
        //バトル中はバトルを中止する
        else
        {
            battlePhase.Value = BattlePhase.Lobby;
            GameManager.instance.SendLogServerRpc("No Contest.");
            StopAllCoroutines();
            ResetPlayerStatus();
        }
    }
    private IEnumerator GameCoroutine()
    {
        GameManager tmp = GameManager.instance;
        ResetPoint();
        tmp.SendLogServerRpc("Starting Game in");

        yield return new WaitForSeconds(2f);

        for(int i = 3; i > 0; i--)
        {
            tmp.SendLogServerRpc(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        battlePhase.Value = BattlePhase.Battle;
        tmp.SendLogServerRpc("Game Start");

        //全員を転送
        StageManager.Singleton.RespawnClientRpc();

        //ゲーム中
        while(battleTime.Value > 0f)
        {
            battleTime.Value--;
            yield return new WaitForSeconds(1f);
        }

        //ゲーム終了
        tmp.SendLogServerRpc("Game Over!!");
        StageManager.Singleton.RespawnClientRpc();
        battlePhase.Value = BattlePhase.Lobby;
        ResetPlayerStatus();
    }

    //キル
    public void Kill(ulong killer, ulong victim)
    {
        PlayerController killerController = NetworkManager.Singleton.ConnectedClients[killer].PlayerObject.gameObject.GetComponent<PlayerController>();
        PlayerController victimController = NetworkManager.Singleton.ConnectedClients[victim].PlayerObject.gameObject.GetComponent<PlayerController>();

        GameManager.instance.SendLogServerRpc(killerController.playerName + " killed " + victimController.playerName + ".");
        Debug.Log(killerController.name + "(" + killer + ")が" + victimController.playerName + "(" + victim + ")をキルしました。");

        switch(gameMode.Value)
        {
            //キングモード
            case GameMode.King:

                break;

            //素直に得点加算
            default:
                //赤に得点
                if(killerController.team.Value == Team.Red)
                {
                    redTeamPoint.Value++;
                }else if(killerController.team.Value == Team.Blue)
                {
                    blueTeamPoint.Value++;
                }
                break;
        }

        //死んだ人のリスポーン
        victimController.RespawnClientRpc();
        victimController.hitPoint.Value = 100f;
    }

    //ゲームモード変更
    [ServerRpc(RequireOwnership = false)] public void ChangeGameModeServerRpc()
    {

    }

    //時間変更
    [ServerRpc(RequireOwnership = false)] public void AddTimeServerRpc(int time)
    {
        battleTime.Value += time;
    }

    //クライアントの表示変更
    [SerializeField] private TextMeshPro settingText;
    private void DisplaySetting()
    {
        string settingString = "GameSetting\nMode:";
        switch (gameMode.Value)
        {
            case GameMode.Normal:
                settingString += "Kill To Win";
                break;

            case GameMode.King:
                settingString += "King";
                break;

            default:
                settingString += "None";
                break;
        }
        settingString += "\nTime:" + battleTime.Value + "\nStage:";
        switch (StageManager.Singleton.stageNum.Value)
        {
            case 0:
                settingString += "Room";
                break;

            case 1:
                settingString += "Forest";
                break;

            default:
                settingString += "None";
                break;
        }
        settingText.text = settingString;
    }

    //ゲームオブジェクトのチームを変更
    public void ChangeTeam(GameObject obj, Team team)
    {
        ChangeTeamServerRpc(obj.GetComponent<NetworkObject>().OwnerClientId, team);
    }
    [ServerRpc(RequireOwnership = false)] private void ChangeTeamServerRpc(ulong id, Team team)
    {
        NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerController>().team.Value = team;
    }

    private void AddRedPointCallBack(int pre, int next)
    {
        UIManager.instance.AddTeamPoint(next, Team.Red);
    }
    private void AddBluePointCallBack(int pre, int next)
    {
        UIManager.instance.AddTeamPoint(next, Team.Blue);
    }

    private void ResetPoint()
    {
        redTeamPoint.Value = 0;
        blueTeamPoint.Value = 0;
    }

    private void ResetPlayerStatus()
    {
        foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            //チームを元にもどす
            player.PlayerObject.gameObject.GetComponent<PlayerController>().team.Value = Team.None;
        }
        ResetAllPlayerHP();
    }

    private void ResetAllPlayerHP()
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            player.PlayerObject.gameObject.GetComponent<PlayerController>().hitPoint.Value = 100f;
        }
    }
}
