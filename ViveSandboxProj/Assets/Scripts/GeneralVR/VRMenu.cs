using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// Handles a menu that you can spawn with your hand, mostly for debugging purposes
/// </summary>
public class VRMenu : MonoBehaviour
{
    enum ButtonStates { Idle, Hovering, Clicked }

    class ButtonData
    {
        public Button ButtonComp;
        public Transform Trans;
        public Func<bool> OnClick;
        public float EvaluationTime;
        public ButtonStates State;
        public ButtonStates LastState;

        public float DefaultZ;
        public float StartZ;
        public float EndZ;
    }
    [Serializable]
    struct ButtonEffectData
    {
        public float Offet;
        public float Duration;
        public AnimationCurve MovementCurve;
    }

    [SerializeField]
    private RectTransform ButtonParent;
    [SerializeField]
    private Button ButtonPrefab;
    [SerializeField]
    private ButtonEffectData HoverZEffect;
    [SerializeField]
    private ButtonEffectData ClickZEffect;

    private List<ButtonData> Buttons = new List<ButtonData>();

    void Awake()
    {
    }

    void Update()
    {
        foreach (var buttonData in Buttons)
        {
            var lastState = buttonData.State;
            switch (buttonData.State)
            {
                case ButtonStates.Idle:
                    if (buttonData.LastState != ButtonStates.Idle)
                    {
                        buttonData.EvaluationTime = Time.time;
                        buttonData.StartZ = buttonData.Trans.localPosition.z;
                        buttonData.EndZ = buttonData.DefaultZ;
                    }
                    else
                    {
                        UpdateButtonEffect(buttonData, HoverZEffect);
                    }
                    break;
                case ButtonStates.Hovering:
                    UpdateButtonEffect(buttonData, HoverZEffect);
                    buttonData.State = ButtonStates.Idle; // The hover state is reset every frame
                    break;
                case ButtonStates.Clicked:
                    float normTime = UpdateButtonEffect(buttonData, ClickZEffect);
                    if (normTime >= 1)
                        buttonData.State = ButtonStates.Idle;
                    break;
            }
            buttonData.LastState = lastState;
        }
    }

    private float UpdateButtonEffect(ButtonData buttonData, ButtonEffectData effectData)
    {
        var normTime = 1 - Mathf.Clamp01(((buttonData.EvaluationTime + effectData.Duration) - Time.time) / effectData.Duration);
        var zPos = Mathf.Lerp(buttonData.StartZ, buttonData.EndZ, effectData.MovementCurve.Evaluate(normTime));
        buttonData.Trans.localPosition = new Vector3(buttonData.Trans.localPosition.x, buttonData.Trans.localPosition.y, zPos);

        return normTime;
    }

    public void RegisterButton(string name, Func<bool> onClick)
    {
        var buttonInst = Instantiate(ButtonPrefab);
        buttonInst.transform.SetParent(ButtonParent, false);
        buttonInst.GetComponentInChildren<Text>().text = name;

        Buttons.Add(new ButtonData()
        {
            ButtonComp = buttonInst,
            Trans = buttonInst.transform,
            DefaultZ = buttonInst.transform.localPosition.z,
            OnClick = onClick,
        });
    }

    public void OnButtonHover(Button button)
    {
        foreach (var buttonData in Buttons)
        {
            if (buttonData.ButtonComp == button && buttonData.State != ButtonStates.Clicked)
            {
                if (buttonData.LastState != ButtonStates.Hovering)
                {
                    buttonData.EvaluationTime = Time.time;
                    buttonData.StartZ = buttonData.Trans.localPosition.z;
                    buttonData.EndZ = HoverZEffect.Offet;
                }

                buttonData.State = ButtonStates.Hovering;
                return;
            }
        }
    }
    public void OnButtonClick(Button button, out bool closeMenu)
    {
        closeMenu = false;
        foreach (var buttonData in Buttons)
        {
            if (buttonData.ButtonComp == button)
            {
                buttonData.EvaluationTime = Time.time;
                buttonData.StartZ = buttonData.Trans.localPosition.z;
                buttonData.EndZ = ClickZEffect.Offet;
                buttonData.State = ButtonStates.Clicked;
                closeMenu = buttonData.OnClick();
                return;
            }
        }
    }
}
