using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * A lightweight MonoBehaviour that lets you attach comments or notes directly to GameObjects in the Inspector.
 * Displays a centered, styled text area for easy annotation of scene objects during development.
 */

public class CommentComponent : MonoBehaviour
{

    [Multiline]
    public string comment = "Enter note here";
}

#if UNITY_EDITOR
[CustomEditor(typeof(CommentComponent))]
[CanEditMultipleObjects]
public class CommentComponentEditor : Editor
{
    GUIStyle style = null;

    public override void OnInspectorGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.textArea);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 12;
            style.stretchWidth = true;
            style.padding = new RectOffset(0, 0, 6, 6);
        }

        var c = target as CommentComponent;

        GUI.color = new Color(1, 1, 0.6f);
        c.comment = EditorGUILayout.TextArea(c.comment, style);
    }
}
#endif
