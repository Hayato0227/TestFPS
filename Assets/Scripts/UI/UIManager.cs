using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Netcode;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public enum UIName
    {
        Lobby,
        Information,
        Setting,
        Game
    }

    //UI表示用パネル
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject informationPanel;
    [SerializeField] GameObject settingPanel;
    [SerializeField] GameObject gamePanel;

    [SerializeField] private GameObject targetObject;

    //ゲーム中パネル
    [SerializeField] Slider trionPowerSlider;
    [SerializeField] Slider trionPointSlider;
    [SerializeField] Slider hpSlider;

    //プレイヤーのトリガーUI
    [SerializeField] GameObject[] weaponObjects;
    [SerializeField] Image[] settingWeaponImages;

    //表示時間
    private float trionDuration = 1f;
    private float hpDuration = 1f;

    //武器設定用
    public string chosenWeaponName = null;
    public void setChoseWeapon(string str) { chosenWeaponName = str; }

    //UI用
    public string[] allWeaponName =
    {
        "Weapon", "Asteroid", "Viper", "Haund", "Shield", "Barrier", "Escudo", "GlassHopper", "HookShot"
    };

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //画面初期化
        ToggleUI(UIName.Lobby, true);
        ToggleUI(UIName.Information, false);
        ToggleUI(UIName.Setting, false);
        ToggleUI(UIName.Game, false);


    }

    private void Update()
    {
        if(BattleManager.Singleton.battlePhase.Value == BattleManager.BattlePhase.Lobby)
        {
            //設定キーが押されたら表示
            if (Input.GetButtonDown("Setting"))
            {
                ToggleUI(UIName.Setting, !settingPanel.activeSelf);
                Cursor.lockState = settingPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }  
    }

    public void ToggleUI(UIName uiName, bool flag)
    {
        switch(uiName)
        {
            case UIName.Lobby:
                lobbyPanel.SetActive(flag);
                    break;

            case UIName.Information:
                informationPanel.SetActive(flag);
                break;

            case UIName.Setting:
                settingPanel.SetActive(flag);
                break;

            case UIName.Game:
                gamePanel.SetActive(flag);
                break;
        }
    }
    
    //トリオン使用量GUI変更関数
    public void ChangeTrionPowerGUI(int strength)
    {
        trionPowerSlider.value = strength;
    }

    //トリオン残量表示GUI変更関数
    private float trionPoint = 100f;
    public void ChangeTrionPointGUI(float value)
    {
        if (trionPoint == value) return;
        trionPointSlider.DOValue(value, trionDuration);
        trionPoint = value;
    }

    //ダメージ表示関数
    private float hitPoint = 100f;
    public void ChangeHP(float value)
    {
        if (hitPoint == value) return;

        //HPバーを動かす
        if (Mathf.Abs(hitPoint - value) >= 5) hpSlider.DOValue(value, hpDuration);
        else hpSlider.value = value;

        //ダメージ、回復表示
        if(hitPoint > value)
        {
            //ダメージ


        } else if (hitPoint < value){
            //回復


        }

        hitPoint = value;
    }

    //UI表示関数
    public bool ShowTarget(GameObject obj)
    {
        //ターゲットがないときは非表示
        if(obj == null)
        {
            targetObject.SetActive(false);
            return false;
        }

        //ターゲットマーカーを表示
        targetObject.SetActive(true);
        targetObject.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(obj.transform.localPosition);
        
        //画面内か判定
        if(obj.GetComponent<Renderer>().isVisible)
        {
            return true;
        }
        return false;
    }

    private int previousWeaponNum = -1;
    //武器決定関数
    public void SetWeaponNum(int weaponNum)
    {
        //一つ前の色を変える
        if(previousWeaponNum != -1)
        {
            settingWeaponImages[previousWeaponNum].color = Color.white;
        }
        //決定した色を赤にする
        settingWeaponImages[weaponNum].color = Color.red;

        previousWeaponNum = weaponNum;
    }

    //武器設定関数
    public void SetWeapon(int weaponNum)
    {
        if (previousWeaponNum == -1) return;

        Image img = weaponObjects[weaponNum].transform.Find("Image").gameObject.GetComponent<Image>();
        Text txt = weaponObjects[weaponNum].GetComponentInChildren<Text>();

        img.sprite = Resources.Load<Sprite>("Icons/" + allWeaponName[previousWeaponNum + 1]);
        txt.text = allWeaponName[previousWeaponNum + 1];

        PlayerController tmp = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        //右手、左手のコントローラーをすべて外す
        for(int i = 0; i < 3; i++)
        {
            if (tmp.rightTriggerName[i] != "")
            {
                Destroy(tmp.rightHand.GetComponent(Type.GetType(tmp.rightTriggerName[i] + "Controller")));
            }
            if (tmp.leftTriggerName[i] != "")
            {
                Destroy(tmp.leftHand.GetComponent(Type.GetType(tmp.leftTriggerName[i] + "Controller")));
            }
        }

        //右手
        if (weaponNum >= 3)
        {
            tmp.rightTriggerName[weaponNum - 3] = txt.text;
        } 
        //左手
        else
        {
            tmp.leftTriggerName[weaponNum] = txt.text;
        }
    }

    [SerializeField] private TMP_Text timeText;
    public void ChangeTime(int time)
    {
        timeText.text = time.ToString();
    }

    [SerializeField] private TMP_Text[] teamPointText;
    public void AddTeamPoint(int point, BattleManager.Team team)
    {
        if(team == BattleManager.Team.Red)
        {
            teamPointText[0].text = point.ToString();
        }
        else if(team == BattleManager.Team.Blue)
        {
            teamPointText[1].text = point.ToString();
        }
    }

}