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
        //情報表示キーが押されたら表示
        if (Input.GetButtonDown("Information"))
        {
            UIManager.instance.ToggleUI(UIManager.UIName.Information, true);
        } 
        else if(Input.GetButtonUp("Information")) //表示キーが離されたら非表示
        {
            UIManager.instance.ToggleUI(UIManager.UIName.Information, false);
        }
    }


}
