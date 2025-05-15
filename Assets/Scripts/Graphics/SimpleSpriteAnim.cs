using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpriteAnim : MonoBehaviour
{
    public Sprite[] sprites;
    public SpriteRenderer spriteRenderer;
    public float interval = 0.2f;
    public float intervalLengthenIndexMultiplier = 0;
    float lastSpriteChange;
    public int curSprite;
    public bool loopBackwards;
    int dirMulti = 1;
    void Update()
    {
        if (Time.time - lastSpriteChange > interval + (curSprite * intervalLengthenIndexMultiplier))
        {
            curSprite += 1 * dirMulti;
            if (curSprite >= sprites.Length-1 || curSprite <= 0) {
            if (loopBackwards) {dirMulti *= -1;}
            else {curSprite = Mathf.Abs(curSprite) % sprites.Length;}}
            spriteRenderer.sprite = sprites[curSprite];
            lastSpriteChange = Time.time;
        }
    }
}
