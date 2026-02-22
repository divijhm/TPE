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
        {
            // Selectable filter
            if (obj.GetComponent<Selectable>() == null)
                continue;

            objects.Add(new SelectedObjectData
            {
                name = obj.name,
                tag = obj.tag,
                position = obj.transform.position,
                rotation = obj.transform.eulerAngles,
                scale = obj.transform.localScale
            });

            Renderer r = obj.GetComponentInChildren<Renderer>();
            if (r != null)
                renderers.Add(r);
        }

        SelectionBounds bounds = ComputeBounds(renderers);

        // return new SelectionContext
        // {
        //     selection = objects,
        //     bounds = bounds
        // };
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