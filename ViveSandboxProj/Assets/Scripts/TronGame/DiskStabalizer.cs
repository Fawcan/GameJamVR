using UnityEngine;
using System.Collections;

public class DiskStabalizer : MonoBehaviour
{
    [SerializeField]
    private float MinSpeed;
    [SerializeField]
    private float MaxSpeed;
    [SerializeField]
    private float MinSpeedAxelRelative;
    [SerializeField]
    private Transform SpeedAxel;
    [SerializeField]
    private float StabalizingSpeed;
    [SerializeField]
    private Transform Model;

    Transform Trans;
    Rigidbody Body;

	void Awake ()
    {
        Trans = transform;
        Body = GetComponent<Rigidbody>();
    }
	
	void Update ()
    {
        float speed = Body.velocity.magnitude;
        if (speed > MaxSpeed)
            Body.velocity = Body.velocity.normalized * MaxSpeed;
        if (speed < MinSpeed)
            Body.velocity = Body.velocity.normalized * MinSpeed;

        //var axisSpeedVec = SpeedAxel.InverseTransformVector(Body.velocity);
        //float axisSpeed = axisSpeedVec.z;

        //if (axisSpeed < MinSpeedAxelRelative)
        {
            vec1 = SpeedAxel.forward * MinSpeedAxelRelative;
            vec2 = Body.velocity;
            var speedPoint = MathHelp.ProjectPointOnLine(Vector3.zero, Body.velocity, SpeedAxel.forward * MinSpeedAxelRelative);
            //Body.velocity = speedPoint;
            vec3 = speedPoint;
        }

        //Trans.rotation = Quaternion.RotateTowards(Trans.rotation, Quaternion.Euler(0, 1, 0), StabalizingSpeed * Time.deltaTime);
    }

    void LateUpdate()
    {
        Model.up = Vector3.up;
    }

    Vector3 vec1;
    Vector3 vec2;
    Vector3 vec3;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, vec1);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, vec2);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, vec3);
    }
}
