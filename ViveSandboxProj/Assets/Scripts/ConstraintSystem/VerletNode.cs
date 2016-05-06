using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum AxisLabel { X, Y, Z, NegX, NegY, NegZ };

public class VerletNode : MonoBehaviour
{
    [Serializable]
    public struct SphereColliderData
    {
        public bool KeepOut;
        public Transform Trans;
        public Transform Parent;
        public Vector3 LocalPosition;
        public AxisLabel UpAxis;
        public float Radius;
        public float Height;
        public float Damping;
        public float Bouncyness;
    }

    public enum RotationModes
    {
        GlobalFixed,
        LocalFixed,
        InheritSpecifiedParent,
        PointToSpecifiedParent,
        NeighbourAverage,
    }

    public Vector3 Gravity;
    public float Mass;
    [Range(0, 1)]
    public float Damping;
    public bool StartFixedPosition;
    public bool NegativeRotationDir;
    public RotationModes RotationMode;
    public VerletNode RotationParent;
    public AxisLabel JointDirection;
    [SerializeField]
    private Transform StableTransform;

    [NonSerialized]
    public Vector3 Position;
    [NonSerialized]
    public Quaternion Rotation;
    [NonSerialized]
    public Vector3 CurrentMoveDir;

    private bool _FixedPosition;
    public bool FixedPosition
    {
        get { return _FixedPosition; }
        set
        {
            if (value != _FixedPosition)
                _LastPosition = Position;

            _FixedPosition = value;
        }
    }

    public Transform StableTrans { get { return StableTransform; } }
    public Vector3 LastPosition { get { return _LastPosition; } }

    //[HideInInspector]
    public string UpdateLayer;

    float _Radius;
    Vector3 ExternalForce;

    Vector3 _LastPosition;
    Quaternion _Rotation;
    
    List<DistanceConstraint> _Constraints = new List<DistanceConstraint>();
    bool ResetRotationOnConnection;

    Transform StableRotator;

    [NonSerialized]
    [HideInInspector]
    public Transform Trans;

    static GameObject _NodeRoot;

    void Start()
    { }

    public void Initilize()
    {
        Trans = transform;
        _LastPosition = Trans.position;
        Position = Trans.position;
        Rotation = Trans.rotation;
        FixedPosition = StartFixedPosition;

        if (RotationMode == RotationModes.LocalFixed)
        {
            Rotation = Trans.localRotation;
        }
    }

    public void Setup()
    {
        if (_NodeRoot == null)
        {
            _NodeRoot = new GameObject("VerletNodeRoot");
            GameObject.DontDestroyOnLoad(_NodeRoot);
        }

        //if (FixedPosition == false)
        Trans.parent = _NodeRoot.transform;

        _Radius = 1;
        if (GetComponent<SphereCollider>() != null)
            _Radius = GetComponent<SphereCollider>().radius;

        switch (RotationMode)
        {
            case RotationModes.GlobalFixed:
                _Rotation = Trans.rotation;
                break;
            case RotationModes.LocalFixed:
                _Rotation = Trans.localRotation;
                break;
            case RotationModes.InheritSpecifiedParent:
                _Rotation = Quaternion.Inverse(RotationParent.Trans.rotation) * Trans.rotation;
                break;
            case RotationModes.PointToSpecifiedParent:
                Vector3 dir = Vector3.one;
                if (NegativeRotationDir)
                    dir = (Position - RotationParent.Position).normalized;
                else
                    dir = (RotationParent.Position - Position).normalized;

                _Rotation = Quaternion.LookRotation(dir);
                break;
        }
    }

    public void UpdateVerletSim()
    {
        if (FixedPosition)
        {
            Position = Trans.position;
            return;
        }

        Vector3 force = ((Gravity * Mass) + ExternalForce) * VerletUpdater.TimeMultiplier * VerletUpdater.TimeMultiplier;
        Vector3 newPos = Position * 2 - _LastPosition + (force / Mass) * (Time.deltaTime * Time.deltaTime);

        CurrentMoveDir = newPos - Position;

        _LastPosition = Position;
        Position = Position + (CurrentMoveDir * (1 - Damping));
        
        ExternalForce = Vector3.zero;
    }

    public void UpdateRotation()
    {
        if (Time.timeScale == 0)
            return;

        if (ResetRotationOnConnection)
        {
            bool allConnected = true;
            foreach (var constr in _Constraints)
            {
                if (constr.Node1 == null || constr.Node2 == null)
                {
                    allConnected = false;
                    break;
                }
            }
            if (allConnected)
            {
                ResetRotationOnConnection = false;
                SetupRotation();
            }
            else
                return;
        }

        switch (RotationMode)
        {
            case RotationModes.NeighbourAverage:
                NeighbourAverageRotation();
                break;
            case RotationModes.GlobalFixed:
                Rotation = _Rotation;
                break;
            case RotationModes.LocalFixed:
                Rotation = Trans.rotation;
                break;
            case RotationModes.InheritSpecifiedParent:
                Rotation = RotationParent.Rotation * _Rotation;
                break;
            case RotationModes.PointToSpecifiedParent:
                Vector3 dir = Vector3.one;
                if (NegativeRotationDir)
                    dir = (Position - RotationParent.Position).normalized;
                else
                    dir = (RotationParent.Position - Position).normalized;
                Vector3 dirAxis = Vector3.up;
                switch (JointDirection)
                {
                    case AxisLabel.X:
                        dirAxis = Vector3.right;
                        break;
                    case AxisLabel.Y:
                        dirAxis = Vector3.up;
                        break;
                    case AxisLabel.Z:
                        dirAxis = Vector3.forward;
                        break;
                    case AxisLabel.NegX:
                        dirAxis = -Vector3.right;
                        break;
                    case AxisLabel.NegY:
                        dirAxis = -Vector3.up;
                        break;
                    case AxisLabel.NegZ:
                        dirAxis = -Vector3.forward;
                        break;
                    default:
                        break;
                }

                Rotation = LookRotationExtended(dir, RotationParent.Rotation * Vector3.up, dirAxis, Vector3.up);
                break;
        }
    }

    private void NeighbourAverageRotation()
    {
        Vector3 upDir = Trans.up;
        Vector3 avgDir = Vector3.forward;

        if (_Constraints.Count == 1)
        {
            Rotation = GetFirstCoinstraintRotation();
        }
        else if (_Constraints.Count > 1)
        {
            Rotation = GetAllAvrageConstraintRotation();
        }
    }

    Quaternion GetFirstCoinstraintRotation()
    {
        Vector3 dir = (_Constraints[0].Trans.position - Trans.position).normalized;
        return Quaternion.LookRotation(dir, Trans.up);
    }

    Quaternion GetAllAvrageConstraintRotation()
    {
        Vector3 avgDir = Vector3.zero;
        foreach (var constr in _Constraints)
            avgDir += (constr.Trans.position - Trans.position).normalized;

        avgDir /= _Constraints.Count;

        Vector3 vec1 = (_Constraints[0].Trans.position - Trans.position).normalized;
        Vector3 vec2 = (_Constraints[1].Trans.position - Trans.position).normalized;
        Vector3 upDir = Vector3.Cross(vec1, vec2).normalized;

        return Quaternion.LookRotation(avgDir, upDir);
    }

    void SetupRotation()
    {
        if (_Constraints.Count == 1)
        {
            StableTransform.SetParent(Trans.parent, true);
            Trans.rotation = GetFirstCoinstraintRotation();
            StableTransform.SetParent(Trans, true);
        }
        if (_Constraints.Count > 1)
        {
            StableTransform.SetParent(Trans.parent, true);
            Trans.rotation = GetAllAvrageConstraintRotation();
            StableTransform.SetParent(Trans, true);
        }
    }

    public void LateTransformUpdate()
    {
        if (FixedPosition == false)
            Trans.position = Position;

        switch (RotationMode)
        {
            case RotationModes.NeighbourAverage:
            case RotationModes.GlobalFixed:
            case RotationModes.InheritSpecifiedParent:
            case RotationModes.PointToSpecifiedParent:
                Trans.rotation = Rotation;
                break;
            case RotationModes.LocalFixed:
                //transform.localRotation = Rotation;
                break;
        }
    }

    public void MoveNode(Vector3 vec)
    {
        Position += vec;
    }
    public void TeleportNode(Vector3 vec)
    {
        Position += vec;
        _LastPosition += vec;
    }

    public void AddForce(Vector3 force)
    {
        ExternalForce += force;
    }

    public void AddConstraint(DistanceConstraint constr)
    {
        _Constraints.Add(constr);
        ResetRotationOnConnection = true;
    }
    public void RemoveConstraint(DistanceConstraint constr)
    {
        _Constraints.Remove(constr);
        ResetRotationOnConnection = true;
    }

    public void InitStableRotationMode(Vector3 direction)
    {
        GameObject newMiddelObj = new GameObject();
        StableRotator = newMiddelObj.transform;
        StableRotator.SetParent(Trans, false);
        StableRotator.forward = direction;
        StableTransform.SetParent(StableRotator, true);
    }

    public void EndStableRotation()
    {
        StableTransform.SetParent(Trans, true);
        Destroy(StableRotator.gameObject);
    }

    public void SetStableTransformForward(Vector3 direction)
    {
        StableRotator.forward = direction;
    }

    public static Quaternion LookRotationExtended(Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 customForward, Vector3 customUp)
    {
        Quaternion rotationA = Quaternion.LookRotation(alignWithVector, alignWithNormal);
        Quaternion rotationB = Quaternion.LookRotation(customForward, customUp);
        
        return rotationA * Quaternion.Inverse(rotationB);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Position, 0.02f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + ExternalForce);
    }

    //Vector4 SmoothCurve(Vector4 x)
    //{
    //    return x * x * (3.0 - 2.0 * x);
    //}

    //Vector4 TriangleWave(Vector4 x)
    //{
    //    return Math.Abs(Frac(x + 0.5) * 2.0 - 1.0);
    //}

    //Vector4 SmoothTriangleWave(Vector4 x)
    //{
    //    return SmoothCurve(TriangleWave(x));
    //}
    //public static double Frac(double value)
    //{
    //    return value - Math.Truncate(value);
    //}

    //void AnimateWind(ref Vector4 vertex, ref Vector4 color, float freq, float size, float strength)
    //{
    //    // Phases (object, vertex, branch)
    //    float fObjPhase = Vector3.Dot(vertex.xyz, 1);
    //    float fBranchPhase = fObjPhase;
    //    float fVtxPhase = Vector3.Dot(vertex.xyz, fBranchPhase) * size; // make the wave larger

    //    // x is used for edges; y is used for branches
    //    Vector2 vWavesIn = _Time.x + new Vector2(fVtxPhase, fBranchPhase);

    //    //float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0 ) * freq;
    //    Vector4 vWaves = (Frac(vWavesIn.xxxx * new Vector4(1.975f, 0.793f, 0.1375f, 1.1193f)) * 2.0 - 1.0) * freq;
    //    vWaves = SmoothTriangleWave(vWaves);
    //    Vector2 vWavesSum = vWaves.xz + vWaves.yw - 1; // -1 it to get in the -1 to 1 range

    //    float fEdgeAtten = color.x * strength;

    //    float waveLerp1 = vWavesSum.x * 0.5 + 0.5;
    //    float waveLerp2 = vWavesSum.y * 0.5 + 0.5;
    //    waveLerp2 = waveLerp2 * waveLerp2;
    //    Vector3 wave1 = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 1, 0), waveLerp1);
    //    Vector3 wave2 = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 1, 0), waveLerp2);
    //    //color.rgb = wave1 * wave1;

    //    // Edge animation
    //    vertex.xyz += waveLerp2 * vWavesSum.xxx * fEdgeAtten * 0.1;
    //}
}




