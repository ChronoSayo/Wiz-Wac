using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput;

public class PlayerInput : MonoBehaviour
{
    static public bool Restart
    {
        get { return Input.GetButtonDown("Restart"); }
    }

    static public bool Quit
    {
        get { return Input.GetButton("Quit"); }
    }

    static public float Gas
    {
        get { return Input.GetAxis("Gas"); }
    }

    static public float Vertical
    {
        get { return Input.GetAxis("Vertical"); }
    }

    static public float Steer
    {
        get { return Input.GetAxis("Horizontal"); }
    }

    static public bool JumpOnce
    {
        get { return Input.GetButtonDown("Jump"); }
    }

    static public bool JumpHold
    {
        get { return Input.GetButton("Jump"); }
    }

    static public bool TurboHold
    {
        get { return Input.GetButton("Turbo"); }
    }

    static public bool TurboRelease
    {
        get { return Input.GetButtonUp("Turbo"); }
    }

    static public bool HandBrake
    {
        get { return Input.GetButton("HandBrake"); }
    }

    static public bool DriftHold
    {
        get { return Input.GetButton("Drift"); }
    }

    static public bool DriftDown
    {
        get { return Input.GetButtonDown("Drift"); }
    }

    static public bool DriftUp
    {
        get { return Input.GetButtonUp("Drift"); }
    }

    static public bool UseRune
    {
        get { return Input.GetButtonDown("UseRune"); }
    }

    static public bool AltRune
    {
        get { return Input.GetButtonDown("AltRune"); }
    }

    static public bool UseRuneHold
    {
        get { return Input.GetButton("UseRune"); }
    }

    static public bool AltRuneHold
    {
        get { return Input.GetButton("AltRune"); }
    }

    static public bool CameraResetHold
    {
        get { return Input.GetButton("CameraReset"); }
    }

    static public bool FPSHold
    {
        get { return Input.GetButton("FPS"); }
    }

    static public bool RuneSwitchRight
    {
        get { return Input.GetAxis("RuneSwitch") >= 1; }
    }

    static public bool RuneSwitchLeft
    {
        get { return Input.GetAxis("RuneSwitch") <= -1; }
    }

    static public bool TriggerNeutral
    {
        get { return Input.GetAxis("RuneSwitch") == 0; }
    }

    static public float HelpPad
    {
        get { return Input.GetAxis("HelpPad"); }
    }

    static public bool HelpKeyboard
    {
        get { return Input.GetButton("HelpKeyboard"); }
    }

    static public bool HelpRunesKB
    {
        get { return Input.GetButton("HelpRunesKB"); }
    }

    static public bool GetAnyButtonDown
    {
        get
        {
            return JumpOnce || TurboHold || HandBrake || DriftDown || UseRune || AltRune ||
                RuneSwitchLeft || RuneSwitchRight || HelpPad != 0 || HelpRunesKB;
        }
    }

    /// <summary>
    /// There is a bug in Unity where the game window flickers if no keyboard input is detected. 
    /// This simulates a keypress to prevent that, when player is idle or using gamepad.
    /// </summary>
    static public void SimulateKeyPress()
    {
        InputSimulator.SimulateKeyPress(VirtualKeyCode.F24);
    }
}
