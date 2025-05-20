using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMaster : MonoBehaviour
{
    public static UIMaster main;
    public static int elementIdCounter = 0;
    public UIForm[] forms;
    List<UIForm> activeForms = new List<UIForm>();
    UIElement selectedElement;
    Vector2 lastMDir; float lastMDist;
    float lastSwitchT;

    public void Awake()
    {
        main = this;
        SetUpForms();
    }

    public void SetUpForms()
    {
        activeForms = new List<UIForm>();
        foreach (UIForm form in forms)
        {
            form.SetupForm();
            if (form.isActive)
            {
                form.ActivateForm();
                activeForms.Add(form);
                if (selectedElement == null)
                {
                    selectedElement = form.GetDefaultSelectedEl();
                }
            }
        }
    }

    public void SetSelectedElement(UIElement newSelEl)
    {
        UIElement oldSelElement = selectedElement;
        if (oldSelElement != newSelEl)
        {
            if (oldSelElement != null)
            {
                oldSelElement.Deselect();
            }
            selectedElement = newSelEl;
            selectedElement.Select();
            lastSwitchT = Time.time;
        }
    }
    float curSwitchInterval = 0.45f;
    public void Update()
    {
        Vector2 inputM = NewInput.GetUIMovement();
        float imMag = inputM.magnitude; Vector2 imNorm = inputM.normalized;
        if (imMag > 0.3f && (lastMDist <= 0.3f || Vector2.Distance(imNorm, lastMDir) > 0.5f) || Time.time - lastSwitchT > curSwitchInterval)
        {
            curSwitchInterval = Mathf.MoveTowards(curSwitchInterval, 0.1f, 0.08f);
            MoveSelection(inputM);
        }
        if (imMag <= 0.3f || Vector2.Distance(imNorm, lastMDir) > 0.5f)
        {
            curSwitchInterval = 0.45f;
        }
        lastMDist = imMag; lastMDir = imNorm;
    }

    public void MoveSelection(Vector2 moveDir)
    {
        if (selectedElement == null) { return; }
        UIForm selForm = selectedElement.GetParentForm();
        (UIElement, float) tuple = selForm.GetNearestElementInDirection(selectedElement, moveDir, -1, null);
        if (selForm.canMoveIntoOtherForms)
        {
            foreach (UIForm form in activeForms)
            {
                if (form.canMoveIntoOtherForms)
                {
                    tuple = form.GetNearestElementInDirection(selectedElement, moveDir, tuple.Item2, tuple.Item1);
                }
            }
        }
        if (tuple.Item1 == null)
        {
            tuple = selForm.GetNearestElementInDirection((Vector2)selectedElement.rectTrans.position - (moveDir * 1000), moveDir, -1, null);
        }
        UIElement nextEl = tuple.Item1;
        if (nextEl != null && nextEl != selectedElement)
        {
            SetSelectedElement(nextEl);
        }
    }
}
