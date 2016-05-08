using UnityEngine;
using System.Collections;

public class SmashTarget : MonoBehaviour {

    public GameObject explosion;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            if(GameObject.FindGameObjectWithTag("Snowball").GetComponent<Snowball>())
            {
                //GameManager.gm.vibrate(3999);

                Destroy(gameObject);

                if (explosion != null)
                {
                    Instantiate(explosion, gameObject.transform.position, gameObject.transform.rotation);
                }
            }
        }
        if (other.tag == "Shurikan") {
            Destroy(gameObject);
            Destroy(other);
            if (explosion != null)
            {
                Instantiate(explosion, gameObject.transform.position, gameObject.transform.rotation);
            }
        }
    }
}
