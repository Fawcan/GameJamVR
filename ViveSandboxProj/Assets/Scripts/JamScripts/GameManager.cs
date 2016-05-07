using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Snowman mSnowman;
    [SerializeField] private EnemySpawner mSpawner;
    [SerializeField] private int mEnemiesLeft;
    [SerializeField] private int mWaveNr = 0;

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
	    if(mSpawner.EnemiesToSpawn <= 0 && EnemiesLeft == 0)
        {
            //Tell the player to do preparations for next wave
        }
	}

    public void NewWave()
    {
        mWaveNr += 1;
        mSpawner.EnemiesToSpawn += 6;
    }
}
