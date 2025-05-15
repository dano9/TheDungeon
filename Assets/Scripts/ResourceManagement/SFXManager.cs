using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioSource source;
    public float volume;
    public bool active;
    public int priority;
    public float timeActivated;
    public float lifeSpan;
    public bool autoDis;
    public bool isFadingOut;
    public Sound(AudioSource aS, float v,int p,float tA,float l, bool aD)
    {
        source = aS;
        volume = v;
        priority = p;
        timeActivated = tA;
        lifeSpan = l;
        autoDis = aD;
        isFadingOut = false;
    }
}
[System.Serializable]
public class ClipsList
{
    public string soundsName;
    public int lastClipUsed;
    public AudioClip[] clips;
}
[System.Serializable]
public class SoundCategory
{
    public string categoryName;
    public SoundCategory[] subCategories;
    public ClipsList[] soundlists;
}
public class SFXManager : MonoBehaviour
{
    public static SFXManager main;
    public SoundCategory[] sCategories;
    public Dictionary<string, ClipsList> clipsLists;
    public List<Sound> freeSounds;
    public List<Sound> sounds;
    public GameObject sourcePrefab;
    public int sourceInstances;
    // Start is called before the first frame update
    void InitCategory(SoundCategory sCat, string pathSoFar="")
    {
        //sCat.clipsLists = new Dictionary<string, ClipsList>();
        pathSoFar += sCat.categoryName + "/";
        if (sCat.soundlists != null)
        {
            foreach (ClipsList clipList in sCat.soundlists)
            {
                //sCat.clipsLists[clipList.soundsName] = clipList;
                clipsLists[pathSoFar + clipList.soundsName] = clipList;
            }
        }
        if (sCat.subCategories != null) {
        foreach (SoundCategory subCat in sCat.subCategories) { InitCategory(subCat,pathSoFar);}}
    }
    void Awake()
    {
        main = this;
        clipsLists = new Dictionary<string, ClipsList>();
        foreach (SoundCategory sCat in sCategories)
        {
            InitCategory(sCat);
        }
        sounds = new List<Sound>();
        freeSounds = new List<Sound>();
        for (int s = 0; s < sourceInstances; s++)
        {
            AudioSource aS = Instantiate(sourcePrefab,Vector3.zero, Quaternion.identity, transform).GetComponent<AudioSource>();
            aS.gameObject.SetActive(false);
            Sound sound = new Sound(aS,1,0,0,0,true);
            freeSounds.Add(sound);
            sounds.Add(sound);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Sound snd in sounds)
        {
            ManageSound(snd);
        }
    }

    public Sound PlaySoundAtPoint(string sndAddress, Vector3 pos, float volume, int priority, bool loop = false, Sound snd = null, int sndIndex = -1, float duration=0f, float ptTime=0f)
    {
        if (snd == null && freeSounds.Count > 0)
        {
            snd = freeSounds[0];
            freeSounds.RemoveAt(0);
            // foreach (Sound s in sounds)
            // {
            //     if (!s.active || ((snd == null || snd.priority > s.priority) && priority >= s.priority))
            //     {
            //         snd = s;break;
            //     }
            // }
        }
        if (snd != null)
        {
            snd.priority = priority;
            snd.source.transform.parent = null;
            snd.source.transform.position = pos;
            snd.timeActivated = Time.time;
            snd.active = true;
            snd.volume = volume;
            snd.source.volume = volume;
            snd.source.time = ptTime;
            snd.source.gameObject.SetActive(true);

            ClipsList cL = clipsLists[sndAddress];//GetClipsListFromAddress(sndAddress);
            if (sndIndex == -1) {sndIndex = (cL.lastClipUsed + 1) % cL.clips.Length;}
            cL.lastClipUsed = sndIndex;
            snd.source.clip = cL.clips[sndIndex];
            snd.lifeSpan = snd.source.clip.length;
            snd.source.loop = loop;
            snd.autoDis = !loop || duration != 0;
            snd.source.Play();
            snd.isFadingOut = false;
        }
        return snd;
    }
    public void ManageSound(Sound snd)
    {
        if (!snd.active) {return;}
        if (Time.time - snd.timeActivated > snd.lifeSpan)
        {
            snd.source.volume = snd.volume;
            if (snd.autoDis)
            {
                DeactivateSound(snd);
            }
        }
    }
    public Sound DeactivateSound(Sound snd, bool fadeOut = false, float fadeSpeed = 1)
    {
        if (!fadeOut || snd.volume<=0.02f)
        {
            snd.source.transform.parent = transform;
            snd.source.gameObject.SetActive(false);
            snd.active = false;
            freeSounds.Add(snd);
            return null;
        }
        else if (!snd.isFadingOut)
        {
            snd.isFadingOut = true;
            StartCoroutine(FadeOutSound(snd, fadeSpeed));
        }
        return snd;
    }
    IEnumerator FadeOutSound(Sound snd, float fadeSpeed)
    {
        snd.isFadingOut = true;
        while (snd.volume > 0.02f)
        {
            snd.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        snd.volume = 0;
        snd.isFadingOut = false;
        DeactivateSound(snd);
    }
}
