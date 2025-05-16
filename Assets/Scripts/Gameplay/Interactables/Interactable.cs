using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public int priority;
    public string inputName;
    public string actionLabel;
    public bool proximityPrompt;
    public float maxDistance;
    public Transform interBeaconTrans;
    public bool prompting;
    public float promptDuration=0.1f;
    public bool disableOnUse=false;
    public bool live=true;

    public virtual void OnStartPrompt()
    {
        //Debug.Log("Started Prompt for: " + actionLabel);
    }
    public virtual void OnEndPrompt()
    {
        //Debug.Log("Ended Prompt for: " + actionLabel);
    }
    public virtual void Use()
    {
        //Debug.Log("Used Interactable: " + actionLabel);
    }
    
}
