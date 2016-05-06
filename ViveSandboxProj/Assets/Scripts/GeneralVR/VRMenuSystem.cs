using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Feeds the VRMenu with input, this is the component you place on the controller
/// </summary>
public abstract class VRMenuSystem : MonoBehaviour
{
    [SerializeField]
    private VRMenu MenuPrefab;
    [SerializeField]
    private bool LockMenuInWorldSpace;
    [SerializeField]
    private Transform MenuInWorldOffset;
    [SerializeField]
    protected ViveControllerBase MenuController;
    [SerializeField]
    protected ViveControllerBase PointerController;

    [System.NonSerialized]
    public VRMenu MenuInstance;
    [System.NonSerialized]
    public Transform MenuTrans;
    private bool _CloseUtilNextOpen;

    protected abstract bool MenuTriggerPress();
    protected abstract bool MenuTriggerPressedDown();
    protected abstract bool MenuClickPressedDown();
    protected abstract void GiveMenuOpenFeedback();

    void Awake()
    {
        MenuInstance = Instantiate(MenuPrefab);
        MenuTrans = MenuInstance.transform;
        MenuInstance.gameObject.SetActive(false);

        if (LockMenuInWorldSpace == false)
            MenuInstance.transform.SetParent(transform, false);
    }

    void Update()
    {
        bool justPressed = MenuTriggerPressedDown();
        if (_CloseUtilNextOpen)
        {
            if (justPressed)
                _CloseUtilNextOpen = false;
            else
            {
                if (MenuInstance.gameObject.activeSelf)
                    MenuInstance.gameObject.SetActive(false);
                return;
            }
        }

        UpdateFunc();

        bool pressed = MenuTriggerPress();
        if (pressed)
            PointerController.ShowControllerPointer();

        if (MenuInstance.gameObject.activeSelf != pressed)
        {
            if (LockMenuInWorldSpace && pressed)
            {
                MenuTrans.position = MenuInWorldOffset.position;
                MenuTrans.rotation = MenuInWorldOffset.rotation;
            }
            MenuInstance.gameObject.SetActive(pressed);
        }

        if (justPressed)
            GiveMenuOpenFeedback();

        var ray = new Ray(MenuController.PointerTrans.position, MenuController.PointerTrans.forward);
        foreach (var hitInfo in Physics.RaycastAll(ray))
        {
            var button = hitInfo.collider.GetComponent<Button>();
            if (button != null)
            {
                bool trig = MenuClickPressedDown();
                if (trig)
                    MenuInstance.OnButtonClick(button, out _CloseUtilNextOpen);
                else
                    MenuInstance.OnButtonHover(button);
            }
        }
    }

    public bool MenuIsShowing()
    {
        return MenuInstance.gameObject.activeSelf;
    }

    protected virtual void UpdateFunc() { }
}
