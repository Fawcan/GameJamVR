using UnityEngine;
using System.Collections;

/// <summary>
/// Bow for archery
/// </summary>
public class Bow : HandInteractionBase<ObjectGrabingV2>
{
    [SerializeField]
    private Transform ArrowSeat;
    [SerializeField]
    private Transform TopString;
    [SerializeField]
    private Transform BottomString;
    [SerializeField]
    private LayerMask StringCollider;
    [SerializeField]
    private float ArrowSeatRadius;
    [SerializeField]
    private float AngleThreshold;
    [SerializeField]
    private float Force;
    [SerializeField]
    private float MaxTenseningDistance;
    [SerializeField]
    private float DropDistance;
    [SerializeField]
    private float HapticPulseDistThreshold;
    [SerializeField]
    private int MinPulseDuration;
    [SerializeField]
    private int MaxPulseDuration;

    protected virtual void TenseningFeedback(int durationMS) { }

    struct TransformCopy
    {
        public TransformCopy(Transform trans)
        {
            position = trans.localPosition;
            rotation = trans.localRotation;
            scale = trans.localScale;
        }

        public void SetTransform(Transform trans)
        {
            trans.localPosition = position;
            trans.localRotation = rotation;
            trans.localScale = scale;
        }

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    struct StateData
    {
        public TransformCopy TopStringStartTrans;
        public TransformCopy BottomStringStartTrans;

        public Transform ArrowInAction;
        public Rigidbody ArrowInActionBody;
        public ObjectGrabingV2 ArrowInActionGraber;
        public Vector3 HandleOffset;

        public int PulseDurDelta;

        public bool ArrowTensed;
        public float LastTenseDistance;
    }
    StateData _State;

    public Transform TenseningGrip
    {
        get
        {
            if (_State.ArrowInAction == null)
                return null;

            return _State.ArrowInActionGraber.Trans;
        }
    }

    void Awake()
    {
        _State.TopStringStartTrans = new TransformCopy(TopString);
        _State.BottomStringStartTrans = new TransformCopy(BottomString);

        _State.PulseDurDelta = MaxPulseDuration - MinPulseDuration;
    }

    void Start()
    {

    }

    Vector3 handPosPub;

    void LateUpdate()
    {
        ObjectGrabingV2 graber;
        if (_State.ArrowInAction != null && ObjectInHand(_State.ArrowInAction, out graber) == false)
            ReleaseArrow();

        if (_State.ArrowInAction == null)
        {
            var colliders = Physics.OverlapSphere(ArrowSeat.position, ArrowSeatRadius, StringCollider.value);
            foreach (var col in colliders)
            {
                var trans = col.transform;
                if (ObjectInHand(trans, out graber) == false)
                    continue;

                float angle = Vector3.Angle(col.transform.forward, ArrowSeat.forward);
                if (angle < AngleThreshold)
                {
                    PlaceArrow(graber, trans);
                }
            }
        }

        if (_State.ArrowInAction != null)
        {
            var handPos = _State.ArrowInActionGraber.Trans.position + _State.HandleOffset;
            var handPosReal = _State.ArrowInActionGraber.Trans.position;
            var arrowBase = MathHelp.ProjectPointOnLine(ArrowSeat.position, ArrowSeat.forward, handPos);
            var dist = Vector3.Distance(handPos, arrowBase);
            //var realDist = Vector3.Distance(handPosReal, arrowBase);
            var dot = Vector3.Dot(ArrowSeat.forward, handPos - ArrowSeat.position);
            _State.ArrowTensed = dot < 0;

            handPosPub = handPos;

            if (dist > DropDistance)
            {
                RemoveArrow();
            }
            else if (_State.ArrowTensed)
            {
                IdleTenseArrow(arrowBase);
            }
        }
    }

    private void IdleTenseArrow(Vector3 arrowBase)
    {
        var tenseDist = Vector3.Distance(ArrowSeat.position, arrowBase);
        tenseDist = Mathf.Min(tenseDist, MaxTenseningDistance);
        arrowBase = ArrowSeat.position - (tenseDist * ArrowSeat.forward);

        float tensingDiff = Mathf.Abs(tenseDist - _State.LastTenseDistance);
        if (tensingDiff > HapticPulseDistThreshold)
        {
            float normDist = tenseDist / MaxTenseningDistance;
            TenseningFeedback((int)(normDist * _State.PulseDurDelta) + MinPulseDuration);

            _State.LastTenseDistance = tenseDist;
        }

        _State.ArrowInAction.position = arrowBase;
        _State.ArrowInAction.forward = ArrowSeat.forward;

        var topStrDir = (arrowBase - TopString.position);
        var buttonStrDir = (arrowBase - BottomString.position);
        TopString.up = topStrDir;
        BottomString.up = buttonStrDir;

        var scale = TopString.localScale;
        scale.y = topStrDir.magnitude;
        TopString.localScale = scale;

        scale = BottomString.localScale;
        scale.y = buttonStrDir.magnitude;
        BottomString.localScale = scale;
    }

    bool ObjectInHand(Transform obj, out ObjectGrabingV2 graber)
    {
        graber = null;
        foreach (var graberObj in Grabers)
        {
            if (graberObj.ObjectInHandTrans == obj)
            {
                graber = graberObj;
                return true;
            }
        }

        return false;
    }

    void PlaceArrow(ObjectGrabingV2 graber, Transform arrow)
    {
        _State.ArrowInAction = arrow;
        _State.HandleOffset = graber.Trans.TransformDirection(arrow.transform.position - graber.Trans.position);
        _State.ArrowInActionBody = arrow.GetComponent<Rigidbody>();
        _State.ArrowInActionGraber = graber;
    }

    void RemoveArrow()
    {
        _State.ArrowInAction = null;
        _State.ArrowInActionBody = null;

        ResetString();
    }
    void ReleaseArrow()
    {
        if (_State.ArrowTensed)
        {
            var releaseVec = ArrowSeat.position - _State.ArrowInAction.position;
            _State.ArrowInAction.forward = ArrowSeat.forward;
            _State.ArrowInActionBody.AddForce(_State.ArrowInAction.forward * Force * releaseVec.magnitude, ForceMode.Impulse);
        }
        
        _State.ArrowInAction = null;

        ResetString();
    }
    void ResetString()
    {
        _State.TopStringStartTrans.SetTransform(TopString);
        _State.BottomStringStartTrans.SetTransform(BottomString);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (ArrowSeat != null)
            Gizmos.DrawSphere(ArrowSeat.position, ArrowSeatRadius);

        Gizmos.DrawSphere(handPosPub, 0.1f);
    }
}
