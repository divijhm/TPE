// v3 — writes directly to .mat YAML bytes; bypasses Unity API caching entirely
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// Patches neon sign .mat files on disk:
///   _BaseColor  → 8% of original hue  (very dark backing)
///   _EmissionColor → original hue normalized to peak=3.5 (glowing HDR)
///   m_ValidKeywords includes _EMISSION
///   m_LightmapFlags: 1  (RealtimeEmissive)
public class NeonDirectFix : MonoBehaviour
{
    static readonly string[] SKIP = {
        "Concrete","concrete","brick","rubber","Rubber","Glass","glass",
        "Metal","metal","floor","nbuild","wall","pavement","asphalt",
        "chrome","steel","Steel","plaster","road","DefaultMaterial",
        "Default","tire","Tire","cable","Cable","PBR"
    };

    [MenuItem("Tools/Neon Direct Fix (Disk Write)")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material",
            new[] { "Assets/Materials", "Assets/blndrmaterials" });

        int patched = 0, skipped = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string absPath   = Path.GetFullPath(assetPath);
            string matName   = Path.GetFileNameWithoutExtension(assetPath);

            bool skip = false;
            foreach (var s in SKIP)
                if (matName.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                { skip = true; break; }
            if (skip) { skipped++; continue; }

            // Read original base colour from the loaded asset
            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null) { skipped++; continue; }
            Color orig = mat.GetColor("_BaseColor");
            if (orig.r + orig.g + orig.b < 0.03f) { skipped++; continue; }

            // Dark base: 8% of original
            Color dark = new Color(
                Mathf.Clamp01(orig.r * 0.08f),
                Mathf.Clamp01(orig.g * 0.08f),
                Mathf.Clamp01(orig.b * 0.08f), 1f);

            // HDR emission: peak channel → 3.5, preserve hue ratios
            float peak = Mathf.Max(orig.r, orig.g, orig.b, 0.001f);
            float scl  = 3.5f / peak;
            Color emit = new Color(orig.r * scl, orig.g * scl, orig.b * scl, 1f);

            string text = File.ReadAllText(absPath);

            // 1. Ensure _EMISSION in m_ValidKeywords (add if missing)
            if (!Regex.IsMatch(text, @"m_ValidKeywords:[\s\S]*?- _EMISSION"))
            {
                text = Regex.Replace(text,
                    @"(m_ValidKeywords:\r?\n)",
                    "$1  - _EMISSION\n");
            }
            // 2. Remove _EMISSION from m_InvalidKeywords if it ended up there
            text = Regex.Replace(text,
                @"(m_InvalidKeywords:(?:\r?\n  - _EMISSION)*\r?\n)(  - _EMISSION\r?\n)?",
                m => {
                    string block = m.Value;
                    block = Regex.Replace(block, @"  - _EMISSION\r?\n", "");
                    return block;
                });

            // 3. Replace colour values directly in the YAML
            SetColor(ref text, "_BaseColor",     dark);
            SetColor(ref text, "_Color",         dark);
            SetColor(ref text, "_EmissionColor", emit);

            // 4. RealtimeEmissive flag
            text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 1");

            File.WriteAllText(absPath, text);
            patched++;
            Debug.Log($"[NeonDirect] {matName}  emit({emit.r:F1},{emit.g:F1},{emit.b:F1})  base({dark.r:F3},{dark.g:F3},{dark.b:F3})");
        }

        // Force full reimport so the scene renderer picks up disk changes
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log($"[NeonDirect] ✓ Patched {patched}, skipped {skipped}.");
    }

    // Replaces: - _Prop: {r: X, g: X, b: X, a: X}
    static void SetColor(ref string text, string prop, Color c)
    {
        // Build replacement using explicit ToString to avoid any format-specifier escaping issues
        string rs = c.r.ToString("0.######");
        string gs = c.g.ToString("0.######");
        string bs = c.b.ToString("0.######");
        string pat = $@"- {Regex.Escape(prop)}:\s*{{r:[^}}]+}}";
        string rep = "- " + prop + ": {r: " + rs + ", g: " + gs + ", b: " + bs + ", a: 1}";
        text = Regex.Replace(text, pat, rep);
    }
}
