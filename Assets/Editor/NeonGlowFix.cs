using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// Finds every neon sign material in the scene, logs before/after, and applies:
///   - Near-black base (faint hue tint)
///   - High HDR emission (4.0 range) so glow is clearly visible
public class NeonGlowFix : MonoBehaviour
{
    // HDR emission palette: vivid, saturated, range 3.5-5.0
    static Color[] neons = {
        C(4f,   0.04f, 1.2f),  // hot pink
        C(0.04f,1.0f,  4f),   // electric blue
        C(0.6f, 4f,   0.2f),  // toxic green
        C(4f,   2.4f,  0f),   // golden yellow
        C(2f,   0f,    4f),   // deep violet
        C(0f,   3.5f,  3f),   // cyan-teal
        C(4f,   0.8f,  0f),   // orange
        C(4f,   0f,    0.2f), // vivid red
        C(0f,   2f,    4f),   // sky blue
        C(4f,   0f,    4f),   // magenta
        C(0.4f, 4f,    1f),   // mint green
        C(4f,   1.6f,  0f),   // amber
        C(1f,   0f,    4f),   // purple
        C(0f,   4f,    2f),   // spring green
        C(4f,   0f,    2f),   // rose
        C(2f,   4f,    0f),   // yellow-green
    };

    [MenuItem("Tools/Neon Glow Fix")]
    public static void Run()
    {
        // Collect every unique material on neon sign objects (by name pattern or sign object name)
        var seen   = new HashSet<int>();
        var fixed1 = new List<string>();
        int colIdx = 0;

        // 1. Walk ALL MeshRenderers in scene
        foreach (var mr in GameObject.FindObjectsOfType<MeshRenderer>())
        {
            if (mr == null) continue;

            bool isSign = false;
            string goName = mr.gameObject.name;

            // Text objects
            if (goName == "Text" || goName.StartsWith("Text.")) isSign = true;
            // Named sign objects
            if (!isSign)
            {
                foreach (var s in new[]{
                    "word with star","word long","sushi","noodle bowl",
                    "vertical japanese sign 1","vertical japanese sign 2",
                    "woman in cirlce","Japanese cirlce 1","OPEN 24/7",
                    "japanese word 3","japanese word 2","japanese word",
                    "cat","cat 2","Circle.001","Circle.002","Circle.003","Circle.004"
                }) { if (goName == s) { isSign = true; break; } }
            }
            // Cube + Material.xxx billboard panels
            if (!isSign && goName.StartsWith("Cube."))
            {
                foreach (var mat in mr.sharedMaterials)
                {
                    if (mat != null && (mat.name.StartsWith("Material.") || mat.name == "Material"))
                    { isSign = true; break; }
                }
            }

            if (!isSign) continue;

            var mats = mr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null) continue;

                // Skip structural materials
                string mn = mat.name;
                if (mn.Contains("Concrete") || mn.Contains("Brick") ||
                    mn.Contains("rubber")   || mn.Contains("Glass") ||
                    mn.Contains("Metal")    || mn.Contains("Floor")) continue;

                int id = mat.GetInstanceID();
                if (seen.Contains(id)) { colIdx++; continue; } // use next colour but skip duplicate
                seen.Add(id);

                Color emitBefore = mat.GetColor("_EmissionColor");
                Color emit = neons[colIdx % neons.Length];

                // Near-black base: faint hue so surface isn't pure void
                Color dark = new Color(
                    Mathf.Clamp01(emit.r * 0.10f),
                    Mathf.Clamp01(emit.g * 0.10f),
                    Mathf.Clamp01(emit.b * 0.10f), 1f);

                mat.SetColor("_BaseColor", dark);
                mat.SetColor("_Color",     dark);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emit);   // Color multiplication keeps HDR >1
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetFloat("_Smoothness", 0.7f);
                mat.SetFloat("_Metallic",   0f);

                Color emitAfter = mat.GetColor("_EmissionColor");
                fixed1.Add($"  {mn}: before=({emitBefore.r:F2},{emitBefore.g:F2},{emitBefore.b:F2}) after=({emitAfter.r:F2},{emitAfter.g:F2},{emitAfter.b:F2})");

                EditorUtility.SetDirty(mat);
                colIdx++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Log every change so you can verify in Console
        foreach (var line in fixed1) Debug.Log(line);
        Debug.Log($"[NeonGlow] ✓ Fixed {fixed1.Count} unique materials (colIdx={colIdx}).");
    }

    static Color C(float r, float g, float b) => new Color(r, g, b, 1f);
}
