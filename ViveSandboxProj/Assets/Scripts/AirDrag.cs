using UnityEngine;
using System.Collections;

/// <summary>
/// Simulated air drag with the up factor as the drag normal
/// </summary>
public class AirDrag : MonoBehaviour
{
    [SerializeField]
    private float Drag;

    private Transform Trans;
    private Rigidbody Body;

    void Awake()
    {
        Trans = transform;
        Body = GetComponent<Rigidbody>();
        if (Body == null)
            Body = GetComponentInParent<Rigidbody>();

        if (Body == null)
            Debug.LogError("AirDrag attachesh to non rigidbody");
    }
    
	void Start ()
    {
	
	}
	
	void Update ()
    {
        float speedFactor = Mathf.Pow(Body.velocity.magnitude / Time.deltaTime, 2);
        float angleFactor = Vector3.Dot(Body.velocity.normalized, Trans.up);

        if (float.IsNaN(speedFactor) == false)
            Body.AddForceAtPosition(Trans.up * Drag * speedFactor * -angleFactor * Time.deltaTime, Trans.position);
        else
        {
            Debug.Log("Body.velocity.magnitude: " + Body.velocity.magnitude);
        }
    }
}
