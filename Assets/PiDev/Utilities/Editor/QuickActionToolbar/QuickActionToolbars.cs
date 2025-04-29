#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

/* Licensed under MIT
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Allows adding custom action buttons to the left and right of Unity's Play, Pause, and Stop buttons.
 * Useful for quickly accessing common tools, shortcuts, or scene actions directly from the main editor toolbar.
 * Customize the toolbar by editing this script to define your own commands.
 */

namespace PiDev.Utilities.Editor
{
    [InitializeOnLoad]
    public static class QuickActionToolbars
    {
        static List<QuickLaunchItem> LeftToolbar = new() {
        new("Learn more about PiDevUtils", ()=>Application.OpenURL("https://github.com/Pi-Dev/PiDevUtils"), null),
        //new("Game Scene", ()=>SceneButton("GameScene"), ()=>SceneButton("GameScene", true)),
    };

        static List<QuickLaunchItem> RightToolbar = new() {
        new("Support the Dev", ()=>Application.OpenURL("https://store.steampowered.com/app/670510/ColorBlend_FX_Desaturation/"), null),
        new("Edit this toolbar", EditThisToolbar, null),
        new(""),
        //new("3D Skins", ()=>SceneButton("SetTool3D")),
        //new("2D Skins", ()=>SceneButton("SetTool2D")),
        //new(""),
        //new("Preloader", ()=>SceneButton("Preloader")),
    };

        static void EditThisToolbar()
        {
            var s = AssetDatabase.FindAssets("QuickActionToolbars t:MonoScript");
            if (s.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(s[0]);
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (asset != null) AssetDatabase.OpenAsset(asset);
            }
        }

        static void SceneButton(string name, bool play = false)
        {
            foreach (var scenePath in GetAllScenes())
            {
                var sn = Path.GetFileNameWithoutExtension(scenePath);
                if (sn.Contains(name))
                {
                    if (play) PlayScene(scenePath);
                    else OpenScene(scenePath);
                    return;
                }
            }
        }

        static QuickActionToolbars()
        {
            ToolbarExtender.LeftToolbarGUI.Add(() => DrawToolbar(LeftToolbar, true));
            ToolbarExtender.RightToolbarGUI.Add(() => DrawToolbar(RightToolbar, false));
        }

        public class QuickLaunchItem
        {
            public string label;
            public System.Action action;
            public System.Action rightAction;
            public QuickLaunchItem(string label, System.Action leftAction = null, System.Action rightAction = null)
            {
                this.label = label;
                this.action = leftAction;
                this.rightAction = rightAction;
            }
        }

        private static void DrawToolbar(List<QuickLaunchItem> items, bool isLeft)
        {
            if (items == null || items.Count == 0) return;
            GUILayout.BeginHorizontal();

            if (isLeft) GUILayout.FlexibleSpace();

            foreach (var item in items)
            {
                GUIStyle buttonStyle = EditorStyles.toolbarButton;
                if (item.action == null && item.rightAction == null)
                {
                    Vector2 ls = EditorStyles.label.CalcSize(new GUIContent(item.label));
                    Rect br = GUILayoutUtility.GetRect(new GUIContent(item.label), buttonStyle, GUILayout.Width(ls.x + 4f), GUILayout.Height(20));
                    GUI.Label(br, item.label);
                    continue;
                }
                Vector2 size = buttonStyle.CalcSize(new GUIContent(item.label));
                Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(item.label), buttonStyle, GUILayout.Width(size.x + 8f), GUILayout.Height(20));

                if (Event.current.type == EventType.MouseUp && buttonRect.Contains(Event.current.mousePosition))
                {
                    //Event.current.Use();
                    if (Event.current.button == 1) item.rightAction?.Invoke();
                    else item.action?.Invoke();
                }
                string activeSceneName = Path.GetFileNameWithoutExtension(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path);
                bool pressed = activeSceneName.Contains(item.label.Split(" ")[0]) || item.label.Contains(activeSceneName);

                if (GUI.Toggle(buttonRect, pressed, item.label, buttonStyle))
                {
                    // nothing
                }
            }

            if (!isLeft) GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
        private static void OpenScene(string scenePath/*, bool makeFirst*/)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // if (makeFirst) SetSceneAsFirstInBuildSettings(scenePath);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        private static void PlayScene(string scenePath/*, bool makeFirst*/)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                //if (makeFirst) SetSceneAsFirstInBuildSettings(scenePath);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        private static string[] GetAllScenes()
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }

    }
}
#endif