using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaterialManager : MonoBehaviour
{
    public MaterialDataSO matDataSO;
    public static Dictionary<PhysicsMaterial2D,MaterialData> matDataDict;
    public static MaterialData defaultMat;
    public void Awake()
    {
        matDataDict = new Dictionary<PhysicsMaterial2D, MaterialData>();
        defaultMat = matDataSO.matDataList[0];
        foreach (MaterialData matData in matDataSO.matDataList)
        {
            foreach (PhysicsMaterial2D physMat in matData.physicsMats)
            {
                matDataDict[physMat] = matData;
            }
        }
    }
}
