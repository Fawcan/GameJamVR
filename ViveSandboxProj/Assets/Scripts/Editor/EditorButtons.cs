using UnityEngine;
using UnityEditor;
using System.IO;

public class EditorButtons : MonoBehaviour
{
    [MenuItem("GameObject/Create Settings Objects/Verlet Sim settings")]
    static void CreateGrabSettings()
    {
        CreateAsset<VerletSimulation>("New Verlet Sim Setting");
    }

    public static void CreateAsset<T>(string name) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + name + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
