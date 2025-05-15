using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : CharacterController
{
    public static PlayerController main;
    public Transform lookTarget;
    public Passage passageOverrideInput;
    public bool readyForInteraction;
    void Awake()
    {
        main = this;
    }
    protected override void Start()
    {
        base.Start();
        
    }
    void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        
    }    
    protected override void Update()
    {
        base.Update();
        readyForInteraction = !cm.rolling;
        if (isDead)
        {
            return;
        }
        
        // if (Input.GetKeyDown("f"))
        // {
        //     Die();
        // }  
    }
    public void GoToPassage(Passage passage)
    {
        cm.onGround=false;
        Vector2 deltaPos = passage.exitPos.position - transform.position;
        transform.position = passage.exitPos.position;
        PlayerCamera.main.GoToPassage(passage);
        ca.SuddenTeleport(deltaPos);
        facingLeft = passage.exitPos.lossyScale.x < 0;
        passageOverrideInput = passage;
        passage.OverridePlayerControl(this, false);
    }
    
    public override void Die()
    {
        isDead = true;
        //if (keepInventory) {LevelManager.main.SetDeathInv(items);}
        GetComponent<Collider2D>().enabled=false;
        Destroy(rb);
        //LevelManager.main.PlayerDie(2f);
        Destroy(gameObject);
    }
    // public override bool SwitchHeldItem(int indx, bool allowQuantZero=true)
    // {
    //     bool success = base.SwitchHeldItem(indx,allowQuantZero);
    //     if (success)
    //     {
    //         PointsManager.main.UpdateSelectedItemImg(items[selectedItemIndx]);
    //     }
    //     return success;
    // }
    // public override bool StopUseItem()
    // {
    //     bool success = base.StopUseItem();
    //     if (curHeldItem != null && curHeldItem.reduceOnEndUse)
    //     {
    //         if (curHeldItem.item.quantity <= 0 && curHeldItem.removeWhenQuantZero)
    //         {
    //             RemoveItem(curHeldItem);
    //         }
    //         else
    //         {
    //             PointsManager.main.UpdateSelectedItemImg(curHeldItem.item);
    //         }
            
    //     }
    //     return success;
    // }
    // public override bool UseItem()
    // {
    //     bool success = base.UseItem();
    //     if (curHeldItem != null && !curHeldItem.reduceOnEndUse)
    //     {
    //         if (curHeldItem.item.quantity <= 0 && curHeldItem.removeWhenQuantZero)
    //         {
    //             RemoveItem(curHeldItem);
    //         }
    //         else
    //         {
    //             PointsManager.main.UpdateSelectedItemImg(curHeldItem.item);
    //         }
            
    //     }
    //     return success;
    // }
    // public override void AquireItem(Item item)
    // {
    //     base.AquireItem(item);
    //     PointsManager.main.UpdateSelectedItemImg(items[selectedItemIndx]);
    //}
}
