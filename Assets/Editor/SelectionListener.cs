using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class SelectionListener
{
    static SelectionListener()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    static void OnSelectionChanged()
    {
        if (Selection.gameObjects.Length == 0)
            return;

        List<SelectedObjectData> context =
            SelectionContextBuilder.Build();

        string json =
            JsonUtility.ToJson(new Wrapper(context), true);

        Debug.Log(json);
    }

    [System.Serializable]
    class Wrapper
    {
        public List<SelectedObjectData> selection;

        public Wrapper(List<SelectedObjectData> data)
        {
            selection = data;
        }
    }
}