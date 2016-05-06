using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityContentEditor;

[CustomEditor(typeof(VerletSimulation))]
public class VerletSimulationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = (VerletSimulation)target;

        GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
        obj.UpdatesPerSecond = EditorGUILayout.IntField("Updates Per Second", obj.UpdatesPerSecond);
        EditorGUILayout.LabelField("Only used when VSync is off");
        EditorGUILayout.EndVertical();

        GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
        EditorGUILayout.LabelField("Update Layers");
        EditorGUI.indentLevel++;
        UnityExtensionsEditor.DoListGUI(obj.LayerNames, () => { return ""; },
            (element, index) =>
        {
            string str = EditorGUILayout.TextField("Layer name", element);
            return str;
        });
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("");
        EditorGUILayout.EndVertical();

        int moveUpIndex = -1;
        int moveDownIndex = -1;
        GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
        EditorGUILayout.LabelField("Layer Settings");
        EditorGUI.indentLevel++;
        UnityExtensionsEditor.DoStructListGUI(obj.LayerSettings, () => new VerletSimulation.LayerData() { Iterations = 1 },
            (element, index) =>
        {
            if (obj.LayerNames.Count > 0)
            {
                int nameIndex = obj.LayerNames.IndexOf(element.Name);
                if (nameIndex == -1)
                {
                    nameIndex = 0;
                    element.Name = obj.LayerNames[nameIndex];
                }
                nameIndex = EditorGUILayout.Popup(nameIndex, obj.LayerNames.ToArray(), GUILayout.Width(100));
                element.Name = obj.LayerNames[nameIndex];
            }
            element.Iterations = EditorGUILayout.IntField(element.Iterations, GUILayout.Width(40));

            if (element.ConstraintsFirst)
            {
                if (GUILayout.Button("Contrs First", GUILayout.Width(80)))
                    element.ConstraintsFirst = element.ConstraintsFirst == false;
            }
            else
            {
                if (GUILayout.Button("Nodes First", GUILayout.Width(80)))
                    element.ConstraintsFirst = element.ConstraintsFirst == false;
            }

            if (element.Reversed)
            {
                if (GUILayout.Button("Reversed", GUILayout.Width(80)))
                    element.Reversed = element.Reversed == false;
            }
            else
            {
                if (GUILayout.Button("In Order", GUILayout.Width(80)))
                    element.Reversed = element.Reversed == false;
            }

            if (GUILayout.Button("Up", GUILayout.Width(30)))
                moveUpIndex = index;
            if (GUILayout.Button("Down", GUILayout.Width(50)))
                moveDownIndex = index;

            return element;
        });
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("");
        EditorGUILayout.EndVertical();

        if (moveUpIndex != -1 && moveUpIndex != 0)
        {
            VerletSimulation.LayerData data = obj.LayerSettings[moveUpIndex];
            obj.LayerSettings.RemoveAt(moveUpIndex);
            obj.LayerSettings.Insert(moveUpIndex - 1, data);
        }
        if (moveDownIndex != -1 && (moveDownIndex + 1) < obj.LayerSettings.Count)
        {
            VerletSimulation.LayerData data = obj.LayerSettings[moveDownIndex];
            obj.LayerSettings.RemoveAt(moveDownIndex);
            obj.LayerSettings.Insert(moveDownIndex + 1, data);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(obj);
    }
}
