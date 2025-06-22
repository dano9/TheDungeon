using UnityEngine;
using System.Collections.Generic;


public class HitboxDetection : MonoBehaviour
{
    public static HitboxDetection main;
    public List<Hitbox> hitboxes;
    public void Awake()
    {
        main = this;
    }
    public void Update()
    {
        ManageHitboxes();
    }
    public void ManageHitboxes()
    {
        if (hitboxes == null) { return; }
        for (int h = hitboxes.Count - 1; h >= 0; h--)
        {
            if (hitboxes[h].lingerTime >= 0 && Time.time - hitboxes[h].beginTime > hitboxes[h].lingerTime)
            { hitboxes.RemoveAt(h); }
            else
            { hitboxes[h].CheckForHits(); }
        }
    }
    public Hitbox AddHitbox(BoxCollider2D collider, Vector2 hitForce, float dmg, float hitInterval, float lingerTime = -1)
    {
        if (hitboxes == null) { hitboxes = new List<Hitbox>(); }
        Hitbox hitbox = new Hitbox(collider, hitForce, dmg, hitInterval, Time.time, lingerTime);
        hitboxes.Add(hitbox);
        return hitbox;
    }
    public Hitbox RemoveHitbox(Hitbox hitbox)
    {
        hitboxes.Remove(hitbox);
        return null;
    }

    public class Hitbox
    {
        public struct HitboxHit
        {
            public Transform hittable;
            public float lastHitTime;
            public HitboxHit(Transform hittable, float lastHitTime) { this.hittable = hittable; this.lastHitTime = lastHitTime; }
        }
        BoxCollider2D collider;
        public float beginTime;
        public float lingerTime;
        float hitInterval;
        Vector2 hitForce;
        float dmg;
        List<HitboxHit> hits;
        public Hitbox(BoxCollider2D collider, Vector2 hitForce, float dmg, float hitInterval, float beginTime, float lingerTime) { this.beginTime = beginTime; this.lingerTime = lingerTime; this.collider = collider; this.hitForce = hitForce; this.dmg = dmg; this.hitInterval = hitInterval; }

        public void ManageHits()
        {
            if (hits == null) { return; }
            for (int h = hits.Count - 1; h >= 0; h--)
            {
                HitboxHit hit = hits[h];
                if (Time.time - hit.lastHitTime > hitInterval) { hits.RemoveAt(h); }
            }
        }

        public void CheckForHits()
        {
            Collider2D[] results = Physics2D.OverlapBoxAll(collider.bounds.center, collider.bounds.size, 0);

            foreach (var col in results)
            {
                Transform hitTrans = col.attachedRigidbody != null ? col.attachedRigidbody.transform : col.transform;
                {
                    bool isInCooldown = false;
                    if (hits != null)
                    {
                        foreach (HitboxHit hit in hits) { if (hit.hittable == hitTrans) { isInCooldown = true; break; } }
                    }
                    else { hits = new List<HitboxHit>(); }

                    if (!isInCooldown)
                    {
                        ApplyHitToCollider(col);
                        hits.Add(new HitboxHit(hitTrans, Time.time));
                    }
                }
            }
        }
        public void ApplyHitToCollider(Collider2D col)
        {
            Hittable hittable = col.GetComponent<Hittable>();
            if (hittable == null)
            {
                Rigidbody2D rb = col.attachedRigidbody;
                if (rb != null)
                {
                    if (rb.gameObject.layer == 7)
                    { rb.AddForce(hitForce);  Debug.Log("HIT Debris: " + rb.gameObject.name);}
                }
            }
            else
            {
                //Debug.Log("HIT hittable: " + hittable.gameObject.name);
                hittable.TakeHit(dmg, hitForce);
            }
        }
    }
}
