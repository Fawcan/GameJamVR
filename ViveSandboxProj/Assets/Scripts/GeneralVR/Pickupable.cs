using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// An objects that you can pick up in hand and hold in hand with out holding a button
/// With 'ControllerMountingPos' you choose the relative position and orientation of the object in hand
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    [SerializeField]
    public Transform ControllerMountingPos;

    [HideInInspector]
    [NonSerialized]
    public Transform Trans;
    [HideInInspector]
    [NonSerialized]
    public Rigidbody Body;

    public bool InHand { get; private set; }

    void Awake()
    {
        Trans = transform;
        Body = GetComponent<Rigidbody>();

        AwakePickupable();
    }
    protected virtual void AwakePickupable() { }

    public void OnGrabbed()
    {
        InHand = true;
        OnGrabbedObject();
    }
    public virtual void OnGrabbedObject() { }

    public void OnDropped()
    {
        InHand = false;
        OnDroppedObject();
    }
    public virtual void OnDroppedObject() { }
}
