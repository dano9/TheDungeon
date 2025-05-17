using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAppearance : CharacterAppearance
{
    public Light2D playerLight;
    public SimpleSpriteAnim slashFX;

    public bool headClothed;
    public bool torsoClothed;
    public bool legsClothed;

    public Color bootsColor;
    public Color feetColorl;
    public SpriteRenderer helmetSR;
    public SpriteRenderer torsoClothesSR;
    public SpriteRenderer headSR;
    public SpriteRenderer torsoSR;
    public SpriteRenderer feetSR;
    Vector2 normalHeadLoc;
    void Awake()
    {
        normalHeadLoc = headSR.transform.localPosition;
    }

    public override void ManageAppearance()
    {
        ManageClothing();
        base.ManageAppearance();
    }
    public void ManageClothing()
    {
        headSR.transform.localPosition = normalHeadLoc + (!headClothed ? Vector2.right * 0.1f : Vector2.zero);

        if (helmetSR.enabled != headClothed) { helmetSR.enabled = headClothed; }
        if (torsoClothesSR.enabled != torsoClothed) { torsoClothesSR.enabled = torsoClothed; }
        Color feetCol = legsClothed ? bootsColor : feetColorl;
        if (feetSR.color != feetCol) { feetSR.color = feetCol; }
        
    }
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
 