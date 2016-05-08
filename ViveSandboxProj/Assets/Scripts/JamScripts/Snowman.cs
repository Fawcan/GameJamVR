using UnityEngine;
using System.Collections;

public class Snowman : MonoBehaviour {
    [SerializeField] private int mCurrentHealth;
    [SerializeField] private LEDBar mLed;
    [SerializeField] private int mMaxHealth = 5;
    [SerializeField] private GameManager mManager;
    [SerializeField] private GameObject FoomObj;

    public int SnowmanHealth
    {
        get { return mCurrentHealth; }
        set { mCurrentHealth = value; }
    }
                                       // Use this for initialization
    void Awake ()
    {
        SnowmanHealth = mMaxHealth;
        mManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(mLed != null)
        {
            mLed.NormFillValue = SnowmanHealth / mMaxHealth;
            
        }
	    if(SnowmanHealth <= 0)
        {
            Application.Quit();
        }
	}

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Snowball")
        {
            if(mManager.BetweenWaves)
            {
                mManager.NewWave();
            }
        }
    }
    public void TakeDamage(int mDamage)
    {
        FoomObj.SetActive(true);
        SnowmanHealth -= mDamage;
    }
}
