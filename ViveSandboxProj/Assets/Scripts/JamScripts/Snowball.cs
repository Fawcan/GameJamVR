using UnityEngine;
using System.Collections;



public class Snowball : MonoBehaviour {

    [SerializeField] private Vector3 mSize1;
    [SerializeField] private float mMass1;
    [SerializeField] private Vector3 mSize2;
    [SerializeField] private float mMass2;
    [SerializeField] private Vector3 mSize3;
    [SerializeField] private float mMass3;
    [SerializeField] private Vector3 mSize4;
    [SerializeField] private float mMass4;
    [SerializeField] private int mSnowballSize = 1;

    [SerializeField] private int mDamage = 1;

    [SerializeField]private GameObject explosion;

    private Rigidbody mRigidBody;



    // Use this for initialization
    void Awake()
    {
        mSize1 = this.transform.localScale;
        mRigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.transform.tag);
        if(other.transform.tag == "Enemy")
        {
            Debug.Log("Collision with enemy");
            other.transform.GetComponent<Enemy>().TakeDamage(mDamage);
            if(mSnowballSize < 3)
            {
                DestroySnowball();
            }
        }
        else if(other.transform.tag == "Ground")
        {
            Debug.Log("Collision with Ground");
            if(mSnowballSize < 4)
            {
                DestroySnowball();
            }
        }

        else
        {
            return;
        }


    }

    void DestroySnowball()
    {
        Debug.Log("Snowball destroyed");
        //Insert the particle system here
        if (explosion != null)
        {
            Instantiate(explosion, gameObject.transform.position, gameObject.transform.rotation);
        }

        DestroyObject(gameObject);
    }

    public void UpScale()
    {
        Debug.Log("Starded the upscaling in merge on :" + gameObject + " current size is " + this.mSnowballSize);
        mSnowballSize++;
   
        if (mSnowballSize > 4)
        {
            mSnowballSize = 4;
        }
        
        switch(mSnowballSize)
        {
            
            case 2:
                this.transform.localScale = mSize2;
                //mRigidBody.mass = mMass2;
                break;

            case 3:
                this.transform.localScale = mSize3;
                //mRigidBody.mass = mMass3;
                break;

            case 4:
                this.transform.localScale = mSize4;
                //mRigidBody.mass = mMass4;
                break;

        }         
            
        
    }
}
