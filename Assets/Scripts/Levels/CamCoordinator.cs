using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LightingCoordination
{
    public float globalLightIntensity;
    public Color globalLightColor;
    public float playerLightIntensity;
    public Color playerLightColor;
    public Color bgColor;
}
[ExecuteInEditMode]
public class CamCoordinator : MonoBehaviour
{
    PlayerCamera playerCam;
    public BoxCollider2D boxCol;
    public LightingCoordination lightingCoordination;
    public int potency;
    public float zoom;
    public Vector4 bounds;
    public int mist;
    public Vector4 origBounds;
    public bool fitBoundsToBoxCol;
    public bool fitBoxColToBounds;
    public bool debugDrawBounds;
    public bool constantlyUpdateBounds;
    public Vector2 offset;
    void UpdateBounds()
    {
        if (origBounds.x != 0) { bounds.x = origBounds.x + transform.position.x; }
        if (origBounds.y != 0) { bounds.y = origBounds.y + transform.position.y; }
        if (origBounds.z != 0) { bounds.z = origBounds.z + transform.position.x; }
        if (origBounds.w != 0) { bounds.w = origBounds.w + transform.position.y; }
    }
    void Start()
    {
        UpdateBounds();
        //origBounds = bounds;
    }
    public void FixedUpdate()
    {
        if (constantlyUpdateBounds)
        {
            UpdateBounds();
        }
    }
    void Update()
    {
        if (offset != Vector2.zero)
        {
            origBounds.x += offset.x; origBounds.y += offset.y; origBounds.z += offset.x;origBounds.w += offset.y;
            offset = Vector2.zero;
        }
        if (fitBoundsToBoxCol)
        {
            fitBoundsToBoxCol = false;
            origBounds.x =  boxCol.offset.x - (boxCol.size.x/2); origBounds.z =  boxCol.offset.x + (boxCol.size.x/2);
            origBounds.y =  boxCol.offset.y + (boxCol.size.y/2); origBounds.w =  boxCol.offset.y - (boxCol.size.y/2);
        }
        if (fitBoxColToBounds)
        {
            fitBoxColToBounds = false;
            boxCol.size = new Vector2(origBounds.z - origBounds.x, origBounds.y - origBounds.w);
            boxCol.offset = new Vector2(origBounds.x,origBounds.w) + (boxCol.size *0.5f);
            //origBounds.x =  boxCol.offset.x - (boxCol.size.x/2); origBounds.z =  boxCol.offset.x + (boxCol.size.x/2);
            //origBounds.y =  boxCol.offset.y + (boxCol.size.y/2); origBounds.w =  boxCol.offset.y - (boxCol.size.y/2);
        }
        if (debugDrawBounds)
        {
            UpdateBounds();
            //Top
            Debug.DrawLine(new Vector2(bounds.x,bounds.y),new Vector2(bounds.z,bounds.y),Color.blue,0.03f);
            //Bottom
            Debug.DrawLine(new Vector2(bounds.x,bounds.w),new Vector2(bounds.z,bounds.w),Color.blue,0.03f);
            //Left
            Debug.DrawLine(new Vector2(bounds.x,bounds.w),new Vector2(bounds.x,bounds.y),Color.blue,0.03f);
            //Right
            Debug.DrawLine(new Vector2(bounds.z,bounds.w),new Vector2(bounds.z,bounds.y),Color.blue,0.03f);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && GetPlayerCam() != null)
        {
            if (playerCam.curCamCoord == null || playerCam.curCamCoord.potency < potency)
            {
                playerCam.SwitchCamCoord(this);
            }
        }
    }
    PlayerCamera GetPlayerCam()
    {
        if (playerCam == null) { playerCam = PlayerCamera.main;}
        return playerCam;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && GetPlayerCam() != null)
        {
            if (playerCam.curCamCoord == this)
            {
                playerCam.curCamCoord = null;
            }
        }
    }
}