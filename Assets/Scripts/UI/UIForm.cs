using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class UIElement
{
    public string name;
    int elementId; public void SetID(int newID) { elementId = newID; }
    public RectTransform rectTrans;
    public Button button;
    public Text text;
    bool active; public void SetActive(bool isActive) { active = isActive; }
    bool selected;
    public UnityEvent onConfirm;
    UIForm parentForm; public void SetParentForm(UIForm pForm) { parentForm = pForm; } public UIForm GetParentForm() { return parentForm; }
    public void Select()
    {
        selected = true;
        if (text != null) { text.color = Color.white; }
        rectTrans.localPosition = new Vector2(5, rectTrans.localPosition.y);
    }
    public void Deselect()
    {
        selected = false;
        if (text != null) { text.color = Color.white * 0.7f; }
        rectTrans.localPosition = new Vector2(0, rectTrans.localPosition.y);
    }
}
public class UIForm : MonoBehaviour
{
    public GameObject formObject;
    public UIElement[] elements;
    public bool canMoveIntoOtherForms = false;
    public bool setSelElOnActivate = true;
    public bool isActive;
    public int defaultSelElementIndx;

    public void SetupForm()
    {
        foreach (UIElement element in elements)
        {
            element.SetID(UIMaster.elementIdCounter);
            element.SetParentForm(this);
            UIMaster.elementIdCounter++;
        }
    }
    public void ActivateForm()
    {
        formObject.SetActive(true);
        if (setSelElOnActivate)
        {
            UIMaster.main.SetSelectedElement(GetDefaultSelectedEl());
        }
    }
    public void DeactivateForm()
    {
        formObject.SetActive(false);
    }
    public UIElement GetDefaultSelectedEl()
    {
        return elements[defaultSelElementIndx];
    }
    public (UIElement, float) GetNearestElementInDirection(UIElement curSelectedEl, Vector2 direction, float curBestVal, UIElement curBestEl = null)
    {
        foreach (UIElement element in elements)
        {
            if (element == curSelectedEl || !element.rectTrans.gameObject.active) { continue; }
            Vector2 elDisp = element.rectTrans.position - curSelectedEl.rectTrans.position;
            Vector2 elDir = elDisp.normalized; float elDist = elDisp.magnitude;
            float dirDist = Vector2.Distance(elDir, direction);
            if (dirDist < 0.5f)
            {
                float nval = (dirDist * 2) + elDist;
                if (nval < curBestVal || curBestEl == null)
                {
                    curBestEl = element;
                    curBestVal = nval;
                }
            }
        }
        return (curBestEl, curBestVal);
    }
    public (UIElement, float) GetNearestElementInDirection(Vector2 curPos, Vector2 direction, float curBestVal, UIElement curBestEl = null)
    {
        foreach (UIElement element in elements)
        {
            if (!element.rectTrans.gameObject.active){ continue; }
            Vector2 elDisp = (Vector2)element.rectTrans.position - curPos;
            Vector2 elDir = elDisp.normalized; float elDist = elDisp.magnitude;
            float dirDist = Vector2.Distance(elDir, direction);
            if (dirDist < 0.5f)
            {
                float nval = (dirDist * 2) + elDist;
                if (nval < curBestVal || curBestEl == null)
                {
                    curBestEl = element;
                    curBestVal = nval;
                }
            }
        }
        return (curBestEl, curBestVal);
    }
}
