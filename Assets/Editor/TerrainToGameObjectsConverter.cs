using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TerrainToGameObjectsConverter
{
    private const int MaxDetailObjectsPerTerrain = 23000;
    private const float RockDetailDensityMultiplier = 0.2f;
    private const float WaterSurfacePadding = 0.05f;

    private static readonly string[] RockKeywords =
    {
        "rock",
        "stone",
        "pebble",
        "boulder"
    };

    private static readonly string[] TreeKeywords =
    {
        "pine_tree",
        "fruit_tree",
        "generic_shrub",
        "tree",
        "shrub",
        "stump",
        "logs",
        "pine",
        "oak",
        "fir",
        "spruce"
    };

    private struct WaterSurface
    {
        public Bounds Bounds;
        public float SurfaceY;
    }

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
            message += "\n\nDetail conversion hit the non-tree per-terrain cap on " + cappedTerrains + " terrain(s). Non-tree details were distributed across each terrain to preserve overall coverage, while tree-like detail prefabs were still fully extracted.";
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
        List<WaterSurface> waterSurfaces = GatherWaterSurfaces(terrain.gameObject.scene);

        int totalRequestedNonTree = 0;
        for (int layer = 0; layer < detailPrototypes.Length; layer++)
        {
            DetailPrototype prototype = detailPrototypes[layer];
            GameObject detailPrefab = prototype.prototype;
            if (detailPrefab == null)
            {
                continue;
            }

            bool isTreeLayer = IsTreeLikeDetailPrefab(detailPrefab);

            int[,] map = data.GetDetailLayer(0, 0, detailWidth, detailHeight, layer);
            for (int y = 0; y < detailHeight; y++)
            {
                for (int x = 0; x < detailWidth; x++)
                {
                    if (!isTreeLayer)
                    {
                        totalRequestedNonTree += map[y, x];
                    }
                }
            }
        }

        float keepRatio = 1f;
        if (totalRequestedNonTree > MaxDetailObjectsPerTerrain)
        {
            keepRatio = (float)MaxDetailObjectsPerTerrain / totalRequestedNonTree;
            wasCapped = true;
            Debug.LogWarning(
                "Detail conversion requested " + totalRequestedNonTree +
                " objects on terrain " + terrain.name +
                ", exceeding cap " + MaxDetailObjectsPerTerrain +
                ". Applying distributed downsampling to non-tree detail prefabs (ratio=" + keepRatio.ToString("F3") + ").");
        }

        System.Random prng = new System.Random(terrain.GetInstanceID());
        int created = 0;
        int createdNonTree = 0;

        for (int layer = 0; layer < detailPrototypes.Length; layer++)
        {
            DetailPrototype prototype = detailPrototypes[layer];
            GameObject detailPrefab = prototype.prototype;

            if (detailPrefab == null)
            {
                skippedLayers++;
                continue;
            }

            bool isTreeLayer = IsTreeLikeDetailPrefab(detailPrefab);

            int[,] map = data.GetDetailLayer(0, 0, detailWidth, detailHeight, layer);

            for (int y = 0; y < detailHeight; y++)
            {
                for (int x = 0; x < detailWidth; x++)
                {
                    if (!isTreeLayer && createdNonTree >= MaxDetailObjectsPerTerrain)
                    {
                        wasCapped = true;
                        continue;
                    }

                    int density = map[y, x];
                    float layerRatio = isTreeLayer ? 1f : keepRatio * GetPrototypeDensityMultiplier(detailPrefab);
                    int targetCount = GetScaledCount(density, layerRatio, prng);

                    for (int i = 0; i < targetCount; i++)
                    {
                        if (!isTreeLayer && createdNonTree >= MaxDetailObjectsPerTerrain)
                        {
                            wasCapped = true;
                            break;
                        }

                        float px = terrain.transform.position.x + (x + (float)prng.NextDouble()) * cellSizeX;
                        float pz = terrain.transform.position.z + (y + (float)prng.NextDouble()) * cellSizeZ;
                        float py = terrain.SampleHeight(new Vector3(px, 0f, pz)) + terrain.transform.position.y;

                        if (IsInsideWaterSurface(px, py, pz, waterSurfaces))
                        {
                            continue;
                        }

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
                        if (!isTreeLayer)
                        {
                            createdNonTree++;
                        }
                    }
                }
            }
        }

        return created;
    }

    private static float GetPrototypeDensityMultiplier(GameObject detailPrefab)
    {
        string prefabName = detailPrefab != null ? detailPrefab.name : string.Empty;
        for (int i = 0; i < RockKeywords.Length; i++)
        {
            if (prefabName.IndexOf(RockKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RockDetailDensityMultiplier;
            }
        }

        return 1f;
    }

    private static bool IsTreeLikeDetailPrefab(GameObject detailPrefab)
    {
        string prefabName = detailPrefab != null ? detailPrefab.name : string.Empty;
        for (int i = 0; i < TreeKeywords.Length; i++)
        {
            if (prefabName.IndexOf(TreeKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
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

    private static List<WaterSurface> GatherWaterSurfaces(UnityEngine.SceneManagement.Scene scene)
    {
        var waters = new List<WaterSurface>();

        GameObject[] sceneObjects = scene.GetRootGameObjects();
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            Renderer[] renderers = sceneObjects[i].GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer renderer = renderers[r];
                if (renderer == null)
                {
                    continue;
                }

                GameObject go = renderer.gameObject;
                bool taggedWater = go.CompareTag("Water");
                bool nameLooksLikeWater = go.name.IndexOf("water", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!taggedWater && !nameLooksLikeWater)
                {
                    continue;
                }

                Bounds b = renderer.bounds;
                waters.Add(new WaterSurface
                {
                    Bounds = b,
                    SurfaceY = b.max.y
                });
            }
        }

        return waters;
    }

    private static bool IsInsideWaterSurface(float x, float y, float z, List<WaterSurface> waters)
    {
        for (int i = 0; i < waters.Count; i++)
        {
            WaterSurface water = waters[i];
            if (x < water.Bounds.min.x || x > water.Bounds.max.x)
            {
                continue;
            }

            if (z < water.Bounds.min.z || z > water.Bounds.max.z)
            {
                continue;
            }

            if (y <= water.SurfaceY + WaterSurfacePadding)
            {
                return true;
            }
        }

        return false;
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
