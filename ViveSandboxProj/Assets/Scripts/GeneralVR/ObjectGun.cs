using UnityEngine;
using System.Collections;

/// <summary>
/// Shoots gameobjects of the ProjectilePrefab type
/// </summary>
public abstract class ObjectGun : Pickupable
{
    [SerializeField]
    private Transform ProjectilePrefab;
    [SerializeField]
    private Transform MuzzlePoint;
    [SerializeField]
    private float ImpulseForce;
    [SerializeField]
    private bool FullAuto;
    [SerializeField]
    private float FireRate;
    [SerializeField]
    private MergeableObject AmmoReciver;

    protected abstract bool FireTriggerPress();
    protected abstract bool FireTriggerPressedDown();

    struct StateData
    {
        public float FireDelay;
        public float FireTime;
    }
    StateData _State;

    protected override void AwakePickupable()
    {
        _State.FireDelay = 1f / FireRate;
    }

    void Update()
    {
        if (FullAuto && FireTriggerPress())
        {
            bool canFire = Time.time > (_State.FireTime + _State.FireDelay);
            if (canFire)
            {
                _State.FireTime = Time.time;
                DoFire();
            }
        }
        else
        {
            if (FireTriggerPressedDown())
                DoFire();
        }
    }

    private void DoFire()
    {
        var proj = Instantiate(ProjectilePrefab);
        var body = proj.GetComponent<Rigidbody>();

        proj.position = MuzzlePoint.position;
        proj.rotation = MuzzlePoint.rotation;

        body.AddForce(MuzzlePoint.forward * ImpulseForce);
    }
}
