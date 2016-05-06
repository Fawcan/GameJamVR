using UnityEngine;
using System.Collections;
using System;

public class ViveControllerBase : MonoBehaviour
{
    [SerializeField]
    private GameObject PointerRayPrefab;
    [SerializeField]
    private bool HideTrackhat;

    [NonSerialized]
    public int ControlIndex = -1;
    [NonSerialized]
    public Transform Trans;
    [NonSerialized]
    public Transform PointerTrans;
    [NonSerialized]
    public bool ShowPointer;

    private bool TrackhatHidden;

    void Awake()
    {
        Trans = transform;

        var instance = Instantiate(PointerRayPrefab);
        PointerTrans = instance.transform;
        PointerTrans.SetParent(Trans, false);
        PointerTrans.gameObject.SetActive(false);
    }

    void Update()
    {
        PointerTrans.gameObject.SetActive(ShowPointer);
        ShowPointer = false;

        DoHideTrackhat();
    }

    private void DoHideTrackhat()
    {
        if (TrackhatHidden == false && HideTrackhat)
        {
            var renderComp = GetComponentInChildren<SteamVR_RenderModel>();
            renderComp.updateDynamically = false;

            foreach (var comp in GetComponentsInChildren<Transform>())
            {
                if (comp.gameObject.name.ToLower() == "trackhat")
                {
                    TrackhatHidden = true;
                    comp.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetDeviceIndex(int index)
    {
        if (Enum.IsDefined(typeof(SteamVR_TrackedObject.EIndex), index))
            ControlIndex = index;
    }

    public void ShowControllerPointer()
    {
        ShowPointer = true;
    }

    public void HapticPulse(int durationMS)
    {
        SteamVR_Controller.Input(ControlIndex).TriggerHapticPulse((ushort)durationMS);
    }
}
