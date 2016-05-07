using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [SerializeField] private int mEnemyHP = 1;

    public int EnemyHP
    {
        get { return mEnemyHP; }
        set { mEnemyHP = value; }
    }
                                       // Use this for initialization
    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(EnemyHP <= 0)
        {
            DestroyObject(gameObject);
        }
	}

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Snowball")
        {
            EnemyHP -= 1;
        }
    }
}
