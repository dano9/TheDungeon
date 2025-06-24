using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct AttackMotion
{

    public string animationName;
    public string[] fxList;
    public string[] sfxList;
    public float minChargeUpTime;
    public float maxChargeUpTime;
    public float cooldownTime;
    public float endLingerTime;
    public float dmgImpactDelay;
    public float dmgMultiplier;
    public Vector2 attackDir;
    public Vector2 hitboxScale;

}
[ExecuteInEditMode]
public class WeaponHI : HeldItem
{
    //public int carryStanceType;
    public CharacterController cc;
    public float damage; public float hitForce;
    public float range = 1f;
    public SpriteRenderer[] spriteRenderers;
    public string defaultCarryStance;
    public Vector2 slashFXOffset;
    public Vector2 slashFXScale = Vector2.one;
    public AttackMotion[] sideAttackAnimations;
    public AttackMotion[] overheadAttackAnimations;
    public BoxCollider2D hitboxCol;


    bool isChargingAttack;
    float chargePerc;
    bool fullyChargedAttack;
    bool awaitingCooldown;
    bool awaitingRelease;
    AttackMotion curAttackMotion;
    List<float> useQueue = new List<float>();
    bool queueReleased;

    int atckIndx;
    bool readyForFx;
    bool readyForSfx;

    Coroutine chargeCoroutine;
    Vector2 attackDir; Vector2 queuedAttackDir; 
    void Start()
    {
        anim.Play(defaultCarryStance);
    }

    public override bool BeginUse()
    {
        if (!awaitingCooldown && !cc.readyForAttack)
        {
            if (!isChargingAttack)
            {
                attackDir = cc.attackDirection;
                queuedAttackDir = attackDir;
                return BeginAttack();
            }
            else { useQueue.Add(Time.time); queuedAttackDir = cc.attackDirection; }
        }
        else
        {
            queuedAttackDir = cc.attackDirection;
            useQueue.Add(Time.time);
        }
        return false;
    }
    public void Update()
    {
        if (Application.isPlaying)
        {
            ManageQueuedAttacks();
            ManageAnimation();
        }
        ManageHitBox();

    }
    public override bool EndUse()
    {
        if (!awaitingCooldown && isChargingAttack)
        {
            if (chargeUpTime > curAttackMotion.minChargeUpTime)
            {
                ReleaseAttack();
            }
            else
            {
                awaitingRelease = true;
            }
            return true;
        }
        else if (useQueue.Count > 0) { queueReleased = true; }
        return false;
    }
    int attackType; 
    public virtual bool BeginAttack()
    {
        if (!isChargingAttack)
        {
            int lastAttackType = attackType;
            if (Mathf.Abs(attackDir.x) >= Mathf.Abs(attackDir.y)) { attackType = 0; }
            else { attackType = 1; }
            if (attackType != lastAttackType) { atckIndx = 0; }
            lastAttackType = attackType;

            AttackMotion attackM = new AttackMotion();
            if (attackType == 0) { atckIndx %= sideAttackAnimations.Length; attackM = sideAttackAnimations[atckIndx]; } //Side
            if (attackType == 1) { atckIndx %= overheadAttackAnimations.Length; attackM = overheadAttackAnimations[atckIndx]; } //Overhead
            chargePerc = 0; fullyChargedAttack = false;
            chargeCoroutine = StartCoroutine(ChargeAttack(attackM));
            isChargingAttack = true;
            atckIndx += 1;

            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual void ReleaseAttack()
    {
        if (chargeUpTime > curAttackMotion.minChargeUpTime)
        {
            chargePerc = (chargeUpTime - curAttackMotion.minChargeUpTime) / (curAttackMotion.maxChargeUpTime - curAttackMotion.minChargeUpTime);
            if (chargePerc > 0.6f) { fullyChargedAttack = true; }

            awaitingRelease = false;
            isChargingAttack = false;
            if (chargeCoroutine != null) { StopCoroutine(chargeCoroutine); chargeCoroutine = null; }

            anim.Play(curAttackMotion.animationName + "Attack");
            cc.ca.cape.ApplyForce(Vector2.right * -10f * cc.ca.flipM, 0.2f);

            awaitingCooldown = true;
            StartCoroutine(CooldownAttack());
        }
    }
    float chargeUpTime = 0;
    public IEnumerator ChargeAttack(AttackMotion attackMotion)
    {
        curAttackMotion = attackMotion;
        isChargingAttack = true;
        chargePerc = 0; fullyChargedAttack = false;
        chargeUpTime = 0;
        cc.ca.anim.SetTrigger("BeginAttack");
        anim.Play(curAttackMotion.animationName + "Charge");
        while (isChargingAttack && chargeUpTime < attackMotion.maxChargeUpTime && (!awaitingRelease || chargeUpTime < attackMotion.minChargeUpTime))
        {
            yield return null;
            chargeUpTime += Time.deltaTime;
        }
        if (isChargingAttack)
        {
            chargeCoroutine = null;
            isChargingAttack = false;
            chargePerc = (chargeUpTime - attackMotion.minChargeUpTime) / (attackMotion.maxChargeUpTime - attackMotion.minChargeUpTime);
            ReleaseAttack();
        }
    }
    float cooldownTime;
    public IEnumerator CooldownAttack()
    {
        cc.ca.anim.SetBool("cancelDodge", false);
        cooldownTime = 0;
        awaitingCooldown = true;
        while (awaitingCooldown && cooldownTime < curAttackMotion.cooldownTime)
        {
            yield return null;
            cooldownTime += Time.deltaTime;
        }
        //curAttackMotion = null;

        awaitingCooldown = false;
        while (!awaitingCooldown && !isChargingAttack && cooldownTime < curAttackMotion.endLingerTime)
        {
            yield return null;
            cooldownTime += Time.deltaTime;
        }
        if (!awaitingCooldown && !isChargingAttack) { anim.Play(defaultCarryStance); atckIndx = 0; }
    }

    public void ManageQueuedAttacks()
    {
        int uqc = useQueue.Count - 1;
        for (int i = uqc; i >= 0; i--)
        {
            if (Time.time - useQueue[i] < 0.3f)
            {
                if (!awaitingCooldown && !awaitingRelease && !isChargingAttack)
                {
                    useQueue.RemoveAt(i);
                    if (queueReleased) { awaitingRelease = true; queueReleased = false; }
                    attackDir = queuedAttackDir; queuedAttackDir = cc.attackDirection;
                    BeginAttack();
                }
            }
            else
            {
                useQueue.RemoveAt(i);
            }
        }
        if (uqc == 0) { queueReleased = false; }
    }
    public void ManageAnimation()
    {
        if (readyForFx && animData.fxInit >= 0 && curAttackMotion.fxList != null && curAttackMotion.fxList.Length > 0 && curAttackMotion.fxList.Length > animData.fxInit)
        {
            readyForFx = false;
            //"Attacks/Swipes/MidRnge"
            Vector2 slshFxPos = slashFXOffset + animData.fXPos; Vector2 slshFXScale = new Vector2((slashFXScale.x * animData.fxScale.x), (slashFXScale.y * animData.fxScale.y));
            slshFXScale.x *= cc.ca.transform.localScale.x; slshFXScale.y *= cc.ca.transform.localScale.y;
            slshFxPos = (Vector2)cc.ca.transform.position + new Vector2(slshFxPos.x * cc.ca.transform.localScale.x, slshFxPos.y * cc.ca.transform.localScale.y);
            float slshFxRot = animData.fxRot; if (cc.facingLeft) { slshFxRot = 360 - slshFxRot; }
            FX slashFx = FXManager.main.PlayEffectAtPoint(curAttackMotion.fxList[animData.fxInit], slshFxPos, slshFXScale, slshFxRot, Color.white, 1, 1);
            slashFx.sAnim.transform.parent = transform;
        }
        else if (animData.fxInit < 0) { readyForFx = true; }

        if (readyForSfx && animData.sfxInit >= 0 && curAttackMotion.sfxList != null && curAttackMotion.sfxList.Length > 0 && curAttackMotion.sfxList.Length > animData.sfxInit)
        {
            //"Weapons/Sword/Slash/Light"
            readyForSfx = false;
            float pitch = 1f;
            if (fullyChargedAttack)
            {
                pitch *= 0.8f;
                //Debug.Log("FULLY CHARGED ATTACK!");
            }
            SFXManager.main.PlaySoundAtPoint(curAttackMotion.sfxList[animData.sfxInit], transform.position, 1, 10, ptTime: 0f, pitch: pitch);
        }
        else if (animData.sfxInit < 0) { readyForSfx = true; }

        for (int s = 0; s < spriteRenderers.Length; s++)
        {
            int targSOrder = (animData.itemInFront ? 9 : 2) + s;
            if (spriteRenderers[s].sortingOrder != targSOrder) { spriteRenderers[s].sortingOrder = targSOrder; }
        }
    }
    HitboxDetection.Hitbox hitbox;
    static Vector2 hitboxOffset = Vector2.one * 0.5f;
    public void ManageHitBox()
    {
        
        hitboxCol.offset = animData.hitBoxCenter;
        hitboxCol.size = animData.hitBoxSize;
        if (!Application.isPlaying) { return; }
        Vector2 rangeBonus = curAttackMotion.hitboxScale * range;
        // if (rangeBonus.x == 0) { rangeBonus.x = curAttackMotion.attackDir.x; }
        // if (rangeBonus.y == 0) { rangeBonus.y = curAttackMotion.attackDir.y; }
        rangeBonus = new Vector2(rangeBonus.x * animData.hitBoxSize.x, rangeBonus.y * animData.hitBoxSize.y);
        hitboxCol.size = rangeBonus; 
        hitboxCol.offset = (rangeBonus*0.5f) + new Vector2(hitboxOffset.x * curAttackMotion.attackDir.x, hitboxOffset.y * curAttackMotion.attackDir.y);
    

        bool enableHB = hitboxCol.enabled;
        if (enableHB)
        {
            if (hitbox == null)
            {
                hitbox = HitboxDetection.main.AddHitbox(hitboxCol, new Vector2(animData.hitDirection.x * -(cc.facingLeft ? -1 : 1),animData.hitDirection.y) * hitForce, damage, 10f, -1f);
            }
        }
        else if (hitbox != null) { hitbox = HitboxDetection.main.RemoveHitbox(hitbox); }
    }
}
