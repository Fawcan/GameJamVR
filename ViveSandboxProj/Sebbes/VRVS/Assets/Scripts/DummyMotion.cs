using UnityEngine;
using System.Collections;
using System;

public class DummyMotion : MonoBehaviour {

    public Vector3 Direction = Vector3.forward;
    public float Velocity = 0f;

    public float EngineForce = 0.8f;
    Vector3 engine;

    public float TargetAltitude = 0.5f;

    Rigidbody rb;
    Transform t;
    
    

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        t = transform;
	}

    void Update()
    {
        t.Rotate(Vector3.up, Mathf.Abs(rb.velocity.z)+ 1f);
    }
	
	// Update is called once per frame
	void FixedUpdate() {

        if (Input.GetKey(KeyCode.Space))
            rb.velocity = ((Vector3.forward * -10f + Vector3.down + Vector3.right * 0.1f) * 5f);

        rb.AddForce(-Physics.gravity*EngineForce);

        if (rb.velocity.y < 0f)
            rb.AddForce(-Vector3.up * rb.velocity.y);

        if (t.position.y < TargetAltitude)
        {
                rb.AddForce(Vector3.up * (TargetAltitude - t.position.y) * 10f);
        }
            
    }
    
    void OnCollisionEnter(Collision c)
    {
        Direction = Vector3.Reflect(Direction, c.contacts[0].normal);
    }
}
