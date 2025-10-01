using Microsoft.CSharp.RuntimeBinder;
using PiDev.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Unity.VisualScripting;
using UnityEngine;
using PiDev.TableView;

public class TableViewExamples : MonoBehaviour
{
    // TableList example
    [Serializable]
    public class PlayerData
    {
        public string name;
        public int score;
        public Color teamColor;
    }

    [Header("      Use the Pi-Dev top level menu to open views")]
    [Space(10)]
    public TableList<PlayerData> players;

#if !PI_DEV_UTILS
    [Header("Install Pi-Dev Utilities Core to see the ActionButtons example")]
    public bool piDevUtilsIsMissing = true;
#endif

#if PI_DEV_UTILS

    // Action Buttons example to call static methods
#if UNITY_EDITOR
    public ActionButtons actions = new ActionButtons("Actions",
        new("Scriptable Object Table", () => TableViewTestDataScriptables.PdTVScriptables()),
        new("Container Table", () => TableViewExampleGameManager.PdTVEditCollection())
    );
#endif

    // Dynamic Action Buttons example
    public ActionButtons dynamicActions = new ActionButtons("Dynamic Actions");
    // You use the C# constructor to initialize these - it will be called before Awake!
    // This is the usual pattern if they are dynaimically generated: ctor and OnValidate
    TableViewExamples() => UpdateDynamicActions();
    void OnValidate() => UpdateDynamicActions();
    void UpdateDynamicActions()
    {
        dynamicActions.SetButtons();
        foreach(var p in players)
        {
            dynamicActions.Add(new ActionButtons.ActionButton($"{p.name}", () => Debug.Log($"<color=#{p.teamColor.ToHexString()}><b>{p.name}</b>, Score: {p.score}</color>")));
        }
    }
#endif
}
