using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class SelectionContextBuilder
{
    public static SelectionContext Build()
    {
        List<SelectedObjectData> objects = new();
        List<Renderer> renderers = new();

        foreach (GameObject obj in Selection.gameObjects)
            CollectRecursive(obj, objects, renderers);

        return FinishBuild(objects, renderers);
    }

    static void CollectRecursive(GameObject obj, List<SelectedObjectData> objects, List<Renderer> renderers)
    {
        objects.Add(new SelectedObjectData
        {
            name = obj.name,
            tag = obj.tag,
            position = obj.transform.position,
            rotation = obj.transform.eulerAngles,
            scale = obj.transform.localScale
        });

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
            renderers.Add(r);

        foreach (Transform child in obj.transform)
            CollectRecursive(child.gameObject, objects, renderers);
    }

    static SelectionContext FinishBuild(List<SelectedObjectData> objects, List<Renderer> renderers)
    {
        SelectionBounds bounds = ComputeBounds(renderers);
        return new SelectionContext
        {
            prompt = "",
            selection = objects,
            bounds = bounds
        };
    }

    static SelectionBounds ComputeBounds(List<Renderer> renderers)
    {
        if (renderers.Count == 0)
            return new SelectionBounds();

        Bounds combined = renderers[0].bounds;

        for (int i = 1; i < renderers.Count; i++)
            combined.Encapsulate(renderers[i].bounds);

        return new SelectionBounds
        {
            center = combined.center,
            size = combined.size
        };
    }
}