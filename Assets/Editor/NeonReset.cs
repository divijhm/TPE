using UnityEngine;
using UnityEditor;

/// Resets all neon sign materials to:
///   - Very dark base color (almost black with a faint hue)
///   - Moderate HDR emission (glow without blinding the whole scene)
public class NeonReset : MonoBehaviour
{
    [MenuItem("Tools/Neon Reset (Dark base + Glow)")]
    public static void Run()
    {
        // Moderate HDR neon values: visible glow without overpowering the scene.
        // Components are in linear HDR space; 1.5-2.0 gives a nice bloom halo in URP.
        Color[] neons = {
            N(2f,   0.02f, 0.6f),   // hot pink
            N(0.02f,0.5f,  2f),    // electric blue
            N(0.3f, 2f,   0.1f),   // toxic green
            N(2f,   1.2f,  0f),    // golden yellow
            N(1.0f, 0f,    2f),    // deep violet
            N(0f,   1.8f,  1.5f),  // cyan-teal
            N(2f,   0.4f,  0f),    // orange
            N(2f,   0f,    0.1f),  // vivid red
            N(0f,   1.0f,  2f),    // sky blue
            N(2f,   0f,    2f),    // magenta
            N(0.2f, 2f,    0.5f),  // mint green
            N(2f,   0.8f,  0f),    // amber
            N(0.5f, 0f,    2f),    // purple
            N(0f,   2f,    1f),    // spring green
            N(2f,   0f,    1f),    // rose
            N(1f,   2f,    0f),    // yellow-green
        };

        // ── Targets: Text objects + named signs ──────────────────────
        string[] textNames = {
            "Text", "Text.001","Text.002","Text.005","Text.006",
            "Text.007","Text.008","Text.009","Text.010","Text.011"
        };
        string[] signNames = {
            "word with star","word long","sushi","noodle bowl",
            "vertical japanese sign 1","vertical japanese sign 2",
            "woman in cirlce","Japanese cirlce 1","OPEN 24/7",
            "japanese word 3","japanese word 2","japanese word",
            "cat","cat 2","Circle.001","Circle.002","Circle.003","Circle.004"
        };

        string[] allTargets = new string[textNames.Length + signNames.Length];
        textNames.CopyTo(allTargets, 0);
        signNames.CopyTo(allTargets, textNames.Length);

        int colIdx = 0;

        foreach (string name in allTargets)
        {
            foreach (var go in GameObject.FindObjectsOfType<GameObject>())
            {
                if (go.name != name) continue;
                foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mats = mr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == null) continue;
                        ApplyNeon(mats[i], neons[colIdx % neons.Length]);
                        colIdx++;
                    }
                    EditorUtility.SetDirty(mr);
                }
            }
        }

        // ── Also reset Material.xxx billboard panels ─────────────────
        foreach (var mr in GameObject.FindObjectsOfType<MeshRenderer>())
        {
            foreach (var mat in mr.sharedMaterials)
            {
                if (mat == null) continue;
                if (!mat.name.StartsWith("Material.") && mat.name != "Material") continue;
                ApplyNeon(mat, neons[colIdx % neons.Length]);
                colIdx++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[NeonReset] ✓ Reset " + colIdx + " neon material slots (dark base + glow).");
    }

    static void ApplyNeon(Material mat, Color emit)
    {
        // Base color: extract hue but keep it very dark — real neon tube backing
        Color dark = new Color(
            Mathf.Clamp01(emit.r * 0.06f),
            Mathf.Clamp01(emit.g * 0.06f),
            Mathf.Clamp01(emit.b * 0.06f),
            1f);

        mat.SetColor("_BaseColor", dark);
        mat.SetColor("_Color",     dark);

        // Emission: use Color multiplication so HDR values above 1 are preserved
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emit); // direct assignment preserves HDR
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        mat.SetFloat("_Smoothness", 0.75f);   // slight reflectivity for wet/glossy neon
        mat.SetFloat("_Metallic",   0f);

        EditorUtility.SetDirty(mat);
    }

    // Helper: creates HDR color directly without clamping
    static Color N(float r, float g, float b) => new Color(r, g, b, 1f);
}
