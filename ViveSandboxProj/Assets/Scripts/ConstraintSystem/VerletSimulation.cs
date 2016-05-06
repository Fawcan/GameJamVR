using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class VerletSimulation : ScriptableObject
{
    [System.Serializable]
    public struct LayerData
    {
        public string Name;
        public int Iterations;
        public bool Reversed;
        public bool ConstraintsFirst;
    }

    public int UpdatesPerSecond;
    public List<string> LayerNames = new List<string>();
    public List<LayerData> LayerSettings = new List<LayerData>();
}
