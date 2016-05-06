using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Holdsn objects in place, can be used like a holder or something
/// </summary>
public class ObjectHolder : MonoBehaviour
{
    [SerializeField]
    private bool Arrange;
    [SerializeField]
    private int MaxObjects;
    [SerializeField]
    private Transform ArrangementStart;
    [SerializeField]
    private float ArrangementOffset;

    struct ObjectData
    {
        public Transform Trans;
        public Rigidbody Body;
        public Transform OldParent;
    }

    private Transform Trans;
    private List<ObjectData> ArrangedObjects = new List<ObjectData>();
    private List<Rigidbody> PotentialObjects = new List<Rigidbody>();

    void Awake()
    {
        Trans = transform;
    }

    void Start()
    {

    }

    void Update()
    {
        for (int i = ArrangedObjects.Count - 1; i >= 0; i--)
        {
            if (ArrangedObjects[i].Trans.parent == Trans)
            {
                if (Arrange)
                {
                    float offset = i * ArrangementOffset;
                    Vector3 arrangedPos = ArrangementStart.position + (offset * ArrangementStart.right);
                    ArrangedObjects[i].Trans.position = arrangedPos;
                    ArrangedObjects[i].Trans.rotation = ArrangementStart.rotation;
                }
            }
            else
            {
                ArrangedObjects.RemoveAt(i);
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        var body = FindRigidbody(col.gameObject);
        if (body != null)
            HandleObject(body);
    }
    void OnTriggerStay(Collider col)
    {
        var body = FindRigidbody(col.gameObject);
        if (body != null)
            HandleObject(body);
    }
    void OnTriggerExit(Collider col)
    {
        var body = FindRigidbody(col.gameObject);
        if (body != null)
        {
            if (PotentialObjects.Contains(body))
                PotentialObjects.Remove(body);

            for (int i = ArrangedObjects.Count - 1; i >= 0; i--)
            {
                if (ArrangedObjects[i].Body == body)
                    ArrangedObjects.RemoveAt(i);
            }
        }
    }

    Rigidbody FindRigidbody(GameObject obj)
    {
        var body = obj.GetComponent<Rigidbody>();
        if (body == null)
            body = obj.GetComponentInParent<Rigidbody>();

        return body;
    }

    void HandleObject(Rigidbody body)
    {
        if (ArrangedObjects.Count >= MaxObjects)
            return;

        for (int i = ArrangedObjects.Count - 1; i >= 0; i--)
        {
            if (ArrangedObjects[i].Body == body)
                return;
        }

        bool inHand = body.transform.parent != null && body.transform.parent.GetComponent<ObjectGrabing>() != null;
        if (inHand && body.isKinematic)
        {
            if (PotentialObjects.Contains(body) == false)
                PotentialObjects.Add(body);
        }
        else
        {
            if (PotentialObjects.Contains(body))
            {
                PotentialObjects.Remove(body);
                ArrangedObjects.Add(new ObjectData()
                {
                    Trans = body.transform,
                    Body = body,
                    OldParent = body.transform.parent
                });

                body.transform.parent = Trans;
                body.isKinematic = true;

                if (Arrange)
                {
                    float offset = ArrangedObjects.Count * ArrangementOffset;
                    Vector3 arrangedPos = ArrangementStart.position + (offset * ArrangementStart.right);
                    body.transform.position = arrangedPos;
                    body.transform.rotation = ArrangementStart.rotation;
                }
            }
        }
    }

    public Transform GetOldParent(Transform obj, out bool exists)
    {
        exists = false;
        foreach (var arrObj in ArrangedObjects)
        {
            if (arrObj.Trans == obj)
            {
                exists = true;
                return arrObj.OldParent;
            }
        }

        return null;
    }
}
