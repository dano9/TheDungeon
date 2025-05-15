using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Spacer : MonoBehaviour
{
    public bool initiate;
    public Transform rootBone;
    public float spaceAmplitude = 10f;
    void Update()
    {
        if (initiate)
        {
            initiate = false;
            SpaceBones(rootBone);
        }
    }
    void SpaceBones(Transform parentBone)
    {
        foreach (Transform childBone in parentBone)
        {
            childBone.localPosition *= spaceAmplitude;
            SpaceBones(childBone);
        }
    }
}
