using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    public InputField ipadressInputField;
    [SerializeField] private GameObject targetObject;

    //ゲーム中パネル
    [SerializeField] Slider trionPowerSlider;
    [SerializeField] Slider trionPointSlider;
    [SerializeField] Slider hpSlider;

    //表示時間
    private float trionDuration = 1f;
    private float hpDuration = 1f;

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
        if(GameManager.instance.nowPhase != GameManager.GamePhase.Lobby)
        {
            //設定キーが押されたら表示
            if (Input.GetButtonDown("Setting"))
            {
                ToggleUI(UIName.Setting, true);
            }
            else if (Input.GetButtonUp("Setting"))
            {
                ToggleUI(UIName.Setting, false);
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
}
