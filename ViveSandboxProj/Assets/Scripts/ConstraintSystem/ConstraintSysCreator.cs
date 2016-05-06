using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public abstract class ConstraintSysCreator : MonoBehaviour
{
    enum Phases { Idle, MoveTool, RotTool, PlacingNodes, AttachingConstrFirst, AttachingConstrSecond, AttachingObject, MovingNodeInHand, RotNodeInHand, Removing };

    [System.Serializable]
    struct PrefabsData
    {
        public VerletNode Node;
        public DistanceConstraint DistanceConst;
        public Propellar Propellar;
        public NodeWing Wing;
    }

    [SerializeField]
    private PrefabsData Prefabs;
    [SerializeField]
    private VRMenuSystem Menu;
    [SerializeField]
    private Transform HandlePoint;
    [SerializeField]
    private VerletUpdater SystemUpdater;
    [SerializeField]
    private CtrlFaceMenuSystem FaceMenu;
    [SerializeField]
    private float ProximityDistance;
    [SerializeField]
    private Transform ConstraintModelPrefab;

    public ConstraintSysCreator OtherCreator;

    private bool WaitUntilAllInputClear;

    protected abstract bool PlaceObject();

    protected abstract bool AttackFirst();
    protected abstract bool AttackSecond();

    protected abstract bool GrabbingObject();
    protected abstract bool RemoveObject();

    struct StateData
    {
        public Phases Phase;

        public VerletNode NodeInHand;
        public DistanceConstraint ConstrInHand;
        public NodeAttachment AttachmentInHand;

        public Transform ConstraintModel;
    }

    private StateData _State;

    private Transform _Trans;

    void Awake()
    {
        _Trans = transform;
    }

    void Start()
    {
        _State.ConstraintModel = Instantiate(ConstraintModelPrefab);

        _State.ConstraintModel.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        _State.ConstraintModel.gameObject.SetActive(false);

        Menu.MenuInstance.RegisterButton("Node", () =>
        {
            InitNodeCreation();
            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Constr", () =>
        {
            InitConstrCreation();
            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Propellar", () =>
        {
            InitAttachmentPlacing(Prefabs.Propellar);
            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Wing", () =>
        {
            InitAttachmentPlacing(Prefabs.Wing);
            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Move Tool", () =>
        {
            ClearOnHandObjects();
            _State.Phase = Phases.MoveTool;
            _State.NodeInHand = null;
            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Rot Tool", () =>
        {
            ClearOnHandObjects();
            _State.Phase = Phases.RotTool;
            _State.NodeInHand = null;
            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Release All", () =>
        {
            foreach (var node in SystemUpdater.DynamicNodes)
                node.FixedPosition = false;

            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Lock All", () =>
        {
            foreach (var node in SystemUpdater.DynamicNodes)
                node.FixedPosition = true;

            WaitUntilAllInputClear = true;
            return true;
        });
        Menu.MenuInstance.RegisterButton("Remove", () =>
        {
            if (_State.Phase != Phases.MovingNodeInHand)
            {
                ClearOnHandObjects();
                _State.Phase = Phases.Removing;
                _State.NodeInHand = null;
                WaitUntilAllInputClear = true;
                return true;
            }

            return false;
        });
        Menu.MenuInstance.RegisterButton("Clear Scene", () =>
        {
            if (_State.Phase != Phases.MovingNodeInHand)
            {
                SystemUpdater.ClearAndDeleteAll();
                return true;
            }
            return false;
        });
    }

    void Update()
    {
        if (WaitUntilAllInputClear)
        {
            if (PlaceObject() == false && PlaceObject() == false && GrabbingObject() == false && RemoveObject() == false)
                WaitUntilAllInputClear = false;
            else
                return;
        }

        switch (_State.Phase)
        {
            case Phases.Idle:
                break;
            case Phases.RotTool:
            case Phases.MoveTool:
                DoProximityCheck();
                if (_State.NodeInHand != null)
                {
                    if (GrabbingObject() && _State.Phase != Phases.MovingNodeInHand)
                    {
                        if (_State.Phase == Phases.MoveTool)
                            _State.Phase = Phases.MovingNodeInHand;
                        else if (_State.Phase == Phases.RotTool)
                        {
                            _State.Phase = Phases.RotNodeInHand;
                            var dir = (HandlePoint.position - _State.NodeInHand.Position);
                            _State.NodeInHand.InitStableRotationMode(dir);
                        }
                    }
                }
                break;
            case Phases.PlacingNodes:
                _State.NodeInHand.transform.position = HandlePoint.position;
                if (PlaceObject() && Menu.MenuIsShowing() == false)
                {
                    SystemUpdater.SetUpdateNode(_State.NodeInHand);
                    _State.NodeInHand = null;
                    InitNodeCreation();
                }
                break;
            case Phases.AttachingConstrFirst:
                if (AttackFirst() && Menu.MenuIsShowing() == false)
                {
                    var node = GetClosestNode(null, ProximityDistance);
                    if (node != null)
                    {
                        _State.ConstrInHand.SetNode1(node);
                        _State.Phase = Phases.AttachingConstrSecond;
                    }
                }
                break;
            case Phases.AttachingConstrSecond:
                if (_State.ConstraintModel.gameObject.activeSelf == false)
                    _State.ConstraintModel.gameObject.SetActive(true);

                UpdateCylinderRepresentation();

                if (AttackSecond() && Menu.MenuIsShowing() == false)
                {
                    var node = GetClosestNode(_State.ConstrInHand.Node1, ProximityDistance);
                    if (node != null)
                    {
                        SystemUpdater.SetUpdateConstraint(_State.ConstrInHand);
                        _State.ConstrInHand.SetNode2(node);
                        _State.ConstrInHand.Setup();
                        _State.ConstrInHand = null;
                        InitConstrCreation();

                        _State.ConstraintModel.gameObject.SetActive(false);
                    }
                }
                break;
            case Phases.AttachingObject:
                _State.AttachmentInHand.transform.position = HandlePoint.position;
                _State.AttachmentInHand.transform.rotation = HandlePoint.rotation;
                if (PlaceObject())
                {
                    var closestNode = GetClosestNode(null, ProximityDistance);
                    if (closestNode != null)
                    {
                        SystemUpdater.SetUpdateAttachment(_State.AttachmentInHand);
                        _State.AttachmentInHand.AttachToNode(closestNode);
                    }
                    else
                    {
                        Destroy(_State.AttachmentInHand.gameObject);
                    }
                    _State.AttachmentInHand = null;
                    WaitUntilAllInputClear = true;
                    _State.Phase = Phases.Idle;
                }
                break;
            case Phases.MovingNodeInHand:
                FaceMenu.ShowMenu();

                _State.NodeInHand.transform.position = HandlePoint.position;
                _State.NodeInHand.Position = HandlePoint.position;
                if (GrabbingObject() == false)
                {
                    _State.NodeInHand = null;
                    _State.Phase = Phases.MoveTool;
                }
                break;
            case Phases.RotNodeInHand:
                {
                    var dir = (HandlePoint.position - _State.NodeInHand.Position);
                    _State.NodeInHand.SetStableTransformForward(dir);
                    if (GrabbingObject() == false)
                    {
                        _State.NodeInHand.EndStableRotation();
                        _State.NodeInHand = null;
                        _State.Phase = Phases.RotTool;
                    }
                    break;
                }
            case Phases.Removing:
                {
                    float cloasesDist;
                    var node = GetClosestNode(null, out cloasesDist, ProximityDistance);
                    var constr = GetClosestConstraint(null, out cloasesDist, Mathf.Min(cloasesDist, ProximityDistance));
                    if (RemoveObject())
                    {
                        if (constr != null)
                        {
                            SystemUpdater.RemoveConstraint(constr);
                            Destroy(constr.gameObject);
                        }
                        else if (node != null)
                        {
                            SystemUpdater.RemoveNode(node);
                            Destroy(node.gameObject);
                        }
                    }
                    break;
                }
        }
    }

    void UpdateCylinderRepresentation()
    {
        var scale = _State.ConstraintModel.localScale;
        var vec = HandlePoint.position - _State.ConstrInHand.Node1.Position;
        scale.y = vec.magnitude / 2;

        _State.ConstraintModel.position = _State.ConstrInHand.Node1.Position + (vec * 0.5f);
        _State.ConstraintModel.up = vec.normalized;
        _State.ConstraintModel.localScale = scale;
    }

    private void ClearOnHandObjects()
    {
        _State.ConstraintModel.gameObject.SetActive(false);

        if (_State.NodeInHand != null)
            Destroy(_State.NodeInHand.gameObject);
        if (_State.ConstrInHand != null)
            Destroy(_State.ConstrInHand.gameObject);
        if (_State.AttachmentInHand != null)
            Destroy(_State.AttachmentInHand.gameObject);
    }

    private void InitNodeCreation()
    {
        ClearOnHandObjects();

        _State.Phase = Phases.PlacingNodes;
        _State.NodeInHand = (VerletNode)Instantiate(Prefabs.Node, HandlePoint.position, Quaternion.identity);
        _State.NodeInHand.Initilize();
        _State.NodeInHand.Setup();
        _State.NodeInHand.FixedPosition = true;
    }

    private void InitConstrCreation()
    {
        if (SystemUpdater.DynamicNodes.Count > 1)
        {
            ClearOnHandObjects();

            _State.Phase = Phases.AttachingConstrFirst;
            _State.ConstrInHand = (DistanceConstraint)Instantiate(Prefabs.DistanceConst, HandlePoint.position, Quaternion.identity);
            _State.ConstrInHand.Initialize();
        }
    }

    private void InitAttachmentPlacing(NodeAttachment attachmentPrefab)
    {
        ClearOnHandObjects();

        _State.AttachmentInHand = (NodeAttachment)Instantiate(attachmentPrefab, HandlePoint.position, Quaternion.identity);
        _State.Phase = Phases.AttachingObject;
    }

    void DoProximityCheck()
    {
        VerletNode closest = null;
        float closestDist = float.MaxValue;
        foreach (var node in SystemUpdater.DynamicNodes)
        {
            float dist = Vector3.Distance(node.Position, HandlePoint.position);
            if (dist < closestDist && dist < ProximityDistance)
            {
                closestDist = dist;
                closest = node;
            }
        }

        if (closest != null && _State.NodeInHand != closest)
        {
            _State.NodeInHand = closest;
            FaceMenu.MenuInstance.ClearButtons();
            FaceMenu.MenuInstance.RegisterButton("Fixed", () =>
            {
                _State.NodeInHand.FixedPosition = true;
            });
            FaceMenu.MenuInstance.RegisterButton("Set Length", () =>
            {
                ReCalcLengthOnConnectedConstraints(_State.NodeInHand);
            });
            FaceMenu.MenuInstance.RegisterButton("Free", () =>
            {
                _State.NodeInHand.FixedPosition = false;
            });
            FaceMenu.MenuInstance.RegisterButton("", () =>
            {
            });
        }

        if (_State.NodeInHand != null)
        {
            float dist = Vector3.Distance(_State.NodeInHand.Position, HandlePoint.position);
            if (dist > ProximityDistance)
            {
                FaceMenu.MenuInstance.ClearButtons();
                _State.NodeInHand = null;
            }
            else
                FaceMenu.ShowMenu();
        }
    }
    VerletNode GetClosestNode(VerletNode except, float maxDistance = float.MaxValue)
    {
        float dummy;
        return GetClosestNode(except, out dummy, maxDistance);
    }
    VerletNode GetClosestNode(VerletNode except, out float closestDist, float maxDistance = float.MaxValue)
    {
        VerletNode closest = null;
        closestDist = float.MaxValue;
        foreach (var node in SystemUpdater.DynamicNodes)
        {
            if (except == node)
                continue;

            float dist = Vector3.Distance(node.Position, HandlePoint.position);
            if (dist < closestDist && dist < maxDistance)
            {
                closestDist = dist;
                closest = node;
            }
        }

        return closest;
    }
    DistanceConstraint GetClosestConstraint(DistanceConstraint except, float maxDistance = float.MaxValue)
    {
        float dummy;
        return GetClosestConstraint(except, out dummy, maxDistance);
    }
    DistanceConstraint GetClosestConstraint(DistanceConstraint except, out float closestDist, float maxDistance = float.MaxValue)
    {
        DistanceConstraint closest = null;
        closestDist = float.MaxValue;
        foreach (var constr in SystemUpdater.AllConstraints)
        {
            if (except == constr)
                continue;

            float dist = Vector3.Distance(constr.transform.position, HandlePoint.position);
            if (dist < closestDist && dist < maxDistance)
            {
                closestDist = dist;
                closest = constr;
            }
        }

        return closest;
    }
    void ReCalcLengthOnConnectedConstraints(VerletNode node)
    {
        foreach (var constr in SystemUpdater.AllConstraints)
        {
            if (constr.Node1 == node || constr.Node2 == node)
            {
                constr.ReCalcLength = true;
            }
        }
    }
}
