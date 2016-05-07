﻿using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject mEnemy;
    [SerializeField] Snowman mSnowman;
    [SerializeField] Transform[] mSpawnPoints;
    [SerializeField] private float mSpawnCD = 5f;
    [SerializeField] private int mNrOfEnemiesToSpawn;

    [SerializeField] private float mSpawnTimer = 0f;

    // Update is called once per frame

    void Update()
    {
        mSpawnTimer += Time.deltaTime;
    }
    void Spawn ()
    {
        if(mSnowman.SnowmanHealth <= 0)
        {
            return;
        }

        if (mSpawnTimer >= mSpawnCD && mNrOfEnemiesToSpawn > 0)
        {
            mSpawnTimer = 0f;
            mNrOfEnemiesToSpawn -= 1;

            int mSpawnPointIndex = Random.Range(0, mSpawnPoints.Length);

            Instantiate(mEnemy, mSpawnPoints[mSpawnPointIndex].position, mSpawnPoints[mSpawnPointIndex].rotation);


        }
    }
}
