using UnityEngine;
using System.Collections;
using System;

public class RotateToFaceFloor : MonoBehaviour {

    Quaternion q;
    

	// Use this for initialization
	void Start () {
        q = CopyRotation();
	}

    private Quaternion CopyRotation()
    {
        return new Quaternion(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w);
    }

    // Update is called once per frame
    void Update () {


        transform.localRotation = Quaternion.Lerp(transform.localRotation, q, Time.time *.1f);
	}


}
