using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ItemDataManager : MonoBehaviour
{
    public static Dictionary<string, ItemData> itemDataDict = new Dictionary<string, ItemData>();

    public HeldItem[] heldItemsArray;
    public ItemDataSO itemDataSO;
    public bool setupDataDictFromSO;

    void Start()
    {
        if (setupDataDictFromSO)
        {
            SetupDataDictFromSO();
        }
        else
        {
            AddHeldItemsToDict();
            UpdateSO();
        }
    }

    public void SetupDataDictFromSO()
    {
        foreach (ItemData id in itemDataSO.itemTypes)
        {
            itemDataDict[id.name] = id;
        }
    }
    public void AddHeldItemsToDict()
    {
        foreach (HeldItem hi in heldItemsArray)
        {
            itemDataDict[hi.itemData.name] = hi.itemData;
            hi.itemData.heldItemPrefab = hi;
        }
    }
    public void UpdateSO()
    {
        itemDataSO.itemTypes = itemDataDict.Values.ToArray();
        for (int i = 0; i < itemDataSO.itemTypes.Length; i++)
        {
            itemDataSO.itemTypes[i].id = i;
        }
    }

}
