using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.InputSystem;

public class PlayerMovement : CharacterMovement
{
    public PlayerController pc;
    public float dodgeRollForce;
    public float dodgeRollTime;
    public float rollRotateSpeed = 50f;
    bool isDodging;
    float lastDodgeT;

    float beginSprintPressT;

    bool hasAppliedDodgeCapeForce;
    protected override void ControlMovement()
    {
        if (pc.passageOverrideInput != null) {return;}
        
        if (NewInput.controls.Gameplay.Jump.WasPressedThisFrame()) {JumpButtonPressed();}
        else if (isJumping && !jumpReleased && !NewInput.controls.Gameplay.Jump.IsPressed()) {JumpButtonReleased();}
        if (isDodging)
        {
            inputMovement.x = flipM;
            //appearance.localRotation *= Quaternion.Euler(0,0,-rollRotateSpeed*flipM*Time.deltaTime);
            if (Time.time - lastDodgeT > dodgeRollTime || (Time.time - lastDodgeT > dodgeRollTime * 0.77f && Time.time - lastJumpPressT < preJumpTime)) {ExitDodgeRoll();}
            else if (Time.time - lastDodgeT < dodgeRollTime *0.5f) {//cc.ca.appearanceStepOffset = Vector2.MoveTowards(cc.ca.appearanceStepOffset,(Vector2.up*1f) + (Vector2.right*-flipM*0.1f),Time.deltaTime*5f);
            if (Time.time - lastDodgeT > dodgeRollTime *0.25f) { pc.ca.cape.ResetBonePositions(lerpSpeed:20f);}
            }
            else {//cc.ca.appearanceStepOffset = Vector2.MoveTowards(cc.ca.appearanceStepOffset,Vector2.zero,Time.deltaTime*5f);
            if (!hasAppliedDodgeCapeForce) {cc.ca.cape.ApplyForce(new Vector2(-flipM*2, 0f),0.25f);hasAppliedDodgeCapeForce = true;}
            }
        }
        else
        {
            hasAppliedDodgeCapeForce = false;
            //appearance.localScale = new Vector3(appearance.localScale.x,Mathf.MoveTowards(appearance.localScale.y,1,Time.deltaTime*2f),1);

            disableJump = false;
            inputMovement =NewInput.GetMovement(); //new Vector2(ControlsManager.GetAxis("PrimaryXAxis"),ControlsManager.GetAxis("PrimaryYAxis"));//Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            
            isSprinting = NewInput.controls.Gameplay.Sprint.IsPressed();//Input.GetKey(KeyCode.LeftShift);

            
            // if (Input.GetKeyDown("g"))
            // {
            //     rb.AddForce(jumpForce*1* ((Vector2.right*-flipM) + (Vector2.up*0.1f)));
            // }

            if (NewInput.controls.Gameplay.Dodge.WasPressedThisFrame()) {beginSprintPressT = Time.time;}
            if (NewInput.controls.Gameplay.Dodge.WasReleasedThisFrame() && Time.time - beginSprintPressT < 0.42f)//Input.GetKeyDown(KeyCode.LeftControl))
            {
                DodgeRoll();
            }
        }
        ManageLookTarget();
    }
    public void DodgeRoll()
    {
        cc.readyForAttack = false;
        //rb.velocity = Vector2.zero;
        if (rb.linearVelocity.x * flipM < moveSpeed * 0.6f) { rb.linearVelocity = new Vector2(moveSpeed * 0.6f * flipM, rb.linearVelocity.y); }
            //rb.velocity = new Vector2(rb.velocity.x * -0.8f,rb.velocity.y);}
        lastDodgeT = Time.time;
        isDodging = true;
        //disableJump = true;
        rb.AddForce((Vector2.right * flipM * dodgeRollForce) + (Vector2.up * jumpForce*0.1f));
        rolling = true;
        //appearance.localScale = new Vector3(appearance.localScale.x,0.85f,1);
        cc.ca.cape.ApplyForce(new Vector2(-flipM*10, 1f),0.15f);
        if (cc.ca != null) {cc.ca.OnDodge();}
    }
    public void ExitDodgeRoll()
    {
        isDodging = false; 
        rolling = false; 
        //appearance.localRotation = Quaternion.identity;
        //appearance.localScale = new Vector3(appearance.localScale.x,0.65f,1);
        if (cc.ca != null) {cc.ca.OnExitDodge();}
        cc.readyForAttack = true;
        //appearance.localScale = Vector3.one;
    }
    public void ManageLookTarget()
    {
        if (inputMovement != Vector2.zero)
        {
            Vector2 targLookPos = new  Vector2(inputMovement.x * 4f,(inputMovement.y * 4f));
            if (inputMovement.x != 0) {targLookPos.y = 0;}
            pc.lookTarget.localPosition = Vector3.MoveTowards(pc.lookTarget.localPosition,targLookPos, Time.deltaTime*4f * (isSprinting ? 1.4f : 1));
        }
        else
        {
            pc.lookTarget.localPosition = Vector3.MoveTowards(pc.lookTarget.localPosition,Vector3.zero, Time.deltaTime*2f);
        }
    }
    protected override void Jump()
    {
        base.Jump();
        cc.ca.cape.ApplyForce(new Vector2(-flipM*0.1f, 0.5f),0.25f);
    }
}
