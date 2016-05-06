using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SplitableObject))]
public class SplitableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = (SplitableObject)target;

        obj.SplitType = (SplitableObject.SplitTypes)EditorGUILayout.EnumPopup("Split Type", obj.SplitType);

        switch (obj.SplitType)
        {
            case SplitableObject.SplitTypes.PreSpawnedSplitObject:
                obj.PreSpawnedObject = (GameObject)EditorGUILayout.ObjectField("Pre Spawned Object", obj.PreSpawnedObject, typeof(GameObject), true);
                break;
            case SplitableObject.SplitTypes.SpawnOnSplit:
                obj.SplitObjectPrefab = (GameObject)EditorGUILayout.ObjectField("Split Object Prefab", obj.SplitObjectPrefab, typeof(GameObject), true);
                obj.SpawnOffset = EditorGUILayout.Vector3Field("Spawn Offset", obj.SpawnOffset);
                break;
            case SplitableObject.SplitTypes.SpawnNewOnSplit:
                obj.FirstSplitObject = (GameObject)EditorGUILayout.ObjectField("First Split Object", obj.FirstSplitObject, typeof(GameObject), true);
                obj.SecondSplitObject = (GameObject)EditorGUILayout.ObjectField("Second Split Object", obj.SecondSplitObject, typeof(GameObject), true);
                obj.FirstSpawnOffset = EditorGUILayout.Vector3Field("First Offset", obj.FirstSpawnOffset);
                obj.SecondSpawnOffset = EditorGUILayout.Vector3Field("Second Offset", obj.SecondSpawnOffset);
                break;
        }

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }
}
