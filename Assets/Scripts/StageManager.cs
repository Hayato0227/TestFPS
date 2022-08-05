using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class StageManager : NetworkBehaviour
{
    //�V���O���g��
    public static StageManager Singleton;

    //�ϐ�
    public NetworkVariable<int> stageNum = new(0);

    //�A�E�g���b�g�ڑ�
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

    //�X�e�[�W�ύX�֐�
    [ServerRpc(RequireOwnership = false)] public void ChangeStageServerRpc()
    {
        stageNum.Value = (stageNum.Value + 1) % AlphaRespawnPoint.Length;
    }
    private void ChangeStageCallBack(int pre, int next)
    {
        securityCameraMeshRender.material = securityCameraMaterial[next];
    }

    //���X�|�[���֐�
    public void Respawn(GameObject obj, BattleManager.Team team)
    {
        //�Q�[�����łȂ��Ƃ��̓X�|�[���n�_��ݒ�
        if (BattleManager.Singleton.battlePhase.Value == BattleManager.BattlePhase.Lobby) obj.transform.position = NoneRespawn.transform.position;
        //�`�[���ɍ��킹�ă��X�|�[��
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
    //�T�[�o�[����N���C�A���g�ւ̈�ă��X�|�[���֐�
    [ClientRpc] public void RespawnClientRpc()
    {
        GameObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        BattleManager.Team tmpTeam = playerObject.GetComponent<PlayerController>().team.Value;
        Respawn(playerObject, tmpTeam);
    }
}
