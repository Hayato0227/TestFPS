using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassHopperController : WeaponController
{
    public GlassHopperController()
    {
        trionPointForGeneration = 7.5f;
    }

    // Update is called once per frame
    void Update()
    {
        //�󒆂ɂ��邩�ǂ���
        if(!playerController.isGround)
        {
            //�W�����v�������ꂽ�Ƃ�
            if(Input.GetButtonDown("Jump"))
            {
                if(UseTrion(trionPointForGeneration))
                {
                    playerController.TryToGenerateGlassHopper(true);
                }
            }
            //���ɔ�ԂƂ�
            if(Input.GetButtonDown("Crouch"))
            {
                if(UseTrion(trionPointForGeneration))
                {
                    playerController.TryToGenerateGlassHopper(false);
                }
            }

        }
    }
}
