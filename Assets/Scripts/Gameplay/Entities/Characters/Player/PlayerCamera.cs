using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera main;
    public PlayerController followPlayer;
    public Camera cam;
    Vector2 curPos;
    Vector2 targPos;
    public float xDistTilFollow;
    public float yDistTilFollow;
    public float ySegmentSize;
    public float yLevelOffset;
    public float yLevelSwitchSpeed = 3f;
    public CamCoordinator curCamCoord;
    void Awake()
    {
        main = this;   
    }
    Vector4 currentBounds;
    Vector4 screenBounds;
    Color curColor;
    bool hadCamCoord; float lostCamCordT; bool lockdownMode=false;
    void LateUpdate()
    {
        Vector2 lastPos = curPos;
        if (followPlayer == null && PlayerController.main != null) {followPlayer = PlayerController.main; curPos = followPlayer.lookTarget.position;}
        if (followPlayer != null) { targPos = PlayerController.main.ca.appearanceTrans.position + followPlayer.lookTarget.localPosition; }

        float targYLevel =  (Mathf.FloorToInt(((targPos.y) / ySegmentSize)) * ySegmentSize) + yLevelOffset;

        Vector2 followDisp = (Vector2)targPos - curPos;
        if (followDisp.x < -xDistTilFollow)
        {curPos.x += followDisp.x + xDistTilFollow;}
        else if (followDisp.x > xDistTilFollow)
        {curPos.x += followDisp.x - xDistTilFollow;}

        
        //else
        if (followPlayer != null && Mathf.Abs(followPlayer.rb.linearVelocity.y) < 4f)
        {
            curPos.y = Mathf.MoveTowards(curPos.y, targYLevel, Time.deltaTime * yLevelSwitchSpeed);
        }
        if (followDisp.y <= -yDistTilFollow)
        {curPos.y += followDisp.y + yDistTilFollow;}
        else if (followDisp.y >= yDistTilFollow)
        {curPos.y += followDisp.y - yDistTilFollow;}

        Vector2 camDimensions = new Vector2(cam.aspect *cam.orthographicSize * 2, cam.orthographicSize * 2);
        screenBounds = new Vector4(lastPos.x - (camDimensions.x/2f),lastPos.y + (camDimensions.y/2f),lastPos.x + (camDimensions.x/2f),lastPos.y - (camDimensions.y/2f));
        float lerpSpeed = 4f * Time.deltaTime;
        Vector4 targetBounds =  Vector4.zero;
        if (curCamCoord != null)
        {
            targetBounds = curCamCoord.bounds;
            if (!hadCamCoord) {hadCamCoord=true;lockdownMode=false;}

            //Try to shrink the current bounds to minimise interpolation time.
            if (currentBounds.z > targetBounds.z && currentBounds.z > screenBounds.z){currentBounds.z = screenBounds.z;}
            if (currentBounds.y > targetBounds.y && currentBounds.y > screenBounds.y){currentBounds.y = screenBounds.y;}
            if (currentBounds.x < targetBounds.x && currentBounds.x < screenBounds.x){currentBounds.x = screenBounds.x;}
            if (currentBounds.w < targetBounds.w && currentBounds.w < screenBounds.w){currentBounds.w = screenBounds.w;}
        }
        else 
        {
            targetBounds = new Vector4(-Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, -Mathf.Infinity);
            if (hadCamCoord) {hadCamCoord = false;lostCamCordT = Time.time;}
            Vector4 nScreenBounds = new Vector4(curPos.x - (camDimensions.x/2f),curPos.y + (camDimensions.y/2f),curPos.x + (camDimensions.x/2f),curPos.y - (camDimensions.y/2f));
            if (currentBounds.z >= nScreenBounds.z && currentBounds.z < nScreenBounds.x) { currentBounds.z = targetBounds.z;}
            if (currentBounds.y >= nScreenBounds.y && currentBounds.y < nScreenBounds.w) { currentBounds.y = targetBounds.y;}
            if (currentBounds.x <= nScreenBounds.x && currentBounds.x > nScreenBounds.z) { currentBounds.x = targetBounds.x;}
            if (currentBounds.w <= nScreenBounds.w && currentBounds.w > nScreenBounds.y) { currentBounds.w = targetBounds.w;}

            if (Time.time - lostCamCordT > 5f || Mathf.Abs(targPos.y - lastPos.y) > camDimensions.y || Mathf.Abs(targPos.x - lastPos.x) > camDimensions.x) {lockdownMode=true;}
            if (lockdownMode){lerpSpeed *= 2f*(1 + (Time.time - lostCamCordT));}//currentBounds = targetBounds;}
            
        }
        currentBounds = new Vector4(Mathf.MoveTowards(currentBounds.x, targetBounds.x,lerpSpeed*2f),Mathf.MoveTowards(currentBounds.y, targetBounds.y,lerpSpeed),Mathf.MoveTowards(currentBounds.z, targetBounds.z,lerpSpeed*2f),Mathf.MoveTowards(currentBounds.w, targetBounds.w,lerpSpeed));


        if (currentBounds.z - currentBounds.x > camDimensions.x)
        {
            if (curPos.x - (camDimensions.x / 2) < currentBounds.x && currentBounds.x != 0) { curPos.x = currentBounds.x + (camDimensions.x / 2); }
            if (curPos.x + (camDimensions.x / 2) > currentBounds.z && currentBounds.z != 0) { curPos.x = currentBounds.z - (camDimensions.x / 2); }
        }
        else
        {
            curPos.x = currentBounds.x + (0.5f * (currentBounds.z - currentBounds.x));
        }
        if (currentBounds.y - currentBounds.w > camDimensions.y)
        {
            if (curPos.y + (camDimensions.y / 2) > currentBounds.y && currentBounds.y != 0) { curPos.y = currentBounds.y - (camDimensions.y / 2); }
            if (curPos.y - (camDimensions.y / 2) < currentBounds.w && currentBounds.w != 0) { curPos.y = currentBounds.w + (camDimensions.y / 2); }
        }
        else
        {
            curPos.y = currentBounds.w + (0.5f * (currentBounds.y - currentBounds.w));
        }
        


        transform.position = curPos;


        if (curCamCoord != null)
        {
            curColor = Color.Lerp(curColor,curCamCoord.bgColor,Time.deltaTime*0.2f);
        }
        cam.backgroundColor = curColor;
        
    }

    public void GoToPassage(Passage passage)
    {
        curPos = passage.exitPos.position;
        transform.position = curPos;
        curCamCoord = passage.camCoordinator;
        if (curCamCoord != null) {currentBounds = curCamCoord.bounds; curColor=curCamCoord.bgColor;}
    }
}
