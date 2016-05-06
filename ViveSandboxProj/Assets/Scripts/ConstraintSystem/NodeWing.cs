using UnityEngine;
using System.Collections;

public class NodeWing : NodeAttachment
{
    [SerializeField]
    private float Drag;
    [SerializeField]
    private Transform Model;

    public override void UpdateFunc()
    {
        if (AttachedTo != null && AttachedTo.FixedPosition == false)
        {
            Vector3 vec = AttachedTo.Position - AttachedTo.LastPosition;    
            float speedFactor = Mathf.Pow(vec.magnitude / Time.deltaTime, 2);
            float angleFactor = Vector3.Dot(vec.normalized, Trans.up);

            AttachedTo.MoveNode(Trans.up * Drag * speedFactor * -angleFactor * Time.deltaTime);
        }
    }
}
