using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Lets you pickup 'Pickupable'
/// </summary>
public abstract class PickupGraberV2 : MonoBehaviour
{
    protected virtual float VelocitySampleDelay { get { return 0.02f; } }

    Transform Trans;

    protected abstract bool GrabTriggerPressedDown();
    protected abstract bool GrabTriggerPressedUp();

    protected struct StateData
    {
        public bool JustPickedUp;
        public Pickupable PickupInHand;
        public Transform PickupInHandTrans;

        public Vector3 HoldPosOffset;
        public Quaternion HoldRotOffset;
        public Quaternion ParentStartRot;

        public Vector3 CurrentVelocity;
        public Vector3 LastPosition;
        public float LastSampleTime;
    }
    protected StateData _State;

    private List<PickupGraberV2> OtherGrabers = new List<PickupGraberV2>();

    public Pickupable PickupableInHand
    {
        get { return _State.PickupInHand; }
    }

    void Awake()
    {
        Trans = transform;
    }
    void Start()
    {
        UpdateVelocity();
    }
    void OnEnable()
    {
        OtherGrabers.Clear();
        OtherGrabers.AddRange(FindObjectsOfType<PickupGraberV2>().Where(e => e != this).ToArray());
        foreach (var Graber in OtherGrabers)
            Graber.AddGraber(this);
    }
    public void AddGraber(PickupGraberV2 Graber)
    {
        if (OtherGrabers.Contains(Graber) == false)
            OtherGrabers.Add(Graber);
    }
    void Update()
    {
        if (_State.JustPickedUp)
        {
            if (GrabTriggerPressedUp())
                _State.JustPickedUp = false;
        }
        else if (_State.PickupInHand != null)
        {
            UpdateObjectTranform();
            UpdateObjectInput();

            bool release = GrabTriggerPressedDown();
            if (release)
                DropPickupable();
        }

        UpdateVelocity();
    }
    protected abstract void UpdateObjectInput();

    private void UpdateObjectTranform()
    {
        // This dublicated the native child - parent behaviour in Unity, without scaling
        var parentMatrix = Matrix4x4.TRS(Trans.position, Trans.rotation, Trans.lossyScale);
        _State.PickupInHandTrans.position = parentMatrix.MultiplyPoint3x4(_State.HoldPosOffset);
        _State.PickupInHandTrans.rotation = (Trans.rotation * Quaternion.Inverse(_State.ParentStartRot)) * _State.HoldRotOffset;
    }
    private void UpdateVelocity()
    {
        if (_State.PickupInHand != null)
        {
            _State.CurrentVelocity = (_State.PickupInHandTrans.position - _State.LastPosition) * (1 / VelocitySampleDelay);
            _State.LastSampleTime = Time.time;
            _State.LastPosition = _State.PickupInHandTrans.position;
        }
    }
    void OnTriggerStay(Collider col)
    {
        if (_State.PickupInHand != null)
            return;
        
        bool grab = GrabTriggerPressedDown();
        if (grab)
        {
            Rigidbody grabbedBody = null;
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
            Pickupable pickupable = null;
            //var objSpawner = col.GetComponent<ObjectSpawner>();
            //if (objSpawner != null)
            //    pickupable = objSpawner.RequestPickupableInstance(Trans);

            if (pickupable == null)
                pickupable = grabbedBody.GetComponent<Pickupable>();

            bool alreadyInHand = false;
            foreach (var graber in OtherGrabers)
            {
                if (graber.PickupableInHand == null)
                    continue;

                if (graber.PickupableInHand == pickupable)
                {
                    alreadyInHand = true;
                    graber.TranferObjectTo(this);
                    break;
                }
            }

            if (alreadyInHand == false && pickupable != null)
                GrabObbject(pickupable);
        }
    }
    public void GrabObbject(Pickupable pickupable)
    {
        pickupable.OnGrabbed();

        _State.JustPickedUp = true;
        _State.PickupInHand = pickupable;
        _State.PickupInHandTrans = pickupable.Trans;

        pickupable.Body.isKinematic = true;

        var oldParent = _State.PickupInHandTrans.parent;
        _State.PickupInHandTrans.parent = Trans;

        _State.PickupInHandTrans.localPosition = -pickupable.ControllerMountingPos.localPosition;
        _State.PickupInHandTrans.localRotation = Quaternion.Inverse(pickupable.ControllerMountingPos.localRotation);

        var vec = pickupable.ControllerMountingPos.position - pickupable.Trans.position;
        _State.PickupInHandTrans.localPosition = -Trans.InverseTransformVector(vec);

        _State.HoldPosOffset = _State.PickupInHandTrans.localPosition;
        _State.HoldRotOffset = _State.PickupInHandTrans.rotation;
        _State.ParentStartRot = Trans.rotation;
        
        _State.PickupInHandTrans.parent = oldParent;
    }
    public void DropPickupable()
    {
        _State.PickupInHand.OnDropped();
        _State.PickupInHand.GetComponent<Rigidbody>().isKinematic = false;

        _State.PickupInHand = null;
        _State.PickupInHandTrans = null;
    }
    void TranferObjectTo(PickupGraberV2 graber)
    {
        _State.PickupInHand.OnDropped();

        graber.GrabObbject(_State.PickupInHand);

        _State.PickupInHand = null;
        _State.PickupInHandTrans = null;
    }
}
