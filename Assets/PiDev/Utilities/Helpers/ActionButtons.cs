using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
 * Utility class for managing a dynamic set of action buttons in the Unity Inspector.
 * Supports flexible runtime button generation and execution of assigned actions.
 * Includes a custom PropertyDrawer for automatic toolbar rendering of the button set.
 *
 * ============= Usage =============
 * Create an ActionButtons instance, add ActionButton entries, and display it as a toolbar in the inspector.
 * Clicking a button will automatically invoke its associated Action delegate.
 */

/* EXAMPLE 
   public class Chess3DTools : MonoBehaviour
   {
       private static int size = 8;
       private static float quadSize = 1f;
       public Material whiteMaterial, blackMaterial;

       [Header("Content tools")]
       public ActionButtons actions = new ActionButtons("",
           new("Generate Board", ()=>FindFirstObjectByType<Chess3DTools>().GenerateChessboard()),
           new("Capture pieces", CapturePreviews),
           new("Export AssetBundle",  () => Debug.Log("TODO!"))
       );

*/

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PiDev.Utilities
{

    [Serializable]
    public class ActionButtons
    {
        [Serializable]
        public class ActionButton
        {
            public string name;
            public Action action;
            public ActionButton(string name, Action action)
            {
                this.name = name;
                this.action = action;
            }
        }

        [NonSerialized] public bool useLabel;
        [NonSerialized] public string label;
        public void Add(ActionButton button) => buttons.Add(button);
        public void Add(string label, Action action) => buttons.Add(new(label, action));
        public ActionButtons() { }
        public ActionButtons(string label, params ActionButton[] buttons)
        {
            this.buttons = buttons.ToList();
            if (!string.IsNullOrEmpty(label))
            {
                useLabel = true;
                this.label = label;
            }
        }

        [NonSerialized] private List<ActionButton> buttons = new();

        public void SetButtons(params ActionButton[] newButtons)
        {
            buttons = (newButtons ?? Array.Empty<ActionButton>()).ToList();
        }

        public ActionButton[] GetButtons() => buttons.ToArray();
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActionButtons))]
    public class ActionButtonsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetObject = property.serializedObject.targetObject;
            var fieldInfo = targetObject.GetType().GetField(property.name);
            if (fieldInfo == null) return;

            var actionArray = (ActionButtons)fieldInfo.GetValue(targetObject);
            if (actionArray == null) return;

            var buttons = actionArray.GetButtons();
            if (buttons.Length == 0) return;

            EditorGUI.BeginProperty(position, label, property);

            if (actionArray.useLabel)
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(actionArray.label));

            // Create toolbar-style buttons
            string[] buttonNames = new string[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
            {
                buttonNames[i] = buttons[i].name;
            }

            int selected = -1;
            selected = GUI.Toolbar(position, selected, buttonNames);

            // Invoke the clicked action
            if (selected >= 0 && selected < buttons.Length)
            {
                buttons[selected].action?.Invoke();
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}