using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UnityContentEditor
{
    public class UnityExtensionsEditor
    {
        public static void CreateAsset<T>(string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static List<string> _Layers;
        public static List<int> _LayerNumbers;
        public static string[] _LayerNames;
        public static long _LastUpdateTick;

        public static LayerMask LayerMaskField<T>(string label, LayerMask selected, bool showSpecial)
        {
            //Unity 3.5 and up

            if (_Layers == null || (System.DateTime.Now.Ticks - _LastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
            {
                _LastUpdateTick = System.DateTime.Now.Ticks;
                if (_Layers == null)
                {
                    _Layers = new List<string>();
                    _LayerNumbers = new List<int>();
                    _LayerNames = new string[4];
                }
                else
                {
                    _Layers.Clear();
                    _LayerNumbers.Clear();
                }

                int emptyLayers = 0;
                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);

                    if (layerName != "")
                    {

                        for (; emptyLayers > 0; emptyLayers--) _Layers.Add("Layer " + (i - emptyLayers));
                        _LayerNumbers.Add(i);
                        _Layers.Add(layerName);
                    }
                    else
                    {
                        emptyLayers++;
                    }
                }

                if (_LayerNames.Length != _Layers.Count)
                {
                    _LayerNames = new string[_Layers.Count];
                }
                for (int i = 0; i < _LayerNames.Length; i++) _LayerNames[i] = _Layers[i];
            }

            selected.value = EditorGUILayout.MaskField(label, selected.value, _LayerNames);

            return selected;
        }

        public static void DoListGUI<T>(List<T> list, System.Func<T> onCreate, System.Func<T, int, T> elemntGUI)
        {
            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
            int removeIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                GUI.Box(EditorGUILayout.BeginHorizontal(), new GUIContent());
                list[i] = elemntGUI(list[i], i);
                if (GUILayout.Button("-", GUILayout.Width(18)))
                    removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1)
                list.RemoveAt(removeIndex);

            if (GUILayout.Button("+", GUILayout.Width(25)))
                list.Add(onCreate());

            EditorGUILayout.EndVertical();
        }

        public static void DoStructListGUI<T>(List<T> list, System.Func<T> onCreate, System.Func<T, int, T> elemntGUI) where T : struct
        {
            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
            int removeIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                GUI.Box(EditorGUILayout.BeginHorizontal(), new GUIContent());
                list[i] = elemntGUI(list[i], i);
                if (GUILayout.Button("-", GUILayout.Width(18)))
                    removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1)
                list.RemoveAt(removeIndex);

            if (GUILayout.Button("+", GUILayout.Width(25)))
                list.Add(onCreate());

            EditorGUILayout.EndVertical();
        }

        public static void DoEnumDataList<E, D>(string name, List<D> list, System.Func<D, E> getEnum, System.Func<D, E, D> setEnum, System.Func<D, D> doDataGUI, Dictionary<string, bool> guiShowData = null)
            where E : struct, System.IConvertible
            where D : struct
        {
            var values = System.Enum.GetValues(typeof(E)).Cast<E>();
            int typeCount = values.Count();

            List<E> toRemove = new List<E>();
            toRemove.AddRange(list.Select(obj => getEnum(obj)));
            foreach (var item in values)
            {
                for (int i = 0; i < toRemove.Count(); i++)
                {
                    if (toRemove[i].Equals(item))
                    {
                        toRemove.RemoveAt(i);
                        break;
                    }
                }
            }

            var typesToRemove = values;

            for (int i = list.Count() - 1; i >= 0; i--)
            {
                if (toRemove.Contains(getEnum(list.ElementAt(i))))
                {
                    toRemove.Remove(getEnum(list.ElementAt(i)));
                    list.RemoveAt(i);
                }
            }

            List<int> existingTypes = new List<int>();
            if (list.Count() < typeCount)
            {
                var typesInList = list.Select((e) => { return getEnum(e); });
                foreach (var type in values)
                {
                    if (typesInList.Contains(type) == false)
                    {
                        var data = new D();
                        data = setEnum(data, type);
                        list.Add(data);
                    }
                }
            }

            bool show = false;
            if (guiShowData != null && guiShowData.TryGetValue(name, out show) == false)
                guiShowData[name] = show;

            GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
            if (guiShowData != null)
                guiShowData[name] = EditorGUILayout.Foldout(show, name);
            if (show || guiShowData == null)
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    GUI.Box(EditorGUILayout.BeginVertical(), new GUIContent());
                    string itemName = getEnum(list[i]).ToString();
                    bool showItem = false;
                    if (guiShowData != null && guiShowData.TryGetValue(name + itemName, out showItem) == false)
                        guiShowData[name + itemName] = showItem;
                    if (guiShowData != null)
                        guiShowData[name + itemName] = EditorGUILayout.Foldout(showItem, itemName);
                    if (showItem || guiShowData == null)
                    {
                        list[i] = doDataGUI(list[i]);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public static void DebugDrawCross(Vector3 position, float size, Color color)
        {
            Vector3 up = position + Vector3.up * size * 0.5f;
            Vector3 down = position - Vector3.up * size * 0.5f;
            Vector3 left = position - Vector3.right * size * 0.5f;
            Vector3 right = position + Vector3.right * size * 0.5f;
            Debug.DrawLine(up, down, color);
            Debug.DrawLine(left, right, color);
        }
    }
}