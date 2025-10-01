#if UNITY_EDITOR
using PiDev.Utilities.Editor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PiDev.Utilities.AutoCompletion;

[CreateAssetMenu(fileName = "TableViewTestData", menuName = "PiDev/TableView Test Data", order = 1)]
public class TableViewTestDataScriptables : ScriptableObject
{
    // Example data class for TableView
    public string cardName;
    public string cardDescription;
    public int cardValue;
    [AutoComplete("Spell", "Trap", "Energy", "Creature")] public string cardType;
    public Sprite cardIcon;
    public Color cardColor;
    public bool isSpecial;
    public int cardLevel;
    public int attack;
    public int defense;

#if UNITY_EDITOR
    [MenuItem("Pi-Dev/Example TableView Scriptable Objects")]
    public static void PdTVScriptables()
    {
        EditorWindow.GetWindow<PdTVScriptablesEditor>("Sample Cards", true, typeof(SceneView));
    }
#endif

}

#if UNITY_EDITOR
class PdTVScriptablesEditor : ScriptableObjectTable<TableViewTestDataScriptables> { };
#endif