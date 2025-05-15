using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PreciseSpriteCollider : MonoBehaviour
{
    public PhysicsMaterial2D physicMaterial;
    public bool generateCol;
    public bool simpleCols;
    public bool triggers;
    void Start()
    {
        if (Application.isPlaying){GenerateCollider();}
    }
    void Update()
    {
        if (generateCol)
        {
            generateCol = false;
            GenerateCollider();
        }
    }
    void GenerateCollider()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            //ColliderGenerator colGen = ColliderGenerator.main;
            //if (colGen == null) {colGen = FindObjectOfType(typeof(ColliderGenerator)) as ColliderGenerator;}
            ColliderGenerator.SpriteBoxColsGen(transform, 1, physicMaterial, simpleCols, triggers);
        }
        DestroyImmediate(this);
    }
}
