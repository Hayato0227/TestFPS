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
    public NetworkVariable<BattlePhase> battlePhase;

    public enum GameMode
    {
        Normal,
        King
    };
    public NetworkVariable<GameMode> gameMode;

    public NetworkVariable<int> redTeamPoint;
    public NetworkVariable<int> blueTeamPoint;

    public NetworkVariable<int> battleTime;

    private ulong redKing;
    private ulong blueKing;

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
        battleTime.OnValueChanged += TimeChangeCallBack;
    }

    private void OnDestroy()
    {
        redTeamPoint.OnValueChanged -= AddRedPointCallBack;
        blueTeamPoint.OnValueChanged -= AddBluePointCallBack;
        battleTime.OnValueChanged -= TimeChangeCallBack;
    }

    private void FixedUpdate()
    {
        if (battlePhase.Value == BattlePhase.Lobby) DisplaySetting();
    }

    //ゲーム開始
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
            GameManager.instance.SendLogServerRpc("Game Stopped");
            StopAllCoroutines();
            ResetPlayerStatus();
        }
    }
    private IEnumerator GameCoroutine()
    {
        GameManager tmp = GameManager.instance;
        ResetPoint();
        tmp.SendLogServerRpc("Game Starting in ...");
        battlePhase.Value = BattlePhase.Battle;

        yield return new WaitForSeconds(2f);
        GameManager.instance.PlayBGMClientRpc(BattlePhase.Battle);

        for (int i = 3; i > 0; i--)
        {
            tmp.SendLogServerRpc("Game Starting in " + i.ToString());
            yield return new WaitForSeconds(1f);
        }

        tmp.SendLogServerRpc("Game Start");
        GameManager.instance.PlayerSEClientRpc(0);

        //全員を転送
        StageManager.Singleton.RespawnClientRpc();

        //キングを設定
        if(gameMode.Value == GameMode.King)
        {
            SetNewKing(Team.Red, true);
            SetNewKing(Team.Blue, true);
        }

        //ゲーム中
        while(battleTime.Value > 0f)
        {
            battleTime.Value--;
            yield return new WaitForSeconds(1f);
        }

        //ゲーム終了
        tmp.SendLogServerRpc("Game Over");
        GameManager.instance.PlayerSEClientRpc(0);
        battlePhase.Value = BattlePhase.Lobby;

        //キング解消
        if(gameMode.Value == GameMode.King)
        {
            SetNewKing(Team.Red, false);
            SetNewKing(Team.Blue, false);
        }

        if (blueTeamPoint.Value == redTeamPoint.Value) tmp.SendLogServerRpc("Draw!");
        else tmp.SendLogServerRpc("Team " + (redTeamPoint.Value > blueTeamPoint.Value ? "Red" : "Blue") + " Win!");

        yield return new WaitForSeconds(1f);
        GameManager.instance.PlayBGMClientRpc(BattlePhase.Lobby);
        StageManager.Singleton.RespawnClientRpc();
        ResetPlayerStatus();
    }

    //キル
    public void Kill(ulong killer, ulong victim)
    {
        PlayerController killerController = NetworkManager.Singleton.ConnectedClients[killer].PlayerObject.gameObject.GetComponent<PlayerController>();
        PlayerController victimController = NetworkManager.Singleton.ConnectedClients[victim].PlayerObject.gameObject.GetComponent<PlayerController>();

        GameManager.instance.SendLogServerRpc(killerController.playerName.Value.ToString() + " killed " + victimController.playerName.Value.ToString() + ".");
        Debug.Log(killerController.playerName.Value.ToString() + "(" + killer + ")が" + victimController.playerName.Value.ToString() + "(" + victim + ")をキルしました。");

        switch(gameMode.Value)
        {
            //キングモード
            case GameMode.King:
                if(victimController.OwnerClientId == redKing || victimController.OwnerClientId == blueKing)
                {
                    //キングの場合は得点を加算
                    if (killerController.team.Value == Team.Red)
                    {
                        redTeamPoint.Value++;
                    }
                    else if(killerController.team.Value == Team.Blue)
                    {
                        blueTeamPoint.Value++;
                    }

                    //キングを設定
                    SetNewKing(victimController.team.Value, true);
                }
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
    /// <summary>
    /// キング生成関数
    /// </summary>
    /// <param name="team">設定するチーム</param>
    /// <param name="flag">true:新しく設定する, false:消去だけ</param>
    private void SetNewKing(Team team, bool flag)
    {
        if (team == Team.None) return;

        //新しいキングを設定
        if(flag)
        {
            //一つ前のKingを消去
            NetworkManager.Singleton.ConnectedClients[team == Team.Red ? redKing : blueKing].PlayerObject.GetComponent<PlayerController>().ChangeOutlineColorClientRpc(false);

            List<PlayerController> playerControllers = new();
            foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                PlayerController tmpController = player.PlayerObject.GetComponent<PlayerController>();
                if (tmpController.team.Value == team) playerControllers.Add(tmpController);
            }

            //ランダムに選定
            if (playerControllers.Count < 1) return;
            PlayerController kingController = playerControllers[Random.Range(0, playerControllers.Count)];
            if(team == Team.Red)
            {
                //アウトライン設定
                kingController.ChangeOutlineColorClientRpc(true);
                //ID設定
                redKing = kingController.OwnerClientId;
            } else
            {
                //アウトライン設定
                kingController.ChangeOutlineColorClientRpc(true);
                blueKing = kingController.OwnerClientId;
            }
        }
        //キングを消す
        else
        {
            ulong tmpID = team == Team.Red ? redKing : blueKing;
            NetworkManager.Singleton.ConnectedClients[tmpID].PlayerObject.gameObject.GetComponent<PlayerController>().ChangeOutlineColorClientRpc(false);

        }
    }

    //ゲームモード変更
    [ServerRpc(RequireOwnership = false)] public void ChangeGameModeServerRpc()
    {
        switch(gameMode.Value)
        {
            case GameMode.Normal:
                gameMode.Value = GameMode.King;
                break;

            case GameMode.King:
                gameMode.Value = GameMode.Normal;
                break;

            default:
                gameMode.Value = GameMode.Normal;
                break;
        }
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
                settingString += "Kill";
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

    private void TimeChangeCallBack(int pre, int next)
    {
        UIManager.instance.ChangeTime(next);
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
