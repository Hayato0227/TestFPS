using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public static TitleManager instance;

    [SerializeField] UNetTransport uNetTransport;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //"-launch-as-server"を受け取るとサーバーになる
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-launch-as-server")
            {
                NetworkManager.Singleton.StartServer();
                Debug.Log("Launched As Server!");
                break;
            }
        }
    }

    [SerializeField] private InputField ipadressInputField;
    public void SetIpAddress()
    {
        uNetTransport.ConnectAddress = ipadressInputField.text;
    }

    public string playerName;
    [SerializeField] private InputField playerNameInputField;
    public void SetPlayerName()
    {
        playerName = playerNameInputField.text;
    }
}
