using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Old grabing system, used Unitys parent-child system, which sucks! Use V2 instead
/// </summary>
public abstract class ObjectGrabing : MonoBehaviour
{
    [SerializeField]
    private bool GrabByCollider;
    [SerializeField]
    private LayerMask GrabableLayers;
    [SerializeField]
    private bool GrabByPointer;

    protected virtual float VelocitySampleDelay { get { return 0.02f; } }

    public Rigidbody ObjectInHand
    {
        get { return _State.ObjectInHand; }
    }
    public Transform ObjectOnHandTrans
    {
        get { return _State.ObjectInHandTrans; }
    }
    public Transform ObjectOnHandOldParent
    {
        get { return _State.ObjectInHandOldParent; }
    }
    public Transform Trans
    {
        get { return _State.Trans; }
    }

    private List<ObjectGrabing> OtherGrabers = new List<ObjectGrabing>();

    struct StateData
    {
        public Transform Trans;
        public Vector3 CurrentVelocity;
        public Vector3 LastPosition;
        public float LastSampleTime;

        public bool GotObjectByGrab;
        public Rigidbody ObjectInHand;
        public Transform ObjectInHandTrans;
        public Transform ObjectInHandOldParent;
    }
    StateData _State;

    protected abstract bool GrabTriggerPress();
    protected abstract bool GrabTriggerPressedDown();

    void Awake()
    {
        _State.Trans = transform;
        UpdateVelocity();
    }
    void OnEnable()
    {
        OtherGrabers.Clear();
        OtherGrabers.AddRange(FindObjectsOfType<ObjectGrabing>().Where(e => e != this).ToArray());
        foreach (var graber in OtherGrabers)
            graber.AddGraber(this);
    }
    void Update()
    {
        bool holding = GrabTriggerPress();
        if (_State.ObjectInHand != null)
        {
            bool justPressed = GrabTriggerPressedDown();
            if (holding == false || (justPressed && _State.GotObjectByGrab == false))
            {
                DropObject();
            }
        }
        else if (holding && GrabByPointer)
        {
            Ray ray = new Ray(Trans.position, Trans.forward);
            foreach (var hitInfo in Physics.RaycastAll(ray))
            {
                DoGrabing(hitInfo.collider);
            }
        }

        if (_State.LastSampleTime + VelocitySampleDelay < Time.time)
            UpdateVelocity();
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
    public void AddGraber(ObjectGrabing graber)
    {
        if (OtherGrabers.Contains(graber) == false)
            OtherGrabers.Add(graber);
    }
    public Rigidbody DropObject()
    {
        if (_State.ObjectInHand == null)
            return null;

        var dropBody = _State.ObjectInHand;

        _State.ObjectInHand.isKinematic = false;
        _State.ObjectInHand.AddForceAtPosition(_State.CurrentVelocity, Trans.position, ForceMode.VelocityChange);
        //_State.ObjectInHand.velocity = _State.CurrentVelocity;
        _State.ObjectInHandTrans.parent = _State.ObjectInHandOldParent;
        
        _State.ObjectInHand = null;
        _State.ObjectInHandTrans = null;
        _State.ObjectInHandOldParent = null;

        return dropBody;
    }
    public void GrabObbject(Rigidbody objBody, bool gotObjectByGrab, Transform objParent)
    {
        _State.GotObjectByGrab = gotObjectByGrab;
        _State.ObjectInHand = objBody;
        _State.ObjectInHandTrans = objBody.transform;
        _State.ObjectInHandOldParent = objParent;

        _State.ObjectInHand.isKinematic = true;
        _State.ObjectInHandTrans.parent = Trans;
    }
    public void TranferObjectTo(ObjectGrabing graber, bool gotObjectByGrab)
    {
        _State.ObjectInHandTrans.parent = _State.ObjectInHandOldParent;

        graber.GrabObbject(_State.ObjectInHand, gotObjectByGrab, _State.ObjectInHandTrans.parent);

        ReleaseObject();
    }

    public void ReleaseObject()
    {
        _State.ObjectInHand = null;
        _State.ObjectInHandTrans = null;
        _State.ObjectInHandOldParent = null;
    }

    void OnTriggerStay(Collider col)
    {
        //Debug.Log("col.gameObject.layer: " + (1 << col.gameObject.layer));
        //Debug.Log("GrabableLayers.value: " + GrabableLayers.value);

        int layer = 1 << col.gameObject.layer;
        var inLayer = (layer & GrabableLayers.value) > 0;
        if (GrabByCollider && inLayer)
            DoGrabing(col);
    }

    private void DoGrabing(Collider col)
    {
        if (_State.ObjectInHand != null || this.enabled == false)
            return;

        if (col.GetComponentInChildren<Pickupable>() != null)
            return;

        bool grab = GrabTriggerPressedDown();
        if (grab)
        {
            Rigidbody grabbedBody = null;
            var objSpawner = col.GetComponent<ObjectSpawner>();
            if (objSpawner != null)
                grabbedBody = objSpawner.RequestObjectInstance(Trans);

            if (grabbedBody == null)
            {
                grabbedBody = col.GetComponentInChildren<Rigidbody>();
                if (grabbedBody == null)
                    grabbedBody = col.GetComponentInParent<Rigidbody>();
            }

            if (grabbedBody == null || grabbedBody.GetComponent<ObjectSpawner>() != null)
                return;

            Transform oldParent;
            bool alreadyInHand = false;
            foreach (var graber in OtherGrabers)
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
                        splitComp.SplitAndGetObject(graber.Trans, Trans, graber.ObjectOnHandTrans, out firstBody, out secondBody);
                        if (firstBody == null)
                            GrabObbject(secondBody, true, graber.ObjectOnHandOldParent);
                        else
                        {
                            oldParent = graber.ObjectOnHandOldParent;
                            var droped = graber.DropObject();
                            Destroy(droped.gameObject);

                            graber.GrabObbject(firstBody, true, oldParent);
                            GrabObbject(secondBody, true, oldParent);
                        }
                    }
                    else
                    {
                        graber.TranferObjectTo(this, true);
                    }
                    break;
                }
            }

            oldParent = grabbedBody.transform.parent;
            if (grabbedBody.transform.parent != null)
            {
                var objHolder = grabbedBody.transform.parent.GetComponent<ObjectHolder>();
                if (objHolder != null)
                {
                    bool exist = false;
                    var parent = objHolder.GetOldParent(grabbedBody.transform, out exist);
                    if (exist)
                        oldParent = parent;
                }
            }

            if (alreadyInHand == false && grabbedBody != null)
                GrabObbject(grabbedBody, true, oldParent);
        }
    }
}
