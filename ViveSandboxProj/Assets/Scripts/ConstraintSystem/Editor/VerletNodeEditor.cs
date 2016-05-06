using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(VerletNode))]
public class VerletNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = (VerletNode)target;

        DrawDefaultInspector();

        //obj.UpdateLayer = DoUpdateLayerNameGUI("Update Layer", obj.UpdateLayer);

        /*if (GUILayout.Button("Adjust Nodes By Constraints"))
            AdjustNodesByConstraints();
        if (GUILayout.Button("Adjust Constraints By Nodes"))
            AdjustConstraintsByNodes();*/
    }

    public static string DoUpdateLayerNameGUI(string label, string updateLayerName, VerletSimulation settings)
    {
        if (settings == null)
            return "";

        if (settings.LayerNames.Count > 0)
        {
            int selected = settings.LayerNames.IndexOf(updateLayerName);
            if (selected == -1)
                selected = 0;

            selected = EditorGUILayout.Popup(label, selected, settings.LayerNames.ToArray());
            return settings.LayerNames[selected];
        }
        return "";
    }

    public static void AdjustNodesByConstraints()
    {
        VerletNode[] nodes = Object.FindObjectsOfType<VerletNode>();
        DistanceConstraint[] constraints = Object.FindObjectsOfType<DistanceConstraint>();

        foreach (var node in nodes)
        {
            if (node.FixedPosition)
                continue;

            DistanceConstraint c1 = null;
            DistanceConstraint c2 = null;
            foreach (var distConst in constraints)
            {
                if (distConst.Node1 == node || distConst.Node2 == node)
                    c1 = distConst;
            }
            foreach (var distConst in constraints)
            {
                if (distConst == c1)
                    continue;
                if (distConst.Node1 == node || distConst.Node2 == node)
                    c2 = distConst;
            }
            if (c1 != null && c2 != null)
            {
                node.transform.position = (c1.transform.position + c2.transform.position) / 2;
            }
        }
    }

    public static void AdjustConstraintsByNodes()
    {
        DistanceConstraint[] constraints = Object.FindObjectsOfType<DistanceConstraint>();
        foreach (var distConst in constraints)
        {
            if (distConst.Node1 == null || distConst.Node2 == null)
                continue;
            Transform n1Trans = distConst.Node1.transform;
            Transform n2Trans = distConst.Node2.transform;
            distConst.transform.position = (n1Trans.position + n2Trans.position) / 2;
        }
    }
}
