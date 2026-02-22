using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// Definitive fix — applies values BOTH via Unity API (in-memory) AND disk YAML write.
/// Structural materials get correct dark cyberpunk colors.
/// Sign/neon materials get dark backing + HDR emission.
public class DefinitiveMaterialFix : MonoBehaviour
{
    // ── Sign materials that get NEON emission ──────────────────────────────
    // Each entry: matName => HDR emission Color
    static readonly Dictionary<string, Color> NEON_EMIT = new Dictionary<string, Color>(
        System.StringComparer.OrdinalIgnoreCase) {
        // Named colour materials (drive emission from their colour identity)
        {"blue",        E(0f,   0.8f, 4f)},
        {"green",       E(0.4f, 4f,  0.2f)},
        {"orange",      E(4f,   1.5f, 0f)},
        {"pink",        E(4f,   0.1f, 1.5f)},
        {"purple",      E(1.5f, 0f,  4f)},
        {"red",         E(4f,   0f,  0.1f)},
        {"violet",      E(2f,   0f,  4f)},
        {"white",       E(3f,   3f,  3.5f)},
        {"yellow",      E(4f,   3f,  0f)},
        {"Emis",        E(3f,   3.5f,4f)},
    };

    // All Material.xxx get cycling neon palette
    static readonly Color[] CYCLE = {
        E(4f,  0.1f,1.5f), E(0f,  0.8f,4f),   E(0.3f,4f, 0.1f),
        E(4f,  3f,  0f),   E(1.5f,0f,  4f),   E(0f,  3.5f,3f),
        E(4f,  0.8f,0f),   E(4f,  0f,  0.1f), E(0f,  2f,  4f),
        E(4f,  0f,  4f),   E(0.3f,4f,  1f),   E(4f,  1.5f,0f),
        E(1f,  0f,  4f),   E(0f,  4f,  2f),   E(4f,  0f,  2f),
        E(2f,  4f,  0f),
    };

    // ── Structural: exact dark base colours, zero emission ─────────────────
    static readonly Dictionary<string, Color> STRUCTURAL_BASE = new Dictionary<string, Color>(
        System.StringComparer.OrdinalIgnoreCase) {
        {"AlleyBuilding",       RGB(0.12f,0.13f,0.16f)},
        {"AlleyBuildingLight",  RGB(0.20f,0.18f,0.12f)},
        {"AlleyCrate",          RGB(0.18f,0.16f,0.12f)},
        {"AlleyStreet",         RGB(0.10f,0.10f,0.11f)},
        {"AlleyWindow",         RGB(0.05f,0.08f,0.15f)},
        {"WF_Building",         RGB(0.10f,0.11f,0.14f)},
        {"WF_BuildingLight",    RGB(0.18f,0.16f,0.10f)},
        {"WF_Crate",            RGB(0.17f,0.15f,0.11f)},
        {"WF_Street",           RGB(0.09f,0.09f,0.10f)},
        {"nightbuidlling",      RGB(0.08f,0.09f,0.13f)},
        {"skyscrapa",           RGB(0.10f,0.11f,0.15f)},
        {"graybrick",           RGB(0.20f,0.20f,0.22f)},
        {"White Brick",         RGB(0.22f,0.22f,0.24f)},
        {"Concrete.001",        RGB(0.18f,0.18f,0.19f)},
        {"Concrete.002",        RGB(0.17f,0.17f,0.18f)},
        {"concrete_22_color",   RGB(0.22f,0.22f,0.24f)},
        {"_012",                RGB(0.18f,0.17f,0.16f)},
        {"DefaultMaterial_Base_Color", RGB(0.15f,0.15f,0.17f)},
        {"01___Default_1001_baseColor", RGB(0.14f,0.14f,0.16f)},
        {"Mich_CC_18_baseColor",RGB(0.12f,0.12f,0.14f)},
        {"Anti-slip plate - square.001", RGB(0.18f,0.17f,0.15f)},
        {"Black plastic old scratched.001", RGB(0.06f,0.06f,0.07f)},
        {"Black plastic PL.001",RGB(0.06f,0.06f,0.07f)},
        {"Black Plastic.001",   RGB(0.06f,0.06f,0.07f)},
        {"Black",               RGB(0.05f,0.05f,0.06f)},
        {"Matte Black",         RGB(0.05f,0.05f,0.06f)},
        {"Dark iron",           RGB(0.08f,0.08f,0.09f)},
        {"Chrome",              RGB(0.40f,0.40f,0.42f)},
        {"Glass.001",           RGB(0.05f,0.08f,0.15f)},
        {"dark glass",          RGB(0.04f,0.05f,0.10f)},
        {"Rust 3.001",          RGB(0.20f,0.10f,0.06f)},
        {"trims",               RGB(0.12f,0.13f,0.14f)},
        {"metal_diffuse",       RGB(0.12f,0.12f,0.13f)},
        {"Procedral Red Silk",  RGB(0.25f,0.04f,0.04f)},
        {"Procedural rubber latex", RGB(0.06f,0.06f,0.06f)},
        {"tires",               RGB(0.08f,0.08f,0.08f)},
        {"TexturesCom_Plastic_CarbonFiber_1K_albedo", RGB(0.07f,0.07f,0.07f)},
        {"Orange",              RGB(0.18f,0.09f,0.02f)},  // structural orange (not sign)
        // Drones
        {"Drone Blades Carbon Fibre_Albedo", RGB(0.07f,0.07f,0.07f)},
        {"Drone Body Metal Paint",  RGB(0.10f,0.10f,0.12f)},
        {"Drone Body Plastic_Black",RGB(0.05f,0.05f,0.06f)},
        {"Drone Body_Bottom",       RGB(0.08f,0.08f,0.09f)},
        {"Drone Camera  Glass_Middle", RGB(0.04f,0.07f,0.12f)},
        {"Drone Camera Glass_Inner",   RGB(0.03f,0.06f,0.10f)},
        {"Drone Camera Glass_Outer",   RGB(0.05f,0.08f,0.14f)},
        {"Drone Camera Plastic_Grey",  RGB(0.18f,0.18f,0.19f)},
        {"Drone Logo_Metal",    RGB(0.20f,0.20f,0.22f)},
        {"Drone Metal Black",   RGB(0.06f,0.06f,0.07f)},
        {"Drone Metal Shiny",   RGB(0.30f,0.30f,0.32f)},
        {"Drone Rubber",        RGB(0.07f,0.07f,0.07f)},
    };

    // ── Window/light: warm subtle emission ─────────────────────────────────
    static readonly HashSet<string> WARM_GLOW = new HashSet<string>(
        System.StringComparer.OrdinalIgnoreCase) {
        "windows","AlleyWindow","AlleyBuildingLight","WF_BuildingLight",
        "Drone Lights","Drone Rotor Light Diffusers","red light","white light","Emis",
    };

    // ──────────────────────────────────────────────────────────────────────

    [MenuItem("Tools/Definitive Material Fix")]
    public static void Run()
    {
        // Build Material.xxx emission map
        int cycle = 0;
        var matNumEmit = new Dictionary<string, Color>(System.StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i <= 50; i++)
        {
            foreach (var suffix in new[]{"","." + i.ToString()})
            {
                string k = "Material" + (suffix == "" ? "" : suffix);
                // we just cycle through all Material.xxx
            }
        }
        // Actually just fill lazily per material name below

        string[] guids = AssetDatabase.FindAssets("t:Material",
            new[] { "Assets/Materials", "Assets/blndrmaterials" });

        int neon = 0, structural = 0, warm = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string absPath   = Path.GetFullPath(assetPath);
            string matName   = Path.GetFileNameWithoutExtension(assetPath);

            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null) continue;

            string text = File.ReadAllText(absPath);
            bool isNeon = false;

            // ── Determine emission colour ──────────────────────────────────
            Color emitCol = Color.black;

            if (NEON_EMIT.TryGetValue(matName, out Color namedEmit))
            {
                emitCol = namedEmit;
                isNeon  = true;
            }
            else if (matName == "Material" ||
                     Regex.IsMatch(matName, @"^Material\.\d+$"))
            {
                emitCol = CYCLE[cycle % CYCLE.Length];
                cycle++;
                isNeon = true;
            }

            // ── Apply ──────────────────────────────────────────────────────
            if (isNeon)
            {
                // Dark backing derived from emission hue
                float pk = Mathf.Max(emitCol.r, emitCol.g, emitCol.b, 0.001f);
                Color dark = new Color(
                    Mathf.Clamp01((emitCol.r / pk) * 0.06f),
                    Mathf.Clamp01((emitCol.g / pk) * 0.06f),
                    Mathf.Clamp01((emitCol.b / pk) * 0.06f), 1f);

                // In-memory (Unity API)
                mat.SetColor("_BaseColor", dark);
                mat.SetColor("_Color",     dark);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emitCol);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetFloat("_Smoothness", 0.6f);
                EditorUtility.SetDirty(mat);

                // Disk YAML
                EnsureEmissionKeyword(ref text);
                SetYamlColor(ref text, "_BaseColor",     dark);
                SetYamlColor(ref text, "_Color",         dark);
                SetYamlColor(ref text, "_EmissionColor", emitCol);
                text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 1");

                neon++;
            }
            else if (WARM_GLOW.Contains(matName))
            {
                // Subtle warm window glow (keep structural base, add faint emit)
                Color warmBase = STRUCTURAL_BASE.TryGetValue(matName, out Color sb)
                    ? sb : RGB(0.10f,0.10f,0.12f);
                Color warmEmit = E(0.30f, 0.22f, 0.10f);

                mat.SetColor("_BaseColor", warmBase);
                mat.SetColor("_Color",     warmBase);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", warmEmit);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                EditorUtility.SetDirty(mat);

                EnsureEmissionKeyword(ref text);
                SetYamlColor(ref text, "_BaseColor",     warmBase);
                SetYamlColor(ref text, "_Color",         warmBase);
                SetYamlColor(ref text, "_EmissionColor", warmEmit);
                text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 1");

                warm++;
            }
            else
            {
                // Structural: hardcoded dark base, zero emission
                Color baseCol = STRUCTURAL_BASE.TryGetValue(matName, out Color hb)
                    ? hb : DeduceDarkBase(mat.GetColor("_BaseColor"));

                mat.SetColor("_BaseColor", baseCol);
                mat.SetColor("_Color",     baseCol);
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                EditorUtility.SetDirty(mat);

                // Disk YAML — remove emission keyword
                text = Regex.Replace(text, @"  - _EMISSION\n", "");
                SetYamlColor(ref text, "_BaseColor",     baseCol);
                SetYamlColor(ref text, "_Color",         baseCol);
                SetYamlColor(ref text, "_EmissionColor", Color.black);
                text = Regex.Replace(text, @"m_LightmapFlags:\s*\d+", "m_LightmapFlags: 4");

                structural++;
            }

            File.WriteAllText(absPath, text);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        Debug.Log($"[DefinitiveFix] ✓  Neon: {neon}  Structural: {structural}  WarmGlow: {warm}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    // If structural base is way too bright (>0.5) or looks neon, return a safe dark grey
    static Color DeduceDarkBase(Color c)
    {
        // If a previous script cranked it to full saturation, just use dark grey
        float mx = Mathf.Max(c.r, c.g, c.b);
        float mn = Mathf.Min(c.r, c.g, c.b);
        float sat = (mx > 0.001f) ? (mx - mn) / mx : 0f;
        if (mx > 0.5f || sat > 0.4f)
            return RGB(0.10f, 0.11f, 0.13f);   // default dark building grey
        return c;   // colour seems reasonable, keep it
    }

    static void EnsureEmissionKeyword(ref string text)
    {
        if (!Regex.IsMatch(text, @"m_ValidKeywords:[\s\S]*?- _EMISSION"))
            text = Regex.Replace(text, @"(m_ValidKeywords:\r?\n)", "$1  - _EMISSION\n");
        // Remove from InvalidKeywords
        text = Regex.Replace(text, @"(m_InvalidKeywords:(?:\r?\n)*)((?:  - _EMISSION\r?\n)*)",
            m => m.Groups[1].Value + m.Groups[2].Value.Replace("  - _EMISSION\n", ""));
    }

    static void SetYamlColor(ref string text, string prop, Color c)
    {
        string r = c.r.ToString("0.######");
        string g = c.g.ToString("0.######");
        string b = c.b.ToString("0.######");
        string pat = $@"- {Regex.Escape(prop)}:\s*{{r:[^}}]+}}";
        text = Regex.Replace(text, pat,
            "- " + prop + ": {r: " + r + ", g: " + g + ", b: " + b + ", a: 1}");
    }

    static Color E(float r, float g, float b) => new Color(r, g, b, 1f);
    static Color RGB(float r, float g, float b) => new Color(r, g, b, 1f);
}
