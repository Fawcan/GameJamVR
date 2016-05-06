using UnityEngine;
using System.Collections;


/// <summary>
/// Controlls the indicator on a watch by a normalized value
/// </summary>
public class StopWatchIndicator : ValueIndicator
{
    [System.Serializable]
    struct WatchSettingsData
    {
        public float IndicatorMinAngle;
        public float IndicatorMaxAngle;

        public Vector3 LocalRotateAxis;
    }
    [SerializeField]
    private WatchSettingsData WatchSettings;

    private float _Value;
    private float _AngleFactor;

    void Start()
    {
        _AngleFactor = WatchSettings.IndicatorMaxAngle - WatchSettings.IndicatorMinAngle;
    }

    void Update()
    {
        float angle = WatchSettings.IndicatorMinAngle + (_AngleFactor * _Value);
        transform.localRotation = Quaternion.Euler(WatchSettings.LocalRotateAxis * angle);
    }

    public override void SetValue(float value)
    {
        _Value = value;
    }
}
