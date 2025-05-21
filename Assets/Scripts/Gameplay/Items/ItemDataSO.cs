using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public class ItemDataSO : ScriptableObject
{
    public ItemData[] itemTypes;

}
[System.Serializable]
public struct ItemData
{
    public string name;
    public int id;
    public HeldItem heldItemPrefab;
    public int maxQuantity;
    public Sprite icon;

    public string description;

}
