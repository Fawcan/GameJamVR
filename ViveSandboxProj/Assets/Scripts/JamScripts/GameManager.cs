using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject mNewWave;
    [SerializeField] private Snowman mSnowman;
    [SerializeField] private EnemySpawner mSpawner;
    [SerializeField] private int mEnemiesLeft;
    [SerializeField] private int mWaveNr = 0;
    [SerializeField] private bool mBetweenWaves = true;
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
       
        
	    if(mSpawner.SpawnsLeft <= 0 && EnemiesLeft == 0 && !BetweenWaves)
        {
            BetweenWaves = true;
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
        mSpawner.EnemiesToSpawn += 8;
        mSpawner.SpawnsLeft = mSpawner.EnemiesToSpawn;

        mEnemiesLeft = mSpawner.EnemiesToSpawn;

        BetweenWaves =  false;
    }
}
