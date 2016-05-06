using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Used for all the different scripts that will interract with a object-holding-system,
/// like Bow interract with object grabing so it can sense arrows that the user holds in his/hers hand
/// </summary>
/// <typeparam name="T">Type of object interractor</typeparam>
public abstract class HandInteractionBase<T> : MonoBehaviour where T : Object
{
    protected List<T> Grabers = new List<T>();
    protected List<HandInteractionBase<T>> Other = new List<HandInteractionBase<T>>();

    void OnEnable()
    {
        Other.Clear();
        Other.AddRange(FindObjectsOfType<HandInteractionBase<T>>().Where(e => e != this).ToArray());

        foreach (var obj in Other)
            obj.AddOther(this);

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
    public void AddOther(HandInteractionBase<T> other)
    {
        if (Other.Contains(other) == false)
            Other.Add(other);
    }
}
