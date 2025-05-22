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

}
public class WeaponHI : HeldItem
{
    //public int carryStanceType;
    public CharacterController cc;
    public SpriteRenderer[] spriteRenderers;
    public string defaultCarryStance;
    public Vector2 slashFXOffset;
    public Vector2 slashFXScale = Vector2.one;
    public AttackMotion[] sideAttackAnimations;
    public AttackMotion[] overheadAttackAnimations;
    public AttackMotion[] AttackAnimations;

    public BoxCollider2D hitDetectionRadius;


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
    void Start()
    {
        anim.Play(defaultCarryStance);
    }

    public override bool BeginUse()
    {
        if (!awaitingCooldown)
        {
            if (!isChargingAttack)
            {
                return BeginAttack();
            }
            else { useQueue.Add(Time.time); }
        }
        else
        {
            useQueue.Add(Time.time);
        }
        return false;
    }
    public void Update()
    {
        ManageQueuedAttacks();
        ManageAnimation();
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
    public virtual bool BeginAttack()
    {
        if (!isChargingAttack)
        {
            chargePerc = 0; fullyChargedAttack = false;
            chargeCoroutine = StartCoroutine(ChargeAttack(sideAttackAnimations[atckIndx]));
            isChargingAttack = true;
            atckIndx += 1;
            atckIndx %= sideAttackAnimations.Length;
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

            FX slashFx = FXManager.main.PlayEffectAtPoint(curAttackMotion.fxList[animData.fxInit], slshFxPos, slshFXScale, 0, Color.white, 1, 1);
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
                Debug.Log("FULLY CHARGED ATTACK!");
            }
            SFXManager.main.PlaySoundAtPoint(curAttackMotion.sfxList[animData.sfxInit], transform.position, 1, 10, ptTime: 0f, pitch:pitch);
        }
        else if (animData.sfxInit < 0) { readyForSfx = true; }

        for (int s = 0; s < spriteRenderers.Length; s++)
        {
            int targSOrder = (animData.itemInFront ? 9 : 2) + s;
            if (spriteRenderers[s].sortingOrder != targSOrder) { spriteRenderers[s].sortingOrder = targSOrder; }
        }
    }
}
