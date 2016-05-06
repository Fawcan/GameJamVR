using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(VerletUpdater))]
public class VerletUpdaterEditor : Editor
{
    public static void DisplayUpdateOrder(VerletUpdater obj)
    {
        if (Application.isPlaying == false)
            return;

        if (obj.RootNodes.Count > 0)
        {
            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
            EditorGUILayout.LabelField("Root Nodes");
            EditorGUI.indentLevel++;
            foreach (var root in obj.RootNodes)
                EditorGUILayout.LabelField(root.name);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        foreach (var updateData in obj.SimulationSettings.LayerSettings)
        {
            if (updateData.Iterations > 0)
            {
                EditorGUILayout.LabelField(updateData.Iterations + "x Iterations");
                if (updateData.Reversed)
                {
                    if (updateData.ConstraintsFirst)
                    {
                        List<DistanceConstraint> contrList = obj.ConstrUpdates[updateData.Name];
                        if (contrList.Count > 0)
                        {
                            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                            EditorGUILayout.LabelField("Constraint");
                            EditorGUI.indentLevel++;
                            for (int j = contrList.Count - 1; j >= 0; j--)
                                EditorGUILayout.LabelField(contrList[j].name);

                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                        }
                    }

                    List<VerletNode> nodeList = obj.NodeUpdates[updateData.Name];
                    if (nodeList.Count > 0)
                    {
                        GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                        EditorGUILayout.LabelField("Nodes");
                        EditorGUI.indentLevel++;
                        for (int j = nodeList.Count - 1; j >= 0; j--)
                            EditorGUILayout.LabelField(nodeList[j].name);

                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                    }

                    if (updateData.ConstraintsFirst == false)
                    {
                        List<DistanceConstraint> contrList = obj.ConstrUpdates[updateData.Name];
                        if (contrList.Count > 0)
                        {
                            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                            EditorGUILayout.LabelField("Constraint");
                            EditorGUI.indentLevel++;
                            for (int j = contrList.Count - 1; j >= 0; j--)
                                EditorGUILayout.LabelField(contrList[j].name);

                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                else
                {
                    if (updateData.ConstraintsFirst)
                    {
                        if (obj.ConstrUpdates[updateData.Name].Count > 0)
                        {
                            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                            EditorGUILayout.LabelField("Constraint");
                            EditorGUI.indentLevel++;
                            foreach (var constr in obj.ConstrUpdates[updateData.Name])
                                EditorGUILayout.LabelField(constr.name);

                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                        }
                    }

                    if (obj.NodeUpdates[updateData.Name].Count > 0)
                    {
                        GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                        EditorGUILayout.LabelField("Nodes");
                        EditorGUI.indentLevel++;
                        foreach (var node in obj.NodeUpdates[updateData.Name])
                            EditorGUILayout.LabelField(node.name);

                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                    }

                    if (updateData.ConstraintsFirst == false)
                    {
                        if (obj.ConstrUpdates[updateData.Name].Count > 0)
                        {
                            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                            EditorGUILayout.LabelField("Constraint");
                            EditorGUI.indentLevel++;
                            foreach (var constr in obj.ConstrUpdates[updateData.Name])
                                EditorGUILayout.LabelField(constr.name);

                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
            }
        }
    }
}
