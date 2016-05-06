using UnityEngine;
using System.Collections;

public class DistanceConstraint : MonoBehaviour
{
    public VerletNode Node1;
    public VerletNode Node2;
    public bool AllowTwist;
    public float Length;
    public bool AutoLength;

    //[HideInInspector]
    public string UpdateLayer;
    [HideInInspector]
    public Vector3 Position;
    [HideInInspector]
    public bool ReCalcLength;
    [HideInInspector]
    [System.NonSerialized]
    public Transform Trans;

    private Transform Cylinder;

    static GameObject _ConstraintRoot;

    void OnDestroy()
    {
        if (Node1 != null)
            Node1.RemoveConstraint(this);
        if (Node2 != null)
            Node2.RemoveConstraint(this);
    }

    public void Initialize()
    {
        Trans = transform;
        if (GetComponentInChildren<MeshRenderer>() != null)
        {
            Cylinder = GetComponentInChildren<MeshRenderer>().transform;
            Cylinder.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Cylinder.gameObject.SetActive(false);
        }
    }

    public void Setup()
    {
        if (_ConstraintRoot == null)
        {
            _ConstraintRoot = new GameObject("VerletConstraintRoot");
            GameObject.DontDestroyOnLoad(_ConstraintRoot);
        }

        transform.parent = _ConstraintRoot.transform;

        if (AutoLength)
            CalcAutoLength();

        UpdateTransform();
        UpdateCylinderRepresentation();
        Cylinder.gameObject.SetActive(true);
    }

    public void UpdateFunc()
    {
        if (Time.timeScale == 0)
            return;

        UpdateDistConstraint();
        UpdateAngConstraint();

        UpdateCylinderRepresentation();

        UpdateTransform();
    }

    public void CalcAutoLength()
    {
        Length = (Node1.transform.position - Node2.transform.position).magnitude;
    }

    public void SetNode1(VerletNode node)
    {
        if (Node1 != null)
            Node1.RemoveConstraint(this);

        Node1 = node;
        Node1.AddConstraint(this);
    }

    public void SetNode2(VerletNode node)
    {
        if (Node2 != null)
            Node2.RemoveConstraint(this);

        Node2 = node;
        Node2.AddConstraint(this);
    }

    void UpdateDistConstraint()
    {
        if (Node1.FixedPosition && Node2.FixedPosition)
        {
            CalcAutoLength();
            return;
        }

        Vector3 delta = Node2.Position - Node1.Position;

        float dist = delta.magnitude;
        delta.Normalize();

        delta *= (dist - Length);

        if (Node1.FixedPosition == false && Node2.FixedPosition == false)
        {
            delta *= 0.5f;
        }

        if (Node1.FixedPosition == false)
            Node1.MoveNode(delta);
        if (Node2.FixedPosition == false)
            Node2.MoveNode(-delta);
    }

    void UpdateAngConstraint()
    {
        if ((Node1.FixedPosition == false && Node1.RotationMode == VerletNode.RotationModes.NeighbourAverage) ||
            (Node2.FixedPosition == false && Node1.RotationMode == VerletNode.RotationModes.NeighbourAverage))
        {
            Vector3 node1TwistRot = Node1.Rotation * Vector3.right;
            Vector3 node2TwistRot = Node2.Rotation * Vector3.right;
            Vector3 avgTwistRot = (node1TwistRot + node2TwistRot) / 2;

            Vector3 targetDir = (Node1.Position - Node2.Position).normalized;
        }
    }

    void UpdateCylinderRepresentation()
    {
        var scale = Cylinder.localScale;
        var vec = Node2.Position - Node1.Position;
        scale.y = vec.magnitude / 2;

        Cylinder.position = Node1.Position + (vec * 0.5f);
        Cylinder.up = vec.normalized;
        Cylinder.localScale = scale;
    }

    void UpdateTransform()
    {
        Position = (Node1.Position + Node2.Position) / 2;
        transform.position = Position;
        Vector3 up = -(Node2.Position - Node1.Position).normalized;

        transform.rotation = VerletNode.LookRotationExtended(up, Node1.Rotation * Vector3.right, Vector3.right, -Vector3.forward);
    }
}
