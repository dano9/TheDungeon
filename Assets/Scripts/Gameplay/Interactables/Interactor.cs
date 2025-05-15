using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PotentialInteraction
{
    public float lastUpdate;
    public Interactable interactable;
    public int priority;
    public float distance;
    public float duration;
    public PotentialInteraction(Interactable inter)
    {
        interactable = inter;
        priority = inter.priority;
        duration = inter.promptDuration;
    }
}
public class Interactor : MonoBehaviour
{
    public Dictionary<Interactable, PotentialInteraction> potentialInters = new Dictionary<Interactable, PotentialInteraction>();
    public float maxInterDistance;
    public Transform interRecieveTrans;
    public List<PotentialInteraction> promptingInters = new List<PotentialInteraction>();
    public bool needUIUpdate;

    public void Update()
    {
        if (PlayerController.main.readyForInteraction) {DetectInterUse();}
    }
    public void FixedUpdate()
    {
        DetectIntersProximity();
        ManagePotentialInteractables();
        ManagePromptingInteractables();
        if (needUIUpdate)
        {
            PromptUI.main.UpdateInteractablePrompts(promptingInters);
            needUIUpdate=false;
        }
    }
    public void DetectInterUse()
    {
        foreach (PotentialInteraction potI in promptingInters)
        {
            if (NewInput.controls.Gameplay.UpInteract.WasPressedThisFrame())
            {
                potI.interactable.Use();
            }
        }
    }
    public void DetectIntersProximity()
    {
        Interactable[] interactables = FindObjectsOfType<Interactable>();
        foreach (Interactable inter in interactables)
        {
            if (!inter.proximityPrompt || !inter.live || !inter.enabled) {continue;}
            float dist = Vector2.Distance(inter.interBeaconTrans.position, interRecieveTrans.position);
            if (dist < maxInterDistance && dist < inter.maxDistance)
            {
                bool alreadyPotential = potentialInters.ContainsKey(inter);
                PotentialInteraction potI = alreadyPotential ? potentialInters[inter] : new PotentialInteraction(inter);
                potI.distance = dist;
                potI.lastUpdate=Time.time;
                potI.priority = inter.priority;
                Debug.Log("UPDATED INTERACTABLE");
                if (!alreadyPotential) {potentialInters[inter] = potI;}
            }
        }
    }
    public void ManagePotentialInteractables()
    {
        PotentialInteraction[] potIs = potentialInters.Values.ToArray();
        int i = potIs.Length-1;
        while (i >= 0)
        {
            PotentialInteraction potI = potIs[i];
            if (Time.time - potI.lastUpdate > potI.duration)
            {
                potentialInters.Remove(potI.interactable);
            }
            else
            {
                bool addToPrompt=true;
                int lowestPriorPI = -1;
                for (int p = 0; p < promptingInters.Count; p++)
                {
                    if (promptingInters[p].interactable == potI.interactable) {addToPrompt=false;break;}
                    //if ()
                    {
                        if (promptingInters[p].interactable.inputName == potI.interactable.inputName)
                        {
                            if (promptingInters[p].priority < potI.priority || (promptingInters[p].priority == potI.priority && promptingInters[p].distance > potI.distance))
                            {
                                promptingInters[p].interactable.OnEndPrompt();
                                needUIUpdate=true; potI.interactable.OnStartPrompt();
                                promptingInters[p] = potI; //Override existing interPrompt
                            }
                            addToPrompt = false; 
                            break;
                        }
                        else if (promptingInters[p].priority < potI.priority && (lowestPriorPI==-1 || promptingInters[p].priority < promptingInters[lowestPriorPI].priority))
                        {
                            lowestPriorPI = p; //Mark as lowest to be overridden
                        }
                    }
                }
                if (addToPrompt)
                {
                    if (promptingInters.Count < 3) {promptingInters.Add(potI);needUIUpdate=true;potI.interactable.OnStartPrompt();}//Space to add without override
                    else if (lowestPriorPI != -1) //Override lowest priority slot
                    {
                        promptingInters[lowestPriorPI].interactable.OnEndPrompt();
                        needUIUpdate=true; potI.interactable.OnStartPrompt();
                        promptingInters[lowestPriorPI] = potI;
                    }
                }
                // else if (lowestPriorPI != -1) //Override Existing interPrompt
                // {
                //     needUIUpdate=true;
                //     promptingInters[lowestPriorPI] = potI;
                // }
            }
            i--;
        }
    }
    public void ManagePromptingInteractables()
    {
        int i = promptingInters.Count-1;
        while (i >= 0)
        {
            PotentialInteraction potI = promptingInters[i];
            if (Time.time - potI.lastUpdate > potI.duration)
            {
                needUIUpdate=true;
                potI.interactable.OnEndPrompt();
                promptingInters.RemoveAt(i);
            }
            i--;
        }
    }
}
