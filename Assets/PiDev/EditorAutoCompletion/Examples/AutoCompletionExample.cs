using PiDev.Utilities.AutoCompletion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDataAC : AutoComplete
{
    public override IEnumerable<string> GetItems()
    {
        var r = new List<string>();
        for (int i = 0; i < 100; i++) r.Add($"Item {i}");
        return r.ToArray();
    }
}

public class AutoCompletionExample : MonoBehaviour
{
    [AutoComplete("Mihad", "Pumzow", "Niki", "Darks")]
    public string besties;

    [DynamicDataAC] public string data;
}

