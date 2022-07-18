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
        Debug.Log("�ڑ����܂����B");
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

    //----------�`���b�g�p----------
    //�T�[�o�[�ɑ���    
    [ServerRpc(RequireOwnership = false)]
    public void SendLogServerRpc(string content)
    {
        LogClientRpc(content);
    }

    //���O�\��
    private Queue<string> logQueue = new Queue<string>();  //���O�p�L���[
    [SerializeField] private Text logText;
    [ClientRpc] private void LogClientRpc(string content)
    {
        //�L���[�ɓo�^
        logQueue.Enqueue(content);

        //�����Ƃ��͈�ڂ�����
        if (logQueue.Count > 5)
        {
            logQueue.Dequeue();
        }

        //���O�o�͕�������
        string tmpString = "";
        foreach (var item in logQueue)
        {
            tmpString += item + "\r\n";
        }

        //���O���f
        logText.text = tmpString;
    }
}
