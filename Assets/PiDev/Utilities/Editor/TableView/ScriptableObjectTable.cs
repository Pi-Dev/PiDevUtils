using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * A generic EditorWindow for managing and displaying ScriptableObject assets using a dynamic table view.
 * Automatically detects fields, generates sortable columns, and supports inline property editing.
 * Designed for rapid editor tooling development with auto-refresh and custom column extensions.
 *
 * ============= Usage =============
 * Inherit from ScriptableObjectTable<MyType> and optionally override CustomizeColumns() for extra config.
 * Then GetWindow<MyType>() to open the window.
 * Use the built-in Create button to add new assets or reload from the asset database.
 */

#if UNITY_EDITOR
namespace PiDev.Utilities.Editor
{
    public abstract class ScriptableObjectTable<T> : EditorWindow where T : ScriptableObject
    {
        private List<T> _data;
        private Dictionary<T, SerializedObject> _serializedCache = new();
        private TableView<T> _tableView = new();
        private float _rowHeight = EditorGUIUtility.singleLineHeight + 4;

        protected virtual void OnEnable()
        {
            ReloadData();
        }

        private void ReloadData()
        {
            _data = new List<T>();
            _serializedCache.Clear();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    _data.Add(asset);
            }

            BuildTable();
        }

        private void BuildTable()
        {
            _tableView.ClearColumns();

            if (_data.Count == 0)
                return;

            try
            {
                // Column: Selectable object label
                _tableView.AddColumn("Asset", 160, (rect, item) =>
                {
                    if (item != null && GUI.Button(rect, item.name, EditorStyles.linkLabel))
                    {
                        Selection.activeObject = item;
                        EditorGUIUtility.PingObject(item);
                    }
                },
                allowToggleVisibility: false,
                sortModes: new List<(string, Comparison<T>)>
                {
                    ("▲", (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal)),
                    ("▼", (a, b) => string.Compare(b.name, a.name, StringComparison.Ordinal)),
                    ("Created", (a, b) =>
                    {
                        var aPath = AssetDatabase.GetAssetPath(a);
                        var bPath = AssetDatabase.GetAssetPath(b);
                        return File.GetCreationTime(aPath).CompareTo(File.GetCreationTime(bPath));
                    }),
                    ("", null)
                });

                var validSample = _data.FirstOrDefault(d => d != null);
                if (validSample == null) return;

                SerializedObject sample = new SerializedObject(validSample);
                SerializedProperty iterator = sample.GetIterator();
                iterator.NextVisible(true); // Skip m_Script

                while (iterator.NextVisible(false))
                {
                    string propName = iterator.name;
                    string label = ObjectNames.NicifyVariableName(propName);

                    List<(string, Comparison<T>)> sortModes = new();

                    SerializedPropertyType type = iterator.propertyType;

                    switch (type)
                    {
                        case SerializedPropertyType.Integer:
                            sortModes.Add(("▲", (a, b) =>
                                new SerializedObject(a).FindProperty(propName).intValue
                                .CompareTo(new SerializedObject(b).FindProperty(propName).intValue)));
                            sortModes.Add(("▼", (a, b) =>
                                new SerializedObject(b).FindProperty(propName).intValue
                                .CompareTo(new SerializedObject(a).FindProperty(propName).intValue)));
                            break;

                        case SerializedPropertyType.Float:
                            sortModes.Add(("▲", (a, b) =>
                                new SerializedObject(a).FindProperty(propName).floatValue
                                .CompareTo(new SerializedObject(b).FindProperty(propName).floatValue)));
                            sortModes.Add(("▼", (a, b) =>
                                new SerializedObject(b).FindProperty(propName).floatValue
                                .CompareTo(new SerializedObject(a).FindProperty(propName).floatValue)));
                            break;

                        case SerializedPropertyType.String:
                            sortModes.Add(("▲", (a, b) =>
                                string.Compare(
                                    new SerializedObject(a).FindProperty(propName).stringValue,
                                    new SerializedObject(b).FindProperty(propName).stringValue,
                                    StringComparison.Ordinal)));
                            sortModes.Add(("▼", (a, b) =>
                                string.Compare(
                                    new SerializedObject(b).FindProperty(propName).stringValue,
                                    new SerializedObject(a).FindProperty(propName).stringValue,
                                    StringComparison.Ordinal)));
                            break;

                        case SerializedPropertyType.ObjectReference:
                            sortModes.Add(("▲", (a, b) =>
                            {
                                var aRef = new SerializedObject(a).FindProperty(propName).objectReferenceValue;
                                var bRef = new SerializedObject(b).FindProperty(propName).objectReferenceValue;
                                return string.Compare(aRef?.name, bRef?.name, StringComparison.Ordinal);
                            }
                            ));
                            sortModes.Add(("▼", (a, b) =>
                            {
                                var aRef = new SerializedObject(a).FindProperty(propName).objectReferenceValue;
                                var bRef = new SerializedObject(b).FindProperty(propName).objectReferenceValue;
                                return string.Compare(bRef?.name, aRef?.name, StringComparison.Ordinal);
                            }
                            ));
                            break;
                    }

                    sortModes.Add(("", null)); // Always allow a no-sort mode

                    _tableView.AddColumn(label, 150, (rect, item) =>
                    {
                        if (item == null) return;

                        if (!_serializedCache.TryGetValue(item, out var so))
                        {
                            so = new SerializedObject(item);
                            _serializedCache[item] = so;
                        }

                        var prop = so.FindProperty(propName);
                        if (prop != null)
                        {
                            EditorGUI.PropertyField(rect, prop, GUIContent.none);
                            so.ApplyModifiedProperties();
                        }
                    }, allowToggleVisibility: true/*, sortModes: sortModes*/);
                }

                CustomizeColumns();
            }
            catch
            {
                ReloadData();
            }
        }

        protected virtual void CustomizeColumns() { }

        protected virtual void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Create", EditorStyles.toolbarButton))
            {
                string path = GetDefaultFolder();

                if (string.IsNullOrEmpty(path))
                {
                    path = EditorUtility.SaveFilePanelInProject(
                        $"Save {typeof(T).Name}",
                        $"New {typeof(T).Name}",
                        "asset",
                        "Create a new asset"
                    );
                }
                else
                {
                    path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, $"New {typeof(T).Name}.asset"));
                }

                if (!string.IsNullOrEmpty(path))
                {
                    T asset = ScriptableObject.CreateInstance<T>();
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    ReloadData();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private string GetDefaultFolder()
        {
            if (_data == null || _data.Count == 0)
                return null;

            var paths = _data
                .Where(d => d != null)
                .Select(obj => AssetDatabase.GetAssetPath(obj))
                .ToList();

            var common = Path.GetDirectoryName(paths[0]);
            foreach (var p in paths)
            {
                if (Path.GetDirectoryName(p) != common)
                    return null;
            }

            return common;
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_data != null && _data.Count > 0)
            {
                try
                {
                    _tableView.Render(_data.Where(d=>d!=null).ToArray());
                }
                catch
                {
                    ReloadData();
                    Repaint();
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"No {typeof(T).Name} assets found.", MessageType.Info);
            }
        }
    }
}
#endif