using UnityEditor;
using UnityEngine;

public static class ToggleSceneGroups
{
    [MenuItem("Tools/Toggle Visibility - init")]
    private static void ToggleInit()
    {
        var root = GameObject.Find("init");
        if (root == null) { Debug.LogWarning("[ToggleSceneGroups] Could not find GameObject named 'init'."); return; }
        ToggleChildren(root, "init");
    }

    [MenuItem("Tools/Toggle Visibility - toreach")]
    private static void ToggleToreach()
    {
        var root = GameObject.Find("toreach");
        if (root == null) { Debug.LogWarning("[ToggleSceneGroups] Could not find GameObject named 'toreach'."); return; }
        ToggleChildren(root, "toreach");
    }

    private static void ToggleChildren(GameObject root, string label)
    {
        int count = root.transform.childCount;
        if (count == 0) { Debug.LogWarning($"[ToggleSceneGroups] '{label}' has no children."); return; }

        // Decide new state based on the first child's current state
        bool firstActive = root.transform.GetChild(0).gameObject.activeSelf;
        bool newState = !firstActive;

        for (int i = 0; i < count; i++)
            root.transform.GetChild(i).gameObject.SetActive(newState);

        Debug.Log($"[ToggleSceneGroups] '{label}' — {count} children set to active={newState}.");
    }
}
