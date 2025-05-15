using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public string levelName;
    public Transform spawnPosition;
    public Passage[] passages;
    public bool asyncInstanced=false;

    public Passage GetPassage(string passageName)
    {
        foreach (Passage passage in passages)
        {
            if (passage.passageName == passageName)
            {
                return passage;
            }
        }
        return null;
    }
}
