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

    //UI�\���p�p�l��
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject informationPanel;
    [SerializeField] GameObject settingPanel;
    [SerializeField] GameObject gamePanel;

    [SerializeField] private GameObject targetObject;

    //�Q�[�����p�l��
    [SerializeField] Slider trionPowerSlider;
    [SerializeField] Slider trionPointSlider;
    [SerializeField] Slider hpSlider;

    //�v���C���[�̃g���K�[UI
    [SerializeField] GameObject[] weaponObjects;
    [SerializeField] Image[] settingWeaponImages;

    //�\������
    private float trionDuration = 1f;
    private float hpDuration = 1f;

    //����ݒ�p
    public string chosenWeaponName = null;
    public void setChoseWeapon(string str) { chosenWeaponName = str; }

    //UI�p
    public string[] allWeaponName =
    {
        "Weapon", "Asteroid", "Viper", "Haund", "Shield", "Barrier", "Escudo", "GlassHopper", "HookShot"
    };

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //��ʏ�����
        ToggleUI(UIName.Lobby, true);
        ToggleUI(UIName.Information, false);
        ToggleUI(UIName.Setting, false);
        ToggleUI(UIName.Game, false);


    }

    private void Update()
    {
        if(BattleManager.Singleton.battlePhase.Value == BattleManager.BattlePhase.Lobby)
        {
            //�ݒ�L�[�������ꂽ��\��
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
    
    //�g���I���g�p��GUI�ύX�֐�
    public void ChangeTrionPowerGUI(int strength)
    {
        trionPowerSlider.value = strength;
    }

    //�g���I���c�ʕ\��GUI�ύX�֐�
    private float trionPoint = 100f;
    public void ChangeTrionPointGUI(float value)
    {
        if (trionPoint == value) return;
        trionPointSlider.DOValue(value, trionDuration);
        trionPoint = value;
    }

    //�_���[�W�\���֐�
    private float hitPoint = 100f;
    public void ChangeHP(float value)
    {
        if (hitPoint == value) return;

        //HP�o�[�𓮂���
        if (Mathf.Abs(hitPoint - value) >= 5) hpSlider.DOValue(value, hpDuration);
        else hpSlider.value = value;

        //�_���[�W�A�񕜕\��
        if(hitPoint > value)
        {
            //�_���[�W


        } else if (hitPoint < value){
            //��


        }

        hitPoint = value;
    }

    //UI�\���֐�
    public bool ShowTarget(GameObject obj)
    {
        //�^�[�Q�b�g���Ȃ��Ƃ��͔�\��
        if(obj == null)
        {
            targetObject.SetActive(false);
            return false;
        }

        //�^�[�Q�b�g�}�[�J�[��\��
        targetObject.SetActive(true);
        targetObject.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(obj.transform.localPosition);
        
        //��ʓ�������
        if(obj.GetComponent<Renderer>().isVisible)
        {
            return true;
        }
        return false;
    }

    private int previousWeaponNum = -1;
    //���팈��֐�
    public void SetWeaponNum(int weaponNum)
    {
        //��O�̐F��ς���
        if(previousWeaponNum != -1)
        {
            settingWeaponImages[previousWeaponNum].color = Color.white;
        }
        //���肵���F��Ԃɂ���
        settingWeaponImages[weaponNum].color = Color.red;

        previousWeaponNum = weaponNum;
    }

    //����ݒ�֐�
    public void SetWeapon(int weaponNum)
    {
        if (previousWeaponNum == -1) return;

        Image img = weaponObjects[weaponNum].transform.Find("Image").gameObject.GetComponent<Image>();
        Text txt = weaponObjects[weaponNum].GetComponentInChildren<Text>();

        img.sprite = Resources.Load<Sprite>("Icons/" + allWeaponName[previousWeaponNum + 1]);
        txt.text = allWeaponName[previousWeaponNum + 1];

        PlayerController tmp = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        //�E��A����̃R���g���[���[�����ׂĊO��
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

        //�E��
        if (weaponNum >= 3)
        {
            tmp.rightTriggerName[weaponNum - 3] = txt.text;
        } 
        //����
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