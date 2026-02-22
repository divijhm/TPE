using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// Two-pass fix:
///   Pass 1 — structural materials: recover original base colour (was darkened to 8%),
///             remove emission entirely.
///   Pass 2 — sign/neon materials: dark base (8%) + HDR emission derived from original hue.
///   Pass 3 — building-light materials: recovered base + very subtle warm emission.
public class SceneMaterialReset : MonoBehaviour
{
    // ── Materials that must have NEON emission ─────────────────────────────
    static readonly HashSet<string> NEON_SIGNS = new HashSet<string>(
        System.StringComparer.OrdinalIgnoreCase) {
        "blue","green","orange","pink","purple","red","violet",
        "white","White","yellow","Emis","Material",
        "Material.001","Material.002","Material.003","Material.004","Material.005",
        "Material.006","Material.007","Material.008","Material.010","Material.011",
        "Material.012","Material.013","Material.014","Material.015","Material.016",
        "Material.018","Material.019","Material.020","Material.021","Material.022",
        "Material.023","Material.024","Material.025","Material.027","Material.028",
        "Material.029","Material.030","Material.031","Material.032","Material.033",
        "Material.034","Material.035","Material.036","Material.037","Material.038",
        "Material.039","Material.040","Material.041","Material.042","Material.043",
        "Material.044","Material.045",
    };

    // ── Materials that should have a subtle warm window glow ───────────────
    static readonly HashSet<string> WARM_GLOW = new HashSet<string>(
        System.StringComparer.OrdinalIgnoreCase) {
        "AlleyBuildingLight","AlleyWindow","WF_BuildingLight","windows",
        "Drone Lights","Drone Rotor Light Diffusers","red light","white light",
    };

    [MenuItem("Tools/Scene Material Reset (Fix Broken Scene)")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material",
            new[] { "Assets/Materials", "Assets/blndrmaterials" });

        // Track by mat name so we don't process duplicates across both folders
        var processed = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        int neon = 0, structural = 0, warm = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string absPath   = Path.GetFullPath(assetPath);
            string matName   = Path.GetFileNameWithoutExtension(assetPath);

            string text = File.ReadAllText(absPath);

            // ── Recover original base colour from the darkened value ──────────
            // NeonDirectFix wrote:  _BaseColor = orig * 0.08
            // So:  orig = _BaseColor_current / 0.08
            Color currentBase = ReadColor(text, "_BaseColor");
            Color origBase = new Color(
                currentBase.r / 0.08f,
                currentBase.g / 0.08f,
                currentBase.b / 0.08f, 1f);

            // Clamp to [0,1] — most structural materials will be <1 original
            origBase.r = Mathf.Clamp(origBase.r, 0f, 1f);
            origBase.g = Mathf.Clamp(origBase.g, 0f, 1f);
            origBase.b = Mathf.Clamp(origBase.b, 0f, 1f);

            // Guard: if we can't recover a sensible original, use neutral dark grey
            if (origBase.r + origBase.g + origBase.b < 0.05f)
                origBase = new Color(0.25f, 0.25f, 0.28f, 1f);

            bool wrote = false;

            // ════════════════════════════════════════════════════════════════
            if (NEON_SIGNS.Contains(matName))
            {
                // ── NEON: dark backing + HDR emission ──────────────────────
                Color dark = new Color(
                    Mathf.Clamp01(origBase.r * 0.08f),
                    Mathf.Clamp01(origBase.g * 0.08f),
                    Mathf.Clamp01(origBase.b * 0.08f), 1f);

                float peak = Mathf.Max(origBase.r, origBase.g, origBase.b, 0.001f);
                Color emit = new Color(origBase.r * (3.5f / peak),
                                       origBase.g * (3.5f / peak),
                                       origBase.b * (3.5f / peak), 1f);

                // Ensure _EMISSION keyword present
                if (!Regex.IsMatch(text, @"m_ValidKeywords:[\s\S]*?- _EMISSION"))
                    text = Regex.Replace(text, @"(m_ValidKeywords:\r?\n)", "$1  - _EMISSION\n");

                SetColor(ref text, "_BaseColor",     dark);
                SetColor(ref text, "_Color",         dark);
                SetColor(ref text, "_EmissionColor", emit);
                text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 1");
                neon++; wrote = true;
            }
            // ════════════════════════════════════════════════════════════════
            else if (WARM_GLOW.Contains(matName))
            {
                // ── WARM GLOW: recovered base + subtle warm emission ───────
                // Warm emission = very faint (0.25 HDR) warm white
                Color warmEmit = new Color(0.25f, 0.20f, 0.10f, 1f);

                // Ensure _EMISSION keyword present
                if (!Regex.IsMatch(text, @"m_ValidKeywords:[\s\S]*?- _EMISSION"))
                    text = Regex.Replace(text, @"(m_ValidKeywords:\r?\n)", "$1  - _EMISSION\n");

                SetColor(ref text, "_BaseColor",     origBase);
                SetColor(ref text, "_Color",         origBase);
                SetColor(ref text, "_EmissionColor", warmEmit);
                text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 1");
                warm++; wrote = true;
            }
            // ════════════════════════════════════════════════════════════════
            else
            {
                // ── STRUCTURAL: recover original base, NO emission ─────────
                // Remove _EMISSION from ValidKeywords
                text = Regex.Replace(text, @"  - _EMISSION\r?\n", "");
                // Add to InvalidKeywords if not there
                if (!Regex.IsMatch(text, @"m_InvalidKeywords:[\s\S]*?- _EMISSION"))
                    text = Regex.Replace(text, @"(m_InvalidKeywords:\r?\n)", "$1  - _EMISSION\n");

                SetColor(ref text, "_BaseColor",     origBase);
                SetColor(ref text, "_Color",         origBase);
                SetColor(ref text, "_EmissionColor", Color.black);
                // LightmapFlags: 4 = EmissiveIsBlack (no GI contribution)
                text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 4");
                structural++; wrote = true;
            }

            if (wrote) File.WriteAllText(absPath, text);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log($"[SceneReset] ✓  Neon signs: {neon}  |  Structural: {structural}  |  Warm glow: {warm}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static Color ReadColor(string text, string prop)
    {
        var m = Regex.Match(text,
            $@"- {Regex.Escape(prop)}:\s*{{r:\s*([0-9Ee.+\-]+),\s*g:\s*([0-9Ee.+\-]+),\s*b:\s*([0-9Ee.+\-]+)");
        if (!m.Success) return Color.black;
        float.TryParse(m.Groups[1].Value, out float r);
        float.TryParse(m.Groups[2].Value, out float g);
        float.TryParse(m.Groups[3].Value, out float b);
        return new Color(r, g, b, 1f);
    }

    static void SetColor(ref string text, string prop, Color c)
    {
        string rs = c.r.ToString("0.######");
        string gs = c.g.ToString("0.######");
        string bs = c.b.ToString("0.######");
        string pat = $@"- {Regex.Escape(prop)}:\s*{{r:[^}}]+}}";
        string rep = "- " + prop + ": {r: " + rs + ", g: " + gs + ", b: " + bs + ", a: 1}";
        text = Regex.Replace(text, pat, rep);
    }
}
