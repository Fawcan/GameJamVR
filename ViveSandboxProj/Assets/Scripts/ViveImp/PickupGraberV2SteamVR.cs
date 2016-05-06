using UnityEngine;
using System.Collections;
using System;
using Valve.VR;

public class PickupGraberV2SteamVR : PickupGraberV2
{
    [SerializeField]
    private ViveControllerBase Controller;

    protected override bool GrabTriggerPressedDown()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Grip);
    }
    protected override bool GrabTriggerPressedUp()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressUp(SteamVR_Controller.ButtonMask.Grip);
    }
    protected override void UpdateObjectInput()
    {
        if (_State.PickupInHand is SteamVRInput)
            ((SteamVRInput)_State.PickupInHand).UpdateInput(Controller.ControlIndex);
    }
}
