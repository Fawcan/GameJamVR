using UnityEngine;
using System.Collections;
using System;

public class CtrlFaceMenuSteamVR : CtrlFaceMenuSystem
{
    [SerializeField]
    private ViveControllerBase Controller;

    protected override bool FaceButtonPress()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPress(SteamVR_Controller.ButtonMask.Touchpad);
    }
    protected override bool FaceButtonPressUp()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
    }
    protected override bool FaceButtonsActive()
    {
        var value = SteamVR_Controller.Input(Controller.ControlIndex).GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        return value.magnitude > 0.3f;

    }
    protected override Vector2 FaceButtonDirection()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
    }
    protected override void GiveButtonsActiveFeedback()
    {
    }
}
