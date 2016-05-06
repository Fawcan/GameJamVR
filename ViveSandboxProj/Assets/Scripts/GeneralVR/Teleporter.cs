using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Teleport system, will "fire" an arch and let you teleport to where it hits the ground. Also support teleport blockers, wall offset and altitude shifts
/// </summary>
public abstract class Teleporter : MonoBehaviour
{
    [System.Serializable]
    struct AxisSettings { public bool X, Y, Z; }

    [System.Serializable]
    struct TeleportSettingsData
    {
        public float MinYLevel;
        public float MinWallDistance;
        public float MinAngleUp;
        public float BeamGravity;
        public float BeamSplitSize;
        public int BeamSplits;

        public float Cooldown;

        public LayerMask GroundLayer;
        public LayerMask BlockLayers;
        public LayerMask InvisBlockLayer;
    }

    [SerializeField]
    private Transform ObjectToTeleport;
    [SerializeField]
    private Transform POVObject;
    [SerializeField]
    private AxisSettings TeleportAxis;
    [SerializeField]
    private Transform HUDPrefab;
    [SerializeField]
    private Material ValidTeleport;
    [SerializeField]
    private Material InValidTeleport;
    [SerializeField]
    private LEDBar TeleportCooldownLEDBar;

    [SerializeField]
    private TeleportSettingsData Settings;

    private LineRenderer Beam;

    private Transform HUDObject;
    private LineRenderer HUDLineRenderer;
    private Transform Trans;

    private float LastTeleportTime;
    private Vector3[] Position;
    private Collider[] ColliderCache;

    private Collider[] TeleportBlockers;

    protected abstract bool HUDButtonPress();
    protected abstract bool TeleportPressedDown();

    private Vector3 HitPoint;
    private List<Vector3> Directions = new List<Vector3>();

    void Awake()
    {
        Trans = transform;
        Beam = GetComponent<LineRenderer>();
        Position = new Vector3[Settings.BeamSplits];
        Beam.SetVertexCount(Settings.BeamSplits);

        HUDObject = Instantiate(HUDPrefab);
        HUDObject.gameObject.SetActive(false);
        HUDLineRenderer = HUDObject.GetComponent<LineRenderer>();
        //CreateCircle(HUDLineRenderer);

        ColliderCache = new Collider[10];
    }

    void Start()
    {
        List<Collider> colInLayer = new List<Collider>();

        var colliders = FindObjectsOfType<Collider>();
        foreach (var collider in colliders)
        {
            if (((1 << collider.gameObject.layer) & Settings.InvisBlockLayer.value) > 0)
                colInLayer.Add(collider);
        }

        TeleportBlockers = colInLayer.ToArray();
    }

    void CreateCircle(LineRenderer line)
    {
        int splits = 6;
        float sliceSize = (Mathf.PI * 2) / splits;

        line.SetVertexCount(splits);

        for (int i = 0; i < splits; i++)
        {
            float angle = i * sliceSize;
            var pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            line.SetPosition(i, pos * 0.1f);
        }
    }

    void Update()
    {
        float normCooldown = Mathf.Clamp01(1 - ((Time.time - LastTeleportTime) / Settings.Cooldown));

        if (TeleportCooldownLEDBar != null)
            TeleportCooldownLEDBar.NormFillValue = 1 - normCooldown;

        if (normCooldown > 0)
        {
            HUDObject.gameObject.SetActive(false);
            Beam.enabled = false;
            return;
        }

        //if (Application.isEditor)
        //    Position = new Vector3[BeamSplits];

        bool tryBeam = HUDButtonPress();
        Vector3 dir = Trans.forward;
        float angle = Vector3.Angle(dir, Vector3.up);

        bool validBeam = false;
        bool blocked = false;
        if (tryBeam)
        {
            bool insideBlocker = false;
            Vector3 teleportPos = Vector3.zero;
            Vector3 pos = Trans.position;
            foreach (var teleportBocker in TeleportBlockers)
            {
                if (teleportBocker == null || teleportBocker.gameObject == null || teleportBocker.gameObject.activeSelf == false)
                    continue;

                if (teleportBocker.bounds.Contains(pos))
                {
                    blocked = true;
                    insideBlocker = true;
                }
            }

            int index = 0;
            while (index < (Settings.BeamSplits - 1))
            {
                Position[index] = pos;
                index++;
                Ray ray = new Ray(pos, dir);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, Settings.BeamSplitSize, Settings.GroundLayer.value | Settings.BlockLayers.value))
                //foreach (var hitInfo in Physics.RaycastAll(ray, BeamSplitSize, GroundLayer.value | BlockLayer.value))
                {
                    teleportPos = hitInfo.point;
                    if (((1 << hitInfo.collider.gameObject.layer) & Settings.BlockLayers.value) > 0)
                    {
                        blocked = true;
                        continue;
                    }

                    if (teleportPos.y < Settings.MinYLevel)
                        continue;

                    if (hitInfo.normal != Vector3.up)
                        continue;

                    HitPoint = teleportPos;

                    var cols = Physics.OverlapSphere(teleportPos, Settings.MinWallDistance, Settings.BlockLayers.value);

                    Directions.Clear();
                    if (cols.Length > 0)
                    {
                        int rays = 36;
                        float slices = (Mathf.PI * 2) / rays;
                        Vector3 closest = teleportPos;
                        float closestDist = float.MaxValue;
                        for (int i = 0; i < rays; i++)
                        {
                            float rayAngle = i * slices;
                            var rayDir = new Vector3(Mathf.Cos(rayAngle), 0, Mathf.Sin(rayAngle)) + Vector3.up * 0.05f;
                            Directions.Add(rayDir);

                            RaycastHit hitInfo2;
                            ray = new Ray(teleportPos, rayDir);
                            if (Physics.Raycast(ray, out hitInfo2, Settings.MinWallDistance, Settings.BlockLayers.value))
                            {
                                float dist = Vector3.Distance(hitInfo2.point, teleportPos);
                                if (dist < closestDist)
                                {
                                    validBeam = true;
                                    blocked = false;
                                    closestDist = dist;
                                    closest = hitInfo2.point + (hitInfo2.normal * Settings.MinWallDistance);
                                }
                            }
                        }

                        if (closestDist != float.MaxValue)
                        {
                            teleportPos = closest;
                            break;
                        }
                    }

                    validBeam = true;
                    break;
                }

                Vector3 lastPos = pos + (dir * Settings.BeamSplitSize);
                Position[index] = lastPos;
                for (int i = index; i < Position.Length; i++)
                    Position[i] = lastPos;

                Vector3 nextPos = pos + (dir * Settings.BeamSplitSize);
                nextPos += Vector3.down * Settings.BeamGravity;

                dir = (nextPos - pos).normalized;
                pos = nextPos;
            }

            if (validBeam && blocked == false && insideBlocker == false)
            {
                HUDObject.transform.position = teleportPos;
                if (TeleportPressedDown())
                    Teleport(teleportPos);

                Beam.material = ValidTeleport;
                //HUDLineRenderer.material = ValidTeleport;
            }
            else
            {
                Beam.material = InValidTeleport;
                //HUDLineRenderer.material = InValidTeleport;

                Position[Position.Length - 1] = pos + (dir * Settings.BeamSplitSize);
            }

            if (tryBeam)
            {
                for (int i = 0; i < Position.Length; i++)
                    Beam.SetPosition(i, Position[i]);
            }
        }

        HUDObject.gameObject.SetActive(tryBeam);
        Beam.enabled = tryBeam;
    }

    private void Teleport(Vector3 teleportPos)
    {
        Vector3 localOffset = POVObject.position - ObjectToTeleport.position;
        Vector3 offset = Vector3.zero;

        Vector3 newPos = ObjectToTeleport.position;
        if (TeleportAxis.X)
        {
            newPos.x = teleportPos.x;
            offset.x = localOffset.x;
        }
        if (TeleportAxis.Y)
        {
            newPos.y = teleportPos.y;
            //offset.y = localOffset.y;
        }
        if (TeleportAxis.Z)
        {
            newPos.z = teleportPos.z;
            offset.z = localOffset.z;
        }

        ObjectToTeleport.position = newPos - offset;
        LastTeleportTime = Time.time;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(HitPoint, Settings.MinWallDistance);
        Gizmos.color = Color.red;
        foreach (var dir in Directions)
        {
            Gizmos.DrawLine(HitPoint, HitPoint + (dir * Settings.MinWallDistance));
        }
    }
}
