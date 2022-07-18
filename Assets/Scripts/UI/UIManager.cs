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

    //UI�\���p�p�l��
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject informationPanel;
    [SerializeField] GameObject settingPanel;
    [SerializeField] GameObject gamePanel;

    public InputField ipadressInputField;
    [SerializeField] private GameObject targetObject;

    //�Q�[�����p�l��
    [SerializeField] Slider trionPowerSlider;
    [SerializeField] Slider trionPointSlider;
    [SerializeField] Slider hpSlider;

    //�\������
    private float trionDuration = 1f;
    private float hpDuration = 1f;

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
        if(GameManager.instance.nowPhase != GameManager.GamePhase.Lobby)
        {
            //�ݒ�L�[�������ꂽ��\��
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
}
