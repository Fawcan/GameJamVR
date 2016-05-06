using UnityEngine;
using System.Collections;

public class Propellar : NodeAttachment
{
    [SerializeField]
    private float Force;
    [SerializeField]
    private Transform Model;

    public override void UpdateFunc()
    {
        if (AttachedTo != null)
            AttachedTo.MoveNode(Trans.forward * Force * Time.deltaTime);

        Model.Rotate(new Vector3(0, 0, -Force * Time.deltaTime * 2000), Space.Self);
    }
}
