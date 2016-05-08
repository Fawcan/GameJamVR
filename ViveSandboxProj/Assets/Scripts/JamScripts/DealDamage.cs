using UnityEngine;
using System.Collections;

public class DealDamage : MonoBehaviour
{

    [SerializeField]
    private float mAttackTimer;

    [SerializeField]
    private Snowman mSnowman;

    [SerializeField]
    private Enemy mThisEnemy;


    // Use this for initialization
    void Awake()
    {
        mThisEnemy = GetComponentInParent<Enemy>();
        mSnowman = GameObject.FindGameObjectWithTag("Snowman").GetComponent<Snowman>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mAttackTimer > 0)
        {

            mAttackTimer -= Time.deltaTime;
        }
        if (mThisEnemy.SnowmanInRange && mAttackTimer <= 0)
        {
            mAttackTimer = mThisEnemy.DamageCD;
            mSnowman.TakeDamage(mThisEnemy.Damage);
        }

        float dist = Vector3.Distance(mSnowman.transform.position, transform.position);
        mThisEnemy.SnowmanInRange = (dist < 1);
    }

}
