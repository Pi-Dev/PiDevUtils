#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

/* Licensed under MIT
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Implements an auto-complete text field in the Unity Editor - the property drawer part.
 */

namespace PiDev.Utilities.AutoCompletion
{
    [CustomPropertyDrawer(typeof(AutoComplete), true)]
    public class AutoCompleteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName("CheckFocus");
            EditorGUI.TextField(fieldRect, property.stringValue);

            if (GUI.GetNameOfFocusedControl() == "CheckFocus")
            {
                GUI.FocusControl(null);

                var attr = (AutoComplete)attribute;
                Rect screenRect = GUIUtility.GUIToScreenRect(fieldRect);

                float expand = 2f;
                screenRect.x -= expand;
                screenRect.y -= expand + EditorGUIUtility.singleLineHeight + 1;
                screenRect.width += expand * 2f;
                screenRect.height += expand * 2f;

                AutoCompletePopup.Show(screenRect, property, attr);
            }
        }
    }
}
#endif
