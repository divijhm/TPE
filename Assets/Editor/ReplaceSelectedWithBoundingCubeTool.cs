using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ReplaceSelectedWithBoundingCubeTool
{
    private const string MenuPath = "Tools/Greybox/Replace Selected With Bounding Cube";

    [MenuItem(MenuPath)]
    private static void ReplaceSelectedWithBoundingCube()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        List<GameObject> roots = FilterTopLevelSelection(selected);
        List<GameObject> created = new List<GameObject>();

        int replacedCount = 0;
        int skippedCount = 0;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Replace Selected With Bounding Cube");

        for (int i = 0; i < roots.Count; i++)
        {
            GameObject source = roots[i];
            Bounds worldBounds;
            Material sourceMaterial;

            if (!TryGetHierarchyBounds(source, out worldBounds))
            {
                skippedCount++;
                continue;
            }

            TryGetRepresentativeMaterial(source, out sourceMaterial);

            Transform sourceTransform = source.transform;
            Transform parent = sourceTransform.parent;
            int siblingIndex = sourceTransform.GetSiblingIndex();

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(cube, "Create Bounding Cube");

            cube.name = source.name + "_BoundingCube";
            cube.layer = source.layer;
            cube.tag = source.tag;
            cube.isStatic = source.isStatic;

            if (parent != null)
            {
                cube.transform.SetParent(parent, true);
                cube.transform.SetSiblingIndex(siblingIndex);
            }

            cube.transform.position = worldBounds.center;
            cube.transform.rotation = Quaternion.identity;
            SetWorldScale(cube.transform, ClampMin(worldBounds.size, 0.0001f));

            if (sourceMaterial != null)
            {
                MeshRenderer cubeRenderer = cube.GetComponent<MeshRenderer>();
                if (cubeRenderer != null)
                {
                    cubeRenderer.sharedMaterial = sourceMaterial;
                }
            }

            Collider col = cube.GetComponent<Collider>();
            if (col != null)
            {
                Undo.DestroyObjectImmediate(col);
            }

            Undo.DestroyObjectImmediate(source);

            created.Add(cube);
            replacedCount++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        Selection.objects = created.ToArray();
        Debug.Log("Replaced " + replacedCount + " object(s) with bounding cubes. Skipped: " + skippedCount + ".");
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateReplaceSelectedWithBoundingCube()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    private static List<GameObject> FilterTopLevelSelection(GameObject[] selection)
    {
        HashSet<Transform> selectedTransforms = new HashSet<Transform>();
        for (int i = 0; i < selection.Length; i++)
        {
            if (selection[i] != null)
            {
                selectedTransforms.Add(selection[i].transform);
            }
        }

        List<GameObject> roots = new List<GameObject>();

        for (int i = 0; i < selection.Length; i++)
        {
            GameObject go = selection[i];
            if (go == null)
            {
                continue;
            }

            if (HasSelectedAncestor(go.transform, selectedTransforms))
            {
                continue;
            }

            roots.Add(go);
        }

        return roots;
    }

    private static bool HasSelectedAncestor(Transform t, HashSet<Transform> selected)
    {
        Transform current = t.parent;
        while (current != null)
        {
            if (selected.Contains(current))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool TryGetHierarchyBounds(GameObject root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers != null && renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        if (colliders != null && colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return true;
        }

        bounds = default(Bounds);
        return false;
    }

    private static bool TryGetRepresentativeMaterial(GameObject root, out Material material)
    {
        Renderer rootRenderer = root.GetComponent<Renderer>();
        if (TryGetFirstMaterial(rootRenderer, out material))
        {
            return true;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (TryGetFirstMaterial(renderers[i], out material))
            {
                return true;
            }
        }

        material = null;
        return false;
    }

    private static bool TryGetFirstMaterial(Renderer renderer, out Material material)
    {
        if (renderer == null)
        {
            material = null;
            return false;
        }

        Material[] mats = renderer.sharedMaterials;
        if (mats != null)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null)
                {
                    material = mats[i];
                    return true;
                }
            }
        }

        material = renderer.sharedMaterial;
        return material != null;
    }

    private static Vector3 ClampMin(Vector3 v, float min)
    {
        return new Vector3(
            Mathf.Max(min, v.x),
            Mathf.Max(min, v.y),
            Mathf.Max(min, v.z));
    }

    private static void SetWorldScale(Transform t, Vector3 worldScale)
    {
        if (t.parent == null)
        {
            t.localScale = worldScale;
            return;
        }

        Vector3 p = t.parent.lossyScale;

        t.localScale = new Vector3(
            SafeDivide(worldScale.x, Mathf.Abs(p.x)),
            SafeDivide(worldScale.y, Mathf.Abs(p.y)),
            SafeDivide(worldScale.z, Mathf.Abs(p.z)));
    }

    private static float SafeDivide(float numerator, float denominator)
    {
        if (Mathf.Abs(denominator) < 0.00001f)
        {
            return numerator;
        }

        return numerator / denominator;
    }
}
