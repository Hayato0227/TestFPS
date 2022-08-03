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
        //‹ó’†‚É‚¢‚é‚©‚Ç‚¤‚©
        if(!playerController.isGround)
        {
            //ƒWƒƒƒ“ƒv‚ª‰Ÿ‚³‚ê‚½‚Æ‚«
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
            //‰º‚É”ò‚Ô‚Æ‚«
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
