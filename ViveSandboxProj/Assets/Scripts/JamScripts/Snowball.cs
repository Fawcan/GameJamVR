using UnityEngine;
using System.Collections;



public class Snowball : MonoBehaviour {

    [SerializeField] private Vector3 mSize = new Vector3(5.5f, 5.5f, 5.5f);
    [SerializeField] private float mWeight = 0.2f;
    [SerializeField] private int mSnowballSize = 1;

    [SerializeField] private int mDamage = 1;

    [SerializeField]private GameObject explosion;

    private Rigidbody mRigidBody;
    private Vector3 mStandardSize = new Vector3(5.5f, 5.5f, 5.5f);
    private float mStandardWeight = 0.2f;



    // Use this for initialization
    void Awake()
    {
        mRigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {

    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log(mSnowballSize);
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

    public Vector3 UpScale()
    {
        Debug.Log("Starded the upscaling in merge on :" + gameObject + " current size is " + this.mSnowballSize);
        mSnowballSize++;
        if (mSnowballSize > 4)
            mSnowballSize = 4;

        mSize = AdjustSize();
        mWeight =  AdjustWeight();
        

        //this.transform.localScale = mSize;
        mRigidBody.mass = mWeight;
        return mSize;
    }

    Vector3 AdjustSize ()
    {
        Debug.Log("Adjusting seize" + mSize);
        Vector3 Size;
        if (mSnowballSize == 4)
            Size = new Vector3(30, 30, 30);
        else if (mSnowballSize == 3)
            Size = new Vector3(18, 18, 18);
        else if (mSnowballSize == 2)
            Size = new Vector3(8, 8, 8);
        else
            Size = mStandardSize;
        Debug.Log(Size);
        return Size;
    }

    float AdjustWeight()
    {
        Debug.Log("Adjusting weight" + mWeight);
        float Weight;
        if (mSnowballSize == 4)
            Weight = 1.2f;
        else if (mSnowballSize == 3)
            Weight = 0.7f;
        else if (mSnowballSize == 2)
            Weight = 0.5f;
        else
            Weight = mStandardWeight;

        Debug.Log(Weight);
        return Weight;
    }


}
