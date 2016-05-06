using UnityEngine;
using System.Collections;

/// <summary>
/// Simple waypoints movement, can be used for moving targets and such
/// </summary>
public class WaypointMovement : MonoBehaviour
{
    [SerializeField]
    private Transform[] Waypoints;
    [SerializeField]
    private bool PingPong;
    [SerializeField]
    private float Speed;
    [SerializeField]
    private float MovementPrecision;

    private Transform Trans;
    private int CurrentTarget;

    void Start()
    {
        Trans = transform;
        CurrentTarget = 0;
    }

    void Update()
    {
        float dist = Vector3.Distance(Waypoints[CurrentTarget].position, Trans.position);
        if (dist < MovementPrecision)
        {
            CurrentTarget++;
            if (CurrentTarget >= Waypoints.Length)
                CurrentTarget = 0;
        }

        var dir = (Waypoints[CurrentTarget].position - Trans.position).normalized;
        Trans.position += (dir * Speed * Time.deltaTime);
    }
}
