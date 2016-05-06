using UnityEngine;
using System.Collections;

/// <summary>
/// Automaticly destroys an particle system when all the particles are gone
/// </summary>
public class AutoDestroyPS : MonoBehaviour
{
    private ParticleSystem ps;

    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public void Update()
    {
        if (ps != null && ps.IsAlive() == false)
            Destroy(gameObject);
    }
}