using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject mEnemy;
    [SerializeField] Snowman mSnowman;
    [SerializeField] Transform[] mSpawnPoints;
    [SerializeField] private float mSpawnCD = 5f;
    [SerializeField] private int mNrOfEnemiesToSpawn = 8;
    [SerializeField] private GameManager mManager;
    [SerializeField] private int mSpawnsLeft;

    [SerializeField] private float mSpawnTimer = 0f;

    void Awake ()
    {
        mManager = GetComponent<GameManager>();
    }

    public int EnemiesToSpawn
    {
        get { return mNrOfEnemiesToSpawn; }
        set { mNrOfEnemiesToSpawn = value; }
    }

    public int SpawnsLeft
    {
        get { return mSpawnsLeft; }
        set { mSpawnsLeft = value; }
    }

    // Update is called once per frame

    void Update()
    {
        mSpawnTimer += Time.deltaTime;
        if (mSpawnTimer >= mSpawnCD && mNrOfEnemiesToSpawn > 0)
        {
            Spawn();
        }
    }
    void Spawn ()
    {
        if(mSnowman.SnowmanHealth <= 0)
        {
            return;
        }


            mSpawnTimer = 0f;
            mSpawnsLeft -= 1;

            int mSpawnPointIndex = Random.Range(0, mSpawnPoints.Length);

            Instantiate(mEnemy, mSpawnPoints[mSpawnPointIndex].position, mSpawnPoints[mSpawnPointIndex].rotation);


        
    }
}

