using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TeamManager : NetworkBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //���\���L�[�������ꂽ��\��
        if (Input.GetButtonDown("Information"))
        {
            UIManager.instance.ToggleUI(UIManager.UIName.Information, true);
        } 
        else if(Input.GetButtonUp("Information")) //�\���L�[�������ꂽ���\��
        {
            UIManager.instance.ToggleUI(UIManager.UIName.Information, false);
        }
    }


}
