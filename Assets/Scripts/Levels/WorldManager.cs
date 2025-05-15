using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public struct LevelPrefab
{
    public Level prefabRef;
    public AssetReference assetReference;
}
public class WorldManager : MonoBehaviour
{
    public Transform player;
    public static WorldManager main;
    public string currentLevelName;
    public Level activeLevel;
    public LevelPrefab[] levelPrefabArray;
    protected Dictionary<string, LevelPrefab> levelPrefabs;
    protected Dictionary<string, Level> levelInstances=new Dictionary<string, Level>();
    bool awaitingLevelLoad;
    TransitionManager transitionManager;
    void Awake()
    {
        main = this;
        levelPrefabs = new Dictionary<string, LevelPrefab>();
        foreach (LevelPrefab level in levelPrefabArray)
        {
            levelPrefabs.Add(level.prefabRef.levelName, level);
        }
    }
    void Start()
    {
        player.transform.position = ActivateLevel(currentLevelName).spawnPosition.position;
        transitionManager = TransitionManager.main;
    }
    public async void GoToPassage(string toLevel, string toPassage, Passage exitPassage=null)
    {
        if (awaitingLevelLoad) {return;}
        PlayerController pc = player.GetComponent<PlayerController>();
        if (exitPassage != null) {exitPassage.OverridePlayerControl(pc, true);}

        LoadInLevel(toLevel);
        //Allow time for transition
        transitionManager.FadeToBlack();
        while (awaitingLevelLoad || transitionManager.awaitingFade)
            await Task.Yield();

        UnloadLevel(activeLevel);
        ActivateLevel(toLevel);
        transitionManager.FadeToClear();

        Passage arrivalPassage = activeLevel.GetPassage(toPassage);
        arrivalPassage.ArriveAtPassage();
        //player.transform.position = arrivalPassage.exitPos.position;
        pc.GoToPassage(arrivalPassage);
    }

    public Level ActivateLevel(string levelName)
    {
        if (!levelInstances.ContainsKey(levelName)) {LoadInLevel(levelName, true);}
        activeLevel = levelInstances[levelName];
        activeLevel.gameObject.SetActive(true);
        return activeLevel;
    }
    public void DeactivateLevel(string levelName)
    {
        if (!levelInstances.ContainsKey(levelName)) {return;}
        DeactivateLevel(levelInstances[levelName]);
    }
    public void DeactivateLevel( Level levelInstance)
    {
        if (levelInstance == null) {return;}
        levelInstance.gameObject.SetActive(false);
    }
    
    public void LoadInLevel(string levelName, bool instant=false)
    {
        //Level levelInstance = Instantiate(levelPrefabs[levelName]);
        //Level levelInstance = 
        if (!instant)
        {
            awaitingLevelLoad=true;
            levelPrefabs[levelName].assetReference.InstantiateAsync().Completed += OnLevelInstanced;
        }
        else
        {
            Level levelInstance = Instantiate(levelPrefabs[levelName].prefabRef.gameObject).GetComponent<Level>();
            levelInstances[levelInstance.levelName] = levelInstance;
        }
    }
    public void UnloadLevel(string levelName)
    {
        if (!levelInstances.ContainsKey(levelName)) {return;}
        UnloadLevel(levelInstances[levelName]);
        
    }
    public void UnloadLevel(Level levelInstance)
    {
        if (levelInstance == null) {return;}
        if (levelInstance.asyncInstanced) {Addressables.ReleaseInstance(levelInstance.gameObject);}
        else {Destroy(levelInstance.gameObject);}
        if (levelInstances.ContainsKey(levelInstance.name)) {levelInstances.Remove(levelInstance.name);}
    }
    public void OnLevelInstanced(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            Level levelInstance = obj.Result.GetComponent<Level>();
            // Do something with the spawned enemy
            levelInstance.gameObject.SetActive(false);
            levelInstances[levelInstance.levelName] = levelInstance;
            levelInstance.asyncInstanced=true;
            awaitingLevelLoad=false;
            Debug.Log("INSTANCED ASYNC LEVEL!");
        }
        else
        {
            Debug.LogError("Failed to instantiate object.");
        }
    }
}
