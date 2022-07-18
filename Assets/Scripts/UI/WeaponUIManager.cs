using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WeaponUIManager : MonoBehaviour
{
    public static WeaponUIManager instance;

    //使用パネル
    [SerializeField] GameObject[] rightPanels = new GameObject[3];
    [SerializeField] GameObject[] leftPanels = new GameObject[3];

    private void Start()
    {
        instance = this;   
    }

    private float[] weaponPositionArray = { -100, 0, 100 };
    public void ChangeWeapon(int nextNum, PlayerController.Place place)
    {
        if(place == PlayerController.Place.Right)
        {
            for(int i = 0; i < rightPanels.Length; i++)
            {
                rightPanels[i].transform.DOLocalMoveY(weaponPositionArray[(i - nextNum + 3) % 3], 0.3f);
            }

        } else if(place == PlayerController.Place.Left)
        {
            for (int i = 0; i < leftPanels.Length; i++)
            {
                leftPanels[i].transform.DOLocalMoveY(weaponPositionArray[(i - nextNum + 3) % 3], 0.3f);
            }
        }
    }
}
