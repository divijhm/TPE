using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SelectionListener
{
    static double lastSelectionTime;
    const double delay = 0.25;

    static SelectionListener()
    {
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += Update;
    }

    static void OnSelectionChanged()
    {
        lastSelectionTime = EditorApplication.timeSinceStartup;
    }

    static void Update()
    {
        if (EditorApplication.timeSinceStartup - lastSelectionTime < delay)
            return;

        if (lastSelectionTime == 0)
            return;

        lastSelectionTime = 0;

        ProcessFinalSelection();
    }

    static void ProcessFinalSelection()
    {
        if (Selection.gameObjects.Length == 0)
            return;

        var context = SelectionContextBuilder.Build();

        Debug.Log(JsonUtility.ToJson(context, true));

        MCPClient client =
            Object.FindObjectOfType<MCPClient>();

        if (client != null)
            client.SendContext(context);
    }
}