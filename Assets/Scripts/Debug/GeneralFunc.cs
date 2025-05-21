using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralFunc
{
    public static float NearestPixel(float val)
    {
        return Mathf.Round(val * 10) / 10f;
    }
    public static Vector2 NearestPixel(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x * 10) / 10f, Mathf.Round(pos.y * 10) / 10f);
    }
}
