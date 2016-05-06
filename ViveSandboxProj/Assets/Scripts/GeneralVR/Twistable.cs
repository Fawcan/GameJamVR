using UnityEngine;
using System.Collections;

/// <summary>
/// Simulated sobjects that it twistable, like a screw or a valve
/// </summary>
public class Twistable : MonoBehaviour
{
    [SerializeField]
    private Transform TwistableObject;
    [SerializeField]
    private float DistanceThreshold;
    [SerializeField]
    private float AngleThreshold;
    [SerializeField]
    private LayerMask TwisterLayers;
    [SerializeField]
    private bool UseLimits;
    [SerializeField]
    private float MinAngle;
    [SerializeField]
    private float MaxAngle;
    [SerializeField]
    private float Spring;
    [SerializeField]
    private float AngularDriveSpeed;

    Transform Trans;

    struct StateData
    {
        public float StartRotOffset;

        public Twister InHoldBy;
        public Transform InHoldByTrans;
    }
    StateData _State;

    void Awake()
    {
        Trans = transform;

        if ((MaxAngle - MinAngle) > 360)
        {
            Debug.LogWarning("Min and max angle covers more than 360 degrees, not supported!");
        }
    }

    void Start ()
    {
        if (Spring != 0 && AngularDriveSpeed != 0)
            Debug.LogWarning("Both Spring and AngularDrivr is active, not supported!");

        TwistableObject.transform.rotation = Quaternion.LookRotation(Trans.forward, Trans.up);
    }
	
	void Update ()
    {
        float signCurrentAngle = MathHelp.SignedVectorAngle(Trans.up, TwistableObject.up, Trans.forward);
        float currentAngle = 0;
        if (signCurrentAngle < 0)
            currentAngle = 180 + (180 + signCurrentAngle);
        else
            currentAngle = signCurrentAngle;

        if (_State.InHoldBy == null)
        {
            if (Spring > 0)
            {
                var springVel = currentAngle * Spring * Time.deltaTime;
                TwistableObject.Rotate(0, 0, springVel, Space.Self);
            }
            else if (AngularDriveSpeed > 0)
            {
                TwistableObject.Rotate(0, 0, AngularDriveSpeed * Time.deltaTime, Space.Self);
            }
        }

        if (_State.InHoldBy == null)
        {
            var colliders = Physics.OverlapSphere(TwistableObject.position, DistanceThreshold, TwisterLayers.value);
            foreach (var col in colliders)
            {
                var twister = col.GetComponent<Twister>();
                if (twister == null)
                    continue;

                float angle = Vector3.Angle(col.transform.forward, TwistableObject.forward);
                if (angle < AngleThreshold)
                {
                    _State.InHoldBy = twister;
                    _State.InHoldByTrans = twister.transform;

                    var projUpVec = MathHelp.ProjectVectorOnPlane(TwistableObject.forward, _State.InHoldByTrans.up);
                    _State.StartRotOffset = MathHelp.SignedVectorAngle(projUpVec, TwistableObject.up, TwistableObject.forward);
                }
            }
        }

        if (_State.InHoldBy != null)
        {
            var projUpVec = MathHelp.ProjectVectorOnPlane(TwistableObject.forward, _State.InHoldByTrans.up);
            float currTwist = MathHelp.SignedVectorAngle(projUpVec, TwistableObject.up, TwistableObject.forward);
            float deltaTwist = currTwist - _State.StartRotOffset;
            currentAngle -= deltaTwist;
            //Debug.Log("currTwist: " + currTwist);
            //Debug.Log("_State.StartRotOffset: " + _State.StartRotOffset);
            //Debug.Log("deltaTwist: " + deltaTwist);

            bool stillInTwist = false;
            var colliders = Physics.OverlapSphere(TwistableObject.position, DistanceThreshold, TwisterLayers.value);
            foreach (var col in colliders)
            {
                if (col.gameObject != _State.InHoldBy.gameObject)
                    continue;

                float angle = Vector3.Angle(col.transform.forward, TwistableObject.forward);
                if (angle < AngleThreshold)
                    stillInTwist = true;
            }

            if (stillInTwist == false)
            {
                _State.InHoldBy = null;
                _State.InHoldByTrans = null;
            }
        }

        currentAngle = currentAngle % 360;

        if (UseLimits)
        {
            //Debug.Log("currentAngle: " + currentAngle);
            //Debug.Log("signCurrentAngle: " + signCurrentAngle);
            //float realMinAngle = 360 + MinAngle;
            if (currentAngle > -MinAngle && currentAngle < (360 - MaxAngle))
            {
                float minDiff = Mathf.Abs(currentAngle + MinAngle);
                float maxDiff = Mathf.Abs((360 - MaxAngle) - currentAngle);

                if (minDiff < maxDiff)
                    currentAngle = -MinAngle;
                else
                    currentAngle = (360 - MaxAngle);

                //Debug.Log("minDiff: " + minDiff);
                //Debug.Log("maxDiff: " + maxDiff);
                //currentAngle = signCurrentAngle;
            }
        }
        //Debug.Log("currentAngle: " + currentAngle);

        TwistableObject.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}
