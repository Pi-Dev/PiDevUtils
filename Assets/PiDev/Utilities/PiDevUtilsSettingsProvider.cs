#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
#if UNITY_2023_1_OR_NEWER     
using UnityEditor.Build;
#endif

namespace PiDev.Utilities
{
    static class PiDevUtilsSettingsProvider
    {
        public static class DefinesHelper
        {
            public static void AddSymbolToAllTargets(string defineSymbol)
            {
                foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
                {
                    if (!IsValidGroup(group)) continue;
                    var defines = GetDefines(group);
                    if (!defines.Contains(defineSymbol))
                    {
                        defines.Add(defineSymbol);
                        SetDefines(group, defines);
                    }
                }
            }

            public static void RemoveSymbolFromAllTargets(string defineSymbol)
            {
                foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
                {
                    if (!IsValidGroup(group)) continue;
                    var defines = GetDefines(group);
                    if (defines.Contains(defineSymbol))
                    {
                        defines.Remove(defineSymbol);
                        SetDefines(group, defines);
                    }
                }
            }

            static List<string> GetDefines(BuildTargetGroup group)
            {
#if UNITY_2023_1_OR_NEWER
                var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
#else
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
                return defines.Split(';').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();
            }

            static void SetDefines(BuildTargetGroup group, List<string> defines)
            {
                var joined = string.Join(";", defines);
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), joined);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, joined);
#endif
            }

            static bool IsValidGroup(BuildTargetGroup group)
            {
                return group != BuildTargetGroup.Unknown && !IsObsolete(group);
            }

            static bool IsObsolete(Enum value)
            {
                var field = value.GetType().GetField(value.ToString());
                return field.GetCustomAttributes(typeof(ObsoleteAttribute), false).Length > 0;
            }
        }

        [InitializeOnLoadMethod]
        static void EnsurePiDevUtilsDefine()
        {
            DefinesHelper.AddSymbolToAllTargets("PI_DEV_UTILS");
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Pi-Dev Utilities", SettingsScope.Project)
            {
                label = "Pi-Dev Utilities",
                guiHandler = (searchContext) =>
                {
                    SerializedObject settings = Settings.GetSerializedSettings();

                    EditorGUIUtility.labelWidth = 200f;

                    EditorGUILayout.LabelField("Singleton Defaults", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(
                        settings.FindProperty("useDontDestroyOnLoad"),
                        new GUIContent("Use DontDestroyOnLoad")
                    );

                    EditorGUILayout.PropertyField(
                        settings.FindProperty("autoCreateIfMissing"),
                        new GUIContent("Auto Create If Missing")
                    );

                    EditorGUILayout.PropertyField(
                        settings.FindProperty("instanceManagementMode"),
                        new GUIContent("Instance Management Mode")
                    );

                    settings.ApplyModifiedProperties();
                },

                keywords = new System.Collections.Generic.HashSet<string>(new[]
                {
                    "singleton", "PiDev", "utilities", "dontdestroyonload", "autocreate", "instance"
                })
            };

            return provider;
        }
    }
}
#endif
