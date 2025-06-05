#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using PiDev.Utilities.Editor;
using PiDev;
using static PiDev.Utils;
using Codice.CM.WorkspaceServer.DataStore.WkTree;
using System;

[CustomPropertyDrawer(typeof(TableList<>), true)]
public class InspectorTable : PropertyDrawer
{
    private Dictionary<string, TableView<SerializedProperty>> tableViews = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return -8;
    }

    public int pendingDelRow = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //EditorGUI.DrawRect(position, new Color(1f, 0.1f, 0.1f, 0.5f));
        var itemsProp = property.FindPropertyRelative("items");
        if (itemsProp == null || !itemsProp.isArray)
        {
            EditorGUI.LabelField(position, label.text, "[TableList] requires a 'List<T>' named 'items'.");
            return;
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button(itemsProp.isExpanded ? "🔽" : "▶️", EditorStyles.toolbarButton)) itemsProp.isExpanded = !itemsProp.isExpanded;
                GUILayout.Label(label.text, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("🗑", EditorStyles.toolbarButton)) itemsProp.ClearArray();

                if (GUILayout.Button("➕", EditorStyles.toolbarButton))
                {
                    int index = itemsProp.arraySize;
                    itemsProp.InsertArrayElementAtIndex(index);
                    var newElement = itemsProp.GetArrayElementAtIndex(index);
                }
            }
            EditorGUILayout.EndHorizontal();

            var tableView = InitTableView(itemsProp);
            var count = itemsProp.arraySize;

            if (tableView != null && count > 0 && itemsProp.isExpanded)
            {

                if (pendingDelRow >= 0)
                {
                    itemsProp.DeleteArrayElementAtIndex(pendingDelRow);
                    pendingDelRow = -1;
                    count = itemsProp.arraySize;
                }
                var rows = new SerializedProperty[count];
                for (int i = 0; i < count; i++) rows[i] = itemsProp.GetArrayElementAtIndex(i);

                float height = itemsProp.arraySize * EditorGUIUtility.singleLineHeight + 32;
                tableView.Render(rows, position, height, inspectorMode: true, controlIdPrefix: property.propertyPath);
            }

        }
        EditorGUILayout.EndVertical();
    }

    private TableView<SerializedProperty> InitTableView(SerializedProperty itemsProp)
    {
        if (tableViews.TryGetValue(itemsProp.propertyPath, out var view)) return view;
        else
        {
            if (itemsProp.arraySize > 0)
            {
                var firstItem = itemsProp.GetArrayElementAtIndex(0);
                if (firstItem != null && firstItem.hasVisibleChildren)
                {
                    var tableView = new TableView<SerializedProperty>();
                    tableView.AllowRowReordering = true;
                    tableView.OnReorder = (from, to) =>
                    {
                        itemsProp.MoveArrayElement(from, to);
                        itemsProp.serializedObject.ApplyModifiedProperties();
                        try { GUIUtility.keyboardControl = 0; } catch { }
                    };
                    BuildColumns(tableView, itemsProp, itemsProp.serializedObject);
                    tableViews[itemsProp.propertyPath] = tableView;
                    return tableView;
                }
            }
            else return null; // no table view yet
        }
        return null;
    }

    private void BuildColumns(TableView<SerializedProperty> tableView, SerializedProperty prop, SerializedObject obj)
    {
        if (prop.arraySize == 0) return;

        var sample = prop.GetArrayElementAtIndex(0);
        if (!sample.hasVisibleChildren) return;

        var iterator = sample.Copy();
        if (!iterator.NextVisible(true)) return;

        int baseDepth = iterator.depth;
        do
        {
            if (iterator.depth != baseDepth) continue;

            string fieldName = iterator.name;
            string label = ObjectNames.NicifyVariableName(fieldName);
            var propType = iterator.propertyType;
            var sortModes = TableView<SerializedProperty>.CreateSortOptionsForField(fieldName, propType);

            tableView.AddColumn(label, 50, (rect, item) =>
            {
                var field = item.FindPropertyRelative(fieldName);
                if (field != null)
                {
                    EditorGUI.PropertyField(rect, field, GUIContent.none, true);
                }
            }, sortModes: sortModes);
        }
        while (iterator.NextVisible(false));

        tableView.AddColumn("Edit", 40, (rect, item) =>
        {
            float buttonWidth = rect.width / 2f;
            Rect dupRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            Rect delRect = new Rect(rect.x + buttonWidth, rect.y, buttonWidth, rect.height);

            int index = GetIndexSuffix(item.propertyPath);

            if (GUI.Button(dupRect, "➕", GUI.skin.label))
            {
                prop.InsertArrayElementAtIndex(index);
                obj.ApplyModifiedProperties();
                try { GUIUtility.keyboardControl = 0; } catch { }
                HandleUtility.Repaint();
                return;
            }

            if (GUI.Button(delRect, "❌", GUI.skin.label))
            {
                pendingDelRow = index;
                // prop.DeleteArrayElementAtIndex(index);
                // obj.ApplyModifiedProperties();
                // try { GUIUtility.keyboardControl = 0; } catch { }
                // HandleUtility.Repaint();
                // return;
            }
        }).SetMaxWidth(40).column.width = 40;
    }

    public static int GetIndexSuffix(string propertyPath)
    {
        int start = propertyPath.LastIndexOf('[');
        int end = propertyPath.LastIndexOf(']');

        if (start >= 0 && end > start)
        {
            string content = propertyPath.Substring(start + 1, end - start - 1);
            if (int.TryParse(content, out int index)) return index;
        }
        return -1;
    }
}

#endif
