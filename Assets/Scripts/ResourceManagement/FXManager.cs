using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class FX
{
    public SimpleSpriteAnim sAnim;
    public float opacity;
    public bool active;
    public int priority;
    public float timeActivated;
    public float lifeSpan;
    public bool autoDis;
    public bool isFadingOut;
    public Color col;
    public FX(SimpleSpriteAnim sA, float o, int p, float tA, float l, bool aD, Color c)
    {
        sAnim = sA;
        opacity = o;
        priority = p;
        timeActivated = tA;
        lifeSpan = l;
        autoDis = aD;
        isFadingOut = false;
        col = c;
    }
}
[System.Serializable]
public struct EffectData
{
    public Sprite[] sprites;
    public float interval;
    public Vector2 scale;
    public Color color;
}
[System.Serializable]
public class EffectList
{
    public string fxName;
    public int lastFXUsed;
    public EffectData[] effects;
}
[System.Serializable]
public class EffectCategory
{
    public string categoryName;
    public EffectCategory[] subCategories;
    public EffectList[] effectlists;
}
public class FXManager : MonoBehaviour
{
    public static FXManager main;
    public EffectCategory[] fxCategories;
    public Dictionary<string, EffectList> fxLists;
    public List<FX> freeFX;
    public List<FX> effects;
    public GameObject fxPrefab;
    public int fxInstances;
    void InitCategory(EffectCategory fxCat, string pathSoFar="")
    {
        pathSoFar += fxCat.categoryName + "/";
        if (fxCat.effectlists != null)
        {
            foreach (EffectList fxList in fxCat.effectlists)
            {
                fxLists[pathSoFar + fxList.fxName] = fxList;
            }
        }
        if (fxCat.subCategories != null) {
        foreach (EffectCategory subCat in fxCat.subCategories) { InitCategory(subCat,pathSoFar);}}
    }
    void Awake()
    {
        main = this;
        fxLists = new Dictionary<string, EffectList>();
        foreach (EffectCategory sCat in fxCategories)
        {
            InitCategory(sCat);
        }
        effects = new List<FX>();
        freeFX = new List<FX>();
        for (int s = 0; s < fxInstances; s++)
        {
            SimpleSpriteAnim sA = Instantiate(fxPrefab,Vector3.zero, Quaternion.identity, transform).GetComponent<SimpleSpriteAnim>();
            sA.gameObject.SetActive(false);
            FX fx = new FX(sA,1,0,0,0,true, Color.white);
            freeFX.Add(fx);
            effects.Add(fx);
        }
    }

    // Update is called o2nce per frame
    void Update()
    {
        foreach (FX fx in effects)
        {
            ManageEffect(fx);
        }
    }

    public FX PlayEffectAtPoint(string fxAddress, Vector2 pos, Vector2 scale, float rot, Color col, float opacity, int priority, bool loop = false, float speedMultiplier=1, FX fx = null, int fxIndex = -1, float duration=0f, float ptTime=0f)
    {
        if (fx == null && freeFX.Count > 0)
        {
            fx = freeFX[0];
            freeFX.RemoveAt(0);
        }
        if (fx != null)
        {
            fx.priority = priority;
            fx.sAnim.transform.parent = null;
            fx.sAnim.transform.position = pos;
            fx.timeActivated = Time.time;
            fx.active = true;
            fx.opacity = opacity;
            fx.sAnim.gameObject.SetActive(true);

            EffectList eL = fxLists[fxAddress];
            if (fxIndex == -1) {fxIndex = (eL.lastFXUsed + 1) % eL.effects.Length;}
            eL.lastFXUsed = fxIndex;
            EffectData eD = eL.effects[fxIndex];
            fx.sAnim.sprites = eD.sprites;
            fx.sAnim.interval = eD.interval/speedMultiplier;
            fx.col = eD.color * col;
            fx.opacity = opacity;
            fx.sAnim.transform.localScale = scale;
            fx.sAnim.transform.rotation = Quaternion.Euler(0, 0, rot);
            fx.sAnim.spriteRenderer.color = new Color(fx.col.r, fx.col.g, fx.col.b, fx.col.a * opacity);
            fx.lifeSpan = (fx.sAnim.sprites.Length) * fx.sAnim.interval;
            fx.sAnim.loop = loop;
            fx.sAnim.curSprite = 0;
            fx.autoDis = !loop || duration != 0;
            fx.sAnim.Play();
            fx.isFadingOut = false;
        }
        return fx;
    }
    public void ManageEffect(FX fx)
    {
        if (!fx.active) {return;}
        if (Time.time - fx.timeActivated > fx.lifeSpan)
        {
            //fx.sAnim.spriteRenderer.color.a = fx.opacity;
            if (fx.autoDis)
            {
                DeactivateEffect(fx);
            }
        }
    }
    public FX DeactivateEffect(FX fx, bool fadeOut = false, float fadeSpeed = 1)
    {
        if (!fadeOut || fx.opacity<=0.02f)
        {
            fx.sAnim.transform.parent = transform;
            fx.sAnim.gameObject.SetActive(false);
            fx.active = false;
            freeFX.Add(fx);
            return null;
        }
        else if (!fx.isFadingOut)
        {
            fx.isFadingOut = true;
            StartCoroutine(FadeOutFX(fx, fadeSpeed));
        }
        return fx;
    }
    IEnumerator FadeOutFX(FX fx, float fadeSpeed)
    {
        fx.isFadingOut = true;
        SpriteRenderer sr = fx.sAnim.spriteRenderer;
        while (fx.opacity > 0.02f)
        {
            fx.opacity -= fadeSpeed * Time.deltaTime;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, fx.col.a * fx.opacity);
            yield return null;
        }
        fx.opacity = 0;
        fx.isFadingOut = false;
        DeactivateEffect(fx);
    }
}
