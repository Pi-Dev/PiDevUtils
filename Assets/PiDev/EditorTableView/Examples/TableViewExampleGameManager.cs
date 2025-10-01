using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using PiDev.Utilities.Editor;
using UnityEditor;
#endif

public class TableViewExampleGameManager : MonoBehaviour
{
    // Example data class for TableView
    [Serializable]
    public class ExampleItemData
    {
        public string itemName;
        public string itemDescription;
        public int itemValue;
        public Sprite itemIcon;
        public Color itemColor;
        public bool isSpecial;
    }
    
    // The collection we will edit
    public List<ExampleItemData> items = new List<ExampleItemData>();

#if UNITY_EDITOR
    [MenuItem("Pi-Dev/Example TableView Collection")]
    public static void PdTVEditCollection()
    {
        var prefabPath = AssetDatabase.FindAssets("ExampleGameManagerPrefab t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var manager = prefab != null ? prefab.GetComponentInChildren<TableViewExampleGameManager>() : null;

        var window = EditorWindow.GetWindow<PdTVCollectionEditor>("Sample Items", true, typeof(SceneView));
        window.ShowIndexOneBased = true;
        window.SetData(manager, "items");
    }
#endif

}

#if UNITY_EDITOR
class PdTVCollectionEditor : CollectionTable<TableViewTestDataScriptables> { }
#endif