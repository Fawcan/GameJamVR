using UnityEngine;
using System.Collections;

public class SetInactiveOverTime : MonoBehaviour
{
    [SerializeField]
    private float Timer;

    // Use this for initialization
    void OnEnable()
    {
        StartCoroutine(StartDelay());
        Debug.Log("Delay started.");
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(Timer);
        Debug.Log("Delay done.");
        gameObject.SetActive(false);
    }
}

