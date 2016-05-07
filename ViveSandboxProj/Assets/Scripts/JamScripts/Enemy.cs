using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [SerializeField] private int mEnemyHP = 1;
    [SerializeField] private int mEnemyDmg = 1;
    [SerializeField] private Transform mGoal;
    [SerializeField] private float mSpeed = 1.2f;
    [SerializeField] private GameObject mSnowman;
    [SerializeField] private bool mSnowmanInRange = false;
    [SerializeField] private float mDamageCD = 5f;

    private GameManager mManager;

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
    public float DamageCD
    {
        get { return mDamageCD; }
    }

    public bool SnowmanInRange
    {
        get { return mSnowmanInRange; }
        set { mSnowmanInRange = value; }
    }
                                       // Use this for initialization
    void Awake ()
    {
        NavMeshAgent mAgent = GetComponent<NavMeshAgent>();
        mGoal = GameObject.FindGameObjectWithTag("Snowman").transform;
        mAgent.destination = mGoal.position;
        mAgent.speed = Speed;
        mSnowman = GameObject.FindGameObjectWithTag("Snowman");
        //mManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();    
	}
	
    
	// Update is called once per frame
	void Update ()
    {
	    if(EnemyHP <= 0)
        {
            mManager.EnemiesLeft -= 1;
            DestroyObject(gameObject);
        }
	}


    void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.transform.tag);
        if (other.transform.tag == "Snowball")
        {
            Debug.Log("Collision with snowball");
            EnemyHP -= 1;
        }
    }

    
}
