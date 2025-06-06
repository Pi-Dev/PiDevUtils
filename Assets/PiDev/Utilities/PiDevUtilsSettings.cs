using UnityEditor;
using UnityEngine;

namespace PiDev.Utilities
{
    [CreateAssetMenu(menuName = "PiDev/Utilities Settings", fileName = "PiDevUtilsSettings")]
    public class Settings : ScriptableObject
    {
        public bool useDontDestroyOnLoad = true;
        public bool autoCreateIfMissing = true;
        public InstanceBehavior instanceManagementMode = InstanceBehavior.AllowDuplicatesOverrideOldInstance;

        private const string ResourcePath = "PiDev/PiDevUtilsSettings"; // Looks for Resources/PiDev/PiDevUtilsSettings.asset

        public static Settings Load()
        {
            var settings = Resources.Load<Settings>(ResourcePath);

#if UNITY_EDITOR
            if (settings == null)
            {
                settings = CreateInstance<Settings>();
                string folderPath = "Assets/Resources/PiDev";
                string assetPath = $"{folderPath}/PiDevUtilsSettings.asset";

                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);

                UnityEditor.AssetDatabase.CreateAsset(settings, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log("Created default PiDevUtilsSettings at " + assetPath);
            }
#endif

            return settings;
        }


#if UNITY_EDITOR
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Load());
        }
#endif
    }
}
