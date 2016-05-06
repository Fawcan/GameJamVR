using UnityEngine;
using System.Collections;

/// <summary>
/// System thet feeds the on touchpad/analog stick menu with input, this is the component you place on the controller
/// </summary>
public abstract class CtrlFaceMenuSystem : MonoBehaviour
{
    [SerializeField]
    private ControllerFaceMenu MenuPrefab;

    protected abstract bool FaceButtonPress();
    protected abstract bool FaceButtonPressUp();
    protected abstract bool FaceButtonsActive();
    protected abstract Vector2 FaceButtonDirection();
    protected abstract void GiveButtonsActiveFeedback();

    [System.NonSerialized]
    [HideInInspector]
    public ControllerFaceMenu MenuInstance;

    private bool ForceShow;

    void Start ()
    {
        MenuInstance = Instantiate(MenuPrefab);
        MenuInstance.gameObject.SetActive(false);
        MenuInstance.transform.SetParent(transform, false);
        MenuInstance.transform.localPosition = Vector3.zero;
        MenuInstance.transform.rotation = Quaternion.identity;
    }
	
	void Update ()
    {
        bool pressed = FaceButtonPress();
        bool released = FaceButtonPressUp();
        MenuInstance.gameObject.SetActive(pressed || ForceShow);

        if (pressed || ForceShow)
        {
            bool buttonsActive = FaceButtonsActive();
            if (buttonsActive)
                GiveButtonsActiveFeedback();
            MenuInstance.SetInput(buttonsActive, FaceButtonDirection());
        }
        if (released || ForceShow)
        {
            MenuInstance.InputReleased();
        }

        UpdateFunc();

        ForceShow = false;
    }

    protected virtual void UpdateFunc() { }

    public void ShowMenu()
    {
        ForceShow = true;
    }
}
