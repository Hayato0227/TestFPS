using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //�R���|�[�l���g
    public PlayerController playerController;

    //�g���I�������ʒu
    protected PlayerController.Place weaponPlace;

    //�g���I�������L�[
    protected string weaponKey;

    //�ő�g���I������������
    protected int maxTrionNum;

    //�g���I�������ɕK�v�ȃg���I����
    protected float trionPointForGeneration;

    //�g���I�������ݒ�p
    public void Initialize(PlayerController playerController, PlayerController.Place place)
    {
        this.playerController = playerController;
        weaponPlace = place;

        weaponKey = weaponPlace == PlayerController.Place.Right ? "RightTrigger" : "LeftTrigger";
    }

    //�g���I�����g����Ƃ�true
    protected bool UseTrion(float useTrion)
    {
        if(playerController.trionPoint >= useTrion)
        {
            playerController.trionPoint -= useTrion;
            return true;
        }
        return false;
    }
}
