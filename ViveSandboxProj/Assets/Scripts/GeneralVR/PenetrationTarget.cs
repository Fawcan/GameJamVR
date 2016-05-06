using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Will register penetrations from 'Penetrating' objects, used for traget boards and such
/// </summary>
public class PenetrationTarget : HandInteractionBase<ObjectGrabingV2>
{
    [SerializeField]
    public bool BecomeParent;
    [SerializeField]
    [Tooltip("Not used when zero")]
    public float SlippAngle;
    [SerializeField]
    private bool DestroyThisOnHit;
    [SerializeField]
    private GameObject[] DestroyOnHit;
    [SerializeField]
    private MonoBehaviour[] DisableOnHit;
    [SerializeField]
    private Rigidbody[] NonKinematicOnHit;
    [SerializeField]
    private ParticleSystem HitEffect;
    [SerializeField]
    private ParticleSystem RemoveEffect;

    public int ArrowHits { get; private set; }

    public System.Action<Transform, Vector3> OnHit;

    struct PenetratinObjectData
    {
        public Transform Trans;
        public Transform OriginalParent;
        public int OriginalLayer;
    }

    private List<PenetratinObjectData> ArrowsStuck = new List<PenetratinObjectData>();

    void Update()
    {
        for (int i = ArrowsStuck.Count - 1; i >= 0; i--)
        {
            foreach (var graber in Grabers)
            {
                if (graber.ObjectInHandTrans == ArrowsStuck[i].Trans)
                {
                    var data = ArrowsStuck[i];
                    data.Trans.parent = data.OriginalParent;
                    data.Trans.gameObject.layer = data.OriginalLayer;
                    ArrowsStuck.RemoveAt(i);
                }
            }

            //if (ArrowsStuck[i].Trans.parent != ArrowsStuck[i].LastParent)
            //    ArrowsStuck.RemoveAt(i);
        }
    }

    public void Hit(Transform arrow, Vector3 point)
    {
        ArrowsStuck.Add(new PenetratinObjectData() { Trans = arrow, OriginalParent = arrow.parent, OriginalLayer = arrow.gameObject.layer });

        if (BecomeParent)
            arrow.parent = transform;

        arrow.gameObject.layer = gameObject.layer;

        ArrowHits++;

        if (OnHit != null)
            OnHit(arrow, point);

        OnTargetHit(arrow, point);

        if (HitEffect != null)
            Instantiate(RemoveEffect, transform.position, transform.rotation);

        foreach (var obj in DestroyOnHit)
            Destroy(obj);
        foreach (var obj in DisableOnHit)
            obj.enabled = false;
        foreach (var body in NonKinematicOnHit)
            body.isKinematic = false;

        if (DestroyThisOnHit)
            Destroy(gameObject);
    }
    protected virtual void OnTargetHit(Transform arrow, Vector3 point) { }

    public void Reset()
    {
        ArrowHits = 0;
        foreach (var arrow in ArrowsStuck)
            Destroy(arrow.Trans.gameObject);

        ArrowsStuck.Clear();
    }

    public void Remove()
    {
        foreach (var arrow in ArrowsStuck)
            Destroy(arrow.Trans.gameObject);

        ArrowsStuck.Clear();

        if (RemoveEffect != null)
            Instantiate(RemoveEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Reset();
    }
    void OnDisable()
    {
        gameObject.layer = 0;
        foreach (var arrow in ArrowsStuck)
            arrow.Trans.gameObject.SetActive(false);
    }
    protected override void OnEnableBase()
    {
        foreach (var arrow in ArrowsStuck)
            arrow.Trans.gameObject.SetActive(true);
    }
}
