using UnityEngine;
using System.Collections;


public class Snowball : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag == "Enemy" || other.transform.tag == "Ground")
        {
            DestroyObject(gameObject);
        }
    }
}
