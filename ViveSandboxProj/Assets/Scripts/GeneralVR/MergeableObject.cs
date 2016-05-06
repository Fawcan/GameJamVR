using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This will let you merge gameobjects with rigidbodies
/// </summary>
public class MergeableObject : HandInteractionBase<ObjectGrabingV2>
{
    public enum MergableObjectTypes { AnyObject, SpecTag, SpecObject }
    public MergableObjectTypes MergableObjectType;

    [HideInInspector]
    public string MergeTag;
    [HideInInspector]
    public Rigidbody MergeObject;

    public bool UseMergeRail;
    public bool DestroyOnMerge;
    public bool MergeOnRelease;
    public bool PermanentlyMerge;
    public float MergeDistance;

    [HideInInspector]
    public Transform MergeRailStart;
    [HideInInspector]
    public Transform MergeTarget;
    [HideInInspector]
    public float MergeRailThresholdDistance;
    [HideInInspector]
    public float MergeRailThresholdAngle;


    public float DistanceThreshold;

    Transform Trans;

    struct StateData
    {
        public int Merged;

        public Vector3 RailDirection;
        public Vector3 HandleOffset;

        public Rigidbody ObjectInHand;
        public Transform ObjectInHandTrans;
        public ObjectGrabingV2 ObjectGraber;

        public Vector3 ObjectTargetPosition;
        public Quaternion ObjectTargetRotation;
    }
    StateData _State;

    public int MergeCount { get { return _State.Merged; } }
    public System.Action<GameObject> OnMerge;

    void Awake()
    {
        Trans = transform;
        if (UseMergeRail)
            _State.RailDirection = (MergeRailStart.position - MergeTarget.position).normalized;

    }

    void LateUpdate()
    {
        if (_State.ObjectInHand != null)
        {
            DoMerging(_State.ObjectInHand);
        }

        var colliders = Physics.OverlapSphere(Trans.position, DistanceThreshold);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject)
                continue;

            var body = collider.GetComponent<Rigidbody>();
            if (body == null)
                continue;

            DoMerging(body);
        }
    }

    private void DoMerging(Rigidbody body)
    {
        switch (MergableObjectType)
        {
            case MergableObjectTypes.AnyObject:
                ProcessMerge(body);
                break;
            case MergableObjectTypes.SpecTag:
                if (body.tag == MergeTag)
                    ProcessMerge(body);
                break;
            case MergableObjectTypes.SpecObject:
                if (body == MergeObject)
                    ProcessMerge(body);
                break;
            default:
                break;
        }
    }

    void ProcessMerge(Rigidbody body)
    {
        if (UseMergeRail)
            ProcessRailMerge(body);
        else
            ProcessSimpleMerge(body);
    }

    void ProcessRailMerge(Rigidbody body)
    {
        if (_State.ObjectInHand == null)
        {
            _State.ObjectGraber = null;
            foreach (var graber in Grabers)
            {
                if (body == graber.ObjectInHand)
                    _State.ObjectGraber = graber;
            }

            if (_State.ObjectGraber == null)
                return;

            _State.ObjectInHand = body;
            _State.ObjectInHandTrans = body.transform;

            _State.HandleOffset = _State.ObjectInHandTrans.parent.TransformDirection(_State.ObjectInHandTrans.localPosition);
        }

        if (_State.ObjectInHand != null)
        {
            bool inGripp = _State.ObjectGraber.ObjectInHand == _State.ObjectInHand;

            if (InAngle(_State.ObjectInHandTrans.forward) == false)
            {
                RemoveObjectFromMerging(true);
                return;
            }

            var pointOnLine = MathHelp.ProjectPointOnLineSegment(MergeRailStart.position, MergeTarget.position, _State.ObjectInHandTrans.position);
            if (InProximity(pointOnLine, _State.ObjectInHandTrans.position) == false)
            {
                RemoveObjectFromMerging(true);
                return;
            }

            _State.ObjectInHandTrans.position = pointOnLine;
            _State.ObjectInHandTrans.LookAt(MergeTarget.position, MergeTarget.up);

            _State.ObjectTargetPosition = _State.ObjectInHandTrans.position;
            _State.ObjectTargetRotation = _State.ObjectInHandTrans.rotation;

            //ObjectTargetPosition = pointOnLine;
            //ObjectTargetForward = RailDirection;
            //ObjectTargetUp = trans.up;

            float mergeDist = Vector3.Distance(MergeTarget.position, _State.ObjectInHandTrans.position);
            if (mergeDist > MergeDistance)
            {
                if (inGripp == false)
                    RemoveObjectFromMerging(false);

                return;
            }

            DoMerge(_State.ObjectInHand, inGripp);
        }
    }

    void ProcessSimpleMerge(Rigidbody body)
    {

        if (_State.ObjectInHand == null)
        {
            ObjectGrabingV2 objectGraber = null;
            foreach (var graber in Grabers)
            {
                if (body == graber.ObjectInHand)
                    objectGraber = graber;
            }

            //Debug.Log("_State.ObjectGraber: " + _State.ObjectGraber);
            if (objectGraber != null)
            {
                if (objectGraber.ObjectInHand != body)
                    return;

                float mergeDist = Vector3.Distance(MergeTarget.position, body.transform.position);
                if (mergeDist > MergeDistance)
                    return;

                _State.ObjectInHand = body;
                _State.ObjectInHandTrans = body.transform;
                _State.ObjectGraber = objectGraber;
            }
        }

        if (_State.ObjectInHand != null)
        {
            float mergeDist = Vector3.Distance(MergeTarget.position, _State.ObjectInHandTrans.position);
            if (mergeDist > MergeDistance)
            {
                _State.ObjectInHand = null;
                _State.ObjectInHandTrans = null;
                _State.ObjectGraber = null;

                return;
            }

            bool inGripp = _State.ObjectGraber != null && _State.ObjectGraber.ObjectInHand == _State.ObjectInHand;

            DoMerge(_State.ObjectInHand, inGripp);
        }
    }

    private void DoMerge(Rigidbody body, bool inGripp)
    {
        if ((MergeOnRelease && inGripp == false) || MergeOnRelease == false)
        {
            _State.Merged++;
            if (OnMerge != null)
                OnMerge(body.gameObject);

            _State.ObjectGraber.ReleaseObject();

            if (PermanentlyMerge)
                Destroy(body);
            else
                _State.ObjectInHand.isKinematic = true;

            _State.ObjectInHandTrans.parent = Trans;
            
            _State.ObjectInHand = null;
            _State.ObjectInHandTrans = null;
            _State.ObjectGraber = null;

            if (DestroyOnMerge)
                Destroy(body.gameObject);
        }
    }

    bool InProximity(Vector3 pointOnLine, Vector3 backupPos)
    {
        Vector3 handPos;
        if (_State.ObjectInHandTrans != null && _State.ObjectInHandTrans.parent != null)
            handPos = _State.ObjectInHandTrans.parent.position + _State.HandleOffset;
        else
            handPos = backupPos;

        float dist = Vector3.Distance(handPos, pointOnLine);
        return dist < MergeRailThresholdDistance;
    }
    bool InAngle(Vector3 dir)
    {
        var currRaidDir = (MergeTarget.position - MergeRailStart.position).normalized;
        var angle = Vector3.Angle(dir, currRaidDir);
        return angle < MergeRailThresholdAngle;
    }

    void RemoveObjectFromMerging(bool resetPosition)
    {
        _State.ObjectInHand = null;
        _State.ObjectInHandTrans = null;
        _State.ObjectGraber = null;
    }
}
