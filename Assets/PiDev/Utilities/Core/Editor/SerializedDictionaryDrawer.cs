#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using static PiDev.Utils;
using System;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    float GetWidestLabelWidth(SerializedProperty prop)
    {
        if (prop == null) return 0;
        if (!prop.hasVisibleChildren)
            return EditorGUIUtility.labelWidth;

        float max = 0f;
        var p = prop.Copy();
        int depth = p.depth;
        if (!p.NextVisible(true)) return EditorGUIUtility.labelWidth;

        do
        {
            if (p.depth != depth + 1) break;
            float width = GUI.skin.label.CalcSize(new GUIContent(p.displayName)).x;
            if (width > max) max = width;
        }
        while (p.NextVisible(false));

        return max + 16f;

        //return Mathf.Clamp(max + 10f, 40f, 140f);
    }
    float GetPropertyHeight(SerializedProperty key, bool includeChildren)
    {
        if (key == null) return EditorGUIUtility.singleLineHeight;
        return EditorGUI.GetPropertyHeight(key, true);
    }

    private readonly Dictionary<string, ReorderableList> listCache = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty entries = property.FindPropertyRelative("entries");

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label, true);

        if (!property.isExpanded)
            return;

        position.y += EditorGUIUtility.singleLineHeight + 2f;

        if (!listCache.TryGetValue(property.propertyPath, out var list))
        {
            list = new ReorderableList(property.serializedObject, entries, true, false, true, true);

            list.elementHeightCallback = index =>
            {
                var entry = entries.GetArrayElementAtIndex(index);
                var key = entry.FindPropertyRelative("Key");
                var val = entry.FindPropertyRelative("Value");
                float keyHeight = GetPropertyHeight(key, true);
                float valHeight = GetPropertyHeight(val, true);
                return Mathf.Max(keyHeight, valHeight) + 6f;
            };

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var entry = entries.GetArrayElementAtIndex(index);
                var key = entry.FindPropertyRelative("Key");
                var val = entry.FindPropertyRelative("Value");

                float keyHeight = GetPropertyHeight(key, true);
                float valHeight = GetPropertyHeight(val, true);
                float maxHeight = Mathf.Max(keyHeight, valHeight);

                float half = (rect.width - 10f) * 0.5f;
                Rect keyRect = new(rect.x, rect.y + 2f, half, keyHeight);
                Rect valRect = new(rect.x + half + 10f, rect.y + 2f, half, valHeight);

                EditorGUIUtility.labelWidth = GetWidestLabelWidth(key);
                if (key != null)
                    EditorGUI.PropertyField(keyRect, key, GUIContent.none, true);
                else EditorGUI.LabelField(valRect, "Null Key");
                EditorGUIUtility.labelWidth = GetWidestLabelWidth(val);
                if (val != null) EditorGUI.PropertyField(valRect, val, GUIContent.none, true);
                else EditorGUI.LabelField(valRect, "Null Value");

                if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Delete"), false, () =>
                    {
                        entries.DeleteArrayElementAtIndex(index);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            };

            list.onAddCallback = l =>
            {
                entries.InsertArrayElementAtIndex(entries.arraySize);
                var newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                var keyProp = newEntry.FindPropertyRelative("Key");
                switch (keyProp.propertyType)
                {
                    case SerializedPropertyType.String:
                        keyProp.stringValue = string.Empty;
                        break;
                    case SerializedPropertyType.Enum:
                        keyProp.intValue = 0;
                        break;
                    case SerializedPropertyType.Integer:
                        keyProp.intValue = 0;
                        break;
                    case SerializedPropertyType.Float:
                        keyProp.floatValue = 0f;
                        break;
                    case SerializedPropertyType.Boolean:
                        keyProp.boolValue = false;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        keyProp.objectReferenceValue = null;
                        break;
                    default:
                        // Unsupported type, just try zeroing the key property itself
                        if (keyProp.hasChildren)
                        {
                            var child = keyProp.Copy();
                            int depth = child.depth;
                            while (child.NextVisible(true) && child.depth > depth)
                            {
                                if (child.propertyType == SerializedPropertyType.String)
                                    child.stringValue = string.Empty;
                                else if (child.propertyType == SerializedPropertyType.Integer)
                                    child.intValue = 0;
                                else if (child.propertyType == SerializedPropertyType.Float)
                                    child.floatValue = 0f;
                                else if (child.propertyType == SerializedPropertyType.Boolean)
                                    child.boolValue = false;
                                else if (child.propertyType == SerializedPropertyType.ObjectReference)
                                    child.objectReferenceValue = null;
                            }
                        }
                        break;
                }
                // Clear value
                var valProp = newEntry.FindPropertyRelative("Value");
                if (valProp != null)
                    if (valProp.propertyType == SerializedPropertyType.ObjectReference)
                        valProp.objectReferenceValue = null;
                    else if (valProp.propertyType == SerializedPropertyType.String)
                        valProp.stringValue = string.Empty;
                    else if (valProp.propertyType == SerializedPropertyType.Integer)
                        valProp.intValue = 0;
                    else if (valProp.propertyType == SerializedPropertyType.Float)
                        valProp.floatValue = 0f;
                    else if (valProp.propertyType == SerializedPropertyType.Boolean)
                        valProp.boolValue = false;
            };

            listCache[property.propertyPath] = list;
        }

        list.DoList(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        try
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            if (!listCache.TryGetValue(property.propertyPath, out var list))
                return EditorGUIUtility.singleLineHeight * 2f;

            var entries = property.FindPropertyRelative("entries");
            float total = EditorGUIUtility.singleLineHeight + 6f;

            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var key = entry.FindPropertyRelative("Key");
                var val = entry.FindPropertyRelative("Value");
                float keyHeight = GetPropertyHeight(key, true);
                float valHeight = GetPropertyHeight(val, true);
                total += Mathf.Max(keyHeight, valHeight) + 4f;
            }
            total += EditorGUIUtility.singleLineHeight + 4f;

            return total;
        }
        catch(Exception ex)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif
