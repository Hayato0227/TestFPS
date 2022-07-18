using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class StatusManager : MonoBehaviour
{
    //�ݒ�
    [SerializeField] private GUIStyle textStyle;

    //�R���|�[�l���g
    [SerializeField] private UNetTransport uNetTransport;

    //�ϐ�
    private int i = 0;
    private int fps = 0;

    private void OnGUI()
    {
        //100���1��FPS�v�Z
        i++;
        if (i % 100 == 0) fps = (int)(1f / Time.deltaTime);
        
        //fps�\�� 
        GUI.Label(new Rect(5, 0, 100, 10), "FPS�F" + fps, textStyle);

        //�g���I�����\��
        GUI.Label(new Rect(5, 10, 100, 10), "�g���I�����F" + GameObject.FindGameObjectsWithTag("Trion").Length, textStyle);

        //Ping�\��
        if(NetworkManager.Singleton.IsConnectedClient)
        {
            GUI.Label(new Rect(5, 20, 100, 10), "Ping�F" + uNetTransport.GetCurrentRtt(NetworkManager.ServerClientId) + "ms", textStyle);
        }
    }
}
