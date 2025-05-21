using UnityEngine;

public class HeldItem : MonoBehaviour
{
    public ItemData itemData;
    public HeldItemAnimData animData;
    public Animation anim;
    public virtual bool BeginUse()
    {
        return true;
    }
    public virtual bool EndUse()
    {
        return true;
    }
    public virtual void OnDrop()
    {

    }
    public virtual void OnPickup()
    {

    }
    public virtual void OnEquip()
    {

    }
    public virtual void OnDequip()
    {
        
    }
}