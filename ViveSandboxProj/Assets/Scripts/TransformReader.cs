using UnityEngine;
using System.Collections;

public class TransformReader : MonoBehaviour
{
    [SerializeField]
    private Transform ReadFrom;

    private Transform Trans;

    void Awake()
    {
        Trans = transform;
    }

    void Update()
    {
        Trans.position = ReadFrom.position;
        Trans.rotation = ReadFrom.rotation;
    }
}
