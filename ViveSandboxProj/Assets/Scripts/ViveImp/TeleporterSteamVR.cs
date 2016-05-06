using UnityEngine;
using System.Collections;

public class TeleporterSteamVR : Teleporter
{
    [SerializeField]
    private ViveControllerBase Controller;

    protected override bool HUDButtonPress()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPress(SteamVR_Controller.ButtonMask.Touchpad);
    }
    protected override bool TeleportPressedDown()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
}
