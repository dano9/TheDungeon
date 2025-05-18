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
    public Transform heldItemPoser;
    public SpriteRenderer helmetSR;
    public SpriteRenderer torsoClothesSR;
    public SpriteRenderer headSR;
    public SpriteRenderer torsoSR;
    public SpriteRenderer feetSR;
    Vector2 normalHeadLoc;

    public int animFeetIndex0;
    public int animFeetIndex1;
    public int animFeetSheetIndex;
    public Vector2 animHeadOffset;
    public Vector2 animHeldItemOffset;
    public float animHeldItemRotation;

    public FeetSheet[] feetSheets;

    void Awake()
    {
        normalHeadLoc = headSR.transform.localPosition;
    }

    public override void ManageAppearance()
    {
        if (!Application.isPlaying)
        {
            ManageAnimation();
            return;
        }
        ManageClothing();
        ManageAnimation();
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
    public void ManageAnimation()
    {
        heldItemPoser.localPosition = animHeldItemOffset;
        heldItemPoser.localRotation = Quaternion.Euler(0, 0, animHeldItemRotation);
        headPoser.localPosition = animHeadOffset;
        if (animFeetSheetIndex >= 0 && (animFeetIndex1 >= 0 || animFeetIndex0 >= 0))
        {
            feetSR.sprite = feetSheets[animFeetSheetIndex].sprites[animFeetIndex1 >= 0 ? animFeetIndex1 : animFeetIndex0];
        }
        if (!cc.cm.isJumpRising) { anim.SetBool("IsJumping", false); }
        bool isMoving = Mathf.Abs(cc.cm.inputMovement.x) > 0.1f;
        bool isSprinting = false;
        bool isWalking = isMoving; if (cc.cm.isSprinting && isMoving) { isSprinting = true; isWalking = false; }
        anim.SetBool("IsWalking",isWalking);
        anim.SetBool("IsSprinting",isSprinting);
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
