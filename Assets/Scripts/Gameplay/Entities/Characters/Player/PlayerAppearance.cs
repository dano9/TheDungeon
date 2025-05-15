using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAppearance : CharacterAppearance
{
    public Light2D playerLight;
    float curLIntensity;
    public override void ManageAppearance()
    {
        base.ManageAppearance();
        ManageLight();
    }
    public void ManageLight()
    {
        CamCoordinator curCamCoord = PlayerCamera.main.curCamCoord;
        if (curCamCoord != null) {curLIntensity = Mathf.MoveTowards(curLIntensity,curCamCoord.playerLightVal,Time.deltaTime*2f);}
        if (curLIntensity < 0.002f)
        {playerLight.enabled=false;}
        else
        {
            playerLight.enabled=true;
            playerLight.intensity = curLIntensity;
        }
    }
}
