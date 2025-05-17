using UnityEngine;

public class Hittable : MonoBehaviour
{
    public SimpleSpriteAnim spriteAnim;
    public float health;
    float curHealth;

    void Awake()
    {
        curHealth = health;
    }
    public void TakeHit(float damage)
    {
        curHealth -= damage;
        if (curHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        if (spriteAnim != null) { spriteAnim.Play(); }
    }


}
