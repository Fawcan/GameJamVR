using UnityEngine;
using System.Collections;

/// <summary>
/// Example  of a target board the counts score
/// </summary>
public class TargetBoard : PenetrationTarget
{
    [SerializeField]
    private float Radius;
    [SerializeField]
    private Transform MidPoint;

    public float LastHitScore { get; private set; }

    protected override void OnTargetHit(Transform arrow, Vector3 point)
    {
        float hitOffset = Vector3.Distance(point, MidPoint.position);
        LastHitScore = 1 - Mathf.Clamp01(hitOffset / Radius);
    }

    void OnDrawGizmosSelected()
    {
        if (MidPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(MidPoint.position, Radius);
        }
    }
}
