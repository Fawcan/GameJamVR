using UnityEngine;
using System.Collections;
using System;

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


    protected virtual void AwakePickupable() { }
}
