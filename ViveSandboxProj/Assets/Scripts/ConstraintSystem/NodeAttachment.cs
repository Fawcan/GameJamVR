using UnityEngine;
using System.Collections;

public class NodeAttachment : MonoBehaviour
{
    public VerletNode StartNode;
    public string UpdateLayer;

    protected VerletNode AttachedTo;
    protected Transform Trans;

    void Awake()
    {
        Trans = transform;
    }

	void Start ()
    {
        if (StartNode != null)
            AttachToNode(StartNode);
    }

    public virtual void Initialize() { }

    public virtual void UpdateFunc() { }

    public void AttachToNode(VerletNode node)
    {
        AttachedTo = node;
        Trans.SetParent(node.StableTrans, true);
        Trans.localPosition = Vector3.zero;
    }
}
