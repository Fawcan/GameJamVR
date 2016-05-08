using UnityEngine;
using System.Collections;

public class DestroyOverTime : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(StartDelay());
        Debug.Log("Delay started.");
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Delay done.");
        DestroyObject(gameObject);
    }
}
