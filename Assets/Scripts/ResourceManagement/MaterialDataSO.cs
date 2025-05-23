using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Material Data" ,menuName ="Data/Material Data")]
public class MaterialDataSO : ScriptableObject
{
    public List<MaterialData> matDataList;
}

[System.Serializable]
public class MaterialData
{
    public string name;
    public PhysicsMaterial2D[] physicsMats;
    public int shatterType;
    public Color shatterBorderCol;
    public bool spreadBorderCol; //When exploding spread border color to other objects
    public float shatterBorderThck;
    public Vector2 shatterShapeDistortion = Vector2.one;
    public int pixelsPerShard = 4;
    public float minShatterForce=1;
    public float mass=0.1f;
    public float bulletDeminishVal = 1f;
    public int maxPixelsToRigifyIsland; //If a resulting island is small enough add a rigidbody to it

}