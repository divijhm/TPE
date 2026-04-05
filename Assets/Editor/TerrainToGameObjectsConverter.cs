using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TerrainToGameObjectsConverter
{
    private const int MaxDetailObjectsPerTerrain = 15000;

    [MenuItem("Tools/Terrain/Convert Trees + Mesh Details To GameObjects (Keep Terrain Data)")]
    private static void ConvertKeepTerrain()
    {
        ConvertTerrains(clearTerrainData: false);
    }

    [MenuItem("Tools/Terrain/Convert Trees + Mesh Details To GameObjects (Keep Data + Hide Terrain Foliage)")]
    private static void ConvertKeepAndHideTerrainFoliage()
    {
        ConvertTerrains(clearTerrainData: false, hideTerrainFoliage: true);
    }

    [MenuItem("Tools/Terrain/Convert Trees + Mesh Details To GameObjects (Clear Terrain Data)")]
    private static void ConvertAndClearTerrain()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Clear Terrain Trees/Details?",
            "This will create editable GameObjects and then remove trees/details from Terrain data. Continue?",
            "Yes, Convert And Clear",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        ConvertTerrains(clearTerrainData: true);
    }

    private static void ConvertTerrains(bool clearTerrainData)
    {
        ConvertTerrains(clearTerrainData, hideTerrainFoliage: false);
    }

    private static void ConvertTerrains(bool clearTerrainData, bool hideTerrainFoliage)
    {
        Terrain[] terrains = GetTargetTerrains();
        if (terrains == null || terrains.Length == 0)
        {
            EditorUtility.DisplayDialog("No Terrain Found", "Select a Terrain or keep one active in the scene.", "OK");
            return;
        }

        int totalTrees = 0;
        int totalDetails = 0;
        int skippedDetailLayers = 0;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Convert Terrain To GameObjects");

        foreach (Terrain terrain in terrains)
        {
            if (terrain == null || terrain.terrainData == null)
            {
                continue;
            }

            Transform root = CreateRoot(terrain, clearTerrainData);

            totalTrees += ConvertTrees(terrain, root);
            totalDetails += ConvertMeshDetails(terrain, root, ref skippedDetailLayers);

            if (clearTerrainData)
            {
                ClearTerrainScatter(terrain);
                EditorUtility.SetDirty(terrain.terrainData);
            }

            if (hideTerrainFoliage)
            {
                terrain.drawTreesAndFoliage = false;
                EditorUtility.SetDirty(terrain);
            }

            EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);
        }

        Undo.CollapseUndoOperations(undoGroup);

        string message =
            "Converted Trees: " + totalTrees + "\n" +
            "Converted Mesh Details: " + totalDetails + "\n" +
            "Skipped Texture-Only Detail Layers: " + skippedDetailLayers + "\n\n" +
            "Note: Grass/texture detail layers are not regular prefabs, so they cannot be converted to editable GameObjects by default.";

        if (clearTerrainData)
        {
            message += "\n\nTerrain trees/details were cleared after conversion.";
        }
        else if (hideTerrainFoliage)
        {
            message += "\n\nTerrain trees/details were preserved, but Terrain drawing for trees/foliage is disabled so only editable GameObjects remain visible.";
        }

        EditorUtility.DisplayDialog("Terrain Conversion Complete", message, "OK");
    }

    private static Terrain[] GetTargetTerrains()
    {
        if (Selection.activeGameObject != null)
        {
            Terrain selectedTerrain = Selection.activeGameObject.GetComponent<Terrain>();
            if (selectedTerrain != null)
            {
                return new[] { selectedTerrain };
            }
        }

        return Terrain.activeTerrains;
    }

    private static Transform CreateRoot(Terrain terrain, bool clearTerrainData)
    {
        string rootName = clearTerrainData
            ? terrain.name + "_ConvertedScatter"
            : terrain.name + "_ConvertedScatter_Copy";

        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Create Scatter Root");
        return root.transform;
    }

    private static int ConvertTrees(Terrain terrain, Transform root)
    {
        TerrainData data = terrain.terrainData;
        TreePrototype[] prototypes = data.treePrototypes;
        TreeInstance[] instances = data.treeInstances;

        int created = 0;

        for (int i = 0; i < instances.Length; i++)
        {
            TreeInstance tree = instances[i];
            if (tree.prototypeIndex < 0 || tree.prototypeIndex >= prototypes.Length)
            {
                continue;
            }

            GameObject prefab = prototypes[tree.prototypeIndex].prefab;
            if (prefab == null)
            {
                continue;
            }

            Vector3 worldPos = terrain.transform.position + Vector3.Scale(tree.position, data.size);
            Quaternion worldRot = Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f);

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, terrain.gameObject.scene) as GameObject;
            if (instance == null)
            {
                continue;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Create Tree Instance");
            instance.transform.SetPositionAndRotation(worldPos, worldRot);

            Vector3 baseScale = prefab.transform.localScale;
            instance.transform.localScale = new Vector3(
                baseScale.x * tree.widthScale,
                baseScale.y * tree.heightScale,
                baseScale.z * tree.widthScale);

            instance.transform.SetParent(root, true);
            created++;
        }

        return created;
    }

    private static int ConvertMeshDetails(Terrain terrain, Transform root, ref int skippedLayers)
    {
        TerrainData data = terrain.terrainData;
        DetailPrototype[] detailPrototypes = data.detailPrototypes;

        if (detailPrototypes == null || detailPrototypes.Length == 0)
        {
            return 0;
        }

        int detailWidth = data.detailWidth;
        int detailHeight = data.detailHeight;
        float cellSizeX = data.size.x / detailWidth;
        float cellSizeZ = data.size.z / detailHeight;

        int created = 0;

        for (int layer = 0; layer < detailPrototypes.Length; layer++)
        {
            DetailPrototype prototype = detailPrototypes[layer];
            GameObject detailPrefab = prototype.prototype;

            if (detailPrefab == null)
            {
                skippedLayers++;
                continue;
            }

            int[,] map = data.GetDetailLayer(0, 0, detailWidth, detailHeight, layer);

            for (int y = 0; y < detailHeight; y++)
            {
                for (int x = 0; x < detailWidth; x++)
                {
                    int density = map[y, x];
                    for (int i = 0; i < density; i++)
                    {
                        if (created >= MaxDetailObjectsPerTerrain)
                        {
                            Debug.LogWarning("Detail conversion capped at " + MaxDetailObjectsPerTerrain + " objects for terrain " + terrain.name + ".");
                            return created;
                        }

                        float px = terrain.transform.position.x + (x + UnityEngine.Random.value) * cellSizeX;
                        float pz = terrain.transform.position.z + (y + UnityEngine.Random.value) * cellSizeZ;
                        float py = terrain.SampleHeight(new Vector3(px, 0f, pz)) + terrain.transform.position.y;

                        Quaternion rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

                        GameObject instance = PrefabUtility.InstantiatePrefab(detailPrefab, terrain.gameObject.scene) as GameObject;
                        if (instance == null)
                        {
                            continue;
                        }

                        Undo.RegisterCreatedObjectUndo(instance, "Create Detail Instance");
                        instance.transform.SetPositionAndRotation(new Vector3(px, py, pz), rot);

                        float width = UnityEngine.Random.Range(prototype.minWidth, prototype.maxWidth);
                        float height = UnityEngine.Random.Range(prototype.minHeight, prototype.maxHeight);
                        instance.transform.localScale = new Vector3(width, height, width);

                        instance.transform.SetParent(root, true);
                        created++;
                    }
                }
            }
        }

        return created;
    }

    private static void ClearTerrainScatter(Terrain terrain)
    {
        TerrainData data = terrain.terrainData;

        data.treeInstances = Array.Empty<TreeInstance>();

        int detailWidth = data.detailWidth;
        int detailHeight = data.detailHeight;
        int layers = data.detailPrototypes.Length;

        int[,] empty = new int[detailHeight, detailWidth];
        for (int layer = 0; layer < layers; layer++)
        {
            data.SetDetailLayer(0, 0, layer, empty);
        }

        terrain.Flush();
    }
}
