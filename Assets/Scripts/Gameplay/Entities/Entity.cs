using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected virtual void Start()
    {
        //LevelManager.main.entities.Add(this);
    }
    public void OnCollisionEnter2D(Collision2D col)
    {
        Vector2 colForce = Vector2.zero;
        foreach (var contact in col.contacts)
        {
            colForce += (contact.normal * contact.normalImpulse)/ col.contacts.Length;
        }
        OnCollide(col.contacts[0].collider.transform, colForce);
    }
    public virtual void OnCollide(Transform other, Vector2 colForce)
    {

    }
    // public virtual void OnProjectileHit(LiveProjectile projectile)
    // {
        
    // }
    public virtual void OnMeleeHit(float dmg)
    {

    }
    public virtual bool TrySplitEntity(float dmg)
    {
        return true;
    }
    public virtual void OnExplode()
    {
        Destroy(this);
    }
}
