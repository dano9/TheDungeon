using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShatterShape
{
    public Vector2 center;
    public List<Vector2> verts;
    public Vector2 force;
    public int pixelsPerShard;
    public int shapeType; //0 is quads, 1 is line separation
    public List<Transform> objsToApply;
    public List<Vector3> oTAOffsets; //z is rotational angle
    public float timeOfShatter;
    public ShatterShape(float tos, Vector2 cntr, List<Vector2> vs ,Vector2 f, int pps, bool genOTA=false, int st=0)
    {
        timeOfShatter = tos;
        center = cntr;
        verts = vs;
        force = f;
        pixelsPerShard = pps;
        if (genOTA) {objsToApply = new List<Transform>(); oTAOffsets = new List<Vector3>();}
        shapeType=st;
    }
}
public struct ShatterShard
{
    public float timeOfShatter;
    public float timeToDespawn;
    public float opacity;
    public GameObject obj;
    public ShatterShard(float tOS, float tTD, GameObject o, float op)
    {
        timeOfShatter = tOS;
        timeToDespawn = tTD;
        opacity = op;
        obj = o;
    }
}
[System.Serializable]
public struct SpritePregenTemp
{
    public Sprite sprite;
    public PhysicsMaterial2D physMat;
}
public class SpriteShatterer : MonoBehaviour
{
    public static SpriteShatterer main;
    static Dictionary<Sprite, ShatteredPreset> shatteredPresetDict = new Dictionary<Sprite, ShatteredPreset>();
    public GameObject shardPrefab;
    List<ShatterShard> activeShatterShards = new List<ShatterShard>();
    List<GameObject> shatterShardPool = new List<GameObject>();
    public int shardPoolSize = 20;
    public float shatterShardDespawnTime;
    public SpritePregenTemp[] spritesToGeneratePresetsFor;
    int curPresetG = 0;
    float lastPresetGenTime;
    public Transform presetGeneratorObj;


    void Awake()
    {
        main = this;
    }

    void FixedUpdate()
    {
        // if (curPresetG < spritesToGeneratePresetsFor.Length && Time.time - lastPresetGenTime > 0.5f)
        // {
        //     Sprite sprite = spritesToGeneratePresetsFor[curPresetG].sprite; PhysicsMaterial2D physMat = spritesToGeneratePresetsFor[curPresetG].physMat;
        //     MaterialData matData = MaterialManager.defaultMat;
        //     if (physMat != null) { matData = MaterialManager.matDataDict[physMat]; }
        //     presetGeneratorObj.GetComponent<SpriteRenderer>().sprite = sprite;
        //     shatteredPresetDict[sprite] = new ShatteredPreset(ShatterFunc.GenerateShatterShardsArray(presetGeneratorObj, matData));
        //     lastPresetGenTime = Time.time; curPresetG++;
        // }
        ManageShatterShards();
    }

    public static void ShatterObject(Transform obj, Vector2 force, bool fullShatter = true)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) { return; }

        if (!fullShatter)
        {

        }
        else
        {
            if (shatteredPresetDict.ContainsKey(sr.sprite))
            {
                ShatteredPreset shatteredPreset = shatteredPresetDict[sr.sprite];
                Debug.Log("Shattering by Preset");
                ShatterFunc.ShatterObjectViaShardList(obj, shatteredPreset.shards, force, Vector2.zero);
            }
            else { ShatterFunc.ShatterObject(obj,force:force); }
        }

    }

    public void ManageShatterShards()
    {
        int ssTR = -1;
        for (int s = 0; s < activeShatterShards.Count; s++)
        {
            ShatterShard shatterShard = activeShatterShards[s];
            if (shatterShard.obj == null) { ssTR = s; }
            else
            {
                float timeLeft = shatterShard.timeToDespawn - (Time.time - shatterShard.timeOfShatter);
                if (timeLeft < 1)
                {
                    shatterShard.opacity=Mathf.MoveTowards(shatterShard.opacity, 0, 0.1f);
                    Color col = shatterShard.obj.GetComponent<SpriteRenderer>().color;
                    shatterShard.obj.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, shatterShard.opacity);
                    if (shatterShard.opacity < 0.6f && shatterShard.obj.GetComponent<Collider2D>() != null)
                    {
                        Collider2D[] eCols = shatterShard.obj.GetComponents<Collider2D>();
                        for (int b = 0; b < eCols.Length; b++)
                        {
                            Destroy(eCols[b]);
                        }
                    }
                }
                if (!shatterShard.obj.activeSelf || timeLeft <= 0 || Mathf.Abs(shatterShard.obj.transform.position.x) > 200 || Mathf.Abs(shatterShard.obj.transform.position.y) > 200)
                {
                    shatterShard.obj.GetComponent<SpriteRenderer>().sprite = null;
                    shatterShard.obj.SetActive(false);
                    shatterShard.obj.transform.parent = transform;
                    shatterShard.obj.GetComponent<SpriteRenderer>().color = Color.white;
                    ssTR = s;
                }
            }
        }
        if (ssTR != -1)
        {
            activeShatterShards.Remove(activeShatterShards[ssTR]);
        }
        int inactiveObjs = 0; GameObject highestIO = null;
        foreach (GameObject obj in shatterShardPool)
        {
            if (obj != null && !obj.active) { inactiveObjs++; highestIO = obj; }
        }
        if (inactiveObjs > shardPoolSize && highestIO != null)
        {
            shatterShardPool.Remove(highestIO);
            Destroy(highestIO);
        }
        else if (shatterShardPool.Count < shardPoolSize)
        {
            GameObject newShatterShard = Instantiate(shardPrefab);
            newShatterShard.transform.parent = transform;
            newShatterShard.SetActive(false);
            shatterShardPool.Add(newShatterShard);
        }
    }
    public ShatterShard InstanceShatterShard(float timeToDespawnM)
    {
        GameObject shatterShardObj = null;
        foreach (GameObject obj in shatterShardPool)
        {
            if (!obj.active) { shatterShardObj = obj; }
        }
        if (shatterShardObj == null)
        {
            shatterShardObj = Instantiate(shardPrefab);
            shatterShardPool.Add(shatterShardObj);
        }
        shatterShardObj.transform.parent = null;
        ShatterShard shatterShard = new ShatterShard(Time.time, timeToDespawnM * shatterShardDespawnTime + Random.Range(0f, 3f), shatterShardObj, shatterShardObj.GetComponent<SpriteRenderer>().color.a);
        activeShatterShards.Add(shatterShard);
        return shatterShard;
    }

    public static void AddShatterPreset(Sprite sprite, ShatteredPreset preset)
    {
        shatteredPresetDict[sprite] = preset;
    }
}
