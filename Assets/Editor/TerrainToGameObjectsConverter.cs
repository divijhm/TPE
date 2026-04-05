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
        int cappedTerrains = 0;

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
            bool detailsWereCapped = false;
            totalDetails += ConvertMeshDetails(terrain, root, ref skippedDetailLayers, ref detailsWereCapped);
            if (detailsWereCapped)
            {
                cappedTerrains++;
            }

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

        if (cappedTerrains > 0)
        {
            message += "\n\nDetail conversion hit the per-terrain cap on " + cappedTerrains + " terrain(s). Converted details were distributed across each terrain to preserve overall coverage.";
        }

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

    private static int ConvertMeshDetails(Terrain terrain, Transform root, ref int skippedLayers, ref bool wasCapped)
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

        int totalRequested = 0;
        for (int layer = 0; layer < detailPrototypes.Length; layer++)
        {
            DetailPrototype prototype = detailPrototypes[layer];
            if (prototype.prototype == null)
            {
                continue;
            }

            int[,] map = data.GetDetailLayer(0, 0, detailWidth, detailHeight, layer);
            for (int y = 0; y < detailHeight; y++)
            {
                for (int x = 0; x < detailWidth; x++)
                {
                    totalRequested += GetDetailDensity(map, x, y);
                }
            }
        }

        float keepRatio = 1f;
        if (totalRequested > MaxDetailObjectsPerTerrain)
        {
            keepRatio = (float)MaxDetailObjectsPerTerrain / totalRequested;
            wasCapped = true;
            Debug.LogWarning(
                "Detail conversion requested " + totalRequested +
                " objects on terrain " + terrain.name +
                ", exceeding cap " + MaxDetailObjectsPerTerrain +
                ". Applying distributed downsampling (ratio=" + keepRatio.ToString("F3") + ").");
        }

        System.Random prng = new System.Random(terrain.GetInstanceID());
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
                    int density = GetDetailDensity(map, x, y);
                    int targetCount = GetScaledCount(density, keepRatio, prng);

                    for (int i = 0; i < targetCount; i++)
                    {
                        if (created >= MaxDetailObjectsPerTerrain)
                        {
                            wasCapped = true;
                            return created;
                        }

                        float px = terrain.transform.position.x + (x + (float)prng.NextDouble()) * cellSizeX;
                        float pz = terrain.transform.position.z + (y + (float)prng.NextDouble()) * cellSizeZ;
                        float py = terrain.SampleHeight(new Vector3(px, 0f, pz)) + terrain.transform.position.y;

                        Quaternion rot = Quaternion.Euler(0f, RandomRange(prng, 0f, 360f), 0f);

                        GameObject instance = PrefabUtility.InstantiatePrefab(detailPrefab, terrain.gameObject.scene) as GameObject;
                        if (instance == null)
                        {
                            continue;
                        }

                        Undo.RegisterCreatedObjectUndo(instance, "Create Detail Instance");
                        instance.transform.SetPositionAndRotation(new Vector3(px, py, pz), rot);

                        float width = RandomRange(prng, prototype.minWidth, prototype.maxWidth);
                        float height = RandomRange(prng, prototype.minHeight, prototype.maxHeight);
                        instance.transform.localScale = new Vector3(width, height, width);

                        instance.transform.SetParent(root, true);
                        created++;
                    }
                }
            }
        }

        return created;
    }

    private static int GetDetailDensity(int[,] map, int x, int y)
    {
        int firstDim = map.GetLength(0);
        int secondDim = map.GetLength(1);

        if (x < firstDim && y < secondDim)
        {
            return map[x, y];
        }

        if (y < firstDim && x < secondDim)
        {
            return map[y, x];
        }

        return 0;
    }

    private static int GetScaledCount(int originalDensity, float keepRatio, System.Random prng)
    {
        if (originalDensity <= 0)
        {
            return 0;
        }

        if (keepRatio >= 1f)
        {
            return originalDensity;
        }

        float scaled = originalDensity * keepRatio;
        int count = Mathf.FloorToInt(scaled);
        float remainder = scaled - count;

        if (remainder > 0f && prng.NextDouble() < remainder)
        {
            count++;
        }

        return count;
    }

    private static float RandomRange(System.Random prng, float min, float max)
    {
        if (max <= min)
        {
            return min;
        }

        return min + (float)prng.NextDouble() * (max - min);
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
