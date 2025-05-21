using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    public CharacterController cc;
    public Transform appearanceTrans;
    public Cape cape;
    public CharacterAnimData animData;
    public Animator anim;
    public float flipM;

    protected float movePerc;
    protected float fallPerc;
    protected float jumpPerc;
    protected float yVelocPerc;
    protected float jogYOffset;
    protected float jogYOffsetRound;
    public Vector2 appearanceStepOffset;
    public Vector2 appearanceOffset;
    public Vector2 attackLungeOffset;
    Sound footstepsSource;
    protected SpriteRenderer[] spriteRenderers;

    protected virtual void Awake()
    {
        UpdateSRList();
    }
    public void LateUpdate()
    {
        ManageAppearance();
        if (!Application.isPlaying) { return; }
        ManageSFX();

        foreach (SpriteRenderer sr in spriteRenderers)
        { }
        //PixelSnapRelativeSR(sr); }
    }
    public void UpdateSRList()
    {
        spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
    }
    protected void PixelSnapRelativeSR(SpriteRenderer relSR, int pixelsPerUnit = 10)
    {
        if (relSR == null || relSR.sprite == null)
            return;

        Vector2 unitsPerPixel = Vector2.one / pixelsPerUnit;
        unitsPerPixel.x /= transform.localScale.x;
        //unitsPerPixel.y /= transform.localScale.y;

        // Step 1: Get local offset of child relative to parent
        Vector2 localOffset = transform.InverseTransformPoint(relSR.transform.position);

        // Step 2: Snap local offset to nearest pixel in local space
        Vector2 snappedLocalOffset = new Vector2(
            Mathf.Round(localOffset.x / unitsPerPixel.x) * unitsPerPixel.x,
            Mathf.Round(localOffset.y / unitsPerPixel.y) * unitsPerPixel.y
        );

        // Step 3: Convert snapped local offset back to world position
        Vector2 snappedWorldPos = transform.TransformPoint(snappedLocalOffset);

        // Step 4: Apply snapped position
        relSR.transform.position = snappedWorldPos;
    }
    bool wasFacingLeft;
    float lastGT;
    bool wasOnG;
    public virtual void ManageAppearance()
    {
        flipM = (cc.facingLeft ? -1 : 1);
        movePerc = Mathf.Clamp01(Mathf.Abs(cc.rb.linearVelocity.x) / cc.cm.moveSpeed);
        fallPerc = Mathf.Clamp01(-cc.rb.linearVelocity.y / cc.cm.maxVelocity);
        jumpPerc = Mathf.Clamp01(cc.rb.linearVelocity.y / cc.cm.maxVelocity);
        yVelocPerc = Mathf.Clamp(cc.rb.linearVelocity.y / cc.cm.maxVelocity, -1, 1);
        jogYOffset = 0f;
        jogYOffsetRound = 0f; ;
        if (cc.cm.isSprinting && cc.cm.inputMovement.x != 0)
        {
            jogYOffset = (Mathf.Sin(Time.time * 10f) + 1f) * 0.5f;
            jogYOffsetRound = Mathf.Round(jogYOffset) * 0.1f; jogYOffset *= 0.1f;
        }

        appearanceTrans.localScale = new Vector3(flipM, appearanceTrans.localScale.y, 1);
        //appearanceOffset.y = jogYOffsetRound;
        //appearanceOffset.x = jogYOffsetRound * flipM;
        ApplyOffset(true);
        ManageCape();
        wasFacingLeft = cc.facingLeft;


        bool onGround = cc.cm.onGround;
        if (onGround != wasOnG)
        {
            lastGT = Time.time;
            wasOnG = onGround;
        }
        //if (Time.time - lastGT > 0.05f)
        { anim.SetBool("OnGround", onGround);}
    }

    public void ManageCape()
    {
        if (wasFacingLeft != cc.facingLeft) {cape.ApplyForce(new Vector2(-flipM*2f, 0.2f),0.2f);}
        //float jogYOffset = cm.isSprinting && movePerc>0 ? ((Mathf.Sin(Time.time*10f))+1f)*0.5f: 0;
        bool facingWind = (flipM * cape.windDirection.x < 0);
        cape.windMulti = facingWind ? 1 : 0.2f;
        cape.isGrounded = cc.cm.onGround; cape.groundLevel = cc.cm.groundLevel;
        
        float windPerc = Mathf.Clamp01(1-(cape.windStrength*1.3f)* Mathf.Abs(cape.windDirection.x));
        cape.rigidityMultipllier = cc.cm.rolling ? 0 : Mathf.Clamp01(fallPerc + ((1-movePerc)*0.2f * (1- windPerc)));
        
        cape.offset.x = -0.17f * flipM * (1-movePerc) * windPerc;
        cape.shrinkMultiplier = cc.cm.rolling ? -0.5f : (-0.1f*movePerc * (cc.cm.isSprinting ? 2 : 1));
        cape.keepRootVelocityXMultiplier = Mathf.Clamp((1-(movePerc * (cc.cm.isSprinting ? 2 : 1)*0.5f)) * 1.5f,0,1);
        //cape.offset.x = cape.rootOffset.x;
        //cape.sR.sortingOrder = !facingWind ? 1 : 3;
        
        float yOffset = ((0.1f-jogYOffset)*-3f)+(cc.cm.isSprinting && cc.cm.inputMovement.x != 0 ? -0.2f : -0.03f);
        cape.rootOffset.y = yOffset;// + (yVelocPerc*0.4f * (yVelocPerc > 1 ? 4 : 1));
        cape.rootOffset.x = jogYOffsetRound*flipM;
        cape.offset.y = Mathf.Lerp(cape.offset.y,0.3f, Time.deltaTime * (cc.cm.isJumping ? 1 : 0) * 20f * (1-(movePerc/2)));
        cape.keepRootVelocityYMultiplier = jumpPerc * (1-movePerc) * 1f;
        //Mathf.Clamp((jogYOffset+0.25f)*0.1f,0,0.1f);
    }
    public void ApplyOffset(bool lerp=false)
    {
        attackLungeOffset = Vector2.MoveTowards(attackLungeOffset,Vector2.zero,Time.deltaTime*1f);
        if (!cc.cm.rolling && lerp) { appearanceStepOffset = Vector2.MoveTowards(appearanceStepOffset, Vector2.zero, Time.deltaTime * 3f); }
        appearanceTrans.localPosition = appearanceStepOffset + appearanceOffset + attackLungeOffset + new Vector2(animData.animOffset.x*flipM,animData.animOffset.y);
    }
    float footstepWalkSFXT=0; float footstepRunSFXT=0;
    bool footstepsRunning =false;
    string ftstepsWalkSFXPath = "Footsteps/Walking/";
    string ftstepsRunSFXPath = "Footsteps/Running/";
    public void ManageSFX()
    {
        if (cc.cm.onGround && cc.rb.linearVelocity.x != 0)
        {
            if (footstepsSource == null || footstepsRunning != cc.cm.isSprinting) {footstepsSource = SFXManager.main.PlaySoundAtPoint((cc.cm.isSprinting?ftstepsRunSFXPath : ftstepsWalkSFXPath)+"Stone",transform.position,10,8,true,footstepsSource,ptTime:(footstepsRunning ? footstepRunSFXT: footstepWalkSFXT));}
            footstepsRunning = cc.cm.isSprinting;
            
            //footstepsSource.volume = 10f;
        }
        //if (footstepsSource == null) {footstepsSource = SFXManager.main.PlaySoundAtPoint("Footsteps","Stone",transform.position,10,8,true,footstepsSource);}
        else if (footstepsSource != null)
        {
            if (footstepsRunning) {footstepRunSFXT=footstepsSource.source.time;} else {footstepWalkSFXT=footstepsSource.source.time;}
            footstepsSource=SFXManager.main.DeactivateSound(footstepsSource);
            // if (Time.time - lastFootstepSFXT > 0.1f || cc.cm.inputMovement.x ==0)
            // {footstepsSource=SFXManager.main.DeactivateSound(footstepsSource);}
            // else {footstepsSource.volume = 1f;}
        }//,true,1f);}
    }


    public void SuddenTeleport(Vector2 deltaPos)
    {
        cape.ResetBonePositions();
        //cape.MoveCape(deltaPos);
    }
    public virtual void OnJump()
    {
        SFXManager.main.PlaySoundAtPoint("Jump/Stone",transform.position,1,10,ptTime:0.05f);
    }
    public virtual void OnDodge()
    {
        SFXManager.main.PlaySoundAtPoint("Dodge/Default",transform.position,1,10);
    }
    public virtual void OnExitDodge()
    {
        if (cc.cm.onGround) {SFXManager.main.PlaySoundAtPoint("Land/Stone",transform.position,1,9,ptTime:0.003f);}
    }
    public virtual void OnLand()
    {
        SFXManager.main.PlaySoundAtPoint("Land/Stone",transform.position,1,9,ptTime:0.003f);
    }
}
