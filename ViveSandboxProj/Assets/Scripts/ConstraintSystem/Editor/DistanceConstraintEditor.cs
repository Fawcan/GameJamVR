using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DistanceConstraint))]
public class DistanceConstraintEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = (DistanceConstraint)target;

        DrawDefaultInspector();

        //obj.UpdateLayer = VerletNodeEditor.DoUpdateLayerNameGUI("Update Layer", obj.UpdateLayer);

        /*if (GUILayout.Button("Adjust Nodes By Constraints"))
            VerletNodeEditor.AdjustNodesByConstraints();
        if (GUILayout.Button("Adjust Constraints By Nodes"))
            VerletNodeEditor.AdjustConstraintsByNodes();*/
    }
}
