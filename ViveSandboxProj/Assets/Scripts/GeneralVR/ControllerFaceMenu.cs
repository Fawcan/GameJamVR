using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// Menu for the touchpad on the vive or a analog stick (like on the touch)
/// </summary>
public class ControllerFaceMenu : MonoBehaviour
{
    class ButtonData
    {
        public RectTransform Trans;
        public Image HightlighEffect;
        public Vector2 Direction;
        public Action OnClick;
    }

    [SerializeField]
    private RectTransform ButtonParent;
    [SerializeField]
    private RectTransform ButtonPrefab;
    [SerializeField]
    private float ButtonOffset;

    private Transform Trans;
    private float ButtonAngle;

    private List<ButtonData> Buttons = new List<ButtonData>();

    void Awake()
    {
        Trans = transform;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void RegisterButton(string name, Action onClick)
    {
        var buttonInst = Instantiate(ButtonPrefab);
        buttonInst.transform.SetParent(ButtonParent, false);
        buttonInst.GetComponentInChildren<Text>().text = name;

        AddButton(onClick, buttonInst);
        GenerateButtonFace();
    }
    public void RegisterButton(Sprite icon, Action onClick)
    {
        var buttonInst = Instantiate(ButtonPrefab);
        buttonInst.transform.SetParent(ButtonParent, false);
        buttonInst.GetComponentInChildren<Image>().sprite = icon;

        AddButton(onClick, buttonInst);
        GenerateButtonFace();
    }
    public void ClearButtons()
    {
        foreach (var button in Buttons)
            Destroy(button.Trans.gameObject);

        Buttons.Clear();
    }

    private void AddButton(Action onClick, RectTransform buttonInst)
    {
        var image = buttonInst.GetComponentInChildren<Image>();
        image.gameObject.SetActive(false);
        Buttons.Add(new ButtonData()
        {
            Trans = (RectTransform)buttonInst.transform,
            HightlighEffect = image,
            OnClick = onClick,
        });
    }
    private void GenerateButtonFace()
    {
        float totButtons = Buttons.Count;
        ButtonAngle = (Mathf.PI * 2f) / totButtons;

        for (int i = 0; i < Buttons.Count; i++)
        {
            float angle = i * ButtonAngle;
            Vector3 localPos = Vector3.zero;
            localPos.x = Mathf.Cos(angle) * ButtonOffset;
            localPos.y = Mathf.Sin(angle) * ButtonOffset;
            Buttons[i].Direction = localPos.normalized;
            Buttons[i].Trans.anchoredPosition = localPos;
        }
    }

    public void SetInput(bool buttonsActive, Vector2 direction)
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            float diff = Mathf.Deg2Rad * Vector2.Angle(direction, Buttons[i].Direction);
            if (diff < (ButtonAngle / 2))
                Buttons[i].HightlighEffect.gameObject.SetActive(buttonsActive);
            else
                Buttons[i].HightlighEffect.gameObject.SetActive(false);
        }
    }
    public void InputReleased()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            if (Buttons[i].HightlighEffect.gameObject.activeSelf)
            {
                Buttons[i].HightlighEffect.gameObject.SetActive(false);
                Buttons[i].OnClick();
            }
        }
    }

    static ControllerFaceMenu _Instance;
    public static ControllerFaceMenu Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<ControllerFaceMenu>();

            return _Instance;
        }
    }
}
