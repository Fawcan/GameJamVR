using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Old pickup system, used Unitys parent-child system, which sucks! Use V2 instead
/// </summary>
public abstract class PickupGraber : MonoBehaviour
{
    protected virtual float VelocitySampleDelay { get { return 0.02f; } }

    public Pickupable PickupableInHand
    {
        get { return _State.PickupInHand; }
    }
    public Rigidbody PickupableInHandBody
    {
        get { return _State.PickupInHandBody; }
    }
    public Transform PickupableInHandTrans
    {
        get { return _State.PickupInHandTrans; }
    }
    public Transform Trans
    {
        get { return _State.Trans; }
    }

    private List<PickupGraber> OtherGrabers = new List<PickupGraber>();

    protected struct StateData
    {
        public Transform Trans;
        public Vector3 CurrentVelocity;
        public Vector3 LastPosition;
        public float LastSampleTime;

        public bool JustPickedUp;
        public Pickupable PickupInHand;
        public Rigidbody PickupInHandBody;
        public Transform PickupInHandTrans;
        public Transform PickupInHandOldTrans;
    }
    protected StateData _State;

    protected abstract bool GrabTriggerPressedDown();
    protected abstract bool GrabTriggerPressedUp();

    void Awake()
    {
        _State.Trans = transform;
    }
    void Start()
    {
        UpdateVelocity();
    }
    void OnEnable()
    {
        OtherGrabers.Clear();
        OtherGrabers.AddRange(FindObjectsOfType<PickupGraber>().Where(e => e != this).ToArray());
        foreach (var Graber in OtherGrabers)
            Graber.AddGraber(this);
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
            UpdateObjectInput();

            bool release = GrabTriggerPressedDown();
            if (release)
                DropPickupable();
        }

        UpdateVelocity();
    }

    protected abstract void UpdateObjectInput();

    public void DropPickupable()
    {
        _State.PickupInHand.OnDropped();

        _State.PickupInHandBody.isKinematic = false;
        _State.PickupInHandTrans.parent = _State.PickupInHandOldTrans;

        _State.PickupInHand = null;
        _State.PickupInHandTrans = null;
        _State.PickupInHandOldTrans = null;
    }

    public void AddGraber(PickupGraber Graber)
    {
        if (OtherGrabers.Contains(Graber) == false)
            OtherGrabers.Add(Graber);
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
    public void GrabObbject(Pickupable pickupable, bool gotObjectByGrab, Transform objParent)
    {
        pickupable.OnGrabbed();

        _State.JustPickedUp = true;
        _State.PickupInHand = pickupable;
        _State.PickupInHandBody = pickupable.Body;
        _State.PickupInHandTrans = pickupable.Trans;
        _State.PickupInHandOldTrans = objParent;

        _State.PickupInHandBody.isKinematic = true;
        _State.PickupInHandTrans.parent = Trans;

        _State.PickupInHandTrans.localPosition = -pickupable.ControllerMountingPos.localPosition;
        _State.PickupInHandTrans.localRotation = Quaternion.Inverse(pickupable.ControllerMountingPos.localRotation);

        var vec = pickupable.ControllerMountingPos.position - pickupable.Trans.position;
        _State.PickupInHandTrans.localPosition = -Trans.InverseTransformVector(vec);
    }
    public void TranferObjectTo(PickupGraber graber, bool gotObjectByGrab)
    {
        _State.PickupInHandTrans.parent = _State.PickupInHandOldTrans;

        _State.PickupInHand.OnDropped();

        graber.GrabObbject(_State.PickupInHand, gotObjectByGrab, _State.PickupInHandTrans.parent);

        _State.PickupInHand = null;
        _State.PickupInHandTrans = null;
        _State.PickupInHandOldTrans = null;
    }
    void OnTriggerStay(Collider col)
    {
        if (_State.PickupInHand != null)
            return;

        bool grab = GrabTriggerPressedDown();
        if (grab)
        {
            Pickupable pickupable = null;
            var objSpawner = col.GetComponent<ObjectSpawner>();
            if (objSpawner != null)
                pickupable = objSpawner.RequestPickupableInstance(Trans);

            if (pickupable == null)
                pickupable = col.GetComponent<Pickupable>();

            var body = col.GetComponent<Rigidbody>();
            bool alreadyInHand = false;
            foreach (var graber in OtherGrabers)
            {
                if (graber.PickupableInHand == null)
                    continue;

                if (graber.PickupableInHand == pickupable)
                {
                    alreadyInHand = true;
                    graber.TranferObjectTo(this, true);
                    break;
                }
            }

            if (alreadyInHand == false && body != null)
                GrabObbject(pickupable, true, body.transform.parent);
        }
    }
}
