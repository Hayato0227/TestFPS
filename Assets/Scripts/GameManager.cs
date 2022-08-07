using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Netcode.Transports;
using DG.Tweening;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public AudioSource audioSource;

    public void StartHost()
    {
        TitleManager.instance.SetPlayerName();
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        TitleManager.instance.SetIpAddress();
        TitleManager.instance.SetPlayerName();
        NetworkManager.Singleton.StartClient();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("接続しました。");
        Cursor.lockState = CursorLockMode.Locked;
        UIManager.instance.ToggleUI(UIManager.UIName.Lobby, false);
        UIManager.instance.ToggleUI(UIManager.UIName.Game, true);
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        RemoveCallbacks();
    }

    public void SetUp()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void RemoveCallbacks()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void OnClientConnect(ulong clientId)
    {
        Debug.Log("Client : " + clientId + " joined.");
    }
    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log("Client : " + clientId + " disconnected.");
    }    

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        PlayerBGM(BattleManager.BattlePhase.Lobby);
    }

    void Update()
    {
        if(Input.GetButtonDown("FullScreen"))
        {
            if(!Screen.fullScreen)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemWidth, true);
            }
            else
            {
                Screen.fullScreen = false;
            }
        }
    }

    //----------チャット用----------
    //サーバーに送る    
    [ServerRpc(RequireOwnership = false)]
    public void LogServerRpc(string content)
    {
        LogClientRpc(content);
    }

    //ログ表示
    private Queue<string> logQueue = new Queue<string>();  //ログ用キュー
    [SerializeField] private Text logText;
    [ClientRpc] public void LogClientRpc(string content)
    {
        //キューに登録
        logQueue.Enqueue(content);

        //長いときは一つ目を消す
        if (logQueue.Count > 5)
        {
            logQueue.Dequeue();
        }

        //ログ出力文字決定
        string tmpString = "";
        foreach (var item in logQueue)
        {
            tmpString += item + "\r\n";
        }

        //ログ反映
        logText.text = tmpString;
    }

    [SerializeField] private AudioClip[] clips;
    public void PlayerBGM(BattleManager.BattlePhase phase)
    {
        int audioClipNum = phase == BattleManager.BattlePhase.Lobby ? 0 : 1;
        audioSource.DOFade(0f, 1.5f).OnComplete(() =>
        {
            audioSource.clip = clips[audioClipNum];
            audioSource.Play();
            audioSource.DOFade(0.2f, 1.5f);
        });
    }
    [ClientRpc] public void PlayBGMClientRpc(BattleManager.BattlePhase phase)
    {
        PlayerBGM(phase);
    }

    public AudioClip[] seClips;
    [ClientRpc] public void PlayerSEClientRpc(int num)
    {
        audioSource.PlayOneShot(seClips[num], 1f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
