using UnityEngine;
using System.Collections;

public class Snowman : MonoBehaviour {
    [SerializeField] private int mHealth;

    public int SnowmanHealth
    {
        get { return mHealth; }
        set { mHealth = value; }
    }
                                       // Use this for initialization
    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(SnowmanHealth <= 0)
        {
            //Run gameover screen
        }
	}

    public void TakeDamage(int mDamage)
    {
        SnowmanHealth -= mDamage;
    }
}
