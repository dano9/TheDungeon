using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptUI : MonoBehaviour
{
    public static PromptUI main;
    public GameObject promptPrefab;
    public Transform promptParent;
    public float promptSpacing;

    public void Awake()
    {
        main = this;
    }
    public void UpdateInteractablePrompts(List<PotentialInteraction> potIs)
    {
        int ppc = promptParent.childCount;
        while (ppc < potIs.Count)
        {
            Instantiate(promptPrefab,promptParent);
            ppc++;
        }
        while (ppc > potIs.Count)
        {
            Destroy(promptParent.GetChild(ppc-1).gameObject);
            ppc--;
        }
        for (int p = 0; p < ppc; p++)
        {
            Transform pITrans = promptParent.GetChild(p);
            pITrans.localPosition = Vector2.down * promptSpacing * p;
            pITrans.GetComponent<TMP_Text>().text = potIs[p].interactable.actionLabel;
        }
    }
}
