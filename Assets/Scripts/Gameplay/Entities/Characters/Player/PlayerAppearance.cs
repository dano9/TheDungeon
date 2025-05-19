using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAppearance : HumanoidAppearance
{
    public Light2D playerLight;
    public SimpleSpriteAnim slashFX;



    public void OnAttack()
    {
        if (Mathf.Abs(cc.cm.rb.linearVelocity.x) > 0.05f)
        {
            //attackLungeOffset.x = 0.2f*flipM;
        }
        slashFX.spriteRenderer.flipY = !slashFX.spriteRenderer.flipY;
        SFXManager.main.PlaySoundAtPoint("Weapons/Sword/Slash/Light", transform.position, 1, 10, ptTime: 0f);
        slashFX.Play();
    }
}
 