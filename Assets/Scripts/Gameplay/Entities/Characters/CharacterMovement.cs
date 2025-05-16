using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public CharacterController cc;
    public Rigidbody2D rb;
    public Transform appearance;
    public Vector2 size;
    public Vector2 inputMovement;


    public LayerMask groundLM;
    public LayerMask detectLM;
    public float moveSpeed = 1f;
    public float moveAccel = 1f;
    public float moveDecel = 1f;
    public float ladderClimbSpeed=1f;
    public float jumpForce = 10f;
    public float jumpPullDownForce=20f;
    public float jumpHoverTime=0.05f;
    public float coyoteTime=0.1f;
    public float preJumpTime=0.1f;
    public bool isSliding;
    public bool onGround;
    public bool disableJump;
    public bool rolling;
    public bool isSprinting;
    public float sprintMultiplier=2;

    protected float lastJumpT = 0;
    protected float lastJumpPressT=-10;
    protected float normGravScale;
    protected float lastGroundT;
    protected Vector2 targMoveVeloc;
    protected float lastMadeStep;
    protected bool touchingLadder;
    protected bool onLadder;
    protected Transform ladder;
    protected bool climbingLadder;
    protected ContactPoint2D[] cPoints;
    protected Vector2 groundNormal;
    protected int flipM;
    public float maxVelocity;
    public float groundLevel;
    
    void Start()
    {
        normGravScale = rb.gravityScale;
    }

    public void Update()
    {
        if (rb.linearVelocity.magnitude > maxVelocity) {
            // Normalize the velocity vector and multiply by maxVelocity
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
        if (onGround) {lastGroundT = Time.time;}
        ControlMovement();
        if (!climbingLadder)
        {
            if (inputMovement.x < 0) {cc.facingLeft = true;}
            else if (inputMovement.x > 0) {cc.facingLeft = false;}
        }
        flipM = cc.facingLeft ? -1 : 1;

        targMoveVeloc = inputMovement * moveSpeed * (isSprinting ? sprintMultiplier : 1);

        targMoveVeloc.y = rb.linearVelocity.y;
        if (climbingLadder) {targMoveVeloc.x = 0;}
        //Jump
        if (!rolling && !disableJump && Time.time - lastJumpPressT < preJumpTime && Time.time - lastJumpT > 0.05f && (onGround || onLadder || isSliding || Time.time - lastGroundT < coyoteTime))
        {
            Jump();
        }
        
        ManageJump();
        ManageLadderClimb();
        if (climbingLadder) {inputMovement.x=0;}
    }
    protected virtual void ControlMovement()
    {

    }
    void FixedUpdate()
    {
        if (!isSliding)
        {
            targMoveVeloc.y = rb.linearVelocity.y;
            float accel = moveAccel;
            if (!cc.facingLeft) { if (targMoveVeloc.x < rb.linearVelocity.x) {accel = moveDecel;}}
            else {if (targMoveVeloc.x > rb.linearVelocity.x) {accel = moveDecel;}}
            accel *= (isSprinting ? sprintMultiplier : 1);
            if (!rolling) {rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity,targMoveVeloc, accel * Time.fixedDeltaTime);}
            ManageStep();
            DetectTriggers();
        }
        rb.gravityScale = !onGround && !onLadder ? normGravScale : 0;
    }
    public void JumpButtonPressed()
    {
        lastJumpPressT = Time.time;
    }
    protected virtual void Jump()
    {
        onGround = false; 
        if (!isSliding || onLadder) {rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); }// transform.position += (Vector3)groundNormal *0.2f;}
        rb.AddForce((isSliding ? Vector2.Lerp(groundNormal,Vector2.up,0.25f) : Vector2.up) * jumpForce);
        onLadder=false;
        isSliding = false;
        lastJumpT = Time.time;
        lastJumpPressT -= preJumpTime;
        isJumping=true; isJumpRising=true; jumpReleased = false;
        if (cc.ca != null) {cc.ca.OnJump();}
    }
    public bool isJumping;
    public bool isJumpRising;
    public bool jumpReleased;
    public float peakJumpT;
    bool pulledJDown;
    public bool jumpInterrupt;
    public void JumpButtonReleased()
    {
        jumpReleased = true;
    }
    public void ManageJump()
    {
        if (jumpInterrupt) {isJumping = false;}
        if (isJumping)
        {
            if (onGround || onLadder) {isJumping = false;isJumpRising=false; pulledJDown=false; if (cc.ca != null) {cc.ca.OnLand();}}
            if (Time.time - lastJumpT > 0.1f)
            {
                if (isJumpRising && rb.linearVelocity.y <= 0){isJumpRising = false; peakJumpT = Time.time;}
                if (isJumpRising && jumpReleased) {isJumpRising=false; rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y*0.4f); peakJumpT=Time.time;}
                if (!isJumpRising && !pulledJDown)
                {
                    if (Time.time - peakJumpT > jumpHoverTime) {pulledJDown = true; rb.linearVelocity += Vector2.down * jumpPullDownForce;}
                    else {rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y*0f);}
                }
            }
        }
        else
        {
            isJumpRising =false; jumpReleased=false;pulledJDown=false;jumpInterrupt=false;
        }
    }
    float lastLadderClimbInt;
    bool dirB4Ladder;
    float lastLadClimb;
    void ManageLadderClimb()
    {
        if (touchingLadder && !isJumpRising && !rolling && Time.time - peakJumpT > 0.2f) {if (!onLadder) {onLadder=true; rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y*0f);dirB4Ladder=cc.facingLeft;}}
        else {onLadder=false;}
        
        if (onLadder)
        {
            float clMulti = 1;
            bool ladC = Mathf.Abs(inputMovement.y) > 0.85f && Mathf.Abs(inputMovement.x) < 0.9f;
            if (Time.time - lastLadClimb < 0.12f || ladC)
            {
                
                float intervalMulti = 1f;
                if (isSprinting) {intervalMulti*=0.75f; clMulti*=1.5f;}

                if (isSprinting && inputMovement.y < 0f) {clMulti*=1.5f;}
                else if (Time.time - lastLadderClimbInt < 0.15f*intervalMulti)
                {   
                    clMulti *= 1;
                }
                else if (Time.time - lastLadderClimbInt < 0.3f*intervalMulti)
                {
                    clMulti *= 0.1f;
                }
                else
                {
                    lastLadderClimbInt = Time.time;
                }
                if (!climbingLadder) {dirB4Ladder = cc.facingLeft;}
                climbingLadder=true;
                if (ladC) {lastLadClimb=Time.time;}
                float disp =  ladder.position.x - transform.position.x;
                float dist = Mathf.Abs(disp);
                if (disp == 0) {disp = 0.01f * flipM; dist = 0.01f;}
                cc.facingLeft = disp < 0; flipM = cc.facingLeft ? -1 : 1;
                float targX = transform.position.x;
                //if (dist==0) {}
                // if (dist > 0.04f)
                // {
                //     targX += (disp/dist) * (dist-0.04f);
                // }
                // else if (dist < 0.039f)
                // {
                //     disp = 0.04f*-flipM; dist = 0.04f;
                //     targX += disp;
                //     //transform.position -= Vector3.right * (disp/dist) * (0.09f-dist);
                // }
                targX = ladder.position.x - ((disp/dist) * 0.06f);
                transform.position = new Vector3(Mathf.MoveTowards(transform.position.x,targX,Time.deltaTime*1f), transform.position.y,transform.position.z);
                if (Mathf.Abs(inputMovement.x) > 0.7f) {dirB4Ladder = inputMovement.x < 0;}
            }
            else
            {
                clMulti = 0;
                if (climbingLadder) {cc.facingLeft = dirB4Ladder;}
                climbingLadder = false;
                
            }
            float ladderVeloc = Mathf.MoveTowards(rb.linearVelocity.y, inputMovement.y * ladderClimbSpeed * clMulti, moveAccel);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * (climbingLadder ? 0 : 1), ladderVeloc);
            
            //if (rb.velocity.y<0){rb.velocity = new Vector2(rb.velocity.x, 0f);}
        }
        else {climbingLadder=false;}
    }

    void ManageStep()
    {
        if (Time.time - lastJumpT > 0.05f && Mathf.Abs(targMoveVeloc.x) > 0.01f)
        {
            int dirM = targMoveVeloc.x > 0 ? 1 : -1;
            float maxRayDist = size.y/2;
            float maxStepHeight = maxRayDist*0.8f;
            Vector2 rayPos = transform.position+(transform.up*maxRayDist)+(transform.right*dirM*(0.02f+(size.x/2f)));
            RaycastHit2D stepHit = Physics2D.Raycast(rayPos, -transform.up, maxRayDist, groundLM);
            if (stepHit)
            {
                float stepDist = (maxRayDist - stepHit.distance);
                if(stepDist>0.0001f && stepDist < maxStepHeight && Vector2.Distance(stepHit.normal,transform.up) < 0.64f)
                {
                    Debug.DrawLine(stepHit.point, stepHit.point + stepHit.normal,Color.green, 1f);

                    Vector2 targPosition =(transform.position + (transform.up * (stepDist*1.1f)) + (transform.right*0.02f*dirM));
                    Vector2 newRayPos = targPosition + (Vector2)(transform.up*size.y*0.5f) + (Vector2)(transform.up*0.01f); //rayPos + ((Vector2)transform.up * (maxRayDist));//+0.025f));
                    Collider2D wallCol = Physics2D.OverlapBox(newRayPos, size*0.95f,transform.rotation.z, groundLM, -Mathf.Infinity, Mathf.Infinity);
                    if (wallCol == null)
                    {
                        cc.ca.appearanceStepOffset += (Vector2)transform.position - targPosition;
                        cc.ca.ApplyOffset();

                        transform.position = targPosition;
                        lastMadeStep=Time.time;
                    }
                }
            }
        }
    }
    void DetectTriggers()
    {
        touchingLadder=false;
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + (transform.up*size.y*0.5f), new Vector2(size.x*0.7f,size.x*0.5f),transform.rotation.z, detectLM, -Mathf.Infinity, Mathf.Infinity);
        foreach(Collider2D col in cols)
        {
            if (col.gameObject.tag=="Ladder")
            {
                touchingLadder=true;
                ladder = col.gameObject.transform;
                //Debug.Log("TOUCHING LADDER!");
            }
        }
        
    }
    void OnCollisionStay2D(Collision2D ourCollision)
    {
        if (!isJumpRising) {CheckGrounded(ourCollision);}
    }

    void OnCollisionExit2D(Collision2D ourCollision)
    {
        if (onGround) {lastGroundT = Time.time;}
        onGround = false;
        isSliding = false;
        groundNormal = new Vector3();
    }

    void CheckGrounded(Collision2D newCol)
    {
        if (Time.time - lastJumpT < 0.1f) {return;}
        cPoints = new ContactPoint2D[newCol.contactCount];
        newCol.GetContacts(cPoints);
        foreach(ContactPoint2D cP in cPoints)
        {
            float hity = GeneralFunc.NearestPixel(cP.point.y+0.01f);
            if (Vector2.Distance(cP.normal,transform.up) < 0.68f && hity <= GeneralFunc.NearestPixel(transform.position.y+0.01f))
            {
                groundLevel = hity;
                groundNormal = cP.normal;
                if (!onGround) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,0); 
                Vector3 targPos =  new Vector3(transform.position.x,groundLevel, transform.position.z);
                {transform.position = targPos;  cc.ca.appearanceStepOffset += (Vector2)(targPos - transform.position);}}
                onGround = true;
                lastGroundT = Time.time;
                
                return;
            }
            else if (Vector2.Distance(cP.normal,transform.up) < 1.26f && cP.point.y <= transform.position.y+0.1f)
            {
                groundNormal = cP.normal;
                isSliding = true;
            }
        }
    }

    public void AddForce()
    {
        jumpInterrupt=true;
    }
}
