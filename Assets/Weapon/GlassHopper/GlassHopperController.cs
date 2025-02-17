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
        //空中にいるかどうか
        if(!playerController.isGround)
        {
            //ジャンプが押されたとき
            if(Input.GetButtonDown("Jump"))
            {
                if (!Physics.Raycast(transform.root.transform.position, transform.root.transform.forward, 0.5f))
                {
                    if (UseTrion(trionPointForGeneration))
                    {
                        playerController.TryToGenerateGlassHopper(true);
                    }
                }
            }
            //下に飛ぶとき
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
