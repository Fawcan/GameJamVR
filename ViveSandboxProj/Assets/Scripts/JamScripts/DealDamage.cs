using UnityEngine;
using System.Collections;

public class DealDamage : MonoBehaviour
{

    [SerializeField]
    private float mAttackTimer;

    [SerializeField]
    private Snowman mSnowman;

    private Enemy mThisEnemy;


    // Use this for initialization
    void Awake ()
    {
        mThisEnemy = GetComponentInParent<Enemy>();
        mSnowman = GameObject.FindGameObjectWithTag("Snowman").GetComponent<Snowman>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (mAttackTimer > 0)
        {

            mAttackTimer -= Time.deltaTime;
        }
	    if(mThisEnemy.SnowmanInRange && mAttackTimer <= 0)
        {
            mAttackTimer = mThisEnemy.DamageCD;
            mSnowman.TakeDamage(mThisEnemy.Damage);
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Snowman")
        {
            mThisEnemy.SnowmanInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.transform.tag == "Snowman")
        {
            mThisEnemy.SnowmanInRange = false;
        }
    }
}
