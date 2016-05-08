using UnityEngine;
using System.Collections;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] private float Timer;

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
        yield return new WaitForSeconds(Timer);
        Debug.Log("Delay done.");
        DestroyObject(gameObject);
    }
}
