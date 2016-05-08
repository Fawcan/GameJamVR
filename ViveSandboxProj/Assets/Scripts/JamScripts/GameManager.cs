using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject mNewWave;
    [SerializeField] private GameObject mWaveCompletedObj;
    [SerializeField] private Snowman mSnowman;
    [SerializeField] private EnemySpawner mSpawner;
    [SerializeField] private int mEnemiesLeft;
    [SerializeField] private int mWaveNr = 0;
    [SerializeField] private bool mBetweenWaves = true;
    [SerializeField] private bool mWaveCompleted;
    [SerializeField] private float mTimer;
    [SerializeField] private float mCD = 10f;
    public bool BetweenWaves
    {
        get { return mBetweenWaves; }
        set { mBetweenWaves = value; }
    }

    public int EnemiesLeft
    {
        get { return mEnemiesLeft; }
        set { mEnemiesLeft = value; }
    }

    public int WaveNr
    {
        get { return mWaveNr; }
        set { mEnemiesLeft = value; }
    }

	// Use this for initialization
	void Awake ()
    {
        mSpawner = GetComponent<EnemySpawner>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(mTimer > 0)
        {
            mTimer -= Time.deltaTime;
        }
        
        if(mTimer <= 0)
        {
            mWaveCompleted = false;
        }

        
	    if(mSpawner.EnemiesToSpawn <= 0 && EnemiesLeft == 0 && !BetweenWaves)
        {
            mWaveCompleted = true;
            mTimer = mCD;
        }

        if (mWaveCompleted)
        {
            if(mWaveCompletedObj != null)
            {
                mWaveCompletedObj.SetActive(true);
            }
        }
        else
        {
            mWaveCompletedObj.SetActive(false);
        }

        if(BetweenWaves)
        {
            if(mNewWave != null)
            {
                mNewWave.SetActive(true);
            }
        }
        else
        {
            mNewWave.SetActive(false);
        }


	}

    public void NewWave()
    {
        mWaveNr += 1;
        mSpawner.EnemiesToSpawn += 6;
    }
}
