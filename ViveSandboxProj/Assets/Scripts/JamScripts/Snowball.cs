using UnityEngine;
using System.Collections;



public class Snowball : MonoBehaviour {
    [SerializeField] private Vector3 mSize;
    [SerializeField] private int mSnowballSize = 1;

    [SerializeField] private int mDamage = 1;


    // Use this for initialization
    void Awake()
    {
        mSize = this.transform.localScale;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Enemy")
        {
            Debug.Log(other.transform.tag);
            other.transform.GetComponent<Enemy>().TakeDamage(mDamage);
            DestroyObject(gameObject);
        }
    }

    public void UpScale()
    {
        if (mSnowballSize < 4)
        {
            mSize *= 2;
            this.transform.localScale = mSize;
            mSnowballSize++;
        }
    }
}
