using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Netcode.Transports;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public enum GamePhase
    {
        Lobby,
        Wait,
        Battle
    }
    public GamePhase nowPhase = GamePhase.Lobby;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Connect();
    }

    public void StartClient()
    {
        TitleManager.instance.SetIpAddress(UIManager.instance.ipadressInputField.text);
        NetworkManager.Singleton.StartClient();
        Connect();
    }

    private void Connect()
    {
        Debug.Log("接続しました。");
        Cursor.lockState = CursorLockMode.Locked;
        UIManager.instance.ToggleUI(UIManager.UIName.Lobby, false);
        UIManager.instance.ToggleUI(UIManager.UIName.Game, true);
        nowPhase = GamePhase.Wait;
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
        //Application.targetFrameRate = 60;
    }

    void Update()
    {

    }

    public void Respawn(GameObject obj)
    {
        obj.transform.position = new Vector3 (0f, 0f, 0f);
    } 

    //----------チャット用----------
    //サーバーに送る    
    [ServerRpc(RequireOwnership = false)]
    public void SendLogServerRpc(string content)
    {
        LogClientRpc(content);
    }

    //ログ表示
    private Queue<string> logQueue = new Queue<string>();  //ログ用キュー
    [SerializeField] private Text logText;
    [ClientRpc] private void LogClientRpc(string content)
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
}
