using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public abstract class HandInterractionBase<T> : MonoBehaviour where T : Object
{
    protected List<T> Grabers = new List<T>();

    void OnEnable()
    {
        Grabers.Clear();
        Grabers.AddRange(FindObjectsOfType<T>().Where(e => e != this).ToArray());
        OnEnableBase();
    }
    protected virtual void OnEnableBase() { }

    public void AddGraber(T graber)
    {
        if (Grabers.Contains(graber) == false)
            Grabers.Add(graber);
    }
}
