using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewInput : MonoBehaviour
{
    public static NewInput main;
    public static PlayerControls controls;

    private void Awake()
    {
        if (main != null) { Destroy(this.gameObject); return; }
        main = this;
        controls = new PlayerControls();
        controls.Enable(); // Enables all actions in all maps
    }
    public static Vector2 GetMovement()
    {
        return controls.Gameplay.PrimaryMovement.ReadValue<Vector2>();
    }
    public static Vector2 GetSecondaryMovement()
    {
        return controls.Gameplay.SecondaryMovement.ReadValue<Vector2>();
    }
    public static Vector2 GetDPad()
    {
        return controls.Gameplay.DPad.ReadValue<Vector2>();
    }
    public static Vector2 GetUIMovement()
    {
        return controls.UI.PrimaryMovement.ReadValue<Vector2>();
    }
}
