using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSound
{
  public float length;
  public float startTime;
  public float volume;
  public float radius;
  public float curPotency;
  public float startPot;
  public Vector3 pos;
  public bool isContinuous;
  public int casterID;
  public AudioSource audS;
  
  public GameSound(float l, float sT, float v, float r, float sP, Vector3 p, bool iC, int cID, AudioSource aS)
  {
      length = l;
      startTime = sT;
      volume = v;
      radius = r;
      startPot = sP;
      pos = p;
      isContinuous = iC;
      casterID = cID;
      audS = aS;
  }
}

public class SoundManager : MonoBehaviour
{
  public Transform soundHolder;
  public AudioSource[] sources;
  public int maxSources;
  public GameObject sourcePrefab;
  AudioListener mainAL;
  public List<GameSound> gameSounds;
  Dictionary<string, GameSound> continuousGSounds;
    // Start is called before the first frame update
    void Start()
    {
      gameSounds = new List<GameSound>();
      continuousGSounds = new Dictionary<string, GameSound>();
        sources = new AudioSource[maxSources];
        for (int a = 0; a < maxSources; a++)
        {
            sources[a] = Instantiate(sourcePrefab,Vector3.zero, Quaternion.identity, soundHolder).GetComponent<AudioSource>();
        }
        mainAL = GameObject.FindObjectOfType<AudioListener>();
    }
    public void FixedUpdate()
    {
      GameSound gSToRemove = null;
      float curTime = Time.time;
      foreach (GameSound gS in gameSounds)
      {
        if ((gS.startTime + gS.length < curTime && !gS.isContinuous))// || (gS.startTime + (gS.length * 5) < curTime && gS.isContinuous))
        {
          //Destroy(gS);
          gSToRemove = gS;
          
        }
        else if (!gS.isContinuous)
        {
          gS.curPotency = (gS.startPot * (1 - ((curTime - gS.startTime) / gS.length)));
        }
        else 
        {
          gS.curPotency = (gS.startPot);
        }
      }
      if (gSToRemove != null)
      {
        gameSounds.Remove(gSToRemove);
      }
    }
    public AudioSource PlaySound(AudioClip clip, float potency,Vector3 pos, float volume, float maxDistance, int casterID, bool gS = true, bool loop = false)
    {
      if (Vector3.Distance(pos, mainAL.transform.position) < maxDistance)
      {
        int s = 0;
        for (int a = 0; a < maxSources; a++)
        {
            if (!sources[a].isPlaying)
            {
              s = a;
            }
        }
        sources[s].transform.position = pos;
        sources[s].clip = clip;
        sources[s].maxDistance = maxDistance;
        sources[s].volume = volume;
        sources[s].loop = loop;
        sources[s].Play();
        if (gS)
        {
          gameSounds.Add(new GameSound(clip.length,Time.time,volume, maxDistance, potency, pos, false, casterID, sources[s]));
          //print("Added Sound " + gameSounds.Count);
        }
        return(sources[s]);
      }
      else
      {
        return null;
      }
    }
  public void PlayUISelSound()
   {
     //if (selectUIVal >= UISelSounds.Length) {selectUIVal = 0;}
    // UIAs.clip =  UISelSounds[selectUIVal];
     //UIAs.Play();
     //selectUIVal += 1;
   }
   public void PlayUIChangeMenuSound()
   {
     //UIAs.clip =  changeMenuSound;
     //UIAs.Play();
   }
   public Vector4 LocateMostPotentNearbySound(Vector3 listenerPos, float radius, float minPotency = 0, int castMask = -1)
   {
    Vector3 nearestPos = Vector3.zero;
    float mostPotent = -1;
    foreach (GameSound gS in gameSounds)
    {
      if (gS.audS.isPlaying)
      {
        float dist = Vector3.Distance(gS.pos, listenerPos);
        if (dist < radius && castMask != gS.casterID)
        {
          float potency = (1 - (dist / radius)) * (gS.curPotency);
          print ("Heard Sound " + potency);
          if (potency > mostPotent && potency > minPotency)
          {
            nearestPos = gS.pos;
            mostPotent = potency;
          }
        }
      }
    }
    return new Vector4(nearestPos.x, nearestPos.y, nearestPos.z, mostPotent);
   }
   public void UpdateContinuousSound(AudioClip clip,string soundID, float potency,Vector3 pos, float volume, float mD, int cID)
   {
      GameSound gS;
      if (continuousGSounds.ContainsKey(soundID))
      {
        gS = continuousGSounds[soundID];
        gS.startTime = Time.time;
        gS.pos = pos;
        gS.startPot = potency;
        gS.volume = volume;
        gS.audS.volume = volume;
        gS.audS.transform.position = pos;
        if (gS.audS.clip != clip)
        {
          gS.audS.clip = clip;
          gS.audS.loop = true;
          gS.audS.Play();
        }
        else if (!gS.audS.isPlaying)
        {
          gS.audS.Play();
        }
      }
      else
      {
        gS = new GameSound(0.1f,Time.time,volume, mD, potency,pos, true, cID, PlaySound(clip, potency, pos, volume, mD, cID, false, true));
        continuousGSounds.Add(soundID, gS);
        gameSounds.Add(gS);
      }
   }
   public void PauseContinuousSound(string soundID)
   {
    if (continuousGSounds.ContainsKey(soundID))
    {
      GameSound gS = continuousGSounds[soundID];
      if (gS.audS.isPlaying)
      {
        gS.audS.Pause();
      }
    }
   }
   public void StopContinuousSound(string soundID)
   {
    if (continuousGSounds.ContainsKey(soundID))
    {
      continuousGSounds[soundID].audS.Stop();
      gameSounds.Remove(continuousGSounds[soundID]);
      continuousGSounds.Remove(soundID);
    }
   }
}
