using UnityEngine;
using System.Collections;

public class Hoop : MonoBehaviour
{
    [SerializeField]
    private bool DestroyOnHit;
    [SerializeField]
    private bool DestroyObjectOnHit;

    void OnTriggerEnter(Collider col)
    {
        if (DestroyOnHit)
            Destroy(gameObject);
        if (DestroyObjectOnHit)
            Destroy(col.gameObject);
    }
}
