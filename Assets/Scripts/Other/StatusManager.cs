using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class StatusManager : MonoBehaviour
{
    //設定
    [SerializeField] private GUIStyle textStyle;

    //コンポーネント
    [SerializeField] private UNetTransport uNetTransport;

    //変数
    private int i = 0;
    private int fps = 0;

    private void OnGUI()
    {
        //100回に1回FPS計算
        i++;
        if (i % 100 == 0) fps = (int)(1f / Time.deltaTime);
        
        //fps表示 
        GUI.Label(new Rect(5, 0, 100, 10), "FPS：" + fps, textStyle);

        //トリオン数表示
        GUI.Label(new Rect(5, 10, 100, 10), "トリオン数：" + GameObject.FindGameObjectsWithTag("Trion").Length, textStyle);

        //Ping表示
        if(NetworkManager.Singleton.IsConnectedClient)
        {
            GUI.Label(new Rect(5, 20, 100, 10), "Ping：" + uNetTransport.GetCurrentRtt(NetworkManager.ServerClientId) + "ms", textStyle);
        }
    }
}
