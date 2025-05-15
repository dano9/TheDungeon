using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputControlType
{
    keyboard, joypad
}
public enum JoypadButtons
{
    A//,B,X,Y,LeftBumper,RightBumper,Back,Start,LeftStickPress,RightStickPress,LeftStick,RightStick,DPadUp,DPadDown,DPadLeft,DPadRight,LeftTrigger,RightTrigger,P1,P2,P3,P4,ProfileSwitchButton
}
[System.Serializable]
public struct InputRef
{
    public string name;
    public KeyCode keyCode;
    public bool isAxis;
    public bool isJoypad;
    public InputRef(string name, KeyCode keyCode, bool isAxis, bool isJoypad) {this.name=name;this.keyCode=keyCode;this.isAxis=isAxis; this.isJoypad =isJoypad;}
    public float GetValue(bool rawAxis=false)
    {
        return 0;
        // if (isAxis) {return rawAxis ? Input.GetAxisRaw(name) : Input.GetAxis(name);}
        // else if (isJoypad) {return Input.GetButton(name) ? 1 : 0;}
        // else {return Input.GetKey(keyCode) ? 1 : 0;}
    }
}
[System.Serializable]
public class GameplayInput
{
    public string name;
    public InputRef[] inputRefs;
    public bool axisOnly;
    public string[] defaultInputs;    
    //public bool isAxis;
    public int axiDir; //E.g: left or right on the joystick (-1 or 1)
    public bool wasPressed;
    bool isPressed; int pressedState;
    public float threshold=0.25f;
    

    public float GetValue(int ckType = -1, bool rawAxis=false)
    {
        bool allCks = ckType < 0;
        if (allCks) {ckType = (int)ControlsManager.curControlType;} 
        float retVal = inputRefs[ckType].GetValue(rawAxis);
        if (Mathf.Abs(retVal) < threshold) {retVal=0f;}
        if (retVal == 0 && allCks) {retVal = GetValue(1,rawAxis);}
        return retVal;
    }
    public bool IsPressed(int ckType = -1)
    {
        return pressedState == 1 || pressedState == 2;
        // if (axisOnly) {return false;}
        // wasPressed = GetValue(ckType, true) * axiDir > 0.25f;
        // return wasPressed;
    }
    public bool JustPressed()
    {
        return pressedState == 1;
    }
    public bool JustReleased()
    {
        return pressedState == 3;
    }
    public void DeterminePressedState(int ckType=-1) //0 Not pressed, 1 justPressed, 2 pressed, 3 justReleased
    {
        wasPressed = isPressed;
        isPressed = GetValue(ckType, true) * axiDir > 0.25f;
        if (!wasPressed) {if (isPressed) {pressedState= 1;} else {pressedState= 0;}}
        else {if (!isPressed) {pressedState= 3;} else {pressedState= 2;}}
    }
}
[ExecuteInEditMode]
public class ControlsManager : MonoBehaviour
{
    public static ControlsManager main;
    public static InputControlType curControlType=0;
    public KeyCode[] includedKeyCodes;
    public InputRef[] inputRefsArray;
    public Dictionary<string,InputRef> inputs = new Dictionary<string, InputRef>();
    public string[] axisNames;
    public GameplayInput[] gameplayInputs;
    public static Dictionary<string, GameplayInput> gameInputDict = new Dictionary<string, GameplayInput>();
    public bool setupInputs;
    public KeyCode[] keycodeExludeMask;
    public void Start()
    {
        foreach (InputRef inputRef in inputRefsArray) {inputs[inputRef.name] = inputRef;}
        SetGameplayInputsToDefault();
    }
    public void Update()
    {
        if (Application.isPlaying)
        {
            //DetectButtonPress();

            foreach (GameplayInput gInput in gameplayInputs) {gameInputDict[gInput.name].DeterminePressedState();}

            if (gameplayInputs[0].JustPressed())
            {
                Debug.Log(gameplayInputs[0].name + ": is Pressed");// + gameplayInputs[0].GetValue());
            }
            if (true)
            {
                // if (Input.GetKeyDown(KeyCode.Joystick1Button0))
                // {
                //     Debug.Log("Pressed A");
                // }
            }
        }
        else
        {
            if (setupInputs)
            {
                setupInputs=false;
                SetupInputs();
            }
        }
    }
    // void LateUpdate()
    // {
    //     //Reset for justPressed stuff
    //     if (Application.isPlaying)
    //     {
    //         foreach (GameplayInput gInput in gameplayInputs)
    //         {
    //             if (gInput.axisOnly) {return;}
    //             //gInput.IsPressed();
    //             //IsPressed(gInput.name);
    //         }
    //     }
    // }
    public void SetupInputs()
    {
        KeyCode[] keyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        List<KeyCode> includedKeycodes = new List<KeyCode>();
        foreach(KeyCode keyCode in keyCodes)
        {
            bool masked = false;
            foreach (KeyCode mask in keycodeExludeMask)
            {
                if (keyCode == mask) {masked = true;}
            }
            if (!masked) {includedKeycodes.Add(keyCode);}
        }
        this.includedKeyCodes = includedKeycodes.ToArray();
        int ikcLength = this.includedKeyCodes.Length;
        string[] joypadButtons = System.Enum.GetNames(typeof(JoypadButtons));
        inputRefsArray = new InputRef[ikcLength + axisNames.Length + joypadButtons.Length];
        for (int r = 0; r < inputRefsArray.Length; r++)
        {
            if (r < ikcLength)
            {
                inputRefsArray[r] = new InputRef(includedKeyCodes[r].ToString(), includedKeyCodes[r], false,false);
            }
            else if (r < ikcLength + axisNames.Length)
            {
                int a = r - ikcLength;
                inputRefsArray[r] = new InputRef(axisNames[a], KeyCode.None, true,false);
                Debug.Log("ADDED AXIS: " + axisNames[a]);
            }
            else
            {
                int a = r - ikcLength - axisNames.Length;
                inputRefsArray[r] = new InputRef(joypadButtons[a], KeyCode.None, false,true);
                Debug.Log("ADDED BUTTON: " + joypadButtons[a]);
            }
        }
    }
    public void SetGameplayInputsToDefault()
    {
        foreach (GameplayInput gInput in gameplayInputs)
        {
            gInput.inputRefs = new InputRef[gInput.defaultInputs.Length];
            for (int di = 0; di < gInput.defaultInputs.Length; di++)
            {
                gInput.inputRefs[di] = inputs[gInput.defaultInputs[di]];
            }
            gameInputDict[gInput.name] = gInput;
        }
    }
    public static KeyCode DetectKeyPress()
    {
        KeyCode pressedKey = KeyCode.None;
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            // if (Input.GetKeyDown(key))
            // {
            //     Debug.Log("Key pressed: " + key);
            //     pressedKey=key;
            //     break;
            // }
        }
        return pressedKey;
    }
    public static string DetectButtonPress()
    {
        string pressedButton = null;
        foreach (string button in System.Enum.GetNames(typeof(JoypadButtons)))
        {
            // if (Input.GetButtonDown(button))
            // {
            //     Debug.LogError("Button pressed: " + button);
            //     pressedButton=button;
            //     break;
            // }
        }
        return pressedButton;
    }



    public static float GetAxis(string gIName)
    {return gameInputDict[gIName].GetValue();}
    public static bool IsPressed(string gIName)
    {return gameInputDict[gIName].IsPressed();}
    public static bool JustPressed(string gIName)
    {return gameInputDict[gIName].JustPressed();}
    public static bool JustReleased(string gIName)
    {return gameInputDict[gIName].JustReleased();}
}
