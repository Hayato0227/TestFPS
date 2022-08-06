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

    //�Q�[���J�n
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

        //�S����]��
        StageManager.Singleton.RespawnClientRpc();

        //�L���O��ݒ�
        if(gameMode.Value == GameMode.King)
        {
            SetNewKing(Team.Red, true);
            SetNewKing(Team.Blue, true);
        }

        //�Q�[����
        while(battleTime.Value > 0f)
        {
            battleTime.Value--;
            yield return new WaitForSeconds(1f);
        }

        //�Q�[���I��
        tmp.SendLogServerRpc("Game Over");
        GameManager.instance.PlayerSEClientRpc(0);
        battlePhase.Value = BattlePhase.Lobby;

        //�L���O����
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

    //�L��
    public void Kill(ulong killer, ulong victim)
    {
        PlayerController killerController = NetworkManager.Singleton.ConnectedClients[killer].PlayerObject.gameObject.GetComponent<PlayerController>();
        PlayerController victimController = NetworkManager.Singleton.ConnectedClients[victim].PlayerObject.gameObject.GetComponent<PlayerController>();

        GameManager.instance.SendLogServerRpc(killerController.playerName.Value.ToString() + " killed " + victimController.playerName.Value.ToString() + ".");
        Debug.Log(killerController.playerName.Value.ToString() + "(" + killer + ")��" + victimController.playerName.Value.ToString() + "(" + victim + ")���L�����܂����B");

        switch(gameMode.Value)
        {
            //�L���O���[�h
            case GameMode.King:
                if(victimController.OwnerClientId == redKing || victimController.OwnerClientId == blueKing)
                {
                    //�L���O�̏ꍇ�͓��_�����Z
                    if (killerController.team.Value == Team.Red)
                    {
                        redTeamPoint.Value++;
                    }
                    else if(killerController.team.Value == Team.Blue)
                    {
                        blueTeamPoint.Value++;
                    }

                    //�L���O��ݒ�
                    SetNewKing(victimController.team.Value, true);
                }
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
    /// <summary>
    /// �L���O�����֐�
    /// </summary>
    /// <param name="team">�ݒ肷��`�[��</param>
    /// <param name="flag">true:�V�����ݒ肷��, false:��������</param>
    private void SetNewKing(Team team, bool flag)
    {
        if (team == Team.None) return;

        //�V�����L���O��ݒ�
        if(flag)
        {
            //��O��King������
            NetworkManager.Singleton.ConnectedClients[team == Team.Red ? redKing : blueKing].PlayerObject.GetComponent<PlayerController>().ChangeOutlineColorClientRpc(false);

            List<PlayerController> playerControllers = new();
            foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                PlayerController tmpController = player.PlayerObject.GetComponent<PlayerController>();
                if (tmpController.team.Value == team) playerControllers.Add(tmpController);
            }

            //�����_���ɑI��
            if (playerControllers.Count < 1) return;
            PlayerController kingController = playerControllers[Random.Range(0, playerControllers.Count)];
            if(team == Team.Red)
            {
                //�A�E�g���C���ݒ�
                kingController.ChangeOutlineColorClientRpc(true);
                //ID�ݒ�
                redKing = kingController.OwnerClientId;
            } else
            {
                //�A�E�g���C���ݒ�
                kingController.ChangeOutlineColorClientRpc(true);
                blueKing = kingController.OwnerClientId;
            }
        }
        //�L���O������
        else
        {
            ulong tmpID = team == Team.Red ? redKing : blueKing;
            NetworkManager.Singleton.ConnectedClients[tmpID].PlayerObject.gameObject.GetComponent<PlayerController>().ChangeOutlineColorClientRpc(false);

        }
    }

    //�Q�[�����[�h�ύX
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
