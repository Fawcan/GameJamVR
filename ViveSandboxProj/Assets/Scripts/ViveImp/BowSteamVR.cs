using UnityEngine;
using System.Collections;

public class BowSteamVR : Bow
{
    protected override void TenseningFeedback(int durationMS)
    {
        var gripper = TenseningGrip.GetComponent<ViveControllerBase>();
        gripper.HapticPulse(durationMS);
    }
}
