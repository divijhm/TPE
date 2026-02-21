using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class RegionSelectionTool
{
    static bool dragging = false;
    static Vector2 startPos;
    static Vector2 endPos;

    static RegionSelectionTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Debug.Log("Region tool active");

        Event e = Event.current;

        // START DRAG
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            dragging = true;
            startPos = e.mousePosition;
            Debug.Log("Drag started");
        }

        // DRAGGING
        if (e.type == EventType.MouseDrag && dragging)
        {
            endPos = e.mousePosition;
            sceneView.Repaint();
        }

        // END DRAG
        if (e.type == EventType.MouseUp && dragging)
        {
            dragging = false;
            SelectObjects(sceneView);
            Debug.Log("Drag ended");
        }

        if (dragging)
            DrawSelectionBox();
    }

    static void DrawSelectionBox()
    {
        Rect rect = GetScreenRect(startPos, endPos);

        Handles.BeginGUI();
        GUI.Box(rect, "");
        Handles.EndGUI();
    }

    static Rect GetScreenRect(Vector2 p1, Vector2 p2)
    {
        return new Rect(
            Mathf.Min(p1.x, p2.x),
            Mathf.Min(p1.y, p2.y),
            Mathf.Abs(p1.x - p2.x),
            Mathf.Abs(p1.y - p2.y)
        );
    }

    static void SelectObjects(SceneView sceneView)
    {
        Rect rect = GetScreenRect(startPos, endPos);

        List<GameObject> selected = new();

        Camera cam = sceneView.camera;

        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (!obj.CompareTag("Selectable"))
                continue;

            Vector3 viewportPos =
                cam.WorldToViewportPoint(obj.transform.position);

            if (viewportPos.z < 0)
                continue;

            Vector2 screenPos = new Vector2(
                viewportPos.x * cam.pixelWidth,
                (1 - viewportPos.y) * cam.pixelHeight
            );

            if (rect.Contains(screenPos))
                selected.Add(obj);
        }

        Selection.objects = selected.ToArray();

        Debug.Log("Region selected: " + selected.Count + " objects");
    }
}