using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns objects out of thing air when grebing with objects grabing inside collider
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    private LEDBar mLed;
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
    [SerializeField]
    private int mNrOfSnowballs;
    [SerializeField] private int mMaxNrOfSnowballs = 20;


    public int NrOfSnowballs
    {
        get { return mNrOfSnowballs; }
        set { NrOfSnowballs = value; }
    }

    void Update()
    {
        if(mLed != null)
        {
            mLed.NormFillValue = mNrOfSnowballs / mMaxNrOfSnowballs;
        }
    }

    void Start()
    { }

    public Rigidbody RequestObjectInstance(Transform grabberTrans)
    {
        if (ObjectPrefab == null)
            return null;
        if (mNrOfSnowballs > 0)
        {
            if (SpawnObjectInOrigo)
                return (Rigidbody)Instantiate(ObjectPrefab, grabberTrans.position, grabberTrans.rotation);
            else
                return (Rigidbody)Instantiate(ObjectPrefab, SpawnPoint.position, SpawnPoint.rotation);
        }
        else
        {
            Debug.Log("No snowballs left");
            return null;
        }
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
