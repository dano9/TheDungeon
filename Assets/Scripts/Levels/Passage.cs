using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passage : MonoBehaviour
{
    public string passageName;
    public Transform entrancePos;
    public Transform exitPos;
    public string toLevel;
    public string toPassage;
    float lastTimeUsed;
    public CamCoordinator camCoordinator;
    bool used=false;
    public float overridePCDuration;
    public Vector2 overridePCDirection;
    public bool onlyOverridePCOnExit;

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag=="Player")
        {
            UsePassage();
            //WorldManager.main.UsePassage(this);
        }
    }
    public void UsePassage()
    {
        if (PlayerController.main.passageOverrideInput == null && Time.time - lastTimeUsed > 1f && !used)
        {
            lastTimeUsed=Time.time;
            WorldManager.main.GoToPassage(toLevel,toPassage, this);
        }
    }
    public void ArriveAtPassage()
    {
        lastTimeUsed=Time.time;
    }
    public void OnEnable()
    {
        used = false;
    }
    public void SetUsed()
    {
        used=true;
    }


    public void OverridePlayerControl(PlayerController pc, bool exiting)
    {
        pc.passageOverrideInput=this;
        StartCoroutine(OverridePCIE(pc,exiting));
    }
    IEnumerator OverridePCIE(PlayerController pc, bool exiting)
    {
        float t = 0;
        while (t < (!exiting ? overridePCDuration : 10))
        {
            if (pc.passageOverrideInput !=this){break;}
            pc.cm.inputMovement = overridePCDirection * (exiting ? 1 : -1) * (!onlyOverridePCOnExit || !exiting ? 1 : 0);
            yield return null;
            t+= Time.deltaTime;
        }
        if (pc.passageOverrideInput ==this){pc.passageOverrideInput=null;}
        
    }
}
