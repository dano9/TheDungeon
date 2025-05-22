using UnityEngine;

[System.Serializable]
public struct FeetSheet
{
    public string sheetName;
    public Sprite[] sprites;
}
[ExecuteInEditMode]
public class HumanoidAppearance : CharacterAppearance
{

    public bool headClothed;
    public bool torsoClothed;
    public bool legsClothed;
    

    public Color bootsColor;
    public Color feetColorl;
    public Transform headPoser;
    public Transform torsoPoser;
    public Transform heldItemPoser;
    public SpriteRenderer helmetSR;
    public SpriteRenderer torsoClothesSR;
    public SpriteRenderer headSR;
    public SpriteRenderer torsoSR;
    public SpriteRenderer feetSR;
    Vector2 normalHeadLoc;
    


    public FeetSheet[] feetSheets;

    protected override void Awake()
    {
        base.Awake();
        normalHeadLoc = headSR.transform.localPosition;
    }

    public override void ManageAppearance()
    {
        ManageFrontalFacing();

        if (!Application.isPlaying)
        {
            ManageAnimation();
            return;
        }
        ManageClothing();
        ManageAnimation();
        base.ManageAppearance();
    }
    Vector2 headHIOffset;
    Vector2 headHIScale=Vector2.one;
    float headHIRot;
    
    public void ManageFrontalFacing()
    {
        HeldItemAnimData hiAD = (cc.curHeldItem != null && cc.curHeldItem.animData != null) ? (HeldItemAnimData)cc.curHeldItem.animData : null;
        flipHead = (hiAD != null && hiAD.flipHeadFront);
        flipTorso = (hiAD != null && hiAD.flipTorsoFront);
        flipFront = (hiAD != null && hiAD.flipBodyFront);
        flipBodyLeftHI = (hiAD != null && hiAD.flipBodyLeft);

        headHIOffset = hiAD != null ? hiAD.headOffset : Vector2.zero;
        bodyHIOffset = hiAD != null ? hiAD.bodyOffset : Vector2.zero;
        headHIScale = hiAD != null ? hiAD.headScale : Vector2.one;
        bodyHIScale = hiAD != null ? hiAD.bodyScale : Vector2.one;
        headHIRot = hiAD != null ? hiAD.headRotation : 0;
        bodyHIRot = hiAD != null ? hiAD.bodyRotation : 0;
    }
    public void ManageClothing()
    {
        headSR.transform.localPosition = normalHeadLoc + (!headClothed ? Vector2.right * 0.1f : Vector2.zero);
        headSR.color = headClothed && (flipFront ^ flipHead) ? bootsColor : Color.white;

        if (helmetSR.enabled != headClothed) { helmetSR.enabled = headClothed; }
        if (torsoClothesSR.enabled != torsoClothed) { torsoClothesSR.enabled = torsoClothed; }
        Color feetCol = legsClothed ? bootsColor : feetColorl;
        if (feetSR.color != feetCol) { feetSR.color = feetCol; }
    }
    public void ManageAnimation()
    {
        HumanoidAnimData hAD = (HumanoidAnimData)animData;

        heldItemPoser.localPosition = hAD.animHeldItemOffset;
        heldItemPoser.localRotation = Quaternion.Euler(0, 0, hAD.animHeldItemRotation);
        headPoser.localPosition = headHIOffset + hAD.animHeadOffset + (Vector2.right * (!torsoClothed && headClothed ? 0.1f : 0));
        headPoser.localScale = new Vector3(headHIScale.x, headHIScale.y, 1);
        headPoser.localRotation = Quaternion.Euler(0, 0, headHIRot);
        //transform.localRotation = Quaternion.Euler(0, 0, bodyHIRot);
        //transform.localScale = bodyHIScale;
        if (hAD.animFeetSheetIndex >= 0 && (hAD.animFeetIndex1 >= 0 || hAD.animFeetIndex0 >= 0))
        {
            feetSR.sprite = feetSheets[hAD.animFeetSheetIndex].sprites[hAD.animFeetIndex1 >= 0 ? hAD.animFeetIndex1 : hAD.animFeetIndex0];
        }
        if (!cc.cm.isJumpRising) { anim.SetBool("IsJumping", false); }
        bool isMoving = Mathf.Abs(cc.cm.inputMovement.x) > 0.1f;
        bool isSprinting = false;
        bool isWalking = isMoving; if (cc.cm.isSprinting && isMoving) { isSprinting = true; isWalking = false; }
        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("IsSprinting", isSprinting);
    }
    public override void OnJump()
    {
        base.OnJump();
        anim.SetBool("IsJumping", true);
        anim.SetTrigger("Jump");
    }
    public override void OnLand()
    {
        base.OnLand();
        anim.SetTrigger("Land");
    }

}

