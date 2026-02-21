using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI Chat Window - Editor plugin stub for AI-driven scene control.
/// Provides a chat interface, mode selector, and a scene-switcher button.
/// </summary>
public class AIChatWindow : EditorWindow
{
    // ── Chat mode ────────────────────────────────────────────────────────────
    private enum ChatMode { AssetGeneration, Agent, Selection }

    private static readonly string[] ChatModeLabels =
    {
        "Asset Generation",
        "Agent",
        "Selection"
    };

    private ChatMode currentMode = ChatMode.Agent;

    // ── Asset-generation model tiers ─────────────────────────────────────────
    // Format: (display label, internal id, usage multiplier label)
    private static readonly (string Label, string Id, string Tier)[] AssetModels =
    {
        // ── Budget  (x0.33) ──────────────────────────────────────────────────
        ("Stable Diffusion 1.5   ×0.33",  "sd15",         "×0.33"),
        ("Shap-E                 ×0.33",  "shape",        "×0.33"),
        ("Point-E                ×0.33",  "pointe",       "×0.33"),
        // ── Standard  (x1) ───────────────────────────────────────────────────
        ("TripoSR                ×1",     "triposr",      "×1"),
        ("Zero123++              ×1",     "zero123pp",    "×1"),
        ("DreamGaussian          ×1",     "dreamgaussian","×1"),
        // ── Premium  (x3) ────────────────────────────────────────────────────
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

    // ── Chat state ───────────────────────────────────────────────────────────
    private string inputText = "";
    private Vector2 scrollPos;
    private readonly List<ChatMessage> messages = new List<ChatMessage>();

    // ── Scene switching ──────────────────────────────────────────────────────
    private const string SCENE_A = "Assets/Scenes/SampleScene.unity";
    private const string SCENE_B = "Assets/Scenes/DemoScene.unity";
    private bool isOnSceneA = true;

    // ── Styles (lazy-initialised) ────────────────────────────────────────────
    private GUIStyle bubbleStyleUser;
    private GUIStyle bubbleStyleBot;
    private GUIStyle inputAreaStyle;
    private GUIStyle tierBudgetStyle;
    private GUIStyle tierStandardStyle;
    private GUIStyle tierPremiumStyle;
    private GUIStyle modeBadgeStyle;
    private bool stylesInitialised;

    // ── Tier colour map ──────────────────────────────────────────────────────
    private static readonly Color ColBudget   = new Color(0.35f, 0.75f, 0.35f);
    private static readonly Color ColStandard = new Color(0.25f, 0.55f, 0.95f);
    private static readonly Color ColPremium  = new Color(0.85f, 0.50f, 0.15f);

    // ── Entry point ──────────────────────────────────────────────────────────
    [MenuItem("AI Chat/Open Chat Window")]
    public static void ShowWindow()
    {
        var win = GetWindow<AIChatWindow>("AI Chat");
        win.minSize = new Vector2(420, 560);
        win.Show();
    }

    // ── Initialise styles once ───────────────────────────────────────────────
    private void InitStyles()
    {
        if (stylesInitialised) return;

        bubbleStyleUser = new GUIStyle(EditorStyles.helpBox)
        {
            wordWrap  = true,
            alignment = TextAnchor.MiddleRight,
            fontSize  = 12,
            padding   = new RectOffset(8, 8, 6, 6)
        };
        bubbleStyleUser.normal.textColor = new Color(0.15f, 0.45f, 0.85f);

        bubbleStyleBot = new GUIStyle(EditorStyles.helpBox)
        {
            wordWrap  = true,
            alignment = TextAnchor.MiddleLeft,
            fontSize  = 12,
            padding   = new RectOffset(8, 8, 6, 6)
        };
        bubbleStyleBot.normal.textColor = new Color(0.15f, 0.65f, 0.3f);

        inputAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            fontSize = 12
        };

        // Tier badge styles
        tierBudgetStyle = MakeBadgeStyle(ColBudget);
        tierStandardStyle = MakeBadgeStyle(ColStandard);
        tierPremiumStyle = MakeBadgeStyle(ColPremium);

        modeBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            padding   = new RectOffset(6, 6, 2, 2)
        };

        stylesInitialised = true;
    }

    private static GUIStyle MakeBadgeStyle(Color textColor)
    {
        var s = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            padding   = new RectOffset(4, 4, 2, 2)
        };
        s.normal.textColor = textColor;
        return s;
    }

    // Fallback for when badge styles haven't been initialised yet
    private GUIStyle SafeBadge => tierBudgetStyle ?? EditorStyles.miniLabel;

    // ── Main GUI ─────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        InitStyles();
        Color prevBg = GUI.backgroundColor;

        // ── Header ──────────────────────────────────────────────────────────
        EditorGUILayout.Space(6);
        GUILayout.Label("AI Scene Assistant  (stub)", EditorStyles.boldLabel);
        DrawHorizontalLine();

        // ── Chat history ─────────────────────────────────────────────────────
        // Reserved: header ~30, mode row ~52, input row ~58, dividers ~32
        float reserved = 30 + 52 + 58 + 32;
        float chatHeight = Mathf.Max(position.height - reserved, 80);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(chatHeight));

        foreach (var msg in messages)
        {
            GUIStyle style = msg.IsUser ? bubbleStyleUser : bubbleStyleBot;
            string prefix  = msg.IsUser ? "You: " : "AI:  ";

            if (msg.IsUser)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(prefix + msg.Text, style, GUILayout.MaxWidth(position.width * 0.78f));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(prefix + msg.Text, style, GUILayout.MaxWidth(position.width * 0.78f));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(3);
        }

        EditorGUILayout.EndScrollView();

        DrawHorizontalLine();

        // ── Mode selector row (above input) ──────────────────────────────────
        DrawModeSelector(prevBg);

        DrawHorizontalLine();

        // ── Input row ────────────────────────────────────────────────────────
        EditorGUILayout.BeginHorizontal();

        GUI.SetNextControlName("ChatInput");
        inputText = EditorGUILayout.TextArea(inputText, inputAreaStyle,
            GUILayout.Height(46), GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginVertical(GUILayout.Width(64));
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f);
        bool sendPressed = GUILayout.Button("Send", GUILayout.Height(46));
        GUI.backgroundColor = prevBg;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);

        // ── Send on button or Ctrl/Cmd+Enter ────────────────────────────────
        bool ctrlEnter = Event.current.type == EventType.KeyDown
                      && Event.current.keyCode == KeyCode.Return
                      && (Event.current.control || Event.current.command);

        if ((sendPressed || ctrlEnter) && !string.IsNullOrWhiteSpace(inputText))
        {
            SendMessage(inputText.Trim());
            if (ctrlEnter) Event.current.Use();
        }
    }

    // ── Mode selector UI ─────────────────────────────────────────────────────
    // IMPORTANT: every branch must draw EXACTLY the same number of controls
    // so that IMGUI's Layout and Repaint passes stay in sync.
    private void DrawModeSelector(Color prevBg)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        // ── Mode label + popup (always rendered) ─────────────────────────────
        EditorGUILayout.LabelField("Mode", GUILayout.Width(38));
        int newMode = EditorGUILayout.Popup((int)currentMode, ChatModeLabels, GUILayout.Height(20));
        // Defer the state change to the end of the current event to keep
        // Layout / Repaint control counts identical within one frame.
        if (newMode != (int)currentMode)
            EditorApplication.delayCall += () => { currentMode = (ChatMode)newMode; Repaint(); };

        GUILayout.Space(6);

        // ── Model picker (always rendered; disabled unless in Asset Generation) ─
        bool assetMode = currentMode == ChatMode.AssetGeneration;
        EditorGUI.BeginDisabledGroup(!assetMode);

        EditorGUILayout.LabelField("Model", GUILayout.Width(40));
        int newModel = EditorGUILayout.Popup(selectedModelIndex, AssetModelLabels, GUILayout.Height(20));
        if (newModel != selectedModelIndex)
            selectedModelIndex = newModel;

        // Tier badge — always drawn so control count is constant
        string tier = AssetModels[selectedModelIndex].Tier;
        GUIStyle badgeStyle;
        string   tierLabel;
        if (tier == "\u00d70.33")      { badgeStyle = tierBudgetStyle   ?? SafeBadge; tierLabel = "BUDGET"; }
        else if (tier == "\u00d71")    { badgeStyle = tierStandardStyle ?? SafeBadge; tierLabel = "STANDARD"; }
        else                           { badgeStyle = tierPremiumStyle  ?? SafeBadge; tierLabel = "PREMIUM"; }
        GUILayout.Label(tierLabel, badgeStyle, GUILayout.Width(58));

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        // ── Hint line ─────────────────────────────────────────────────────────
        EditorGUILayout.Space(2);
        string hint;
        if (currentMode == ChatMode.AssetGeneration)
            hint = $"Generate assets · {AssetModels[selectedModelIndex].Id}  ({AssetModels[selectedModelIndex].Tier} usage)";
        else if (currentMode == ChatMode.Agent)
            hint = "Agent mode — AI can read and modify the active scene.";
        else
            hint = "Selection mode — AI operates on the currently selected objects.";
        EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }

    // ── Chat helpers ──────────────────────────────────────────────────────────
    private void SendMessage(string text)
    {
        messages.Add(new ChatMessage(text, isUser: true));
        inputText = "";
        GUI.FocusControl("ChatInput");

        // Switch scene on every send (stub behaviour — simulates AI taking action)
        string targetScene = isOnSceneA ? SCENE_B : SCENE_A;
        string targetName  = isOnSceneA ? "DemoScene" : "SampleScene";
        SwitchScene(targetScene);

        string aiReply = GenerateStubReply(text, targetName);
        messages.Add(new ChatMessage(aiReply, isUser: false));

        scrollPos.y = float.MaxValue;
        Repaint();
    }

    private string GenerateStubReply(string userText, string switchedTo)
    {
        string lower = userText.ToLower();

        if (lower.Contains("hello") || lower.Contains("hi"))
            return $"Hello! I've switched the scene to \"{switchedTo}\" as a demo action.";

        if (lower.Contains("help"))
            return "Send any message and I'll switch the active scene as a proof-of-concept. Real AI editing is coming soon!";

        return currentMode switch
        {
            ChatMode.AssetGeneration =>
                $"[Asset Generation — {AssetModels[selectedModelIndex].Id} / {AssetModels[selectedModelIndex].Tier}] Switched scene to \"{switchedTo}\". Asset generation coming soon!",
            ChatMode.Agent =>
                $"[Agent] Switched scene to \"{switchedTo}\". Full scene-editing capabilities coming soon!",
            ChatMode.Selection =>
                $"[Selection] Switched scene to \"{switchedTo}\". Selection-based editing coming soon!",
            _ => $"Switched scene to \"{switchedTo}\"."
        };
    }

    private void SwitchScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            isOnSceneA = scenePath == SCENE_A;
            Repaint();
        }
    }

    private static void DrawHorizontalLine()
    {
        EditorGUILayout.Space(4);
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f, 0.5f));
        EditorGUILayout.Space(4);
    }

    // ── Data struct ───────────────────────────────────────────────────────────
    private readonly struct ChatMessage
    {
        public readonly string Text;
        public readonly bool   IsUser;
        public ChatMessage(string text, bool isUser) { Text = text; IsUser = isUser; }
    }
}
