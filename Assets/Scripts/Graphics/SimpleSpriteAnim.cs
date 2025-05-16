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
    public bool playOnWake = true;
    public bool loop = true;
    public bool loopBackwards;
    public bool hideOnEnd=true;
    int dirMulti = 1;
    bool isPlaying;
    void Awake() { if (playOnWake) { Play(); } }
    void Update()
    {
        if (isPlaying)
        {
            if (Time.time - lastSpriteChange > interval + (curSprite * intervalLengthenIndexMultiplier))
            {
                curSprite += 1 * dirMulti;
                if (curSprite >= sprites.Length - 1 || curSprite <= 0)
                {
                    if (!loop) { isPlaying = false; if (hideOnEnd) { spriteRenderer.enabled = false; } }
                    else if (loopBackwards) { dirMulti *= -1; }
                    else { curSprite = Mathf.Abs(curSprite) % sprites.Length; }
                }
                spriteRenderer.sprite = sprites[curSprite];
                lastSpriteChange = Time.time;
            }
        }
    }
    public void Play()
    {
        curSprite = 0;
        spriteRenderer.sprite = sprites[curSprite];
        lastSpriteChange = Time.time;
        spriteRenderer.enabled = true;
        isPlaying = true;
    }
}
