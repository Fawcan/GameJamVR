using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Automaticly penetrates what ever, used for arrows and such
/// </summary>
public class Penetrating : MonoBehaviour
{
    [SerializeField]
    private float MinForce = 7;
    [SerializeField]
    private float MaxForce = 30;
    [SerializeField]
    private float MaxAngle;
    [SerializeField]
    private float SlippAngle;
    [SerializeField]
    private float MaxPentrationDistance;
    [Range(0, 1)]
    [SerializeField]
    private float Sharpness;

    private Transform Trans;
    private Rigidbody Body;
    private Vector3 LastForward;
    private Vector3 LastVelocity;
    private float DeltaForce;

    private int DefaultLayer;
    private Transform DefaultParent;
    private Transform LastParent;

    void Awake()
    {
        DefaultLayer = gameObject.layer;
        DefaultParent = transform.parent;

        DeltaForce = MaxForce - MinForce;
        Trans = transform;
        Body = GetComponent<Rigidbody>();
        if (Body == null)
            Body = GetComponentInParent<Rigidbody>();

        if (Body == null)
            Debug.LogError("AirDrag attachesh to non rigidbody");
    }

    void FixedUpdate()
    {
        LastForward = Trans.forward;
        LastVelocity = Body.velocity;

        if (DefaultParent != Trans.parent)
        {
            if (DefaultParent != LastParent)
                gameObject.layer = DefaultLayer;
            else
                LastParent = Trans.parent;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.contacts.Length <= 0)
            return;

        var velDir = LastVelocity.normalized;
        float penetrationAngle = Vector3.Angle(col.contacts[0].normal, -velDir);
        float velocityAngle = Vector3.Angle(LastForward, velDir);

        if (velocityAngle > MaxAngle)
            return;

        PenetrationTarget target = col.collider.GetComponent<PenetrationTarget>();
        if (target == null)
            target = col.gameObject.GetComponentInParent<PenetrationTarget>();

        if (target != null && target.SlippAngle != 0)
        {
            if (penetrationAngle > target.SlippAngle)
                return;
        }
        else
        {
            if (penetrationAngle > SlippAngle)
                return;
        }

        float normAngle = 1 - (velocityAngle / MaxAngle);
        float penetratingForce = col.relativeVelocity.magnitude * Body.mass * Sharpness * normAngle;

        if (penetratingForce > MinForce)
        {
            //Debug.Log("col: " + col.gameObject);
            //Debug.Log("target: " + target);

            float normPenForce = (penetratingForce - MinForce) / DeltaForce;
            float moveDist = normPenForce * MaxPentrationDistance;
            Trans.forward = LastForward;
            Trans.position += LastForward * moveDist;
            Body.isKinematic = true;

            if (target != null)
            {
                target.Hit(Trans, col.contacts[0].point);
            }
        }
    }
}
