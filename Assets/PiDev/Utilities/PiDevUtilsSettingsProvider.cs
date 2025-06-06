#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PiDev.Utilities
{
    static class PiDevUtilsSettingsProvider
    {
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
