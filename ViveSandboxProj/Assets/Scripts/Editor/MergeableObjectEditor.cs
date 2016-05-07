using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MergeableObject))]
public class MergeableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var obj = (MergeableObject)target;
        switch (obj.MergableObjectType)
        {
            case MergeableObject.MergableObjectTypes.SpecTag:
                obj.MergeTag = EditorGUILayout.TextField("Merge Tag", obj.MergeTag);
                break;
            case MergeableObject.MergableObjectTypes.SpecObject:
                obj.MergeObject = (Rigidbody)EditorGUILayout.ObjectField("Merge Object", obj.MergeObject, typeof(Rigidbody), true);
                break;
        }

        if (obj.UseMergeRail)
        {
            obj.MergeRailStart = (Transform)EditorGUILayout.ObjectField("Merge Rail Start", obj.MergeRailStart, typeof(Transform), true);
            obj.MergeTarget = (Transform)EditorGUILayout.ObjectField("Merge Rail End", obj.MergeTarget, typeof(Transform), true);
            obj.MergeRailThresholdDistance = EditorGUILayout.FloatField("Merge Rail Threshold Distance", obj.MergeRailThresholdDistance);
            obj.MergeRailThresholdAngle = EditorGUILayout.FloatField("Merge Rail Threshold Angle", obj.MergeRailThresholdAngle);
        }
        else
        {
            obj.MergeTarget = (Transform)EditorGUILayout.ObjectField("Merge Target", obj.MergeTarget, typeof(Transform), true);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }
}
