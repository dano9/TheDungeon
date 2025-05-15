// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerCamera : MonoBehaviour
// {
//     public static PlayerCamera main;
//     public PlayerController followPlayer;
//     public Camera cam;
//     Vector2 curPos;
//     Vector2 targPos;
//     public float xDistTilFollow;
//     public float yDistTilFollow;
//     //public int curYLevel;
//     public float ySegmentSize;
//     public float yLevelOffset;
//     public float yLevelSwitchSpeed = 3f;
//     public CamCoordinator curCamCoord;
//     void Awake()
//     {
//         main = this;   
//     }
//     Vector4 currentBounds;
//     Vector4 screenBounds;
//     Vector2 curLocBoundSize;
//     Vector2 curLocBoundPos;
//     bool hadCamCoord;
//     float lostCamCordT;


//     public void TryShrinkBounds(Vector4 targetBounds, Vector4 assignBounds)
//     {
//         if (currentBounds.x < targetBounds.x) { currentBounds.x = -assignBounds.x;} if (currentBounds.z > targetBounds.z) { currentBounds.z = assignBounds.z;}
//         if (currentBounds.y > targetBounds.y) { currentBounds.y = assignBounds.y;} if (currentBounds.w < targetBounds.w) { currentBounds.w = -assignBounds.w;}
//     }
//     void LateUpdate()
//     {
//         Vector2 lastPos = curPos;
//         if (followPlayer == null && PlayerController.main != null) {followPlayer = PlayerController.main; curPos = followPlayer.lookTarget.position;}
//         if (followPlayer != null) { targPos = PlayerController.main.appearance.position + followPlayer.lookTarget.localPosition; }

//         float targYLevel =  (Mathf.FloorToInt(((targPos.y) / ySegmentSize)) * ySegmentSize) + yLevelOffset;

//         Vector2 followDisp = (Vector2)targPos - curPos;
//         if (followDisp.x < -xDistTilFollow)
//         {curPos.x += followDisp.x + xDistTilFollow;}
//         else if (followDisp.x > xDistTilFollow)
//         {curPos.x += followDisp.x - xDistTilFollow;}

        
//         //else
//         if (followPlayer != null && Mathf.Abs(followPlayer.rb.velocity.y) < 4f)
//         {
//             curPos.y = Mathf.MoveTowards(curPos.y, targYLevel, Time.deltaTime * yLevelSwitchSpeed);
//         }
//         bool correctedPos = false;
//         if (followDisp.y <= -yDistTilFollow)
//         {curPos.y += followDisp.y + yDistTilFollow; correctedPos=true;}
//         else if (followDisp.y >= yDistTilFollow)
//         {curPos.y += followDisp.y - yDistTilFollow; correctedPos=true;}
//         //if (correctedPos && curCamCoord == null) {currentBounds = new Vector4(-Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, -Mathf.Infinity);}

//         Vector2 camDimensions = new Vector2(cam.aspect *cam.orthographicSize * 2, cam.orthographicSize * 2);
//         screenBounds = new Vector4(curPos.x - (camDimensions.x/2f),curPos.y + (camDimensions.y/2f),curPos.x + (camDimensions.x/2f),curPos.y - (camDimensions.y/2f));
        
//         Vector4 targetBounds = Vector4.one;
//         float lerpSpeed = 4f * Time.deltaTime;
//         if (curCamCoord != null)
//         {
//             if (!hadCamCoord) {hadCamCoord =true;} //currentBounds=screenBounds;}
//             targetBounds = curCamCoord.bounds;

//             //TryShrinkBounds(targetBounds, targetBounds);

//             //Try to shrink the current bounds to minimise interpolation time.
//             if (currentBounds.z > targetBounds.z && currentBounds.z > screenBounds.z){currentBounds.z = screenBounds.z;}
//             if (currentBounds.y > targetBounds.y && currentBounds.y > screenBounds.y){currentBounds.y = screenBounds.y;}
//             if (currentBounds.x < targetBounds.x && currentBounds.x < screenBounds.x && screenBounds.x < targetBounds.x){currentBounds.x = screenBounds.x;}
//             if (currentBounds.w < targetBounds.w && currentBounds.w < screenBounds.w){currentBounds.w = screenBounds.w;}

//             //curLocBoundSize = new Vector2(currentBounds.z - currentBounds.x, currentBounds.y - currentBounds.w);
//             //curLocBoundPos.x = //currentBounds.x+(curLocBoundSize.x/2); curLocBoundPos.y = currentBounds.w+(curLocBoundSize.y/2); curLocBoundPos -= curPos;
//         }
//         else 
//         {
//             //targetBounds = currentBounds;
//             targetBounds = new Vector4(-Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, -Mathf.Infinity);
//             if (hadCamCoord) {hadCamCoord = false;lostCamCordT = Time.time;}

//             if (currentBounds.z >= screenBounds.z) { currentBounds.z = targetBounds.z;}
//             if (currentBounds.y >= screenBounds.y) { currentBounds.y = targetBounds.y;}
//             if (currentBounds.x <= screenBounds.x) { currentBounds.x = targetBounds.x;}
//             if (currentBounds.w <= screenBounds.w) { currentBounds.w = targetBounds.w;}

//             if (Time.time - lostCamCordT > 2.5f || Mathf.Abs(targPos.y - curPos.y) > camDimensions.y/2){lerpSpeed *= 100f;}
            

//             //currentBounds = targetBounds;
//             // curLocBoundSize = Vector2.MoveTowards(curLocBoundSize, camDimensions, lerpSpeed);
//             // curLocBoundPos 
//             // currentBounds = new Vector4(curPos.x - (curLocBoundSize.x/2f),curPos.y + (curLocBoundSize.y/2f),curPos.x + (curLocBoundSize.x/2f),curPos.y - (curLocBoundSize.y/2f));
//         }

//         currentBounds = new Vector4(Mathf.MoveTowards(currentBounds.x, targetBounds.x,lerpSpeed*2f),Mathf.MoveTowards(currentBounds.y, targetBounds.y,lerpSpeed),Mathf.MoveTowards(currentBounds.z, targetBounds.z,lerpSpeed*2f),Mathf.MoveTowards(currentBounds.w, targetBounds.w,lerpSpeed));
        
        

//         if (currentBounds.z - currentBounds.x > camDimensions.x)
//         {
//             if (curPos.x - (camDimensions.x / 2) < currentBounds.x && currentBounds.x != 0) { //currentBounds.x=Mathf.MoveTowards(currentBounds.x,targetBounds.x,lerpSpeed*2f); 
//             curPos.x = currentBounds.x + (camDimensions.x / 2); }
//             if (curPos.x + (camDimensions.x / 2) > currentBounds.z && currentBounds.z != 0) { //currentBounds.z=Mathf.MoveTowards(currentBounds.z,targetBounds.z,lerpSpeed*2f); 
//             curPos.x = currentBounds.z - (camDimensions.x / 2); }
//         }
//         else
//         {
//             curPos.x = currentBounds.x + (0.5f * (currentBounds.z - currentBounds.x));
//         }
//         if (currentBounds.y - currentBounds.w > camDimensions.y)
//         {
//             if (curPos.y + (camDimensions.y / 2) > currentBounds.y && currentBounds.y != 0) { //currentBounds.y=Mathf.MoveTowards(currentBounds.y,targetBounds.y,lerpSpeed); 
//             curPos.y = currentBounds.y - (camDimensions.y / 2); }
//             if (curPos.y - (camDimensions.y / 2) < currentBounds.w && currentBounds.w != 0) { //currentBounds.w=Mathf.MoveTowards(currentBounds.w,targetBounds.w,lerpSpeed); 
//             curPos.y = currentBounds.w + (camDimensions.y / 2); }
//         }
//         else
//         {
//             curPos.y = currentBounds.w + (0.5f * (currentBounds.y - currentBounds.w));
//         }
        


//         transform.position = curPos;

        
//     }
// }
