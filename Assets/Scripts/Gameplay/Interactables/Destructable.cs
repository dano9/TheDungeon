using UnityEngine;

public class Destructable : Hittable
{
    public bool destructFromImpact;
    public BoxCollider2D boxCol;
    public string breakSound;


    public void Shatter()
    {
        if (boxCol != null) { boxCol.enabled = false; }
        SpriteShatterer.ShatterObject(transform,lastForce);
        //Destroy(gameObject);
    }
    public override void Die()
    {
        base.Die();
        Shatter();
    }
}