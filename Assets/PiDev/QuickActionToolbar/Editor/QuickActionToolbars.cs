#if UNITY_EDITOR
// #define QUICK_ACTION_TOOLBAR_FORCE_LEGACY

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_6000_3_OR_NEWER && !QUICK_ACTION_TOOLBAR_FORCE_LEGACY
using UnityEditor.Toolbars;
#else
using UnityToolbarExtender;
#endif

/* Licensed under MIT
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * A hacky toolbar extender for old Unity versions and a MainToolbarElement provider for 2023 and newer,
 * providing quick access buttons to common tools, shortcuts, and scene actions directly from the main editor toolbar.
 * You can customize the toolbar by editing this script.
 * On newer Unity versions you may need to manually enable the toolbars from the main toolbar context menu.
 */

namespace PiDev.Utilities.Editor
{
    [InitializeOnLoad]
    public static class QuickActionToolbars
    {
        // Toolbar registration paths for newer Unity versions. 
        // Not used for legacy Unity
        const string LeftToolbarPath = "Pi-Dev Utils/Left Actions Toolbar";
        const string RightToolbarPath = "Pi-Dev Utils/Right Actions Toolbar";

        // Change these to add/remove/modify toolbar items.
        // Each item can have multiple actions accessible via dropdown or right-click.
        static readonly List<QuickLaunchItem> LeftToolbar = new()
        {
            new("Learn more about PiDevUtils", () => Application.OpenURL("https://github.com/Pi-Dev/PiDevUtils")),
            new(""),
            new("Sample Scene",
                ("Open", () => SceneButton("GameScene")),
                ("Play", () => SceneButton("GameScene", true))),
        };

        static readonly List<QuickLaunchItem> RightToolbar = new()
        {
            new("Support the Dev", () => Application.OpenURL("https://store.steampowered.com/app/670510/ColorBlend_FX_Desaturation/")),
            new("Edit this toolbar", EditThisToolbar),
            new(""),
            //new("3D Skins", () => SceneButton("SetTool3D")),
            //new("2D Skins", () => SceneButton("SetTool2D")),
            //new(""),
            //new("Preloader", () => SceneButton("Preloader")),
            new("Dropdown",
                ("Item 1", () => Debug.Log("Item 1")),
                ("Item 2", () => Debug.Log("Item 2")),
                ("Item 3", () => Debug.Log("Item 3"))
            )
        };

        static void EditThisToolbar()
        {
            var scripts = AssetDatabase.FindAssets("QuickActionToolbars t:MonoScript");
            if (scripts.Length <= 0) return;

            var path = AssetDatabase.GUIDToAssetPath(scripts[0]);
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
            }
        }

        static void SceneButton(string name, bool play = false)
        {
            foreach (var scenePath in GetAllScenes())
            {
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (!sceneName.Contains(name)) continue;
                if (play) PlayScene(scenePath);
                else OpenScene(scenePath);
                return;
            }
        }

#if !UNITY_6000_3_OR_NEWER || QUICK_ACTION_TOOLBAR_FORCE_LEGACY
        static QuickActionToolbars()
        {
            ToolbarExtender.LeftToolbarGUI.Add(() => DrawToolbar(LeftToolbar, true));
            ToolbarExtender.RightToolbarGUI.Add(() => DrawToolbar(RightToolbar, false));
        }
#else
        [MainToolbarElement(LeftToolbarPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -1)]
        public static IEnumerable<MainToolbarElement> CreateLeftToolbar()
        {
            return CreateMainToolbarElements(LeftToolbar);
        }

        [MainToolbarElement(RightToolbarPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 1)]
        public static IEnumerable<MainToolbarElement> CreateRightToolbar()
        {
            return CreateMainToolbarElements(RightToolbar);
        }
#endif

        public class QuickLaunchItem
        {
            public readonly string label;
            public readonly List<(string label, Action action)> items;

            public QuickLaunchItem(string label, Action action)
            {
                this.label = label;
                items = action == null
                    ? new List<(string label, Action action)>()
                    : new List<(string label, Action action)> { (label, action) };
            }

            public QuickLaunchItem(string label, params (string label, Action action)[] items)
            {
                this.label = label;
                this.items = items?.Where(item => item.action != null).ToList()
                    ?? new List<(string label, Action action)>();
            }
        }

#if UNITY_6000_3_OR_NEWER && !QUICK_ACTION_TOOLBAR_FORCE_LEGACY
        static IEnumerable<MainToolbarElement> CreateMainToolbarElements(IEnumerable<QuickLaunchItem> items)
        {
            foreach (var item in items)
            {
                if (item.items.Count == 0)
                {
                    yield return new MainToolbarLabel(new MainToolbarContent(item.label));
                    continue;
                }

                if (item.items.Count == 1)
                {
                    var actionItem = item.items[0];
                    yield return new MainToolbarButton(
                        new MainToolbarContent(item.label, actionItem.label),
                        () => actionItem.action?.Invoke());
                    continue;
                }

                yield return new MainToolbarDropdown(
                    new MainToolbarContent(item.label, item.label + " options"),
                    rect => ShowActionMenu(rect, item.items));
            }
        }
#endif

        static void DrawToolbar(List<QuickLaunchItem> items, bool isLeft)
        {
            if (items == null || items.Count == 0) return;
            GUILayout.BeginHorizontal();

            if (isLeft) GUILayout.FlexibleSpace();
            foreach (var item in items)
            {
                if (item.items.Count == 0)
                {
                    DrawToolbarLabel(item.label);
                    continue;
                }

                if (item.items.Count == 1)
                {
                    DrawSingleActionButton(item);
                    continue;
                }

                DrawDropdownButton(item);
            }

            if (!isLeft) GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static void DrawToolbarLabel(string label)
        {
            var style = EditorStyles.toolbarButton;
            var size = EditorStyles.label.CalcSize(new GUIContent(label));
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), style, GUILayout.Width(size.x + 4f), GUILayout.Height(20));
            GUI.Label(rect, label);
        }

        static void DrawSingleActionButton(QuickLaunchItem item)
        {
            var style = EditorStyles.toolbarButton;
            var content = new GUIContent(item.label, item.items[0].label);
            var size = style.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.Width(size.x + 8f), GUILayout.Height(20));

            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
                item.items[0].action?.Invoke();

            if (GUI.Toggle(rect, IsItemPressed(item), item.label, style))
            {
                // nothing
            }
        }

        static void DrawDropdownButton(QuickLaunchItem item)
        {
            var style = EditorStyles.toolbarDropDown;
            var content = new GUIContent(item.label, item.label + " options");
            var size = style.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.Width(size.x + 8f), GUILayout.Height(20));
            var currentEvent = Event.current;

            if (currentEvent.type == EventType.MouseUp && rect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.button == 1 && item.items.Count == 2)
                {
                    item.items[1].action?.Invoke();
                    currentEvent.Use();
                }
                else if (currentEvent.button == 1)
                {
                    ShowActionMenu(rect, item.items);
                    currentEvent.Use();
                }
            }

            if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, style))
                ShowActionMenu(rect, item.items);
        }

        static bool IsItemPressed(QuickLaunchItem item)
        {
            var activeSceneName = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
            return activeSceneName.Contains(item.label.Split(" ")[0]) || item.label.Contains(activeSceneName);
        }

        static void ShowActionMenu(Rect rect, IReadOnlyList<(string label, Action action)> actions)
        {
            var menu = new GenericMenu();

            foreach (var actionItem in actions)
            {
                var captured = actionItem;
                menu.AddItem(new GUIContent(captured.label), false, () => captured.action?.Invoke());
            }

            menu.DropDown(rect);
        }

        static void OpenScene(string scenePath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        static void PlayScene(string scenePath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        static string[] GetAllScenes()
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }
    }
}
#endif
