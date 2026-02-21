using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class SelectionContextBuilder
{
    public static List<SelectedObjectData> Build()
    {
        List<SelectedObjectData> result = new();

        foreach(GameObject obj in Selection.gameObjects)
        {
            result.Add(new SelectedObjectData
            {
                name = obj.name,
                tag = obj.tag,
                position = obj.transform.position,
                rotation = obj.transform.eulerAngles,
                scale = obj.transform.localScale
            });
        }

        return result;
    }
}