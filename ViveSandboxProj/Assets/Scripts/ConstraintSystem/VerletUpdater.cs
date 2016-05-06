using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class VerletUpdater : MonoBehaviour
{
    public VerletSimulation SimulationSettings;
    [SerializeField]
    private VerletNode[] ManualyDefinedNodes;
    [SerializeField]
    private DistanceConstraint[] ManualyDefinedConstraints;

    public VerletConstraintCollision[] SphereColliders;
    public Transform[] PlaneColliders;
    
    private VerletNode.SphereColliderData[] _GenSphereColliders;

    [NonSerialized]
    public List<VerletNode> DynamicNodes = new List<VerletNode>();
    [NonSerialized]
    public List<VerletNode> RootNodes = new List<VerletNode>();
    [NonSerialized]
    public List<DistanceConstraint> AllConstraints = new List<DistanceConstraint>();
    public Dictionary<string, List<VerletNode>> NodeUpdates = new Dictionary<string, List<VerletNode>>();
    public Dictionary<string, List<DistanceConstraint>> ConstrUpdates = new Dictionary<string, List<DistanceConstraint>>();

    [NonSerialized]
    public List<NodeAttachment> AllAttachments  = new List<NodeAttachment>();
    [NonSerialized]
    public Dictionary<string, List<NodeAttachment>> Attachments = new Dictionary<string, List<NodeAttachment>>();

    bool JustEnabled;
    float UpdateDelay;
    float TimeAccumilator;

    public static float TimeMultiplier;

    public static bool LockYSimulation;

    protected virtual bool SetupInBase { get { return true; } }

    void OnDestroy()
    {
        foreach (var node in DynamicNodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        foreach (var node in RootNodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        foreach (var constraint in AllConstraints)
        {
            if (constraint != null)
                Destroy(constraint.gameObject);
        }
    }

    //void OnDrawGizmosSelected()
    //{
    //    float sphereSize = 0.015f;

    //    Gizmos.color = Color.red;
    //    foreach (var node in RootNodes)
    //    {
    //        Gizmos.DrawWireSphere(node.transform.position, sphereSize);
    //    }

    //    Gizmos.color = Color.red;
    //    foreach (var node in DynamicNodes)
    //    {
    //        Gizmos.DrawWireSphere(node.transform.position, sphereSize);
    //    }

    //    Gizmos.color = Color.yellow;
    //    foreach (var node in AllConstraints)
    //    {
    //        Gizmos.DrawWireSphere(node.transform.position, sphereSize - 0.05f);
    //    }
    //}

    void OnEnable()
    {
        JustEnabled = true;
    }

    void Start()
    {
        foreach (var layerName in SimulationSettings.LayerNames)
        {
            NodeUpdates[layerName] = new List<VerletNode>();
            ConstrUpdates[layerName] = new List<DistanceConstraint>();
            Attachments[layerName] = new List<NodeAttachment>();
        }

        if (SetupInBase)
        {
            foreach (var node in ManualyDefinedNodes)
                node.Initilize();

            foreach (var node in ManualyDefinedNodes)
                SetUpdateNode(node);
            foreach (var _const in ManualyDefinedConstraints)
                SetUpdateConstraint(_const);

            foreach (var node in ManualyDefinedNodes)
                node.Setup();
            foreach (var _const in ManualyDefinedConstraints)
                _const.Setup();
        }

        SetupColliders();

        StartVerletUpdater();

        UpdateDelay = 1f / SimulationSettings.UpdatesPerSecond;
        TimeAccumilator = 0;

        JustEnabled = true;
    }

    void SetupColliders()
    {
        _GenSphereColliders = new VerletNode.SphereColliderData[SphereColliders.Length];
        for (int i = 0; i < SphereColliders.Length; i++)
        {
            var capsuleComp = SphereColliders[i].GetComponent<CapsuleCollider>();
            var sphereComp = SphereColliders[i].GetComponent<SphereCollider>();

            float radius = 0;
            float height = 0;

            AxisLabel axis = AxisLabel.Y;
            if (capsuleComp != null)
            {
                radius = capsuleComp.radius;
                height = capsuleComp.height;
                switch (capsuleComp.direction)
                {
                    case 0:
                        axis = AxisLabel.X;
                        break;
                    case 1:
                        axis = AxisLabel.Y;
                        break;
                    case 2:
                        axis = AxisLabel.Z;
                        break;
                }
            }
            else if (sphereComp != null)
            {
                radius = sphereComp.radius;
            }

            _GenSphereColliders[i] = new VerletNode.SphereColliderData()
            {
                KeepOut = SphereColliders[i].KeepOut,
                Parent = SphereColliders[i].transform.parent,
                LocalPosition = SphereColliders[i].transform.localPosition,
                UpAxis = axis,
                Radius = radius,
                Height = height,
                Damping = SphereColliders[i].Damping,
                Bouncyness = SphereColliders[i].Bouncyness,
            };
        }
    }

    public virtual void StartVerletUpdater() { }

    public void SetUpdateNode(VerletNode node)
    {
        if (node.UpdateLayer.Equals("root"))
        {
            RootNodes.Add(node);
            return;
        }

        DynamicNodes.Add(node);

        if (SimulationSettings.LayerNames.Contains(node.UpdateLayer) == false)
        {
            Debug.Log("Tried to add update func by layer name " + node.UpdateLayer + " which des not exist, GameObject: " + node.name);
            return;
        }

        NodeUpdates[node.UpdateLayer].Add(node);
    }

    public void SetUpdateConstraint(DistanceConstraint constr)
    {
        if (SimulationSettings.LayerNames.Contains(constr.UpdateLayer) == false)
        {
            Debug.Log("Tried to add update func by layer name " + constr.UpdateLayer + " which des not exist, GameObject: " + constr.name);
            return;
        }

        ConstrUpdates[constr.UpdateLayer].Add(constr);
        AllConstraints.Add(constr);
    }

    public void SetUpdateAttachment(NodeAttachment attachment)
    {
        if (SimulationSettings.LayerNames.Contains(attachment.UpdateLayer) == false)
        {
            Debug.Log("Tried to add update func by layer name " + attachment.UpdateLayer + " which des not exist, GameObject: " + attachment.name);
            return;
        }

        AllAttachments.Add(attachment);
        Attachments[attachment.UpdateLayer].Add(attachment);
    }

    public void RemoveNode(VerletNode node)
    {
        if (node.UpdateLayer.Equals("root"))
        {
            RootNodes.Remove(node);
            return;
        }

        DynamicNodes.Remove(node);

        if (SimulationSettings.LayerNames.Contains(node.UpdateLayer) == false)
        {
            Debug.Log("Tried to Remove update func by layer name " + node.UpdateLayer + " which des not exist, GameObject: " + node.name);
            return;
        }

        NodeUpdates[node.UpdateLayer].Remove(node);
    }

    public void RemoveConstraint(DistanceConstraint constr)
    {
        if (SimulationSettings.LayerNames.Contains(constr.UpdateLayer) == false)
        {
            Debug.Log("Tried to Remove update func by layer name " + constr.UpdateLayer + " which des not exist, GameObject: " + constr.name);
            return;
        }

        ConstrUpdates[constr.UpdateLayer].Remove(constr);
        AllConstraints.Remove(constr);
    }

    public void RemoveAttachment(NodeAttachment attachment)
    {
        if (SimulationSettings.LayerNames.Contains(attachment.UpdateLayer) == false)
        {
            Debug.Log("Tried to Remove update func by layer name " + attachment.UpdateLayer + " which des not exist, GameObject: " + attachment.name);
            return;
        }

        AllAttachments.Remove(attachment);
        Attachments[attachment.UpdateLayer].Remove(attachment);
    }

    public void ClearAndDeleteAll()
    {
        foreach (var node in RootNodes)
            Destroy(node.gameObject);

        foreach (var node in DynamicNodes)
            Destroy(node.gameObject);

        foreach (var constr in AllConstraints)
            Destroy(constr.gameObject);

        foreach (var attach in AllAttachments)
            Destroy(attach.gameObject);

        foreach (var keyValue in NodeUpdates)
            keyValue.Value.Clear();

        foreach (var keyValue in ConstrUpdates)
            keyValue.Value.Clear();

        foreach (var keyValue in Attachments)
            keyValue.Value.Clear();

        RootNodes.Clear();
        DynamicNodes.Clear();
        AllConstraints.Clear();
        AllAttachments.Clear();
    }

    void Update()
    {
        if (JustEnabled)
        {
            JustEnabled = false;
            for (int i = 0; i < 10; i++)
                UpdateFunc(true);
        }

        UpdateFunc();
    }

    void UpdateFunc(bool parsial = false)
    {
        if (Time.timeScale == 0)
            return;

        //TimeAccumilator += Time.deltaTime;
        //while (TimeAccumilator >= UpdateDelay)
        {
            if (Time.deltaTime == 0)
                TimeMultiplier = 1;
            else
                TimeMultiplier = UpdateDelay / Time.deltaTime;

            //Debug.Log("TimeMultiplier: " + TimeMultiplier);
            //TimeAccumilator -= UpdateDelay;

            foreach (var root in RootNodes)
            {
                root.UpdateVerletSim();
            }

            foreach (var contr in AllConstraints)
            {
                if (contr.ReCalcLength)
                {
                    contr.ReCalcLength = false;
                    contr.CalcAutoLength();
                }
            }

            foreach (var updateData in SimulationSettings.LayerSettings)
            {
                for (int i = 0; i < updateData.Iterations; i++)
                {
                    List<NodeAttachment> attachmentsList = Attachments[updateData.Name];
                    foreach (var attachment in attachmentsList)
                        attachment.UpdateFunc();

                    if (updateData.Reversed)
                    {
                        if (updateData.ConstraintsFirst)
                        {
                            List<DistanceConstraint> contrList = ConstrUpdates[updateData.Name];
                            for (int j = contrList.Count - 1; j >= 0; j--)
                                contrList[j].UpdateFunc();
                        }
                        List<VerletNode> nodeList = NodeUpdates[updateData.Name];
                        for (int j = nodeList.Count - 1; j >= 0; j--)
                            nodeList[j].UpdateVerletSim();

                        if (updateData.ConstraintsFirst == false)
                        {
                            List<DistanceConstraint> contrList = ConstrUpdates[updateData.Name];
                            for (int j = contrList.Count - 1; j >= 0; j--)
                                contrList[j].UpdateFunc();
                        }
                    }
                    else
                    {
                        if (updateData.ConstraintsFirst)
                        {
                            foreach (var constr in ConstrUpdates[updateData.Name])
                                constr.UpdateFunc();
                        }

                        foreach (var node in NodeUpdates[updateData.Name])
                            node.UpdateVerletSim();

                        if (updateData.ConstraintsFirst == false)
                        {
                            foreach (var constr in ConstrUpdates[updateData.Name])
                                constr.UpdateFunc();
                        }
                    }
                }
            }

            if (parsial)
                return;

            DoCollision();

            foreach (var root in RootNodes)
            {
                root.UpdateRotation();
            }
            foreach (var node in DynamicNodes)
            {
                node.UpdateRotation();
            }

            foreach (var root in RootNodes)
            {
                root.LateTransformUpdate();
            }
            foreach (var node in DynamicNodes)
            {
                node.LateTransformUpdate();
            }

            UpdateVerletUpdater();
        }
    }

    void aLateUpdate()
    {
        DoCollision();

        //foreach (var root in RootNodes)
        //{
        //    root.UpdateRotation();
        //}
        //foreach (var node in DynamicNodes)
        //{
        //    node.UpdateRotation();
        //}

        foreach (var root in RootNodes)
        {
            root.LateTransformUpdate();
        }
        foreach (var node in DynamicNodes)
        {
            node.LateTransformUpdate();
        }
    }

    void DoCollision()
    {
        foreach (var collider in _GenSphereColliders)
        {
            var colPos = collider.Parent.position + collider.Parent.TransformVector(collider.LocalPosition);
            foreach (var node in DynamicNodes)
            {
                if (node.FixedPosition)
                    continue;

                Vector3 vec = colPos - node.Position;
                float dist = vec.magnitude;

                if (collider.KeepOut && dist < collider.Radius)
                {
                    float diff = (collider.Radius - dist);
                    node.MoveNode(-vec.normalized * diff);
                }
                else if (collider.KeepOut == false && dist > collider.Radius)
                {
                    float diff = (dist - collider.Radius);
                    node.MoveNode(vec.normalized * diff);
                }
            }
        }

        foreach (var node in DynamicNodes)
        {
            foreach (var plane in PlaneColliders)
            {
                var nodeVec = (node.Position - plane.position);
                float dot = Vector3.Dot(nodeVec, plane.up);
                if (dot < 0)
                {
                    Vector3 tragetPos = ProjectPointOnPlane(plane.up, plane.position, node.Position);
                    Vector3 vec = tragetPos - node.Position;
                    node.MoveNode(vec);
                }
            }
        }
    }

    public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
    {
        float distance = Vector3.Dot(planeNormal, (point - planePoint));
        distance *= -1;
        
        Vector3 translationVector = planeNormal * distance;
        
        return point + translationVector;
    }

    public virtual void UpdateVerletUpdater() { }

    public static Vector3[] FindLineSphereIntersections(Vector3 linePoint0, Vector3 linePoint1, Vector3 circleCenter, float circleRadius)
    {
        // http://www.codeproject.com/Articles/19799/Simple-Ray-Tracing-in-C-Part-II-Triangles-Intersec

        float cx = circleCenter.x;
        float cy = circleCenter.y;
        float cz = circleCenter.z;

        float px = linePoint0.x;
        float py = linePoint0.y;
        float pz = linePoint0.z;

        float vx = linePoint1.x - px;
        float vy = linePoint1.y - py;
        float vz = linePoint1.z - pz;

        float A = vx * vx + vy * vy + vz * vz;
        float B = 2.0f * (px * vx + py * vy + pz * vz - vx * cx - vy * cy - vz * cz);
        float C = px * px - 2 * px * cx + cx * cx + py * py - 2 * py * cy + cy * cy +
                   pz * pz - 2 * pz * cz + cz * cz - circleRadius * circleRadius;

        // discriminant
        float D = B * B - 4 * A * C;

        if (D < 0)
        {
            return new Vector3[0];
        }

        float t1 = (-B - (float)Math.Sqrt(D)) / (2.0f * A);

        Vector3 solution1 = new Vector3(linePoint0.x * (1 - t1) + t1 * linePoint1.x,
                                         linePoint0.y * (1 - t1) + t1 * linePoint1.y,
                                         linePoint0.z * (1 - t1) + t1 * linePoint1.z);
        if (D == 0)
        {
            return new Vector3[] { solution1 };
        }

        float t2 = (-B + (float)Math.Sqrt(D)) / (2.0f * A);
        Vector3 solution2 = new Vector3(linePoint0.x * (1 - t2) + t2 * linePoint1.x,
                                         linePoint0.y * (1 - t2) + t2 * linePoint1.y,
                                         linePoint0.z * (1 - t2) + t2 * linePoint1.z);

        // prefer a solution that's on the line segment itself

        if (Math.Abs(t1 - 0.5) < Math.Abs(t2 - 0.5))
        {
            return new Vector3[] { solution1, solution2 };
        }

        return new Vector3[] { solution2, solution1 };
    }
}
