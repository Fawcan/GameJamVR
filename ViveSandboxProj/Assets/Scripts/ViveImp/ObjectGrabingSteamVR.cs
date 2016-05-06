using UnityEngine;
using System.Collections;
using System;

public class ObjectGrabingSteamVR : ObjectGrabing
{
    [SerializeField]
    private ViveControllerBase Controller;

    protected override bool GrabTriggerPress()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
    }
    protected override bool GrabTriggerPressedDown()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
}
