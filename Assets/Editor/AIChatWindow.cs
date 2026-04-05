using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI Chat Window - Editor plugin stub for AI-driven scene control.
/// Provides a chat interface, mode selector, and a scene-switcher button.
/// </summary>
public partial class AIChatWindow : EditorWindow
{
    private const string ApiKeyPrefKey = "AIChatWindow.ApiKey";
    private const bool DebugPasteLogs = true;

    // -- Chat mode -----------------------------------------------------------
    private enum ChatMode { AssetGeneration, Agent, Selection }

    private static readonly string[] ChatModeLabels =
    {
        "Asset Generation",
        "Agent",
        "Selection"
    };

    private ChatMode currentMode = ChatMode.Agent;

    // -- Asset-generation model tiers ----------------------------------------
    // Format: (display label, internal id, usage multiplier label)
    private static readonly (string Label, string Id, string Tier)[] AssetModels =
    {
        // -- Budget  (x0.33) -------------------------------------------------
        ("Stable Diffusion 1.5   ×0.33",  "sd15",         "×0.33"),
        ("Shap-E                 ×0.33",  "shape",        "×0.33"),
        ("Point-E                ×0.33",  "pointe",       "×0.33"),
        // -- Standard  (x1) --------------------------------------------------
        ("TripoSR                ×1",     "triposr",      "×1"),
        ("Zero123++              ×1",     "zero123pp",    "×1"),
        ("DreamGaussian          ×1",     "dreamgaussian","×1"),
        // -- Premium  (x3) ---------------------------------------------------
        ("InstantMesh            ×3",     "instantmesh",  "×3"),
        ("CraftsMan              ×3",     "craftsman",    "×3"),
        ("Wonder3D               ×3",     "wonder3d",     "×3"),
    };

    private static readonly string[] AssetModelLabels;

    // Build the label array once from AssetModels
    static AIChatWindow()
    {
        AssetModelLabels = new string[AssetModels.Length];
        for (int i = 0; i < AssetModels.Length; i++)
            AssetModelLabels[i] = AssetModels[i].Label;
    }

    private int selectedModelIndex = 0;

    // -- Chat state ----------------------------------------------------------
    private string inputText = "";
    private Vector2 scrollPos;
    private readonly List<ChatMessage> messages = new List<ChatMessage>();
    private readonly List<ImageAttachment> pendingAttachments = new List<ImageAttachment>();

    // -- Scene switching -----------------------------------------------------
    private const string SCENE_A = "Assets/Scenes/SampleScene.unity";
    private const string SCENE_B = "Assets/Scenes/DemoScene.unity";
    private bool isOnSceneA = true;

    // -- Asset generation loading state --------------------------------------
    private static readonly string[] WeaponPrefabPaths =
    {
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 1.prefab",
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 2.prefab",
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 3.prefab",
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 4.prefab",
    };

    private bool isGenerating = false;
    private double generateStartTime;
    private int generatingMsgIdx = -1;
    private double lastDotTime;
    private int dotCount = 1;

    // -- Agent mode state machine --------------------------------------------
    private enum AgentPhase { None, ChangingScene, AdjustingScene, Done }
    private AgentPhase agentPhase = AgentPhase.None;
    private double agentStepTime;
    private int agentStepIdx;
    private int agentBotMsgIdx = -1;
    private List<GameObject> _initChildren = new List<GameObject>();
    private List<GameObject> _toreachChildren = new List<GameObject>();

    private const float ToggleDelay = 0.15f;
    private const float ToolDelay = 2.0f;

    private static readonly string[] AdjustToolPaths =
    {
        "Tools/Cyberpunk Patch - Brightness & Floor",
        "Tools/Neon Glow Fix",
        "Tools/Neon Reset (Dark base + Glow)",
        "Tools/Neon Signs & Text Fix",
        "Tools/Setup Cyberpunk Scene V3",
    };

    private static readonly string[] AdjustToolMessages =
    {
        "Adjusting scene...",
        "Adjusting lighting...",
        "Rethinking the environment...",
        "Fine-tuning the neon signs...",
        "Applying final cyberpunk atmosphere...",
    };

    // -- Styles (lazy-initialised) -------------------------------------------
    private GUIStyle bubbleStyleUser;
    private GUIStyle bubbleStyleBot;
    private GUIStyle inputAreaStyle;
    private GUIStyle tierBudgetStyle;
    private GUIStyle tierStandardStyle;
    private GUIStyle tierPremiumStyle;
    private GUIStyle modeBadgeStyle;
    private bool stylesInitialised;
    private string apiKeyInput = "";
    private string apiKeyError = "";

    // -- Tier colour map -----------------------------------------------------
    private static readonly Color ColBudget = new Color(0.35f, 0.75f, 0.35f);
    private static readonly Color ColStandard = new Color(0.25f, 0.55f, 0.95f);
    private static readonly Color ColPremium = new Color(0.85f, 0.50f, 0.15f);

    // -- Entry point ---------------------------------------------------------
    [MenuItem("AI Chat/Open Chat Window")]
    public static void ShowWindow()
    {
        var win = GetWindow<AIChatWindow>("AI Chat");
        win.minSize = new Vector2(420, 560);
        win.Show();
    }

    [MenuItem("AI Chat/Clear API Key")]
    public static void ClearApiKey()
    {
        EditorPrefs.DeleteKey(ApiKeyPrefKey);
        Debug.Log("API key cleared. Restart the window to test the input screen.");
    }

    // -- Editor update subscription -----------------------------------------
    private void OnEnable()
    {
        apiKeyInput = EditorPrefs.GetString(ApiKeyPrefKey, "");
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        DestroyRuntimeAttachments(pendingAttachments);
        pendingAttachments.Clear();

        foreach (var msg in messages)
        {
            if (msg.ImageAttachments != null)
                DestroyRuntimeAttachments(msg.ImageAttachments);
        }
    }

    private void OnEditorUpdate()
    {
        // -- Agent phase pump ------------------------------------------------
        if (agentPhase != AgentPhase.None)
        {
            OnEditorAgentUpdate();
            return;
        }

        if (!isGenerating)
        {
            // Keep repainting while Unity loads asset preview thumbnails
            if (AssetPreview.IsLoadingAssetPreviews()) Repaint();
            return;
        }

        double now = EditorApplication.timeSinceStartup;

        // Cycle loading dots every 0.4 s
        if (now - lastDotTime >= 0.4)
        {
            lastDotTime = now;
            dotCount = (dotCount % 3) + 1;
            if (generatingMsgIdx >= 0 && generatingMsgIdx < messages.Count)
            {
                string dots = new string('.', dotCount);
                messages[generatingMsgIdx].Text = "Generating assets" + dots;
            }
            Repaint();
        }

        // After 2 seconds, show weapon cards in the chat window
        if (now - generateStartTime >= 2.0)
        {
            isGenerating = false;
            ShowWeaponAssets();
        }
    }

    // -- Agent phase state machine -------------------------------------------
    private void OnEditorAgentUpdate()
    {
        double now = EditorApplication.timeSinceStartup;

        if (agentPhase == AgentPhase.ChangingScene)
        {
            // -- Bootstrap: collect children & enable wireframe ---------------
            if (agentStepIdx == -1)
            {
                var initGO = GameObject.Find("init");
                _initChildren.Clear();
                if (initGO != null)
                    foreach (Transform t in initGO.transform) _initChildren.Add(t.gameObject);

                var toreachGO = GameObject.Find("toreach");
                _toreachChildren.Clear();
                if (toreachGO != null)
                    foreach (Transform t in toreachGO.transform) _toreachChildren.Add(t.gameObject);

                SetWireframeMode(true);
                agentStepIdx = 0;
                agentStepTime = now;
                return;
            }

            // -- Interleave: each tick hides one init child AND shows one toreach child
            // agentStepIdx counts how many pairs/singles have been processed.
            // We continue until both lists are exhausted.
            int maxSteps = Mathf.Max(_initChildren.Count, _toreachChildren.Count);
            if (agentStepIdx < maxSteps)
            {
                if (now - agentStepTime >= ToggleDelay)
                {
                    if (agentStepIdx < _initChildren.Count)
                        _initChildren[agentStepIdx].SetActive(false);

                    if (agentStepIdx < _toreachChildren.Count)
                        _toreachChildren[agentStepIdx].SetActive(true);

                    agentStepIdx++;
                    agentStepTime = now;
                    Repaint();
                }
                return;
            }

            // -- All toggles done: restore shaded view, move to adjusting -----
            SetWireframeMode(false);
            agentPhase = AgentPhase.AdjustingScene;
            agentStepIdx = 0;
            agentStepTime = now;
            Repaint();
        }
        else if (agentPhase == AgentPhase.AdjustingScene)
        {
            if (agentStepIdx < AdjustToolPaths.Length)
            {
                if (now - agentStepTime >= ToolDelay)
                {
                    AddBotMsg(AdjustToolMessages[agentStepIdx]);
                    EditorApplication.ExecuteMenuItem(AdjustToolPaths[agentStepIdx]);
                    agentStepIdx++;
                    agentStepTime = now;
                    Repaint();
                }
            }
            else
            {
                if (now - agentStepTime >= ToolDelay)
                {
                    AddBotMsg("Your scene is ready!");
                    agentPhase = AgentPhase.Done;
                    agentStepTime = now;
                    Repaint();
                }
            }
        }
        else if (agentPhase == AgentPhase.Done)
        {
            agentPhase = AgentPhase.None;
        }
    }

    private void UpdateBotMsg(string text)
    {
        if (agentBotMsgIdx >= 0 && agentBotMsgIdx < messages.Count)
            messages[agentBotMsgIdx].Text = text;
    }

    private void AddBotMsg(string text)
    {
        messages.Add(new ChatMessage(text, isUser: false));
        agentBotMsgIdx = messages.Count - 1;
        scrollPos.y = float.MaxValue;
    }

    private static void SetWireframeMode(bool enable)
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        sv.cameraMode = SceneView.GetBuiltinCameraMode(
            enable ? DrawCameraMode.TexturedWire : DrawCameraMode.Textured);
        sv.Repaint();
    }
}
