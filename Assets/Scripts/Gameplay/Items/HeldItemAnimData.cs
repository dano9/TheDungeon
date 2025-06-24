using UnityEngine;

public class HeldItemAnimData : MonoBehaviour
{
    public bool itemInFront;
    public int sfxInit;
    public int fxInit;
    public Vector2 fXPos;
    public Vector2 fxScale;
    public float fxRot;
    public Vector2 hitDirection;
    public Vector2 hitBoxCenter;
    public Vector2 hitBoxSize;

    public bool flipBodyLeft;
    public bool flipBodyFront;
    public bool flipHeadFront;
    public bool flipTorsoFront;

    public Vector2 bodyOffset;
    public Vector2 headOffset;
    public float bodyRotation;
    public float headRotation;
    public Vector2 headScale;
    public Vector2 bodyScale;
    public int feetSheetIndx;
    public int feetIndx = -1;
}
