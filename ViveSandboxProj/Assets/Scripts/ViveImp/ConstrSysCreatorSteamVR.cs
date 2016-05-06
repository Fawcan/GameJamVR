using UnityEngine;
using System.Collections;

public class ConstrSysCreatorSteamVR : ConstraintSysCreator
{
    [SerializeField]
    private ViveControllerBase Controller;
    
    protected override bool PlaceObject()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
    protected override bool AttackFirst()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
    protected override bool AttackSecond()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressUp(SteamVR_Controller.ButtonMask.Trigger);
    }
    protected override bool GrabbingObject()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
    }
    protected override bool RemoveObject()
    {
        return SteamVR_Controller.Input(Controller.ControlIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }
}
