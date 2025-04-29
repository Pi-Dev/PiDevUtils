using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
 * A reusable generic EditorWindow for displaying and editing serialized collections in table format.
 * Automatically generates columns from property fields and supports sorting, inline editing, and row operations.
 * Includes drag-and-drop reordering, context menu duplication/removal, and toolbar interaction.
 *
 * ============= Usage =============
 * Inherit from CollectionTable<T> and call SetData(targetObject, "propertyPath") to bind a serialized array.
 * CustomizeColumns() can be overridden to add or modify table columns manually.
 * Use GetWindow<YourDerivedCollectionTableThing>() to create and display the window.
 */

namespace PiDev.Utilities.Editor
{
    public abstract class CollectionTable<T> : EditorWindow
    {
        [SerializeField] private UnityEngine.Object _boundTarget;
        [SerializeField] private string _boundPropertyPath;

        private SerializedObject obj;
        private SerializedProperty prop;
        [NonSerialized] public bool ShowIndexOneBased = false;

        private TableView<SerializedProperty> _tableView = new();

        private int nextId = 0;

        protected virtual void OnEnable()
        {
            if (_boundTarget != null && !string.IsNullOrEmpty(_boundPropertyPath))
            {
                Rebind();
            }
        }

        public void SetData(UnityEngine.Object target, string propertyPath)
        {
            _boundTarget = target;
            _boundPropertyPath = propertyPath;
            Rebind();
        }

        private void Rebind()
        {
            obj = new SerializedObject(_boundTarget);
            prop = obj.FindProperty(_boundPropertyPath);

            _tableView.AllowRowReordering = true;
            _tableView.OnReorder = (from, to) =>
            {
                prop.MoveArrayElement(from, to);
                obj.ApplyModifiedProperties();
                Repaint();
            };

            if (prop == null || !prop.isArray)
            {
                Debug.LogError($"Property '{_boundPropertyPath}' is invalid or not an array.");
                return;
            }

            RebuildTable();
            Repaint();
        }

        private void RebuildTable()
        {
            _tableView.ClearColumns();

            if (prop == null || prop.arraySize == 0)
                return;

            _tableView.AddColumn("#", 30, (rect, item) =>
            {
                int row = nextId++;
                GUI.Label(rect, "≡" + GetIndexSuffix(item.propertyPath, ShowIndexOneBased), EditorStyles.miniLabel);

                if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
                {
                    var menu = new GenericMenu();
                    int index = GetRowIndex(item);

                    menu.AddItem(new GUIContent("Duplicate"), false, () =>
                    {
                        prop.InsertArrayElementAtIndex(index);
                        obj.ApplyModifiedProperties();
                        Repaint();
                    });

                    menu.AddItem(new GUIContent("Delete"), false, () =>
                    {
                        prop.DeleteArrayElementAtIndex(index);
                        obj.ApplyModifiedProperties();
                        Repaint();
                    });

                    menu.ShowAsContext();
                    Event.current.Use();
                }

            }).SetMaxWidth(30).column.width = 30;

            var element = prop.GetArrayElementAtIndex(0);

            if (!element.hasVisibleChildren)
                return;

            var iterator = element.Copy();
            if (!iterator.NextVisible(true))
                return;

            int baseDepth = iterator.depth;
            do
            {
                if (iterator.depth != baseDepth)
                    continue;

                string fieldName = iterator.name;
                string baseLabel = ObjectNames.NicifyVariableName(fieldName);
                List<(string, Comparison<SerializedProperty>)> sortModes = null;

                switch (iterator.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        sortModes = new()
                        {
                            ("▲", (a, b) => a.FindPropertyRelative(fieldName).intValue
                                             .CompareTo(b.FindPropertyRelative(fieldName).intValue)),
                            ("▼", (a, b) => b.FindPropertyRelative(fieldName).intValue
                                             .CompareTo(a.FindPropertyRelative(fieldName).intValue)),
                            ("", null)
                        };
                        break;

                    case SerializedPropertyType.Float:
                        sortModes = new()
                        {
                            ("▲", (a, b) => a.FindPropertyRelative(fieldName).floatValue
                                             .CompareTo(b.FindPropertyRelative(fieldName).floatValue)),
                            ("▼", (a, b) => b.FindPropertyRelative(fieldName).floatValue
                                             .CompareTo(a.FindPropertyRelative(fieldName).floatValue)),
                            ("", null)
                        };
                        break;

                    case SerializedPropertyType.String:
                        sortModes = new()
                        {
                            ("▲", (a, b) => string.Compare(
                                a.FindPropertyRelative(fieldName).stringValue,
                                b.FindPropertyRelative(fieldName).stringValue, StringComparison.Ordinal)),
                            ("▼", (a, b) => string.Compare(
                                b.FindPropertyRelative(fieldName).stringValue,
                                a.FindPropertyRelative(fieldName).stringValue, StringComparison.Ordinal)),
                            ("", null)
                        };
                        break;

                    case SerializedPropertyType.ObjectReference:
                        sortModes = new()
                        {
                            ("▲", (a, b) => string.Compare(
                                a.FindPropertyRelative(fieldName).objectReferenceValue?.name,
                                b.FindPropertyRelative(fieldName).objectReferenceValue?.name, StringComparison.Ordinal)),
                            ("▼", (a, b) => string.Compare(
                                b.FindPropertyRelative(fieldName).objectReferenceValue?.name,
                                a.FindPropertyRelative(fieldName).objectReferenceValue?.name, StringComparison.Ordinal)),
                            ("", null)
                        };
                        break;
                }

                _tableView.AddColumn(baseLabel, 150, (rect, item) =>
                {
                    var field = item.FindPropertyRelative(fieldName);
                    if (field != null)
                        EditorGUI.PropertyField(rect, field, GUIContent.none, true);
                }, allowToggleVisibility: true, sortModes: sortModes);


            } while (iterator.NextVisible(false));

            CustomizeColumns();

            _tableView.AddColumn("Edit", 40, (rect, item) =>
            {
                float buttonWidth = rect.width / 2f;
                Rect dupRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
                Rect delRect = new Rect(rect.x + buttonWidth, rect.y, buttonWidth, rect.height);

                int index = GetRowIndex(item);

                if (GUI.Button(dupRect, "➕", GUI.skin.label))
                {
                    prop.InsertArrayElementAtIndex(index);
                    obj.ApplyModifiedProperties();
                    try { Repaint(); } catch { }
                    return;
                }

                if (GUI.Button(delRect, "❌", GUI.skin.label))
                {
                    prop.DeleteArrayElementAtIndex(index);
                    obj.ApplyModifiedProperties();
                    try { Repaint(); } catch { }
                    return;
                }
            }).SetMaxWidth(40).column.width = 40;
        }

        protected virtual void CustomizeColumns() { }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Add Entry", EditorStyles.toolbarButton))
            {
                prop.arraySize++;
                obj.ApplyModifiedProperties();
                Repaint();
            }

            if (GUILayout.Button("Select Object", EditorStyles.toolbarButton))
            {
                if (_boundTarget != null)
                {
                    Selection.activeObject = _boundTarget;
                    EditorGUIUtility.PingObject(_boundTarget);
                }
                else
                {
                    Debug.LogWarning("No bound object to select.");
                }
            }

            if (GUILayout.Button("Select This", EditorStyles.toolbarButton))
            {
                Selection.activeObject = this;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            if (obj == null || prop == null)
            {
                EditorGUILayout.HelpBox("No collection is bound.", MessageType.Warning);
                return;
            }

            obj.Update();
            DrawToolbar();

            // Extract array elements as SerializedProperties
            var elements = new List<SerializedProperty>();
            for (int i = 0; i < prop.arraySize; i++)
            {
                elements.Add(prop.GetArrayElementAtIndex(i));
            }

            nextId = 0;
            try
            {
                _tableView.Render(elements.ToArray());
            }
            catch (Exception) { Repaint(); }

            obj.ApplyModifiedProperties();
        }

        public static string GetIndexSuffix(string propertyPath, bool Plus1)
        {
            int start = propertyPath.LastIndexOf('[');
            int end = propertyPath.LastIndexOf(']');

            if (start >= 0 && end > start)
            {
                string content = propertyPath.Substring(start + 1, end - start - 1);
                if (int.TryParse(content, out int index))
                {
                    if (Plus1) index++;
                    return $"[{index}]";
                }
            }

            return string.Empty;
        }


        private int GetRowIndex(SerializedProperty prop)
        {
            for (int i = 0; i < this.prop.arraySize; i++)
            {
                if (this.prop.GetArrayElementAtIndex(i).propertyPath == prop.propertyPath)
                    return i;
            }
            return -1;
        }
    }
}
