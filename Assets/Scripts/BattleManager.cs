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

    //�Q�[���J�n
    public NetworkVariable<int> battleTime;
    [ServerRpc(RequireOwnership = false)] public void StartGameServerRpc()
    {
        //�o�g���J�n
        if(battlePhase.Value != BattlePhase.Battle)
        {
            StartCoroutine(GameCoroutine());
        }
        //�o�g�����̓o�g���𒆎~����
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

        //�S����]��
        StageManager.Singleton.RespawnClientRpc();

        //�Q�[����
        while(battleTime.Value > 0f)
        {
            battleTime.Value--;
            yield return new WaitForSeconds(1f);
        }

        //�Q�[���I��
        tmp.SendLogServerRpc("Game Over!!");
        StageManager.Singleton.RespawnClientRpc();
        battlePhase.Value = BattlePhase.Lobby;
        ResetPlayerStatus();
    }

    //�L��
    public void Kill(ulong killer, ulong victim)
    {
        PlayerController killerController = NetworkManager.Singleton.ConnectedClients[killer].PlayerObject.gameObject.GetComponent<PlayerController>();
        PlayerController victimController = NetworkManager.Singleton.ConnectedClients[victim].PlayerObject.gameObject.GetComponent<PlayerController>();

        GameManager.instance.SendLogServerRpc(killerController.playerName + " killed " + victimController.playerName + ".");
        Debug.Log(killerController.name + "(" + killer + ")��" + victimController.playerName + "(" + victim + ")���L�����܂����B");

        switch(gameMode.Value)
        {
            //�L���O���[�h
            case GameMode.King:

                break;

            //�f���ɓ��_���Z
            default:
                //�Ԃɓ��_
                if(killerController.team.Value == Team.Red)
                {
                    redTeamPoint.Value++;
                }else if(killerController.team.Value == Team.Blue)
                {
                    blueTeamPoint.Value++;
                }
                break;
        }

        //���񂾐l�̃��X�|�[��
        victimController.RespawnClientRpc();
        victimController.hitPoint.Value = 100f;
    }

    //�Q�[�����[�h�ύX
    [ServerRpc(RequireOwnership = false)] public void ChangeGameModeServerRpc()
    {

    }

    //���ԕύX
    [ServerRpc(RequireOwnership = false)] public void AddTimeServerRpc(int time)
    {
        battleTime.Value += time;
    }

    //�N���C�A���g�̕\���ύX
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

    //�Q�[���I�u�W�F�N�g�̃`�[����ύX
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
            //�`�[�������ɂ��ǂ�
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
