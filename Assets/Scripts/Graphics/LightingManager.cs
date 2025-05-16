using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingManager : MonoBehaviour
{
    public static LightingManager main;
    public LightingCoordination curLC;
    public Light2D globalLight;

    public float colorChangeSpeed;
    public float intensityChangeSpeed;

    private Camera mainCam;
    private Light2D playerLight;
    private Color bgColor;
    private Color globalLColor;
    private float globalLIntensity;
    private float playerLIntensity;
    private Color playerLColor;

    public void Awake()
    {
        main = this;
        ShiftTowardCurLC(true);
    }
    public void FixedUpdate()
    {
        ShiftTowardCurLC();
        UpdateLighting();
        //Camera.main.backgroundColor = Color.white;
    }
    public void AssignLightingCoordination(LightingCoordination newLC,bool instant=false)
    {
        curLC = newLC;
        if (instant) { ShiftTowardCurLC(true); }
    }
    public void ShiftTowardCurLC(bool instant=false)
    {
        //if (curLC != null)
        {
            if (instant)
            {
                bgColor = curLC.bgColor;
                globalLColor = curLC.globalLightColor;
                globalLIntensity = curLC.globalLightIntensity;
                playerLColor = curLC.playerLightColor;
                playerLIntensity = curLC.playerLightIntensity;
            }
            else
            {
                bgColor = Color.Lerp(bgColor,curLC.bgColor,colorChangeSpeed);
                globalLColor = Color.Lerp(globalLColor,curLC.globalLightColor,colorChangeSpeed);
                globalLIntensity = Mathf.Lerp(globalLIntensity, curLC.globalLightIntensity, intensityChangeSpeed);
                playerLColor = Color.Lerp(playerLColor,curLC.playerLightColor,colorChangeSpeed);
                playerLIntensity = Mathf.Lerp(playerLIntensity, curLC.playerLightIntensity, intensityChangeSpeed);
            }
        }
    }
    public void StaggerLightingValues()
    {
        float colorStaggeration = 6; float intensityStaggeration = 6;
        bgColor = StaggerColor(bgColor, colorStaggeration);
    }
    public Color StaggerColor(Color col, float staggeration)
    {
        return new Color(Mathf.Floor(col.r * staggeration) / staggeration, Mathf.Floor(col.g * staggeration) / staggeration, Mathf.Floor(col.b * staggeration) / staggeration, Mathf.Floor(col.a * staggeration) / staggeration);
    }
    public void UpdateLighting()
    {
        if (mainCam == null) { mainCam = Camera.main; }
        if (playerLight == null) { if (PlayerController.main != null) { playerLight = ((PlayerAppearance)PlayerController.main.ca).playerLight; } }

        if (globalLight != null)
        {
            if (globalLight.intensity != globalLIntensity) { globalLight.intensity = globalLIntensity; }
            if (globalLight.color != globalLColor) { globalLight.color = globalLColor; }
        }
        if (playerLight != null)
        {
            if (playerLight.intensity != playerLIntensity) { playerLight.intensity = playerLIntensity; }
            if (playerLight.color != playerLColor) { playerLight.color = playerLColor; }


            if (playerLIntensity < 0.002f){playerLight.enabled=false;}
            else{playerLight.enabled=true;}
        }
        if (mainCam != null)
        {
            if (mainCam.backgroundColor != bgColor)
            { mainCam.backgroundColor = bgColor; }
        }
    }
}
