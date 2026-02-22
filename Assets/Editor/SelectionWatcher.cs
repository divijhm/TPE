using UnityEditor;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public static class SelectionWatcher
{
    static SelectionWatcher()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    static void OnSelectionChanged()
    {
        var selected = Selection.gameObjects;

        if(selected.Length == 0)
            return;

        // Debug.Log($"Selected {selected.Length} object(s)");

        // foreach(var obj in selected)
        //     Debug.Log(" -> " + obj.name);
    }
}