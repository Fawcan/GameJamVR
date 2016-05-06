using UnityEngine;
using System.Collections;

/// <summary>
/// For making an object splitable in to to or more other objects
/// </summary>
public class SplitableObject : MonoBehaviour
{
    public enum SplitTypes  { PreSpawnedSplitObject, SpawnOnSplit, SpawnNewOnSplit }

    public SplitTypes SplitType;
    
    public GameObject PreSpawnedObject;

    public GameObject SplitObjectPrefab;
    public Vector3 SpawnOffset;

    public GameObject FirstSplitObject;
    public GameObject SecondSplitObject;
    public Vector3 FirstSpawnOffset;
    public Vector3 SecondSpawnOffset;

    [HideInInspector]
    [System.NonSerialized]
    public bool Splited;

    private Transform Trans;
    private Transform SplitObjectTrans;

    void Start ()
    {
        Trans = transform;

        if (SplitType == SplitTypes.PreSpawnedSplitObject)
        {
            SplitObjectTrans = PreSpawnedObject.transform;
        }
    }
	
	void Update ()
    {
	
	}

    public void SplitAndGetObject(Transform firstGraberTrans, Transform secondGraberTrans, Transform objectInHandTrans, out Rigidbody firstFirstObject, out Rigidbody secondFirstObject)
    {
        firstFirstObject = null;
        secondFirstObject = null;
        GameObject obj;
        switch (SplitType)
        {
            case SplitTypes.PreSpawnedSplitObject:
                Splited = true;
                secondFirstObject = PreSpawnedObject.AddComponent<Rigidbody>();
                break;
            case SplitTypes.SpawnOnSplit:
                obj = Instantiate(SplitObjectPrefab);
                secondFirstObject = obj.GetComponent<Rigidbody>();
                secondFirstObject.isKinematic = true;
                obj.transform.localRotation = secondGraberTrans.localRotation;
                obj.transform.position = objectInHandTrans.position - objectInHandTrans.TransformVector(SpawnOffset);
                break;
            case SplitTypes.SpawnNewOnSplit:
                Splited = true;

                obj = Instantiate(FirstSplitObject);
                firstFirstObject = obj.GetComponent<Rigidbody>();
                firstFirstObject.isKinematic = true;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.position = objectInHandTrans.position - objectInHandTrans.TransformVector(FirstSpawnOffset);

                obj = Instantiate(SecondSplitObject);
                secondFirstObject = obj.GetComponent<Rigidbody>();
                secondFirstObject.isKinematic = true;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.position = objectInHandTrans.position - objectInHandTrans.TransformVector(SecondSpawnOffset);
                break;
        }
    }
}
