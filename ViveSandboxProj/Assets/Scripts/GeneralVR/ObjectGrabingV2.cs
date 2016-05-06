using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Lets you grab objects and tos them
/// </summary>
public abstract class ObjectGrabingV2 : HandInteractionBase<ObjectGrabingV2>
{
    protected virtual float VelocitySampleDelay { get { return 0.02f; } }

    [SerializeField]
    private bool GrabByCollider;
    [SerializeField]
    private LayerMask GrabableLayers;
    [SerializeField]
    private bool GrabByPointer;

    protected abstract bool GrabTriggerPress();
    protected abstract bool GrabTriggerPressedDown();

    [NonSerialized]
    public Transform Trans;

    struct StateData
    {
        public Rigidbody ObjectInHand;
        public Transform ObjectInHandTrans;

        public Vector3 HoldPosOffset;
        public Quaternion HoldRotOffset;
        public Quaternion ParentStartRot;

        public Vector3 CurrentVelocity;
        public Vector3 LastPosition;
        public float LastSampleTime;
    }
    StateData _State;

    public Rigidbody ObjectInHand
    {
        get { return _State.ObjectInHand; }
    }
    public Transform ObjectInHandTrans
    {
        get { return _State.ObjectInHandTrans; }
    }

    protected override void OnEnableBase()
    {
        foreach (var graber in Grabers)
            graber.AddGraber(this);
        foreach (var other in Other)
            other.AddGraber(this);
    }

    void Awake()
    {
        Trans = transform;
        UpdateVelocity();
    }
    void Update()
    {
        if (_State.ObjectInHand != null)
        {
            UpdateObjectTranform();

            bool holding = GrabTriggerPress();
            bool justPressed = GrabTriggerPressedDown();
            if (holding == false)
            {
                DropObject();
            }
        }

        if (_State.LastSampleTime + VelocitySampleDelay < Time.time)
            UpdateVelocity();
    }

    private void UpdateObjectTranform()
    {
        // This dublicated the native child - parent behaviour in Unity, without scaling
        var parentMatrix = Matrix4x4.TRS(Trans.position, Trans.rotation, Trans.lossyScale);
        _State.ObjectInHandTrans.position = parentMatrix.MultiplyPoint3x4(_State.HoldPosOffset);
        _State.ObjectInHandTrans.rotation = (Trans.rotation * Quaternion.Inverse(_State.ParentStartRot)) * _State.HoldRotOffset;
    }

    private void UpdateVelocity()
    {
        if (_State.ObjectInHand != null)
        {
            _State.CurrentVelocity = (_State.ObjectInHandTrans.position - _State.LastPosition) * (1 / VelocitySampleDelay);
            _State.LastSampleTime = Time.time;
            _State.LastPosition = _State.ObjectInHandTrans.position;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (_State.ObjectInHand != null)
            return;

        int layer = 1 << col.gameObject.layer;
        var inLayer = (layer & GrabableLayers.value) > 0;
        bool grabbing = GrabTriggerPressedDown();
        if (GrabByCollider && grabbing && inLayer)
            DoGrabing(col);
    }

    void DoGrabing(Collider col)
    {
        Rigidbody grabbedBody = null;
        var objSpawner = col.GetComponent<ObjectSpawner>();
        if (objSpawner != null)
            grabbedBody = objSpawner.RequestObjectInstance(Trans);

        if (grabbedBody == null)
        {
            grabbedBody = col.GetComponent<Rigidbody>();

            if (grabbedBody == null)
            {
                Transform parent = col.transform.parent;
                while (parent != null && grabbedBody == null)
                {
                    grabbedBody = parent.GetComponent<Rigidbody>();
                    parent = parent.parent;
                }
            }
        }

        if (grabbedBody == null)
            return;
        
        var pickupable = grabbedBody.GetComponent<Pickupable>();
        if (pickupable != null && pickupable.InHand)
            return;
        
        bool alreadyInHand = false;
        foreach (var graber in Grabers)
        {
            if (graber.ObjectInHand == null)
                continue;

            if (graber.ObjectInHand == grabbedBody)
            {
                alreadyInHand = true;
                var splitComp = grabbedBody.GetComponent<SplitableObject>();
                if (splitComp != null && splitComp.Splited == false)
                {
                    Rigidbody firstBody;
                    Rigidbody secondBody;
                    splitComp.SplitAndGetObject(graber.Trans, Trans, graber.ObjectInHandTrans, out firstBody, out secondBody);

                    if (firstBody == null)
                        GrabObject(secondBody);
                    else
                    {
                        var droped = graber.DropObject();
                        Destroy(droped.gameObject);

                        graber.GrabObject(firstBody);
                        GrabObject(secondBody);
                    }
                }
                else
                {
                    graber.TranferObjectTo(this);
                }
                break;
            }
        }

        if (alreadyInHand == false && grabbedBody != null)
            GrabObject(grabbedBody);
    }

    void GrabObject(Rigidbody grabbedBody)
    {
        if (_State.ObjectInHand != null)
            DropObject();

        grabbedBody.isKinematic = true;

        var oldParent = grabbedBody.transform.parent;
        grabbedBody.transform.parent = Trans;

        _State.HoldPosOffset = grabbedBody.transform.localPosition;
        _State.HoldRotOffset = grabbedBody.transform.rotation;
        _State.ParentStartRot = Trans.rotation;

        grabbedBody.transform.parent = oldParent;

        _State.ObjectInHand = grabbedBody;
        _State.ObjectInHandTrans = grabbedBody.transform;
    }

    Rigidbody DropObject()
    {
        if (_State.ObjectInHand == null)
            return null;

        var body = _State.ObjectInHand;
        _State.ObjectInHand.isKinematic = false;
        _State.ObjectInHand.AddForceAtPosition(_State.CurrentVelocity, Trans.position, ForceMode.VelocityChange);

        _State.ObjectInHand = null;
        _State.ObjectInHandTrans = null;

        return body;
    }
    void TranferObjectTo(ObjectGrabingV2 graber)
    {
        graber.GrabObject(_State.ObjectInHand);

        ReleaseObject();
    }

    public void ReleaseObject()
    {
        _State.ObjectInHand = null;
        _State.ObjectInHandTrans = null;
    }
}
