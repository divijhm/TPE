using UnityEngine;
using UnityEditor;

public class NeonSignsFix : MonoBehaviour
{
    [MenuItem("Tools/Neon Signs & Text Fix")]
    public static void Fix()
    {
        // Neon palette — saturated HDR colours that bloom hard
        Color[] neons = {
            E(6f,  0.05f,0.4f),  // hot pink
            E(0f,  0.5f, 6f),    // electric blue
            E(0.4f,6f,  0.2f),   // toxic green
            E(6f,  3.5f,0f),     // golden yellow
            E(3f,  0f,  6f),     // deep violet
            E(0f,  5f,  4f),     // cyan-teal
            E(6f,  0.5f,0f),     // orange
            E(6f,  0f,  0.2f),   // vivid red
            E(0f,  3f,  6f),     // sky blue
            E(5f,  0f,  5f),     // magenta
            E(0.5f,6f,  1f),     // mint green
            E(6f,  2f,  0f),     // amber
            E(1f,  0f,  6f),     // purple
            E(0f,  6f,  3f),     // spring green
            E(6f,  0f,  3f),     // rose
            E(3f,  6f,  0f),     // yellow-green
        };

        // ── All Text.xxx objects ──────────────────────────────────────
        string[] textNames = {
            "Text", "Text.001","Text.002","Text.005","Text.006",
            "Text.007","Text.008","Text.009","Text.010","Text.011"
        };

        // ── Named sign objects ─────────────────────────────────────
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
            // FindObjectsOfType to get all matches (some names may be duplicated)
            foreach (var go in GameObject.FindObjectsOfType<GameObject>())
            {
                if (go.name != name) continue;
                var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var mr in renderers)
                {
                    // Create a new material instance so we don't share
                    Material[] mats = mr.sharedMaterials;
                    bool dirty = false;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == null) continue;
                        Color emitCol = neons[colIdx % neons.Length];
                        Color baseCol = new Color(
                            Mathf.Clamp01(emitCol.r * 0.25f),
                            Mathf.Clamp01(emitCol.g * 0.25f),
                            Mathf.Clamp01(emitCol.b * 0.25f), 1f);

                        mats[i].SetColor("_BaseColor", baseCol);
                        mats[i].SetColor("_Color",     baseCol);
                        mats[i].EnableKeyword("_EMISSION");
                        mats[i].SetColor("_EmissionColor", emitCol);
                        mats[i].globalIlluminationFlags =
                            MaterialGlobalIlluminationFlags.RealtimeEmissive;
                        mats[i].SetFloat("_Smoothness", 0.7f);
                        EditorUtility.SetDirty(mats[i]);
                        colIdx++;
                        dirty = true;
                    }
                    if (dirty) mr.sharedMaterials = mats;
                    EditorUtility.SetDirty(mr);
                }
            }
        }

        // ── Also directly hit all Billboard Cube.xxx sign panels ───
        // From diagnostic: Material.001-045 are on Cube.xxx sign panels — already set
        // but also directly set via renderer so it's live in scene
        foreach (var mr in GameObject.FindObjectsOfType<MeshRenderer>())
        {
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
            {
                var mat = mr.sharedMaterials[i];
                if (mat == null) continue;
                string mn = mat.name;
                // Only re-confirm sign/billboard materials already set, skip structural
                if (!mn.StartsWith("Material.") && mn != "Material") continue;
                // Check it already has emission set by V3 — if not, set a neon
                if (mat.GetColor("_EmissionColor") == Color.black ||
                    mat.GetColor("_EmissionColor") == new Color(0,0,0,0))
                {
                    Color emit = neons[colIdx % neons.Length];
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emit);
                    mat.globalIlluminationFlags =
                        MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    EditorUtility.SetDirty(mat);
                    colIdx++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Neon] ✓ Applied neon to " + colIdx + " material slots.");
    }

    static Color E(float r, float g, float b) => new Color(r, g, b, 1f);
}
