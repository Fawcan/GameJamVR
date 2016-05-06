using UnityEngine;
using System.Collections;

public class VRMenuSteamVR : VRMenuSystem
{
    protected override bool MenuTriggerPress()
    {
        return SteamVR_Controller.Input(MenuController.ControlIndex).GetPress(SteamVR_Controller.ButtonMask.Grip);
    }
    protected override bool MenuTriggerPressedDown()
    {
        return SteamVR_Controller.Input(MenuController.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Grip);
    }
    protected override bool MenuClickPressedDown()
    {
        return SteamVR_Controller.Input(PointerController.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
    protected override void GiveMenuOpenFeedback()
    {
        SteamVR_Controller.Input(MenuController.ControlIndex).TriggerHapticPulse(1000);
    }
}
