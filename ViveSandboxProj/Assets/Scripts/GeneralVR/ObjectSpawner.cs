using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns objects out of thing air when grebing with objects grabing inside collider
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    private Transform SpawnPoint;
    [SerializeField]
    private Rigidbody ObjectPrefab;
    [SerializeField]
    private bool SpawnObjectInOrigo;
    [SerializeField]
    private Pickupable PickupablePrefab;
    [SerializeField]
    private bool SpawnPickupableInOrigo;

    void Start()
    { }

    public Rigidbody RequestObjectInstance(Transform grabberTrans)
    {
        if (ObjectPrefab == null)
            return null;

        if (SpawnObjectInOrigo)
            return (Rigidbody)Instantiate(ObjectPrefab, grabberTrans.position, grabberTrans.rotation);
        else
            return (Rigidbody)Instantiate(ObjectPrefab, SpawnPoint.position, SpawnPoint.rotation);
    }
    public Pickupable RequestPickupableInstance(Transform grabberTrans)
    {
        if (PickupablePrefab == null)
            return null;

        if (SpawnPickupableInOrigo)
            return (Pickupable)Instantiate(PickupablePrefab, grabberTrans.position, grabberTrans.rotation);
        else
            return (Pickupable)Instantiate(PickupablePrefab, SpawnPoint.position, SpawnPoint.rotation);
    }
}
