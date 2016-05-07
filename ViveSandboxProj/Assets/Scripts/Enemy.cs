using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [SerializeField] private int mEnemyHP = 1;
    [SerializeField] private int mEnemyDmg = 1;
    [SerializeField] private Transform mGoal;
    [SerializeField] private float mSpeed = 1.2f;
    [SerializeField] private float mDamageCD = 5f;

    [SerializeField] private float mAttackTimer;
    [SerializeField] private bool mSnowmanInRange = false;
    [SerializeField] private Snowman mSnowman;

    public int EnemyHP
    {
        get { return mEnemyHP; }
        set { mEnemyHP = value; }
    }

    public int Damage
    {
        get { return mEnemyDmg; }
        set { mEnemyDmg = value; }
    }

    public float Speed
    {
        get { return mSpeed; }
        set { mSpeed = value; }
    }
                                       // Use this for initialization
    void Awake ()
    {
        NavMeshAgent mAgent = GetComponent<NavMeshAgent>();
        mGoal = GameObject.FindGameObjectWithTag("Snowman").transform;
        mAgent.destination = mGoal.position;
        mAgent.speed = Speed;
	}
	
    
	// Update is called once per frame
	void Update ()
    {
	    if(EnemyHP <= 0)
        {
            DestroyObject(gameObject);
        }

        mAttackTimer += Time.deltaTime;
        
        if(mAttackTimer >= mDamageCD && mSnowmanInRange && EnemyHP > 0)
        {
            DealDamage();
        }       
	}


    void OnCollisionEnter(Collision other)
    {
        
        if(other.transform.tag == "Snowball")
        {
            Debug.Log("Collision with snowball");
            EnemyHP -= 1;
        }
        if(other.transform.tag == "Snowman")
        {
            other.transform.GetComponent<Snowman>().TakeDamage(Damage);
            DestroyObject(gameObject);
        }

    }

    void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Snowman")
        {
            mSnowmanInRange = true;
        }
    }

    void OnTriggerExit (Collider other)
    {
        if(other.tag == "Snowman")
        {
            mSnowmanInRange = false; 
        }
    }

    void DealDamage()
    {
        mSnowman.TakeDamage(Damage);
    }
}
