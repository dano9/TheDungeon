using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager main;
    public Material ditherMat;
    public Image fadeOverlay;
    public float transitionVal;
    public float fadeSegmentation = 10f;
    public bool overlayBlack;
    public float fadeToBlackSpeed;
    public float fadeToClearSpeed;
    public bool awaitingFade;
    // Start is called before the first frame update
    void Awake()
    {
        main =this;
    }

    // Update is called once per frame
    void Update()
    {
        if (overlayBlack) {transitionVal = Mathf.MoveTowards(transitionVal,1,fadeToBlackSpeed*Time.unscaledDeltaTime); if (transitionVal >= 0.99f) {awaitingFade=false;}}
        else {transitionVal = Mathf.MoveTowards(transitionVal,0,fadeToClearSpeed*Time.unscaledDeltaTime); if (transitionVal <= 0.01f) {awaitingFade=false;}}

        float segmentedTVal = Mathf.Clamp(Mathf.Floor(transitionVal*fadeSegmentation) / fadeSegmentation,0,1);
        fadeOverlay.color = new Color(fadeOverlay.color.r,fadeOverlay.color.g,fadeOverlay.color.b,segmentedTVal);
        ditherMat.SetFloat("_FadeVal",((segmentedTVal*2.5f)-(transitionVal > 0.05f ? 0.5f : 1)));
    }
    public void FadeToBlack()
    {
        overlayBlack = true;
        awaitingFade = true;
    }
    public void FadeToClear()
    {
        overlayBlack = false;
        awaitingFade = true;
    }
}
