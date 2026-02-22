using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// ================================================================
///  Cyberpunk Scene Setup V3
///  Informed by full scene diagnostic — accurate material routing
///  Tools ▶ Setup Cyberpunk Scene V3
/// ================================================================
public class CyberpunkSceneSetupV3 : MonoBehaviour
{
    [MenuItem("Tools/Setup Cyberpunk Scene V3")]
    public static void Run()
    {
        FixEnvironment();
        FixAllLights();
        FixMaterials();
        SetupPostProcessing();
        SaveAll();
        Debug.Log("[V3] ✓ Complete!");
    }

    // ════════════════════════════════════════════════════════════
    //  ENVIRONMENT
    // ════════════════════════════════════════════════════════════
    static void FixEnvironment()
    {
        RenderSettings.skybox = null;
        // Dark trilight — very dim blue-black night, slight warm ground bounce
        RenderSettings.ambientMode         = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.03f, 0.04f, 0.09f);
        RenderSettings.ambientEquatorColor = new Color(0.04f, 0.04f, 0.07f);
        RenderSettings.ambientGroundColor  = new Color(0.03f, 0.025f, 0.02f);
        RenderSettings.fog        = true;
        RenderSettings.fogColor   = new Color(0.02f, 0.03f, 0.06f);
        RenderSettings.fogMode    = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.006f;
        DynamicGI.UpdateEnvironment();
    }

    // ════════════════════════════════════════════════════════════
    //  LIGHTS  — from diagnostic: 40 total (directionals + points)
    // ════════════════════════════════════════════════════════════
    static void FixAllLights()
    {
        // Neon palette for area lights cycling
        Color[] neonPalette = {
            new Color(0f,   0.85f, 1f),    // cyan
            new Color(1f,   0.15f, 0.55f), // hot pink
            new Color(1f,   0.65f, 0f),    // amber/yellow
            new Color(0.55f,0f,    1f),    // purple
            new Color(0.1f, 1f,    0.35f), // green
            new Color(1f,   0.22f, 0.1f),  // red-orange
            new Color(0f,   0.5f,  1f),    // blue
            new Color(1f,   0.9f,  0.8f),  // warm white (street lamp)
        };

        int areaIdx = 0;
        foreach (Light L in GameObject.FindObjectsOfType<Light>())
        {
            string nm = L.gameObject.name.ToLower();

            // ── Directional (Unity default + Blender "Sun") ──────
            if (L.type == LightType.Directional)
            {
                L.intensity = 0.04f;
                L.color     = new Color(0.4f, 0.5f, 0.75f); // cool blue moonlight
                L.shadows   = LightShadows.None;
                EditorUtility.SetDirty(L);
                continue;
            }

            // ── Blender Area lights → coloured neon point lights ─
            if (nm.StartsWith("area"))
            {
                L.type      = LightType.Point;
                L.color     = neonPalette[areaIdx % neonPalette.Length];
                // First few = warm street lamps overhead, rest = neon accent
                if (areaIdx < 3) { L.color = new Color(1f, 0.72f, 0.35f); L.intensity = 12f; L.range = 28f; }
                else             { L.intensity = 8f;  L.range = 22f; }
                L.shadows   = LightShadows.None;
                areaIdx++;
                EditorUtility.SetDirty(L);
                continue;
            }

            // ── Anything else ────────────────────────────────────
            L.shadows = LightShadows.None;
            if (L.intensity > 50f)  L.intensity = 10f;
            else if (L.intensity > 10f) L.intensity = 5f;
            EditorUtility.SetDirty(L);
        }
    }

    // ════════════════════════════════════════════════════════════
    //  MATERIALS
    // ════════════════════════════════════════════════════════════
    static void FixMaterials()
    {
        // ── NAMED NEON SIGNS (small objects: Japanese words, logos etc) ──
        Emissive("pink",       C(1f,0.18f,0.48f),  CE(3.5f,0.1f,1.2f));
        Emissive("blue",       C(0f,0.42f,1f),     CE(0.05f,1.2f,5f));
        Emissive("green",      C(0.12f,1f,0.25f),  CE(0.15f,4f,0.5f));
        Emissive("red",        C(1f,0.06f,0.02f),  CE(5f,0.06f,0.06f));
        Emissive("red light",  C(1f,0.06f,0.02f),  CE(4f,0.06f,0.06f));
        Emissive("yellow",     C(1f,0.88f,0f),     CE(5f,3.8f,0f));
        Emissive("violet",     C(0.55f,0f,1f),     CE(2.5f,0f,5f));
        Emissive("purple",     C(0.65f,0.1f,1f),   CE(3f,0.1f,5f));
        Emissive("Orange",     C(1f,0.45f,0f),     CE(5f,1.8f,0f));
        Emissive("orange 1",   C(1f,0.45f,0f),     CE(5f,1.8f,0f));
        Emissive("orange",     C(1f,0.45f,0f),     CE(5f,1.8f,0f));
        Emissive("white",      C(0.9f,0.9f,0.95f), CE(3f,3f,4f));
        Emissive("white light",C(1f,0.9f,0.7f),    CE(4f,3.5f,2.5f));
        Emissive("white 1",    C(1f,0.9f,0.7f),    CE(4f,3.5f,2.5f));
        Emissive("White",      C(0.9f,0.9f,0.95f), CE(3f,3f,4f));
        Emissive("Emis",       C(0.8f,0.85f,1f),   CE(2.5f,3f,5f));

        // "Lit" material — on text/circle objects, make it emissive cool-white
        Emissive("Lit",        C(0.85f,0.95f,1f),  CE(3f,4f,5f));

        // ── BUILDING LIGHT STRIPS (warm overhead tram-rail lights) ──
        Emissive("AlleyBuildingLight", C(1f,0.72f,0.35f), CE(3.5f,2f,0.6f));
        Emissive("WF_BuildingLight",   C(1f,0.72f,0.35f), CE(3.5f,2f,0.6f));

        // ── BILLBOARD / SIGN PANELS (Material.001-045) ───────────────────
        // These are the colored sign face cubes — KEEP emissive but moderate.
        // Colors chosen to recreate the mixed neon billboard look from reference.
        // Groups loosely matching: blue-cyan tech, pink social, red food, green nature, yellow warning, purple, mixed
        SetBillboard("Material",     CE(1.8f,1.8f,2.2f), C(0.5f,0.5f,0.7f));  // neutral
        SetBillboard("Material.001", CE(2f,2.2f,0.8f),   C(0.8f,0.9f,0.3f));  // yellow-green (NO.999 sign)
        SetBillboard("Material.002", CE(0.15f,1.5f,2.5f),C(0.1f,0.5f,0.8f));  // cyan (NO.322)
        SetBillboard("Material.003", CE(2.5f,0.1f,0.1f), C(0.9f,0.1f,0.1f));  // red
        SetBillboard("Material.004", CE(0.8f,0.8f,2.5f), C(0.3f,0.3f,0.9f));  // blue
        SetBillboard("Material.005", CE(2.5f,0.8f,0.1f), C(0.9f,0.4f,0.1f));  // orange
        SetBillboard("Material.006", CE(0.1f,2.5f,1f),   C(0.1f,0.9f,0.4f));  // cyan-green
        SetBillboard("Material.007", CE(2.5f,0.15f,1f),  C(0.9f,0.1f,0.4f));  // pink-red
        SetBillboard("Material.008", CE(0.8f,0.1f,2.5f), C(0.3f,0.1f,0.9f));  // violet
        SetBillboard("Material.009", CE(1.8f,1.8f,1.8f), C(0.7f,0.7f,0.7f));  // white panel
        SetBillboard("Material.010", CE(2.5f,0.1f,0.8f), C(0.9f,0.1f,0.4f));  // hot pink
        SetBillboard("Material.011", CE(0.1f,1.5f,2.5f), C(0.1f,0.5f,0.9f));  // sky blue
        SetBillboard("Material.012", CE(2f,2f,0.1f),     C(0.8f,0.8f,0.1f));  // yellow
        SetBillboard("Material.013", CE(0.1f,2.5f,0.8f), C(0.1f,0.9f,0.3f));  // green
        SetBillboard("Material.014", CE(2.5f,0.8f,0.1f), C(0.9f,0.35f,0.1f)); // amber
        SetBillboard("Material.015", CE(1f,0.1f,2.5f),   C(0.4f,0.1f,0.9f));  // purple
        SetBillboard("Material.016", CE(2.5f,0.2f,0.2f), C(0.9f,0.15f,0.15f));// deep red
        SetBillboard("Material.017", CE(1.5f,1.5f,2.5f), C(0.5f,0.5f,0.9f));  // lavender
        SetBillboard("Material.018", CE(0.1f,2f,2.5f),   C(0.1f,0.7f,0.9f));  // teal
        SetBillboard("Material.019", CE(2.5f,1f,0.1f),   C(0.9f,0.5f,0.1f));  // gold
        SetBillboard("Material.020", CE(0.5f,0.1f,2.5f), C(0.25f,0.05f,0.9f));// indigo
        SetBillboard("Material.021", CE(2.5f,0.1f,1.5f), C(0.9f,0.1f,0.6f));  // magenta
        SetBillboard("Material.022", CE(0.1f,2.5f,0.4f), C(0.1f,0.9f,0.2f));  // electric green
        SetBillboard("Material.023", CE(2f,1.5f,0.1f),   C(0.75f,0.6f,0.1f)); // warm yellow
        SetBillboard("Material.024", CE(0.1f,1f,2.5f),   C(0.1f,0.4f,0.9f));  // azure
        SetBillboard("Material.025", CE(2.5f,0.5f,0.1f), C(0.9f,0.2f,0.1f));  // red-orange
        SetBillboard("Material.026", CE(0.8f,2.5f,0.1f), C(0.3f,0.9f,0.1f));  // lime
        SetBillboard("Material.027", CE(0.1f,1.5f,2.5f), C(0.1f,0.5f,0.9f));  // deep cyan
        SetBillboard("Material.028", CE(2.5f,0.1f,0.5f), C(0.9f,0.1f,0.25f)); // crimson
        SetBillboard("Material.029", CE(0.8f,0.8f,2.5f), C(0.3f,0.3f,0.9f));  // periwinkle
        SetBillboard("Material.030", CE(2.5f,1.5f,0.1f), C(0.9f,0.6f,0.1f));  // orange-yellow
        SetBillboard("Material.031", CE(0.1f,2.5f,1.5f), C(0.1f,0.9f,0.6f));  // seafoam
        SetBillboard("Material.032", CE(2f,0.1f,2f),     C(0.75f,0.1f,0.75f));// violet-pink
        SetBillboard("Material.033", CE(0.1f,0.8f,2.5f), C(0.1f,0.3f,0.9f));  // cobalt
        SetBillboard("Material.034", CE(2.5f,1.8f,0.1f), C(0.9f,0.7f,0.1f));  // golden
        SetBillboard("Material.035", CE(2f,0.1f,0.8f),   C(0.75f,0.1f,0.35f));// rose
        SetBillboard("Material.036", CE(0.3f,2.5f,2.5f), C(0.15f,0.9f,0.9f)); // cyan-white
        SetBillboard("Material.037", CE(2.5f,2f,0.1f),   C(0.9f,0.75f,0.1f)); // yellow-gold
        SetBillboard("Material.038", CE(1f,0.1f,2.5f),   C(0.4f,0.1f,0.9f));  // purple
        SetBillboard("Material.039", CE(0.1f,2.5f,2f),   C(0.1f,0.9f,0.75f)); // mint
        SetBillboard("Material.040", CE(2.5f,0.3f,1.5f), C(0.9f,0.15f,0.6f)); // hot pink
        SetBillboard("Material.041", CE(0.3f,1f,2.5f),   C(0.15f,0.4f,0.9f)); // deep blue
        SetBillboard("Material.042", CE(2.5f,0.8f,0.8f), C(0.9f,0.35f,0.35f));// salmon
        SetBillboard("Material.043", CE(1.5f,2.5f,0.3f), C(0.6f,0.9f,0.15f)); // yellow-green
        SetBillboard("Material.044", CE(2.5f,0.5f,2f),   C(0.9f,0.25f,0.75f));// orchid
        SetBillboard("Material.045", CE(0.3f,2.5f,0.8f), C(0.15f,0.9f,0.3f)); // green
        // Windows emissive (interior glow)
        Emissive("AlleyWindow", C(0.55f,0.6f,0.8f), CE(1.2f,1.5f,2.5f));
        // Drone lights
        Emissive("Drone Lights", C(0f,0.9f,1f), CE(0f,4f,6f));

        // ── BUILDING SHELLS (dark, no emission) ──────────────────
        Dark("nightbuidlling",  C(.14f,.15f,.20f), 0f,   .38f);
        Dark("AlleyBuilding",   C(.18f,.19f,.24f), 0f,   .30f);
        Dark("skyscrapa",       C(.12f,.13f,.19f), .15f, .45f);
        Dark("WF_Building",     C(.15f,.16f,.22f), 0f,   .35f);
        Dark("graybrick",       C(.22f,.19f,.17f), 0f,   .16f);
        Dark("White Brick",     C(.30f,.26f,.24f), 0f,   .16f);

        // ── FLOOR / GROUND ───────────────────────────────────────
        // Use concrete_22_color texture for the dark wet tile look
        DarkTex("Concrete",      "Assets/textures/concrete_22_color.jpg",
                                 "Assets/textures/concrete_22_normal.jpg",
                                 C(.07f,.07f,.08f), 0f, 0.92f);
        DarkTex("Concrete.001",  "Assets/textures/concrete_22_color.jpg",
                                 "Assets/textures/concrete_22_normal.jpg",
                                 C(.08f,.08f,.10f), 0f, 0.90f);
        DarkTex("Concrete.002",  "Assets/textures/concrete_22_color.jpg",
                                 "Assets/textures/concrete_22_normal.jpg",
                                 C(.08f,.08f,.10f), 0f, 0.90f);
        DarkTex("concrete_22_color","Assets/textures/concrete_22_color.jpg",
                                 "Assets/textures/concrete_22_normal.jpg",
                                 C(.09f,.09f,.11f), 0f, 0.88f);
        DarkTex("AlleyStreet",   "Assets/textures/concrete_22_color.jpg",
                                 "Assets/textures/concrete_22_normal.jpg",
                                 C(.07f,.07f,.09f), 0f, 0.93f);
        DarkTex("WF_Street",     "Assets/textures/concrete_22_color.jpg",
                                 "Assets/textures/concrete_22_normal.jpg",
                                 C(.07f,.07f,.09f), 0f, 0.93f);

        // ── METALS / STRUCTURAL ──────────────────────────────────
        Dark("Dark iron",        C(.07f,.07f,.09f), .9f, .38f);
        Dark("Chrome",           C(.62f,.64f,.68f), 1f,  .92f);
        Dark("Matte Black",      C(.05f,.05f,.06f), .1f, .10f);
        Dark("Black",            C(.04f,.04f,.05f), 0f,  .10f);
        Dark("Lightly soiled black metal", C(.08f,.08f,.10f), .8f, .28f);
        Dark("Rust 3.001",       C(.30f,.12f,.07f), 0f,  .08f);
        Dark("trims",            C(.28f,.30f,.33f), .6f, .42f);
        Dark("Anti-slip plate - square.001", C(.15f,.15f,.18f), .8f, .22f);
        Dark("Black Plastic.001",           C(.04f,.04f,.05f), 0f, .14f);
        Dark("Black plastic PL.001",        C(.04f,.04f,.05f), 0f, .14f);
        Dark("Black plastic old scratched.001", C(.05f,.05f,.06f), 0f, .08f);
        Dark("metal_diffuse",    C(.20f,.20f,.23f), .8f, .42f);
        Dark("AlleyCrate",       C(.20f,.18f,.15f), .2f, .22f);
        Dark("Trash Can",        C(.08f,.09f,.10f), .3f, .26f);
        Dark("tires",            C(.06f,.06f,.07f), 0f,  .14f);
        Dark("DefaultMaterial_Base_Color", C(.18f,.18f,.22f), .1f, .42f);

        // ── CABLES (bezier curves — thick black cables overhead) ─
        Dark("Procedural rubber latex", C(.05f,.05f,.06f), 0f, .08f);
        Dark("Black plastic old scratched.001", C(.04f,.04f,.05f), 0f,.06f);

        // ── GLASS ────────────────────────────────────────────────
        Glass("Glass.001");
        Glass("windows");
        Glass("dark glass");
        Glass("AlleyWindow");

        // ── SIGNS WITH TEXTURES ──────────────────────────────────
        Textured("01___Default_1001_baseColor",
            "Assets/extracted_textures/01___Default_1001_baseColor.jpeg",
            "Assets/extracted_textures/01___Default_1001_normal.jpg");
        DarkTex("_012",
            "Assets/textures/.012_baseColor.jpeg", null,
            C(.35f,.3f,.28f), 0f, .3f);

        // ── DECORATIVE / SILK / MISC ─────────────────────────────
        Dark("Procedral Red Silk", C(.55f,.04f,.04f), 0f, .18f);
    }

    // ════════════════════════════════════════════════════════════
    //  POST-PROCESSING
    // ════════════════════════════════════════════════════════════
    static void SetupPostProcessing()
    {
        Volume vol = null;
        foreach (var v in GameObject.FindObjectsOfType<Volume>())
            if (v.isGlobal) { vol = v; break; }
        if (!vol) { Debug.LogWarning("[V3] No global volume!"); return; }

        // Fix the camera
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.gameObject.name != "Main Camera") continue;
            cam.allowHDR      = true;
            cam.clearFlags    = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.015f, 0.04f, 1f);
            EditorUtility.SetDirty(cam);
        }

        var p = ScriptableObject.CreateInstance<VolumeProfile>();

        var bloom = p.Add<Bloom>(true);
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(2.2f);
        bloom.scatter.Override(0.72f);
        bloom.tint.Override(new Color(0.95f, 0.88f, 1f, 1f));

        var ca = p.Add<ColorAdjustments>(true);
        ca.postExposure.Override(0.4f);
        ca.contrast.Override(20f);
        ca.colorFilter.Override(new Color(0.88f, 0.93f, 1f, 1f));
        ca.saturation.Override(15f);

        var smh = p.Add<ShadowsMidtonesHighlights>(true);
        smh.shadows.Override(new Vector4(0.85f, 0.85f, 1.1f, 0f));
        smh.midtones.Override(new Vector4(0.95f, 0.93f, 1f, 0f));
        smh.highlights.Override(new Vector4(1.05f, 0.98f, 0.9f, 0f));

        var tm = p.Add<Tonemapping>(true);
        tm.mode.Override(TonemappingMode.ACES);

        var vig = p.Add<Vignette>(true);
        vig.intensity.Override(0.32f);
        vig.smoothness.Override(0.4f);
        vig.rounded.Override(true);

        var chr = p.Add<ChromaticAberration>(true);
        chr.intensity.Override(0.12f);

        var fg = p.Add<FilmGrain>(true);
        fg.type.Override(FilmGrainLookup.Medium1);
        fg.intensity.Override(0.08f);

        const string path = "Assets/Scenes/CyberpunkPostProfile.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(p, path);
        AssetDatabase.SaveAssets();
        vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        EditorUtility.SetDirty(vol);
    }

    // ════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════
    static Color C(float r, float g, float b)  => new Color(r, g, b, 1f);
    static Color CE(float r, float g, float b) => new Color(r, g, b, 1f); // HDR

    // Named emissive — applies to both Materials/ and blndrmaterials/
    static void Emissive(string name, Color base_, Color emit)
    {
        Apply("Assets/Materials/"      + name + ".mat", base_, emit, true);
        Apply("Assets/blndrmaterials/" + name + ".mat", base_, emit, true);
    }

    // Billboard (emissive, high smoothness for bloom reflection)
    static void SetBillboard(string name, Color emit, Color base_)
    {
        Apply("Assets/Materials/"      + name + ".mat", base_, emit, true);
        Apply("Assets/blndrmaterials/" + name + ".mat", base_, emit, true);
    }

    static void Dark(string name, Color base_, float met, float smo)
    {
        Apply("Assets/Materials/"      + name + ".mat", base_, Color.black, false, met, smo);
        Apply("Assets/blndrmaterials/" + name + ".mat", base_, Color.black, false, met, smo);
    }

    static void DarkTex(string name, string albedo, string normal, Color tint, float met, float smo)
    {
        ApplyTex("Assets/Materials/"      + name + ".mat", albedo, normal, tint, met, smo);
        ApplyTex("Assets/blndrmaterials/" + name + ".mat", albedo, normal, tint, met, smo);
    }

    static void Textured(string name, string albedo, string normal)
    {
        ApplyTex("Assets/Materials/"      + name + ".mat", albedo, normal, Color.white, 0f, 0.3f);
        ApplyTex("Assets/blndrmaterials/" + name + ".mat", albedo, normal, Color.white, 0f, 0.3f);
    }

    static void Glass(string name)
    {
        ApplyGlass("Assets/Materials/"      + name + ".mat");
        ApplyGlass("Assets/blndrmaterials/" + name + ".mat");
    }

    // ── Low-level ────────────────────────────────────────────────
    static void Apply(string path, Color base_, Color emit, bool emissive,
                      float met = 0f, float smo = 0.55f)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        m.SetColor("_BaseColor", base_);
        m.SetColor("_Color",     base_);
        m.SetFloat("_Metallic",  met);
        m.SetFloat("_Smoothness",smo);
        if (emissive)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emit);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            m.DisableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        EditorUtility.SetDirty(m);
    }

    static void ApplyTex(string path, string albedoPath, string normalPath,
                         Color tint, float met, float smo)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        if (!string.IsNullOrEmpty(albedoPath))
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if (t) { m.SetTexture("_BaseMap", t); m.SetTexture("_MainTex", t); }
        }
        if (!string.IsNullOrEmpty(normalPath))
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (t) m.SetTexture("_BumpMap", t);
        }
        m.SetColor("_BaseColor", tint);
        m.SetColor("_Color",     tint);
        m.SetFloat("_Metallic",  met);
        m.SetFloat("_Smoothness",smo);
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        EditorUtility.SetDirty(m);
    }

    static void ApplyGlass(string path)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f);
        m.SetFloat("_ZWrite", 0f);  m.SetFloat("_Cull", 0f);
        m.SetFloat("_SrcBlend", 5f); m.SetFloat("_DstBlend", 10f);
        m.SetFloat("_SrcBlendAlpha", 1f); m.SetFloat("_DstBlendAlpha", 0f);
        m.SetColor("_BaseColor", new Color(.22f,.38f,.58f,.08f));
        m.SetColor("_Color",     new Color(.22f,.38f,.58f,.08f));
        m.SetFloat("_Smoothness", .96f);
        m.SetFloat("_Metallic", 0f);
        m.renderQueue = 3000;
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        EditorUtility.SetDirty(m);
    }

    static void SaveAll()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
