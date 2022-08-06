using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //コンポーネント
    public PlayerController playerController;

    //トリオン生成位置
    protected PlayerController.Place weaponPlace;

    //トリオン生成キー
    protected string weaponKey;

    //最大トリオン同時生成量
    protected int maxTrionNum;

    //トリオン生成に必要なトリオン量
    protected float trionPointForGeneration;

    //トリオン初期設定用
    public void Initialize(PlayerController playerController, PlayerController.Place place)
    {
        this.playerController = playerController;
        weaponPlace = place;

        weaponKey = weaponPlace == PlayerController.Place.Right ? "RightTrigger" : "LeftTrigger";
    }

    //トリオンが使えるときtrue
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
