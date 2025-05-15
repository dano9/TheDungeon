using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassageInteractable : Interactable
{
    public Passage passage;
    public override void Use()
    {
        base.Use();
        passage.UsePassage();
        //live=false;
    }
    
}
