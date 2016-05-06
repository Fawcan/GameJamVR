using UnityEngine;
using System.Collections;

public class ModulesSimSettings : ScriptableObject
{
    public float UpdatesPerSecond;
    public Vector3 Gravity;
    public float Mass;
    [Range(0, 1)]
    public float Damping;
}
