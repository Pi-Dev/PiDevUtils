#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

/* Licensed under MIT
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Implements an auto-complete text field in the Unity Editor - the popup part.
 */

namespace PiDev.Utilities.AutoCompletion
{
    public class AutoCompletePopup : EditorWindow
    {
        private SerializedProperty property;
        private AutoComplete provider;
        private string search = "";
        private Vector2 scroll;
        private int selectedIndex = -1;
        private bool initialized = false;

        public static void Show(Rect rect, SerializedProperty prop, AutoComplete provider)
        {
            var win = CreateInstance<AutoCompletePopup>();
            win.property = prop;
            win.provider = provider;
            win.search = prop != null ? prop.stringValue : "";
            win.selectedIndex = -1;
            win.initialized = false;

            int maxVisible = 8;
            int count = provider.GetItems(win.search).Count(); 
            int visibleCount = Mathf.Min(count, maxVisible) + 1; // +1 search bar
            float height = visibleCount * EditorGUIUtility.singleLineHeight;
            win.FixSize(count);
            win.ShowAsDropDown(rect, new Vector2(rect.width, height));
        }

        void DrawBackground()
        {
            float expand = 2f; // how many pixels to expand outside the window

            // Expanded rect
            Rect bgRect = new Rect(
                -expand,
                -expand,
                position.width + expand * 2f,
                position.height + expand * 2f
            );

            // Draw background with TextArea skin
            GUI.Box(bgRect, GUIContent.none, GUI.skin.textArea);
        }
        private void OnGUI()
        {
            DrawBackground();

            // Raw text field so arrow keys are not swallowed
            Rect searchRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
            GUI.SetNextControlName("AutoCompleteSearch");

            string[] matches = provider.GetItems(search).ToArray();
            FixSize(matches.Length);

            HandleKeyboard(matches); // MUST be before the call to GUI.TextField for kb nav
            string newSearch = GUI.TextField(searchRect, search);

            // Apply any edits immediately to the property
            if (newSearch != search)
            {
                search = newSearch;
                if (property != null)
                {
                    property.stringValue = search;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            if (!initialized)
            {
                EditorGUI.FocusTextInControl("AutoCompleteSearch");
                initialized = true;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < matches.Length; i++)
            {
                Rect row = GUILayoutUtility.GetRect(new GUIContent(matches[i]), EditorStyles.label, GUILayout.ExpandWidth(true));

                bool isHover = row.Contains(Event.current.mousePosition);
                bool isSelected = (i == selectedIndex);

                // Highlight: selected first, else hover
                if (isSelected)
                {
                    EditorGUI.DrawRect(row, new Color(0.24f, 0.48f, 0.90f, 0.3f)); // bluish selection
                }
                else if (isHover)
                {
                    EditorGUI.DrawRect(row, new Color(0.24f, 0.48f, 0.90f, 0.15f)); // lighter hover
                }

                // Handle mouse click
                if (Event.current.type == EventType.MouseDown && isHover)
                {
                    ApplySelection(matches[i]);
                    Event.current.Use();
                }

                // Draw label text
                GUI.Label(row, matches[i], EditorStyles.label);
            }
            EditorGUILayout.EndScrollView();
        }

        private void FixSize(int count)
        {
            int maxVisible = 8;
            int visibleCount = Mathf.Min(count, maxVisible) + 1; // +1 search bar
            float height = visibleCount * EditorGUIUtility.singleLineHeight + 0;

            // adjust window size dynamically
            minSize = new Vector2(position.width, height);
            maxSize = new Vector2(position.width, height);
        }

        private void HandleKeyboard(string[] matches)
        {
            Event e = Event.current;
            if (e.rawType != EventType.KeyDown || matches.Length == 0)
                return;

            if (GUI.GetNameOfFocusedControl() == "AutoCompleteSearch")
            {
                if (e.keyCode == KeyCode.DownArrow)
                {
                    selectedIndex = Mathf.Min(selectedIndex + 1, matches.Length - 1);
                    e.Use();
                }
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    selectedIndex = Mathf.Max(selectedIndex - 1, 0);
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    if (selectedIndex >= 0 && selectedIndex < matches.Length)
                        ApplySelection(matches[selectedIndex]);
                    else if (!string.IsNullOrEmpty(search))
                        ApplySelection(search);
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    Close();
                    e.Use();
                }
            }
        }

        private void ApplySelection(string value)
        {
            if (property != null)
            {
                property.stringValue = value;
                property.serializedObject.ApplyModifiedProperties();
            }
            Close();
        }
    }
}
#endif
