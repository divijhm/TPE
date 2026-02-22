using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class CyberpunkPatch : MonoBehaviour
{
    [MenuItem("Tools/Cyberpunk Patch - Brightness & Floor")]
    public static void Patch()
    {
        // ── Ambient – much brighter, neon-tinged night city glow ──
        RenderSettings.ambientMode         = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.10f, 0.13f, 0.22f);   // dark blue sky
        RenderSettings.ambientEquatorColor = new Color(0.12f, 0.13f, 0.20f);   // neon-lit midtone
        RenderSettings.ambientGroundColor  = new Color(0.06f, 0.05f, 0.04f);   // warm ground bounce
        RenderSettings.fog        = true;
        RenderSettings.fogColor   = new Color(0.04f, 0.06f, 0.12f);
        RenderSettings.fogMode    = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.005f;
        DynamicGI.UpdateEnvironment();

        // ── All lights – raise everything ──────────────────────────
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        int areaIdx = 0;
        Color[] neons = {
            new Color(0f,   0.85f, 1f),    // cyan
            new Color(1f,   0.1f,  0.5f),  // pink
            new Color(1f,   0.65f, 0f),    // amber
            new Color(0.5f, 0f,    1f),    // violet
            new Color(0.1f, 1f,    0.3f),  // green
            new Color(1f,   0.9f,  0.8f),  // warm white
            new Color(0.2f, 0.5f,  1f),    // blue
        };
        foreach (Light L in lights)
        {
            string nm = L.gameObject.name.ToLower();
            if (L.type == LightType.Directional)
            {
                L.intensity = 0.08f;
                L.color     = new Color(0.45f, 0.55f, 0.85f);
                L.shadows   = LightShadows.None;
                EditorUtility.SetDirty(L); continue;
            }
            if (nm.StartsWith("area"))
            {
                L.type      = LightType.Point;
                L.shadows   = LightShadows.None;
                // First 3 = overhead warm street lamps, rest = neon accent fills
                if (areaIdx < 3) { L.color = new Color(1f,0.75f,0.38f); L.intensity = 25f; L.range = 40f; }
                else             { L.color = neons[areaIdx % neons.Length]; L.intensity = 15f; L.range = 30f; }
                areaIdx++;
                EditorUtility.SetDirty(L); continue;
            }
            // All other point/spot lights
            L.shadows = LightShadows.None;
            if (L.intensity < 1f) L.intensity = 3f;
            EditorUtility.SetDirty(L);
        }

        // ── Floor materials – visible mid-dark with tile texture ──
        FixFloor("Assets/Materials/Concrete.mat");
        FixFloor("Assets/blndrmaterials/Concrete.mat");
        FixFloor("Assets/Materials/Concrete.001.mat");
        FixFloor("Assets/blndrmaterials/Concrete.001.mat");
        FixFloor("Assets/Materials/Concrete.002.mat");
        FixFloor("Assets/blndrmaterials/Concrete.002.mat");
        FixFloor("Assets/Materials/AlleyStreet.mat");
        FixFloor("Assets/Materials/WF_Street.mat");
        FixFloor("Assets/Materials/concrete_22_color.mat");

        // ── Buildings – make noticeably visible ───────────────────
        FixBuilding("Assets/Materials/nightbuidlling.mat",  new Color(.20f,.21f,.27f));
        FixBuilding("Assets/Materials/AlleyBuilding.mat",   new Color(.21f,.22f,.28f));
        FixBuilding("Assets/Materials/skyscrapa.mat",       new Color(.18f,.20f,.27f));
        FixBuilding("Assets/Materials/WF_Building.mat",     new Color(.19f,.21f,.27f));
        FixBuilding("Assets/Materials/graybrick.mat",       new Color(.26f,.23f,.20f));
        FixBuilding("Assets/Materials/White Brick.mat",     new Color(.32f,.28f,.25f));
        FixBuilding("Assets/blndrmaterials/White Brick.mat",new Color(.32f,.28f,.25f));

        AssetDatabase.SaveAssets();
        Debug.Log("[PATCH] ✓ Brightness & floor patched!");
    }

    static Texture2D LoadTex(string path)
        => AssetDatabase.LoadAssetAtPath<Texture2D>(path);

    static void FixFloor(string matPath)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (!m) return;

        // Use the concrete colour texture — dark but not black
        var albedo = LoadTex("Assets/textures/concrete_22_color.jpg");
        var normal = LoadTex("Assets/textures/concrete_22_normal.jpg");

        if (albedo) { m.SetTexture("_BaseMap", albedo); m.SetTexture("_MainTex", albedo); }
        if (normal)   m.SetTexture("_BumpMap", normal);

        // Mid-dark tint so tile pattern is visible but stays dark
        var tint = new Color(0.30f, 0.30f, 0.34f, 1f);
        m.SetColor("_BaseColor", tint);
        m.SetColor("_Color",     tint);
        m.SetFloat("_Metallic",  0f);
        m.SetFloat("_Smoothness",0.88f);  // wet-floor reflectivity
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        EditorUtility.SetDirty(m);
    }

    static void FixBuilding(string matPath, Color col)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (!m) return;
        m.SetColor("_BaseColor", col);
        m.SetColor("_Color",     col);
        m.SetFloat("_Smoothness", 0.35f);
        EditorUtility.SetDirty(m);
    }
}
