using UnityEngine;
using System.Collections;
using System;
using Valve.VR;


abstract class DiskCatcher : MonoBehaviour
{
    protected abstract bool TriggerPressed();
    protected abstract bool TriggerDown();
    protected abstract bool TriggerUp();

    bool diskInHand = false;
    float charge = 0f;
    GameObject disk = null;

    public Transform AttachPoint;
    private bool applyForce = false;

    public AnimationCurve SpinCurve;
    public float BaseForce;

    void Update()
    {
        if (diskInHand && TriggerPressed())
        {
            charge += 0.1f * Time.deltaTime;
            charge = Mathf.Clamp(charge, 0f, 1f);
            SpinDisk();
        }

        if (diskInHand && TriggerUp() && charge > 0f)
            ReleaseDisk();


    }

    private void ReleaseDisk()
    {
        disk.transform.parent = null;
        applyForce = true;
        diskInHand = false;
        
    }

    void SpinDisk()
    {
        AttachPoint.transform.Rotate(Vector3.up, SpinCurve.Evaluate(charge) * 100f);
    }

    void FixedUpdate()
    {
        if(applyForce)
        {
            disk.GetComponent<Rigidbody>().velocity = transform.forward * charge * BaseForce;
            
            charge = 0f;

            applyForce = false;
        }
            
    }

    void OnTriggerStay(Collider c)
    {
        if (diskInHand)
            return;
        if(disk != null)
        {
            if (disk.name == c.name)
                return;
        }
        

        var rb = c.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        c.transform.parent = AttachPoint;
        c.transform.localPosition = Vector3.zero;
        c.transform.localRotation = AttachPoint.localRotation;

        disk = c.gameObject;

        diskInHand = true;
    }

    void OnTriggerExit(Collider c)
    {
        if(c.name == disk.name)
        {
            disk = null;
        }
    }
}