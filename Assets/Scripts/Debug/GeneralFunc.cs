using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralFunc
{
    public static float NearestPixel(float val)
    {
        return Mathf.Round(val * 10) / 10f;
    }
}
