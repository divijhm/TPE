using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// ================================================================
///  Cyberpunk Scene Setup V2
///  Run via  Tools ▶ Setup Cyberpunk Scene V2
/// ================================================================
public class CyberpunkSceneSetupV2 : MonoBehaviour
{
    [MenuItem("Tools/Setup Cyberpunk Scene V2")]
    public static void SetupScene()
    {
        FixEnvironment();
        FixCamera();
        FixAllSceneLights();
        SetupPostProcessing();
        FixMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Cyberpunk V2] ✓ Scene setup complete!");
    }

    // ─────────────────────────────────────────────────────────────
    //  ENVIRONMENT  (ambient + fog + sky)
    // ─────────────────────────────────────────────────────────────
    static void FixEnvironment()
    {
        // Very dark trilight ambient – warm ground, cool sky
        RenderSettings.ambientMode        = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor    = new Color(0.02f, 0.028f, 0.055f);
        RenderSettings.ambientEquatorColor= new Color(0.035f, 0.035f, 0.055f);
        RenderSettings.ambientGroundColor = new Color(0.025f, 0.02f, 0.018f);

        // Subtle distance fog
        RenderSettings.fog        = true;
        RenderSettings.fogColor   = new Color(0.02f, 0.028f, 0.07f);
        RenderSettings.fogMode    = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.007f;

        // Remove skybox so camera solid-colour shows through
        RenderSettings.skybox = null;
        DynamicGI.UpdateEnvironment();
        Debug.Log("[Cyberpunk V2] Environment configured.");
    }

    // ─────────────────────────────────────────────────────────────
    //  CAMERA
    // ─────────────────────────────────────────────────────────────
    static void FixCamera()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            string nm = cam.gameObject.name.ToLower();
            // Only touch the MainCamera, not the Blender-imported Camera / Camera.001
            if (nm != "main camera") continue;
            cam.allowHDR      = true;
            cam.clearFlags    = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.015f, 0.04f, 1f); // very dark navy
            EditorUtility.SetDirty(cam);
        }
        Debug.Log("[Cyberpunk V2] Camera configured.");
    }

    // ─────────────────────────────────────────────────────────────
    //  ALL SCENE LIGHTS
    // ─────────────────────────────────────────────────────────────
    static void FixAllSceneLights()
    {
        Light[] all = GameObject.FindObjectsOfType<Light>();
        Debug.Log("[Cyberpunk V2] Configuring " + all.Length + " lights…");

        foreach (Light L in all)
        {
            string nm = L.gameObject.name.ToLower();

            // ── Directional (main sunlight / moon) ──────────────
            if (L.type == LightType.Directional)
            {
                L.intensity = 0.05f;
                L.color     = new Color(0.35f, 0.45f, 0.7f);
                L.shadows   = LightShadows.None;
                EditorUtility.SetDirty(L);
                continue;
            }

            // ── Blender Area lights → warm overhead street lamps ─
            if (nm.StartsWith("area"))
            {
                L.type      = LightType.Point;
                L.color     = new Color(1f, 0.72f, 0.38f); // warm amber
                L.intensity = 2f;
                L.range     = 18f;
                L.shadows   = LightShadows.None;
                EditorUtility.SetDirty(L);
                continue;
            }

            // ── Blender Camera objects – ignore lights inside them ─
            if (nm.StartsWith("camera")) { L.enabled = false; EditorUtility.SetDirty(L); continue; }

            // ── All other scene lights ────────────────────────────
            // Scale down extreme Blender intensities
            if (L.intensity > 10f) L.intensity = 3f;
            else if (L.intensity > 4f) L.intensity = 2f;
            else if (L.intensity > 1.5f) L.intensity = 1.5f;

            // Give neutral-white lights a cool neon tint
            if (IsNeutralColor(L.color))
                L.color = new Color(0.75f, 0.85f, 1f);

            L.shadows = LightShadows.None; // no shadow cost on secondary lights
            EditorUtility.SetDirty(L);
        }
        Debug.Log("[Cyberpunk V2] All lights configured.");
    }

    static bool IsNeutralColor(Color c)
    {
        return Mathf.Abs(c.r - c.g) < 0.15f && Mathf.Abs(c.r - c.b) < 0.15f;
    }

    // ─────────────────────────────────────────────────────────────
    //  POST-PROCESSING
    // ─────────────────────────────────────────────────────────────
    static void SetupPostProcessing()
    {
        Volume globalVol = null;
        foreach (var v in GameObject.FindObjectsOfType<Volume>())
            if (v.isGlobal) { globalVol = v; break; }
        if (globalVol == null) { Debug.LogWarning("[Cyberpunk V2] No global volume!"); return; }

        var p = ScriptableObject.CreateInstance<VolumeProfile>();

        // ── Bloom – restrained & neon-coloured ──────────────────
        var bloom = p.Add<Bloom>(true);
        bloom.active = true;
        bloom.threshold.Override(0.88f);
        bloom.intensity.Override(1.6f);
        bloom.scatter.Override(0.65f);
        bloom.tint.Override(new Color(0.95f, 0.88f, 1f, 1f));

        // ── Color Adjustments ────────────────────────────────────
        var ca = p.Add<ColorAdjustments>(true);
        ca.active = true;
        ca.postExposure.Override(0.25f);
        ca.contrast.Override(18f);
        ca.colorFilter.Override(new Color(0.86f, 0.91f, 1f, 1f));
        ca.saturation.Override(8f); // slight boost so neons pop

        // ── Shadows-Midtones-Highlights (warm/cool split) ────────
        var smh = p.Add<ShadowsMidtonesHighlights>(true);
        smh.active = true;
        smh.shadows.Override(new Vector4(0.88f, 0.88f, 1.12f, 0f));    // cool shadows
        smh.midtones.Override(new Vector4(0.94f, 0.92f, 1f, 0f));      // slightly cool mids
        smh.highlights.Override(new Vector4(1.02f, 0.96f, 0.88f, 0f)); // warm highlights

        // ── Tonemapping ACES ────────────────────────────────────
        var tm = p.Add<Tonemapping>(true);
        tm.active = true;
        tm.mode.Override(TonemappingMode.ACES);

        // ── Vignette ────────────────────────────────────────────
        var vig = p.Add<Vignette>(true);
        vig.active = true;
        vig.intensity.Override(0.36f);
        vig.smoothness.Override(0.4f);
        vig.rounded.Override(true);

        // ── Chromatic aberration ────────────────────────────────
        var chr = p.Add<ChromaticAberration>(true);
        chr.active = true;
        chr.intensity.Override(0.12f);

        // ── Film grain ──────────────────────────────────────────
        var fg = p.Add<FilmGrain>(true);
        fg.active = true;
        fg.type.Override(FilmGrainLookup.Medium1);
        fg.intensity.Override(0.09f);
        fg.response.Override(0.5f);

        string path = "Assets/Scenes/CyberpunkPostProfile.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(p, path);
        AssetDatabase.SaveAssets();
        globalVol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        EditorUtility.SetDirty(globalVol);
        Debug.Log("[Cyberpunk V2] Post-processing profile saved.");
    }

    // ─────────────────────────────────────────────────────────────
    //  MATERIALS
    // ─────────────────────────────────────────────────────────────
    static void FixMaterials()
    {
        // ══ EMISSIVE (named neon colours) ════════════════════════
        //                         base colour               HDR emission
        SetEmissive("pink",        C(1f,  0.15f, 0.45f),   C(3f,   0.1f,  0.8f));
        SetEmissive("blue",        C(0f,  0.4f,  1f),      C(0.05f,0.9f,  4f));
        SetEmissive("green",       C(0.1f,1f,    0.2f),    C(0.1f, 3.5f,  0.4f));
        SetEmissive("red",         C(1f,  0.05f, 0f),      C(4f,   0.05f, 0f));
        SetEmissive("red light",   C(1f,  0.05f, 0f),      C(3f,   0.05f, 0f));
        SetEmissive("yellow",      C(1f,  0.85f, 0f),      C(4f,   3f,    0f));
        SetEmissive("violet",      C(0.6f,0f,    1f),      C(2f,   0f,    4f));
        SetEmissive("purple",      C(0.7f,0.1f,  1f),      C(2.5f, 0.1f,  4f));
        SetEmissive("Orange",      C(1f,  0.45f, 0f),      C(4f,   1.5f,  0f));
        SetEmissive("orange 1",    C(1f,  0.45f, 0f),      C(4f,   1.5f,  0f));
        SetEmissive("Emis",        C(0.8f,0.85f, 1f),      C(2.5f, 3f,    4f));
        SetEmissive("white light", C(1f,  0.9f,  0.75f),   C(3.5f, 3f,    2.5f));
        SetEmissive("white 1",     C(1f,  0.9f,  0.75f),   C(3.5f, 3f,    2.5f));
        SetEmissive("White",       C(0.85f,0.85f,0.9f),    C(2f,   2f,    3f));
        SetEmissive("Drone Lights",C(0f,  0.9f,  1f),      C(0f,   3.5f,  5f));

        // Building light strips → warm amber (overhead tram/walkway lights)
        SetEmissive("AlleyBuildingLight", C(1f,0.72f,0.38f), C(3f, 1.6f, 0.5f));
        SetEmissive("WF_BuildingLight",   C(1f,0.72f,0.38f), C(3f, 1.6f, 0.5f));
        SetEmissive("AlleyWindow",        C(0.55f,0.6f,0.75f), C(1f,1.3f,2f));

        // ══ DARK BUILDINGS ═══════════════════════════════════════
        SetDark("nightbuidlling",  C(.12f,.13f,.17f), 0f,   .4f);
        SetDark("AlleyBuilding",   C(.16f,.17f,.22f), 0f,   .32f);
        SetDark("skyscrapa",       C(.10f,.11f,.16f), .2f,  .48f);
        SetDark("WF_Building",     C(.14f,.15f,.21f), 0f,   .38f);
        SetDark("graybrick",       C(.21f,.18f,.16f), 0f,   .18f);
        SetDark("White Brick",     C(.28f,.24f,.22f), 0f,   .18f);
        SetDark("DefaultMaterial_Base_Color", C(.17f,.17f,.21f), .1f, .42f);

        // ══ STREET / GROUND (dark wet tiles) ═════════════════════
        SetDarkWithTex("AlleyStreet",
            "Assets/textures/tiled_plane_DefaultMaterial_BaseColor.jpg",
            "Assets/textures/tiled_plane_DefaultMaterial_Normal.jpg",
            C(.09f,.09f,.11f), 0f, 0.92f);
        SetDarkWithTex("WF_Street",
            "Assets/textures/tiled_plane_DefaultMaterial_BaseColor.jpg",
            "Assets/textures/tiled_plane_DefaultMaterial_Normal.jpg",
            C(.09f,.09f,.11f), 0f, 0.92f);

        SetDark("concrete_22_color", C(.13f,.13f,.17f), .05f, .68f);
        SetDark("Concrete.001",      C(.16f,.14f,.13f), 0f,   .18f);
        SetDark("Concrete.002",      C(.16f,.14f,.13f), 0f,   .18f);
        SetDark("Concrete",          C(.16f,.14f,.13f), 0f,   .18f);

        // ══ METAL / STRUCTURAL ═══════════════════════════════════
        SetDark("Dark iron",       C(.07f,.07f,.09f), .9f,  .4f);
        SetDark("Chrome",          C(.65f,.67f,.70f), 1f,   .9f);
        SetDark("Matte Black",     C(.05f,.05f,.06f), .1f,  .1f);
        SetDark("Black",           C(.03f,.03f,.04f), 0f,   .1f);
        SetDark("Black Plastic.001",             C(.04f,.04f,.05f), 0f, .15f);
        SetDark("Black plastic PL.001",          C(.04f,.04f,.05f), 0f, .15f);
        SetDark("Black plastic old scratched.001",C(.05f,.05f,.06f),0f, .08f);
        SetDark("Lightly soiled black metal",    C(.08f,.08f,.10f), .8f,.30f);
        SetDark("Rust 3.001",      C(.30f,.12f,.07f), 0f,   .08f);
        SetDark("trims",           C(.26f,.28f,.31f), .6f,  .42f);
        SetDark("AlleyCrate",      C(.20f,.18f,.15f), .2f,  .22f);
        SetDark("Trash Can",       C(.08f,.09f,.10f), .3f,  .28f);
        SetDark("Anti-slip plate - square.001", C(.14f,.14f,.16f), .8f,.22f);
        SetDark("metal_diffuse",   C(.20f,.20f,.23f), .8f,  .42f);
        SetDark("tires",           C(.05f,.05f,.06f), 0f,   .15f);

        // ══ GLASS ════════════════════════════════════════════════
        SetGlass("Glass.001");
        SetGlass("windows");
        SetGlass("dark glass");
        SetGlass("AlleyWindow");

        // ══ SIGN-BOARD TEXTURES (use extracted textures where available) ══
        SetTextured("01___Default_1001_baseColor",
            "Assets/extracted_textures/01___Default_1001_baseColor.jpeg",
            "Assets/extracted_textures/01___Default_1001_normal.jpg",
            null);
        SetTextured("DefaultMaterial_Base_Color",
            "Assets/extracted_textures/DefaultMaterial_Base_Color.jpg",
            null,
            "Assets/extracted_textures/DefaultMaterial_Metallic.jpg");

        // ══ GENERIC Material.XXX → dark neutral urban ════════════
        // These cover building walls, structural, floor sections etc.
        // Do NOT set them emissive – that's what caused everything to glow.
        Color[] pal =
        {
            C(.20f,.18f,.16f), // warm grey concrete
            C(.15f,.16f,.20f), // cool blue-grey
            C(.22f,.20f,.18f), // stone
            C(.12f,.12f,.15f), // dark slate
            C(.24f,.22f,.19f), // medium warm grey
            C(.17f,.16f,.20f), // blue-grey steel
            C(.13f,.15f,.18f), // dark steel
            C(.19f,.18f,.17f), // concrete
        };
        string[] mats =
        {
            "Material",
            "Material.001","Material.002","Material.003","Material.004",
            "Material.005","Material.006","Material.007","Material.008","Material.009",
            "Material.010","Material.011","Material.012","Material.013","Material.014",
            "Material.015","Material.016","Material.017","Material.018","Material.019",
            "Material.020","Material.021","Material.022","Material.023","Material.024",
            "Material.025","Material.026","Material.027","Material.028","Material.029",
            "Material.030","Material.031","Material.032","Material.033","Material.034",
            "Material.035","Material.036","Material.037","Material.038","Material.039",
            "Material.040","Material.041","Material.042","Material.043","Material.044","Material.045",
            "_012","Mich_CC_18_baseColor"
        };
        for (int i = 0; i < mats.Length; i++)
        {
            float met = (i % 3 == 0) ? 0.3f : 0f;
            float smo = (i % 4 == 0) ? 0.42f : 0.22f;
            SetDark(mats[i], pal[i % pal.Length], met, smo);
        }

        Debug.Log("[Cyberpunk V2] All materials fixed.");
    }

    // ─────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────
    static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

    static void SetEmissive(string name, Color base_, Color emit)
    {
        Apply("Assets/Materials/"     + name + ".mat", base_, emit);
        Apply("Assets/blndrmaterials/"+ name + ".mat", base_, emit);
    }
    static void SetDark(string name, Color base_, float met, float smo)
    {
        ApplyDark("Assets/Materials/"     + name + ".mat", base_, met, smo);
        ApplyDark("Assets/blndrmaterials/"+ name + ".mat", base_, met, smo);
    }
    static void SetGlass(string name)
    {
        ApplyGlass("Assets/Materials/"     + name + ".mat");
        ApplyGlass("Assets/blndrmaterials/"+ name + ".mat");
    }
    static void SetDarkWithTex(string name, string albedoPath, string normalPath, Color tint, float met, float smo)
    {
        ApplyDarkWithTex("Assets/Materials/"     + name + ".mat", albedoPath, normalPath, tint, met, smo);
        ApplyDarkWithTex("Assets/blndrmaterials/"+ name + ".mat", albedoPath, normalPath, tint, met, smo);
    }
    static void SetTextured(string name, string albedoPath, string normalPath, string metallicPath)
    {
        ApplyTextured("Assets/Materials/"     + name + ".mat", albedoPath, normalPath, metallicPath);
        ApplyTextured("Assets/blndrmaterials/"+ name + ".mat", albedoPath, normalPath, metallicPath);
    }

    // ─────────────────────────────────────────────────────────────
    static void Apply(string path, Color base_, Color emit)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        m.SetColor("_BaseColor", base_);
        m.SetColor("_Color",     base_);
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", emit);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        m.SetFloat("_Smoothness", 0.55f);
        m.SetFloat("_Metallic",   0f);
        EditorUtility.SetDirty(m);
    }

    static void ApplyDark(string path, Color base_, float met, float smo)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        m.SetColor("_BaseColor", base_);
        m.SetColor("_Color",     base_);
        m.SetFloat("_Metallic",  met);
        m.SetFloat("_Smoothness",smo);
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        EditorUtility.SetDirty(m);
    }

    static void ApplyDarkWithTex(string path, string albedoPath, string normalPath, Color tint, float met, float smo)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        Texture2D alb = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        Texture2D nrm = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        if (alb) { m.SetTexture("_BaseMap", alb); m.SetTexture("_MainTex", alb); }
        if (nrm) m.SetTexture("_BumpMap", nrm);
        m.SetColor("_BaseColor", tint);
        m.SetColor("_Color",     tint);
        m.SetFloat("_Metallic",  met);
        m.SetFloat("_Smoothness",smo);
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        EditorUtility.SetDirty(m);
    }

    static void ApplyTextured(string path, string albedoPath, string normalPath, string metallicPath)
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
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        EditorUtility.SetDirty(m);
    }

    static void ApplyGlass(string path)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) return;
        m.SetFloat("_Surface",  1f);
        m.SetFloat("_Blend",    0f);
        m.SetFloat("_Cull",     0f);
        m.SetFloat("_ZWrite",   0f);
        m.SetFloat("_SrcBlend",       5f);
        m.SetFloat("_DstBlend",      10f);
        m.SetFloat("_SrcBlendAlpha",  1f);
        m.SetFloat("_DstBlendAlpha",  0f);
        m.SetFloat("_AlphaToMask",    0f);
        m.SetColor("_BaseColor", new Color(.25f,.4f,.6f, .10f));
        m.SetColor("_Color",     new Color(.25f,.4f,.6f, .10f));
        m.SetFloat("_Smoothness", .95f);
        m.SetFloat("_Metallic",    0f);
        m.renderQueue = 3000;
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        EditorUtility.SetDirty(m);
    }
}
