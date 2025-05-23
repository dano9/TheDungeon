using UnityEngine;

public class Hittable : MonoBehaviour
{
    public SimpleSpriteAnim spriteAnim;
    public float health;
    float curHealth;
    protected Vector2 lastForce;

    void Awake()
    {
        curHealth = health;
    }
    public void TakeHit(float damage, Vector2 hitForce)
    {
        curHealth -= damage;
        lastForce = hitForce;
        if (curHealth <= 0)
        {
            Die();
        }
    }
    public virtual void Die()
    {
        if (spriteAnim != null) { spriteAnim.Play(); }
    }


}
