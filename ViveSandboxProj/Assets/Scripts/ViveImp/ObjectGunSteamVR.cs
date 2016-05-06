using UnityEngine;
using System.Collections;
 
public class ObjectGunSteamVR : ObjectGun, SteamVRInput
{
    private bool FireTrigger;
    private bool FireTriggerDown;

    protected override bool FireTriggerPress()
    {
        return FireTrigger;
    }
    protected override bool FireTriggerPressedDown()
    {
        return FireTriggerDown;
    }

    public void UpdateInput(int ctrlIndxe)
    {
        FireTrigger = SteamVR_Controller.Input(ctrlIndxe).GetPress(SteamVR_Controller.ButtonMask.Trigger);
        FireTriggerDown = SteamVR_Controller.Input(ctrlIndxe).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
}
