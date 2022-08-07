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

    private bool flag = false;
    // Update is called once per frame
    void Update()
    {
        //情報表示キーが押されたら表示
        if (Input.GetButtonDown("Information"))
        {
            flag = !flag;
            UIManager.instance.ToggleUI(UIManager.UIName.Information, flag);
            Cursor.lockState = flag ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }


}
