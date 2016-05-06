using UnityEngine;
using System.Collections;
using System;

public class DeathCollision : MonoBehaviour {

    public TextMesh OpponentScore;
    int opponentScore = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision c)
    {
        if (c.collider.name == "DeathDisk")
            transform.parent.GetComponent<DeathCollision>().Die();

    }

    public void Die()
    {
        print(name + " died.");

        opponentScore++;
        OpponentScore.text = opponentScore.ToString();
    }
}
