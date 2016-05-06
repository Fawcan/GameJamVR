using UnityEngine;
using System.Collections;
using System;
using Valve.VR;


class DiskCatcherSteamVR : DiskCatcher
{
    [SerializeField]
    private ViveControllerBase Controller;

    protected override bool TriggerDown()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }

    protected override bool TriggerPressed()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
    }

    protected override bool TriggerUp()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressUp(SteamVR_Controller.ButtonMask.Trigger);
        
    }

}

