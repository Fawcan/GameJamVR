using UnityEngine;
using System.Collections;

public class ConstraintModule : Pickupable
{
    [System.Serializable]
    struct NodeData
    {
        public Transform Node;
        [System.NonSerialized]
        public Vector3 Position;
        [System.NonSerialized]
        public Vector3 LastPosition;
        [System.NonSerialized]
        public Vector3 CurrentMoveDir;
    }
    [System.Serializable]
    struct ConstraintData
    {
        public Transform Trans;
    }

    [SerializeField]
    private NodeData Node1;
    [SerializeField]
    private NodeData Node2;
    [SerializeField]
    private ConstraintData Constraint;
    [SerializeField]
    private ModulesSimSettings Settings;

    struct StateData
    {

    }
    StateData _State;

    Transform Trans;
    float UpdateDelay;
    float TimeMultiplier;

    static Transform _Root;

    void Start ()
    {
        UpdateDelay = 1f / Settings.UpdatesPerSecond;
        Trans = transform;

        if (_Root == null)
            _Root = new GameObject("ConstraintModulesRoot").transform;

        OnDroppedObject();
    }
	
	void Update ()
    {
        if (Time.deltaTime == 0)
            TimeMultiplier = 1;
        else
            TimeMultiplier = UpdateDelay / Time.deltaTime;

        if (InHand == false)
        {
            UpdateVerletSim(Node1);
            UpdateVerletSim(Node2);

            Vector3 vec = Node2.Position - Node1.Position;
            Trans.position = Node1.Position + (vec * 0.5f);
        }
        else
        {

        }
    }

    void UpdateVerletSim(NodeData node)
    {
        Vector3 force = (Settings.Gravity * Settings.Mass) * TimeMultiplier * TimeMultiplier;
        Vector3 newPos = node.Position * 2 - node.LastPosition + (force / Settings.Mass) * (Time.deltaTime * Time.deltaTime);

        node.CurrentMoveDir = newPos - node.Position;

        node.LastPosition = node.Position;
        node.Position = node.Position + (node.CurrentMoveDir * (1 - Settings.Damping));

        node.Node.position = node.Position;
    }
    public override void OnGrabbedObject()
    {
        Node1.Node.parent = Trans;
        Node2.Node.parent = Trans;
        Constraint.Trans.parent = Trans;
    }
    public override void OnDroppedObject()
    {
        Node1.Node.parent = _Root;
        Node2.Node.parent = _Root;
        Constraint.Trans.parent = _Root;
    }
}
