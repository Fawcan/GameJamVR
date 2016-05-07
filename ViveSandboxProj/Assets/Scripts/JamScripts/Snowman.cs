using UnityEngine;
using System.Collections;

public class Snowman : MonoBehaviour {
    [SerializeField] private int mCurrentHealth;
    [SerializeField] private LEDBar mLed;
    [SerializeField] private int mMaxHealth = 5;

    public int SnowmanHealth
    {
        get { return mCurrentHealth; }
        set { mCurrentHealth = value; }
    }
                                       // Use this for initialization
    void Awake ()
    {
        SnowmanHealth = mMaxHealth;
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

    public void TakeDamage(int mDamage)
    {
        SnowmanHealth -= mDamage;
    }
}
