using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : Entity
{
    public Rigidbody2D rb;
    public CharacterMovement cm;
    public CharacterAppearance ca;
    //public Transform appearance;
    //public HeldItem curHeldItem;
    //public List<Item> items;
    public List<Vector2Int> itemsSetup;
    public List<Vector2Int> itemDrops;
    public int selectedItemIndx;
    public Transform holdTrans;
    public bool facingLeft;
    public LayerMask groundLM;

    public SpriteRenderer spriteRenderer;
    public float health=100;
    public float colDmgMultiplier=1f;
    public float minColForDmg=1f;
    public float maxColForInstaKill=30f;
    float lastDmgTime=-10;
    Color normalCol;
    public Color hitColor;
    public bool isDead=false;
    public bool keepInventory;
    
    
    protected override void Start()
    {
        base.Start();
        //normalCol = spriteRenderer.color;
    }

    protected virtual void Update()
    {
        if (isDead)
        {
            return;
        }
    }
    



    public void TakeDamage(float dmg, int hitType=0) //0 for normal, 1 for crushes/explosions, 2 split (externally)
    {
        health -= dmg;
        lastDmgTime = Time.time;
        spriteRenderer.color = hitColor;
        if (health <= 0)
        {
            health = 0;
            spriteRenderer.color = normalCol;
            if (hitType == 0)
            {
                KillNormal();
            }
            else if (hitType == 1)
            {
                KillCrush();
            }
            else if (hitType == 2)
            {
                Destroy(this);
            }
        }
    }


    public virtual void KillCrush()
    {
        spriteRenderer.transform.parent = null;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) {spriteRenderer.gameObject.AddComponent<BoxCollider2D>().sharedMaterial = col.sharedMaterial;spriteRenderer.gameObject.GetComponent<BoxCollider2D>().isTrigger=true;}
        Debug.Log("Crush Kill!");
        //DestructionManager.main.ShatterObject(spriteRenderer.transform,10f);
        Die();
    }
    public virtual void KillNormal()
    {
        KillCrush();
    }
    public virtual void Die()
    {
        isDead = true;
        // if (!keepInventory)
        // {
        //     foreach (Vector2Int itemDrop in itemDrops)
        //     {
        //         ItemManager.main.InstanceLooseItem(transform.position,itemDrop);
        //     }
        // }
        Destroy(gameObject);
    }
    // public override void OnCollide(Transform other, Vector2 colForce)
    // {
    //     float forceMag = colForce.magnitude;
    //     PhysicsMaterial2D physMat = other.GetComponent<Collider2D>().sharedMaterial;
    //     MaterialData matData = MaterialManager.defaultMat;;
    //     if (physMat != null) {matData = MaterialManager.matDataDict[physMat];}
    //     if (matData.shatterType==1)
    //     {
    //         if (forceMag > 2f) {
    //         //DestructionManager.main.ShatterObject(other,forceMag);
    //         return;} else {Debug.Log("NOT HARD ENOUGH TO SHATTER");}
    //     }
    //     if (forceMag >= maxColForInstaKill)
    //     {
    //         TakeDamage(health, 1);
    //     }
    //     else if (forceMag >= minColForDmg)
    //     {
    //         float dmg = forceMag * colDmgMultiplier;
    //         TakeDamage(dmg);
    //     }
    // }
    // public override void OnProjectileHit(LiveProjectile projectile)
    // {
    //     TakeDamage(projectile.damage);
    // }
    public override void OnMeleeHit(float dmg)
    {
        TakeDamage(dmg);
    }
    public override bool TrySplitEntity(float dmg)
    {
        if (health <= dmg)
        {
            TakeDamage(health, 1);
            return false;
        }
        else {return false;}
    }
    public override void OnExplode()
    {
        TakeDamage(health, 1);
    }


    // public virtual bool UseItem()
    // {
    //     if (curHeldItem == null) { return false;}
    //     return curHeldItem.Use();
    // }
    // public virtual bool StopUseItem()
    // {
    //     if (curHeldItem == null) { return false;}
    //     return curHeldItem.StopUse();
    // }
    // public bool SelectNextItem(int dir)
    // {
    //     if (items==null || items.Count==0) { return false;}
    //     Debug.Log("item count: " + items.Count);
    //     int newIndx = (selectedItemIndx + dir + items.Count) % items.Count;
    //     Debug.Log("new selected item indx: " + newIndx);
    //     return SwitchHeldItem(newIndx);
    // }
    // public virtual bool SwitchHeldItem(int indx, bool allowQuantZero=true)
    // {
    //     if (selectedItemIndx == indx || indx < 0 || indx >= items.Count || (!allowQuantZero && items[indx] != null && items[indx].quantity <= 0)) {return false;}
    //     if (curHeldItem != null)
    //     {
    //         curHeldItem.OnDequip();
    //         Destroy(curHeldItem.gameObject);
    //     }
    //     if (items[indx] != null && items[indx].itemData.heldItem != null)
    //     {
    //         curHeldItem = Instantiate(items[indx].itemData.heldItem,holdTrans);
    //         curHeldItem.transform.localPosition = Vector3.zero;
    //         curHeldItem.OnEquip(this,items[indx]);
    //     }
    //     else 
    //     {
    //         Debug.Log("No new item to switch to...");
    //     }
    //     selectedItemIndx = indx;
    //     return true;
    // }
    // public virtual void RemoveItem(HeldItem heldItem)
    // {
    //     heldItem.OnDequip();
    //     RemoveItem(heldItem.item);
    //     Destroy(heldItem.gameObject);
    // }
    // public virtual void RemoveItem(Item item)
    // {
    //     items.Remove(item);
    //     SelectNextItem(1);
    // }
    // public virtual void AquireItem(Item item)
    // {
    //     bool alreadyHas = false;
    //     for (int i = 0; i < items.Count; i++)
    //     {
    //         if (items[i].id == item.id)
    //         {
    //             alreadyHas = true;
    //             int prevQuant = items[i].quantity;
    //             items[i].quantity += item.quantity;
    //             if (//prevQuant <= 0 && 
    //             item.quantity > 0)
    //             {   
    //                 SwitchHeldItem(i);
    //             }
    //             return;
    //             break;
    //         }
    //     }
    //     if (!alreadyHas)
    //     {
    //         items.Add(item);
    //         SwitchHeldItem(items.Count-1);
    //     }
    // }
}
