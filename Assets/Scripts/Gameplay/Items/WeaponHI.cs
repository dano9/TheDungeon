using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct AttackMotion
{

    public string animationName;
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

    public string defaultCarryStance;
    public AttackMotion[] sideAttackAnimations;
    public AttackMotion[] overheadAttackAnimations;
    public AttackMotion[] AttackAnimations;

    public BoxCollider2D hitDetectionRadius;


    bool isChargingAttack;
    bool awaitingCooldown;
    bool awaitingRelease;
    AttackMotion curAttackMotion;
    List<float> useQueue = new List<float>();
    bool queueReleased;

    int atckIndx;

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
            awaitingRelease = false;
            isChargingAttack = false;
            if (chargeCoroutine != null) { StopCoroutine(chargeCoroutine); chargeCoroutine = null; }

            anim.Play(curAttackMotion.animationName + "Attack");

            awaitingCooldown = true;
            StartCoroutine(CooldownAttack());
        }
    }
    float chargeUpTime = 0;
    public IEnumerator ChargeAttack(AttackMotion attackMotion)
    {
        curAttackMotion = attackMotion;
        isChargingAttack = true;
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
}
