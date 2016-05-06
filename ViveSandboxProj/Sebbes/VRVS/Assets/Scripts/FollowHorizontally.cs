using UnityEngine;
using System.Collections;

public class FollowHorizontally : MonoBehaviour {

    public Transform Target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Target.position.x, transform.position.y, transform.position.z) + Vector3.up * Mathf.Sin(Time.time) * 0.001f;

	}
}
