using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

/// <summary>
/// AI Chat Window - Editor plugin stub for AI-driven scene control.
/// Provides a chat interface, mode selector, and a scene-switcher button.
/// </summary>
public class AIChatWindow : EditorWindow
{
    private const string ApiKeyPrefKey = "AIChatWindow.ApiKey";
    private const bool DebugPasteLogs = false;

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
    private readonly List<ImageAttachment> pendingAttachments = new List<ImageAttachment>();

    // ── Scene switching ──────────────────────────────────────────────────────
    private const string SCENE_A = "Assets/Scenes/SampleScene.unity";
    private const string SCENE_B = "Assets/Scenes/DemoScene.unity";
    private bool isOnSceneA = true;

    // ── Asset generation loading state ───────────────────────────────────────
    private static readonly string[] WeaponPrefabPaths =
    {
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 1.prefab",
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 2.prefab",
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 3.prefab",
        "Assets/Stylized Modular Weapons/Prefabs/Your Weapon Example 4.prefab",
    };

    private bool   isGenerating     = false;
    private double generateStartTime;
    private int    generatingMsgIdx = -1;
    private double lastDotTime;
    private int    dotCount         = 1;

    // ── Agent mode state machine ──────────────────────────────────────────────
    private enum AgentPhase { None, ChangingScene, AdjustingScene, Done }
    private AgentPhase   agentPhase      = AgentPhase.None;
    private double       agentStepTime;
    private int          agentStepIdx;
    private int          agentBotMsgIdx  = -1;
    private List<GameObject> _initChildren    = new List<GameObject>();
    private List<GameObject> _toreachChildren = new List<GameObject>();

    private const float ToggleDelay = 0.15f;
    private const float ToolDelay   = 2.0f;

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

    // ── Styles (lazy-initialised) ────────────────────────────────────────────
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

    // ── Editor update subscription ──────────────────────────────────────────
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
        // ── Agent phase pump ─────────────────────────────────────────────────
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

    // ── Agent phase state machine ─────────────────────────────────────────────
    private void OnEditorAgentUpdate()
    {
        double now = EditorApplication.timeSinceStartup;

        if (agentPhase == AgentPhase.ChangingScene)
        {
            // ── Bootstrap: collect children & enable wireframe ────────────────
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

            // ── Interleave: each tick hides one init child AND shows one toreach child
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

            // ── All toggles done: restore shaded view, move to adjusting ──────
            SetWireframeMode(false);
            agentPhase   = AgentPhase.AdjustingScene;
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
                    agentPhase   = AgentPhase.Done;
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

    // ── Show 4 weapon assets as cards inside the chat window ───────────────────
    private void ShowWeaponAssets()
    {
        var cards = new List<AssetCard>();

        foreach (var path in WeaponPrefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            // Kick off async preview generation
            AssetPreview.GetAssetPreview(prefab);
            cards.Add(new AssetCard(prefab.name, path));
        }

        string headerText = cards.Count > 0
            ? $"Here are {cards.Count} weapon assets from your project:"
            : "Asset generation complete, but no prefabs could be loaded.";

        var resultMsg = new ChatMessage(headerText, isUser: false);
        if (cards.Count > 0) resultMsg.AssetCards = cards;

        if (generatingMsgIdx >= 0 && generatingMsgIdx < messages.Count)
            messages[generatingMsgIdx] = resultMsg;
        else
            messages.Add(resultMsg);

        generatingMsgIdx = -1;
        scrollPos.y = float.MaxValue;
        Repaint();
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

        if (!HasApiKey())
        {
            DrawApiKeyGate();
            return;
        }

        // Handle image paste early so focused text controls don't consume
        // the paste event before we can inspect clipboard image data.
        HandleClipboardPaste();

        Color prevBg = GUI.backgroundColor;

        // ── Header ──────────────────────────────────────────────────────────
        EditorGUILayout.Space(6);
        GUILayout.Label("DAMN 3D Editor", EditorStyles.boldLabel);
        DrawHorizontalLine();

        // ── Chat history ─────────────────────────────────────────────────────
        // Let IMGUI allocate the remaining vertical space so bottom controls
        // (mode, attachments, input) do not overflow out of the window.
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

        foreach (var msg in messages)
        {
            GUIStyle style = msg.IsUser ? bubbleStyleUser : bubbleStyleBot;
            string prefix  = msg.IsUser ? "You: " : "AI:  ";

            if (msg.IsUser)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(position.width * 0.78f));
                if (!string.IsNullOrWhiteSpace(msg.Text))
                    GUILayout.Label(prefix + msg.Text, style, GUILayout.MaxWidth(position.width * 0.72f));

                DrawMessageAttachments(msg.ImageAttachments);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            else if (msg.AssetCards != null && msg.AssetCards.Count > 0)
            {
                // ── Asset card message ──────────────────────────────────────
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("AI:  " + msg.Text, bubbleStyleBot);
                EditorGUILayout.Space(4);

                // Draw cards in a horizontal strip
                EditorGUILayout.BeginHorizontal();
                foreach (var card in msg.AssetCards)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(88));

                    // Thumbnail — clickable button that adds the prefab to the scene
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(card.Path);
                    Texture2D thumb = prefab != null ? AssetPreview.GetAssetPreview(prefab) : null;
                    if (thumb == null && prefab != null) thumb = AssetPreview.GetMiniThumbnail(prefab);

                    Rect thumbRect = GUILayoutUtility.GetRect(80, 80,
                        GUILayout.Width(80), GUILayout.Height(80));

                    // Highlight on hover
                    if (thumbRect.Contains(Event.current.mousePosition))
                        EditorGUI.DrawRect(thumbRect, new Color(1f, 1f, 1f, 0.08f));

                    if (thumb != null)
                        GUI.DrawTexture(thumbRect, thumb, ScaleMode.ScaleToFit);
                    else
                        EditorGUI.DrawRect(thumbRect, new Color(0.25f, 0.25f, 0.25f, 0.8f));

                    // Overlay "Added ✓" badge if already placed
                    if (card.AddedToScene)
                    {
                        var badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontStyle = FontStyle.Bold,
                            fontSize  = 9
                        };
                        badgeStyle.normal.textColor = Color.white;
                        var badgeRect = new Rect(thumbRect.x, thumbRect.yMax - 16, thumbRect.width, 16);
                        EditorGUI.DrawRect(badgeRect, new Color(0.1f, 0.6f, 0.1f, 0.85f));
                        GUI.Label(badgeRect, "Added ✓", badgeStyle);
                    }

                    // Detect click on thumbnail
                    if (Event.current.type == EventType.MouseDown && thumbRect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                        if (prefab != null)
                        {
                            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                            // Place at a slight offset so multiple placements don't overlap
                            instance.transform.position = Vector3.zero;
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                            Selection.activeGameObject = instance;
                            card.AddedToScene = true;
                            Repaint();
                        }
                    }

                    // Name label (word-wrap)
                    var nameStyle = new GUIStyle(EditorStyles.miniLabel)
                    { wordWrap = true, alignment = TextAnchor.MiddleCenter };
                    GUILayout.Label(card.Name, nameStyle, GUILayout.Width(80));

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(4);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(position.width * 0.78f));

                if (!string.IsNullOrWhiteSpace(msg.Text))
                    GUILayout.Label(prefix + msg.Text, style, GUILayout.MaxWidth(position.width * 0.72f));

                DrawMessageAttachments(msg.ImageAttachments);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(3);
        }

        EditorGUILayout.EndScrollView();

        // Keep composer controls pinned to the bottom like a chat UI.
        GUILayout.FlexibleSpace();

        DrawHorizontalLine();

        // ── Mode selector row (above input) ──────────────────────────────────
        DrawModeSelector(prevBg);

        DrawHorizontalLine();

        // ── Attachment preview row (only when images are attached) ─────────
        if (pendingAttachments.Count > 0)
            DrawAttachmentComposer();

        // ── Input row ────────────────────────────────────────────────────────
        EditorGUILayout.BeginHorizontal();

        GUI.SetNextControlName("ChatInput");
        inputText = EditorGUILayout.TextArea(inputText, inputAreaStyle,
            GUILayout.Height(46), GUILayout.ExpandWidth(true));
        Rect inputRect = GUILayoutUtility.GetLastRect();

        EditorGUILayout.BeginVertical(GUILayout.Width(64));
        GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f);
        bool sendPressed = GUILayout.Button("Send", GUILayout.Height(46));
        GUI.backgroundColor = prevBg;
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);

        HandleDragAndDropImages(inputRect);

        // ── Send on button or Ctrl/Cmd+Enter ────────────────────────────────
        bool ctrlEnter = Event.current.type == EventType.KeyDown
                      && Event.current.keyCode == KeyCode.Return
                      && (Event.current.control || Event.current.command);

        bool hasText = !string.IsNullOrWhiteSpace(inputText);
        bool hasAttachments = pendingAttachments.Count > 0;

        if ((sendPressed || ctrlEnter) && (hasText || hasAttachments))
        {
            SendMessage(inputText.Trim(), pendingAttachments);
            if (ctrlEnter) Event.current.Use();
        }
    }

    private bool HasApiKey()
    {
        return !string.IsNullOrWhiteSpace(EditorPrefs.GetString(ApiKeyPrefKey, ""));
    }

    private void DrawApiKeyGate()
    {
        EditorGUILayout.Space(8);
        GUILayout.Label("DAMN 3D Editor", EditorStyles.boldLabel);
        DrawHorizontalLine();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("API Key Required", EditorStyles.boldLabel);
        GUILayout.Label("Enter your API key to unlock the editor window.", EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.Space(6);
        GUI.SetNextControlName("ApiKeyInput");
        apiKeyInput = EditorGUILayout.PasswordField("API Key", apiKeyInput);

        if (!string.IsNullOrEmpty(apiKeyError))
            EditorGUILayout.HelpBox(apiKeyError, MessageType.Error);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save Key", GUILayout.Width(100), GUILayout.Height(24)))
        {
            SaveApiKey();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            SaveApiKey();
            Event.current.Use();
        }
    }

    private void SaveApiKey()
    {
        string trimmed = (apiKeyInput ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            apiKeyError = "API key cannot be empty.";
            Repaint();
            return;
        }

        EditorPrefs.SetString(ApiKeyPrefKey, trimmed);
        apiKeyInput = trimmed;
        apiKeyError = "";
        GUI.FocusControl(null);
        Repaint();
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
            EditorApplication.delayCall += () =>
            {
                currentMode      = (ChatMode)newMode;
                messages.Clear();
                isGenerating     = false;
                generatingMsgIdx = -1;
                scrollPos        = Vector2.zero;
                Repaint();
            };

        GUILayout.Space(6);

        // ── Model picker (only shown in Asset Generation mode) ────────────────
        if (currentMode == ChatMode.AssetGeneration)
        {
            EditorGUILayout.LabelField("Model", GUILayout.Width(40));
            int newModel = EditorGUILayout.Popup(selectedModelIndex, AssetModelLabels, GUILayout.Height(20));
            if (newModel != selectedModelIndex)
                selectedModelIndex = newModel;

            string tier = AssetModels[selectedModelIndex].Tier;
            GUIStyle badgeStyle;
            string   tierLabel;
            if (tier == "\u00d70.33")      { badgeStyle = tierBudgetStyle   ?? SafeBadge; tierLabel = "BUDGET"; }
            else if (tier == "\u00d71")    { badgeStyle = tierStandardStyle ?? SafeBadge; tierLabel = "STANDARD"; }
            else                           { badgeStyle = tierPremiumStyle  ?? SafeBadge; tierLabel = "PREMIUM"; }
            GUILayout.Label(tierLabel, badgeStyle, GUILayout.Width(58));
        }

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
    private void SendMessage(string text, List<ImageAttachment> attachments)
    {
        var sentAttachments = new List<ImageAttachment>(attachments);
        messages.Add(new ChatMessage(text, isUser: true, sentAttachments));

        inputText = "";
        attachments.Clear();
        GUI.FocusControl("ChatInput");

        // ── Asset Generation mode: show animated loading, then spawn weapons ──
        if (currentMode == ChatMode.AssetGeneration)
        {
            messages.Add(new ChatMessage("Generating assets.", isUser: false));
            generatingMsgIdx  = messages.Count - 1;
            isGenerating      = true;
            generateStartTime = EditorApplication.timeSinceStartup;
            lastDotTime       = generateStartTime;
            dotCount          = 1;
            scrollPos.y       = float.MaxValue;
            Repaint();
            return;
        }

        // ── Agent mode: run the scene-change + tool sequence ─────────────────
        if (currentMode == ChatMode.Agent)
        {
            messages.Add(new ChatMessage("Changing scene...", isUser: false));
            agentBotMsgIdx = messages.Count - 1;
            agentPhase     = AgentPhase.ChangingScene;
            agentStepIdx   = -1;
            agentStepTime  = EditorApplication.timeSinceStartup;
            scrollPos.y    = float.MaxValue;
            Repaint();
            return;
        }

        // Switch scene on every send (stub behaviour — simulates AI taking action)
        string targetScene = isOnSceneA ? SCENE_B : SCENE_A;
        string targetName  = isOnSceneA ? "DemoScene" : "SampleScene";
        SwitchScene(targetScene);

        string aiReply = GenerateStubReply(text, targetName);
        messages.Add(new ChatMessage(aiReply, isUser: false));

        scrollPos.y = float.MaxValue;
        Repaint();
    }

    private void DrawAttachmentComposer()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Attachments", EditorStyles.miniBoldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Paste image with Ctrl/Cmd+V", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        if (pendingAttachments.Count == 0)
        {
            GUILayout.Label("No images attached", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < pendingAttachments.Count; i++)
            {
                var attachment = pendingAttachments[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(78));

                Rect thumbRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
                if (attachment.Texture != null)
                    GUI.DrawTexture(thumbRect, attachment.Texture, ScaleMode.ScaleToFit);
                else
                    EditorGUI.DrawRect(thumbRect, new Color(0.25f, 0.25f, 0.25f, 0.9f));

                Rect removeRect = new Rect(thumbRect.xMax - 18, thumbRect.y + 2, 16, 16);
                if (GUI.Button(removeRect, "x", EditorStyles.miniButton))
                {
                    if (attachment.IsRuntime && attachment.Texture != null)
                        DestroyImmediate(attachment.Texture);

                    pendingAttachments.RemoveAt(i);
                    Repaint();
                    i--;
                    EditorGUILayout.EndVertical();
                    continue;
                }

                var nameStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
                GUILayout.Label(attachment.Name, nameStyle, GUILayout.Width(64));
                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    private void DrawMessageAttachments(List<ImageAttachment> attachments)
    {
        if (attachments == null || attachments.Count == 0) return;

        EditorGUILayout.Space(3);
        EditorGUILayout.BeginHorizontal();

        foreach (var attachment in attachments)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(88));

            Rect thumbRect = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80), GUILayout.Height(80));
            if (attachment.Texture != null)
                GUI.DrawTexture(thumbRect, attachment.Texture, ScaleMode.ScaleToFit);
            else
                EditorGUI.DrawRect(thumbRect, new Color(0.25f, 0.25f, 0.25f, 0.9f));

            var nameStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label(attachment.Name, nameStyle, GUILayout.Width(80));
            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void HandleClipboardPaste()
    {
        Event evt = Event.current;
        if (evt == null) return;

        bool pasteValidate = evt.type == EventType.ValidateCommand && evt.commandName == "Paste";
        bool pasteCommand = evt.type == EventType.ExecuteCommand && evt.commandName == "Paste";
        bool pastePressed = evt.type == EventType.KeyDown
                        && evt.keyCode == KeyCode.V
                        && (evt.control || evt.command);
        bool shiftInsert = evt.type == EventType.KeyDown
                        && evt.keyCode == KeyCode.Insert
                        && evt.shift;

        bool wantsPaste = pasteValidate || pasteCommand || pastePressed || shiftInsert;
        if (!wantsPaste) return;

        if (DebugPasteLogs)
        {
            Debug.Log($"[AIChatWindow] Paste event detected. type={evt.type}, command={evt.commandName}, key={evt.keyCode}, ctrl={evt.control}, cmd={evt.command}, shift={evt.shift}");
        }

        if (TryAddImageFromClipboard())
        {
            if (DebugPasteLogs)
                Debug.Log($"[AIChatWindow] Image attachment added. pendingAttachments={pendingAttachments.Count}");
            evt.Use();
            Repaint();
        }
        else if (DebugPasteLogs)
        {
            Debug.LogWarning("[AIChatWindow] Paste triggered but no clipboard image could be imported.");
        }
    }

    private bool TryAddImageFromClipboard()
    {
        if (DebugPasteLogs)
            Debug.Log("[AIChatWindow] Trying clipboard path/image import...");

        if (TryAddImageFromClipboardPath())
        {
            if (DebugPasteLogs)
                Debug.Log("[AIChatWindow] Imported image from clipboard path/text.");
            return true;
        }

#if UNITY_EDITOR_WIN
        if (DebugPasteLogs)
            Debug.Log("[AIChatWindow] Clipboard path import failed. Trying Windows clipboard image APIs...");

        bool ok = TryAddImageFromWindowsClipboard();
        if (DebugPasteLogs)
            Debug.Log($"[AIChatWindow] Windows clipboard import result={ok}");
        return ok;
#else
        if (DebugPasteLogs)
            Debug.LogWarning("[AIChatWindow] Platform is not Windows in this build path; no native clipboard image fallback available.");
        return false;
#endif
    }

    private bool TryAddImageFromClipboardPath()
    {
        string clip = EditorGUIUtility.systemCopyBuffer;
        if (DebugPasteLogs)
            Debug.Log($"[AIChatWindow] systemCopyBuffer length={(clip == null ? 0 : clip.Length)}");

        if (string.IsNullOrWhiteSpace(clip))
        {
            if (DebugPasteLogs)
                Debug.Log("[AIChatWindow] systemCopyBuffer is empty/whitespace.");
            return false;
        }

        foreach (string raw in clip.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = raw.Trim().Trim('"');
            if (DebugPasteLogs)
                Debug.Log($"[AIChatWindow] Clipboard line candidate: {candidate}");

            if (TryAddImageFromPath(candidate))
            return true;
        }

        if (DebugPasteLogs)
            Debug.Log("[AIChatWindow] No valid image file paths found in systemCopyBuffer.");

        return false;
    }

#if UNITY_EDITOR_WIN
    private bool TryAddImageFromWindowsClipboard()
    {
        try
        {
            byte[] bytes = TryReadWindowsClipboardImageBytes();
            if (bytes == null || bytes.Length == 0)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] Windows clipboard returned no image bytes.");
                return false;
            }

            if (DebugPasteLogs)
                Debug.Log($"[AIChatWindow] Windows clipboard image bytes length={bytes.Length}");

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, bytes, false))
            {
                if (DebugPasteLogs)
                    Debug.LogWarning("[AIChatWindow] ImageConversion.LoadImage failed for Windows clipboard image bytes.");
                DestroyImmediate(tex);
                return false;
            }

            pendingAttachments.Add(new ImageAttachment(tex, "clipboard-image.png", isRuntime: true));
            if (DebugPasteLogs)
                Debug.Log("[AIChatWindow] Added image from Windows clipboard image bytes.");
            return true;
        }
        catch (Exception ex)
        {
            if (DebugPasteLogs)
                Debug.LogWarning($"[AIChatWindow] TryAddImageFromWindowsClipboard exception: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    private static byte[] TryReadWindowsClipboardImageBytes()
    {
        byte[] pngBytes = null;

        var staThread = new Thread(() =>
        {
            try
            {
                Type clipboardType = ResolveType("System.Windows.Forms.Clipboard", "System.Windows.Forms");
                if (clipboardType == null)
                {
                    if (DebugPasteLogs)
                        Debug.LogWarning("[AIChatWindow] Could not resolve System.Windows.Forms.Clipboard type.");
                    TryReadWpfClipboardImageBytes(ref pngBytes);
                    return;
                }

                MethodInfo getDataObject = clipboardType.GetMethod("GetDataObject", BindingFlags.Public | BindingFlags.Static);
                if (getDataObject != null && DebugPasteLogs)
                {
                    object dataObject = getDataObject.Invoke(null, null);
                    if (dataObject != null)
                    {
                        MethodInfo getFormats = dataObject.GetType().GetMethod("GetFormats", Type.EmptyTypes);
                        if (getFormats != null)
                        {
                            string[] formats = getFormats.Invoke(dataObject, null) as string[];
                            if (formats != null && formats.Length > 0)
                                Debug.Log("[AIChatWindow] WinForms clipboard formats: " + string.Join(", ", formats));
                            else
                                Debug.Log("[AIChatWindow] WinForms clipboard formats: <none>");
                        }
                    }
                }

                // Some apps place image data as raw PNG data but not as
                // Clipboard.ContainsImage() bitmap. Try this path first.
                MethodInfo containsData = clipboardType.GetMethod("ContainsData", new[] { typeof(string) });
                MethodInfo getData = clipboardType.GetMethod("GetData", new[] { typeof(string) });
                if (containsData != null && getData != null)
                {
                    bool hasPng = (bool)containsData.Invoke(null, new object[] { "PNG" });
                    if (DebugPasteLogs)
                        Debug.Log($"[AIChatWindow] Clipboard.ContainsData('PNG')={hasPng}");
                    if (hasPng)
                    {
                        object pngData = getData.Invoke(null, new object[] { "PNG" });
                        if (pngData is MemoryStream pngStream)
                        {
                            pngBytes = pngStream.ToArray();
                            if (DebugPasteLogs)
                                Debug.Log($"[AIChatWindow] Read PNG clipboard data from MemoryStream. bytes={pngBytes.Length}");
                            return;
                        }

                        if (pngData is byte[] rawPng)
                        {
                            pngBytes = rawPng;
                            if (DebugPasteLogs)
                                Debug.Log($"[AIChatWindow] Read PNG clipboard data from byte[]. bytes={pngBytes.Length}");
                            return;
                        }

                        if (DebugPasteLogs)
                            Debug.Log($"[AIChatWindow] Clipboard PNG payload type={pngData?.GetType().FullName ?? "null"}");
                    }
                }
                else if (DebugPasteLogs)
                {
                    Debug.Log("[AIChatWindow] Clipboard.ContainsData/GetData methods unavailable.");
                }

                MethodInfo containsImage = clipboardType.GetMethod("ContainsImage", BindingFlags.Public | BindingFlags.Static);
                if (containsImage == null)
                {
                    if (DebugPasteLogs)
                        Debug.LogWarning("[AIChatWindow] Clipboard.ContainsImage method unavailable.");
                    return;
                }

                bool hasImage = (bool)containsImage.Invoke(null, null);
                if (DebugPasteLogs)
                    Debug.Log($"[AIChatWindow] Clipboard.ContainsImage()={hasImage}");
                if (!hasImage)
                {
                    TryReadWpfClipboardImageBytes(ref pngBytes);
                    return;
                }

                MethodInfo getImage = clipboardType.GetMethod("GetImage", BindingFlags.Public | BindingFlags.Static);
                if (getImage == null)
                {
                    if (DebugPasteLogs)
                        Debug.LogWarning("[AIChatWindow] Clipboard.GetImage method unavailable.");
                    return;
                }

                object imageObj = getImage.Invoke(null, null);
                if (imageObj == null)
                {
                    if (DebugPasteLogs)
                        Debug.LogWarning("[AIChatWindow] Clipboard.GetImage returned null.");
                    return;
                }

                Type imageFormatType = ResolveType("System.Drawing.Imaging.ImageFormat", "System.Drawing")
                                     ?? ResolveType("System.Drawing.Imaging.ImageFormat", "System.Drawing.Common");
                if (imageFormatType == null)
                {
                    if (DebugPasteLogs)
                        Debug.LogWarning("[AIChatWindow] Could not resolve System.Drawing.Imaging.ImageFormat type.");
                    return;
                }

                object pngFormat = imageFormatType.GetProperty("Png", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (pngFormat == null)
                {
                    if (DebugPasteLogs)
                        Debug.LogWarning("[AIChatWindow] ImageFormat.Png resolved to null.");
                    return;
                }

                using (var stream = new MemoryStream())
                {
                    MethodInfo save = imageObj.GetType().GetMethod("Save", new[] { typeof(Stream), imageFormatType });
                    if (save == null)
                    {
                        if (DebugPasteLogs)
                            Debug.LogWarning("[AIChatWindow] Could not find Save(Stream, ImageFormat) on clipboard image object.");
                        return;
                    }

                    save.Invoke(imageObj, new[] { (object)stream, pngFormat });
                    pngBytes = stream.ToArray();
                    if (DebugPasteLogs)
                        Debug.Log($"[AIChatWindow] Converted clipboard bitmap to PNG bytes. bytes={pngBytes.Length}");
                }

                if (imageObj is IDisposable disposable)
                    disposable.Dispose();
            }
            catch (Exception ex)
            {
                // Swallow clipboard access errors and report as no-image.
                if (DebugPasteLogs)
                    Debug.LogWarning($"[AIChatWindow] STA clipboard read failed: {ex.Message}\n{ex.StackTrace}");
                TryReadWpfClipboardImageBytes(ref pngBytes);
            }
        });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        return pngBytes;
    }

    private static void TryReadWpfClipboardImageBytes(ref byte[] pngBytes)
    {
        if (pngBytes != null && pngBytes.Length > 0) return;

        try
        {
            Type wpfClipboardType = ResolveType("System.Windows.Clipboard", "PresentationCore")
                                 ?? ResolveType("System.Windows.Clipboard", "PresentationFramework");
            if (wpfClipboardType == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] WPF Clipboard type not available.");
                return;
            }

            if (DebugPasteLogs)
            {
                MethodInfo containsData = wpfClipboardType.GetMethod("ContainsData", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (containsData != null)
                {
                    bool hasDib = (bool)containsData.Invoke(null, new object[] { "DeviceIndependentBitmap" });
                    bool hasBitmap = (bool)containsData.Invoke(null, new object[] { "Bitmap" });
                    bool hasPng = (bool)containsData.Invoke(null, new object[] { "PNG" });
                    Debug.Log($"[AIChatWindow] WPF Clipboard.ContainsData: DIB={hasDib}, Bitmap={hasBitmap}, PNG={hasPng}");
                }
            }

            MethodInfo wpfContainsImage = wpfClipboardType.GetMethod("ContainsImage", BindingFlags.Public | BindingFlags.Static);
            MethodInfo wpfGetImage = wpfClipboardType.GetMethod("GetImage", BindingFlags.Public | BindingFlags.Static);
            if (wpfContainsImage == null || wpfGetImage == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] WPF Clipboard image methods unavailable.");
                return;
            }

            bool hasImage = (bool)wpfContainsImage.Invoke(null, null);
            if (DebugPasteLogs)
                Debug.Log($"[AIChatWindow] WPF Clipboard.ContainsImage()={hasImage}");
            if (!hasImage) return;

            object bitmapSource = wpfGetImage.Invoke(null, null);
            if (bitmapSource == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] WPF Clipboard.GetImage() returned null.");
                return;
            }

            Type bitmapSourceType = ResolveType("System.Windows.Media.Imaging.BitmapSource", "PresentationCore");
            Type bitmapFrameType = ResolveType("System.Windows.Media.Imaging.BitmapFrame", "PresentationCore");
            Type pngEncoderType = ResolveType("System.Windows.Media.Imaging.PngBitmapEncoder", "PresentationCore");

            if (bitmapSourceType == null || bitmapFrameType == null || pngEncoderType == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] WPF imaging types unavailable for PNG encoding.");
                return;
            }

            MethodInfo createFrame = bitmapFrameType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { bitmapSourceType }, null);
            if (createFrame == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] BitmapFrame.Create(BitmapSource) not found.");
                return;
            }

            object frame = createFrame.Invoke(null, new[] { bitmapSource });
            object encoder = Activator.CreateInstance(pngEncoderType);
            if (frame == null || encoder == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] Failed to create WPF frame/encoder.");
                return;
            }

            PropertyInfo framesProp = pngEncoderType.GetProperty("Frames", BindingFlags.Public | BindingFlags.Instance);
            object framesCollection = framesProp?.GetValue(encoder);
            MethodInfo addFrame = framesCollection?.GetType().GetMethod("Add");
            if (framesCollection == null || addFrame == null)
            {
                if (DebugPasteLogs)
                    Debug.Log("[AIChatWindow] Unable to access PngBitmapEncoder.Frames collection.");
                return;
            }

            addFrame.Invoke(framesCollection, new[] { frame });

            using (var ms = new MemoryStream())
            {
                MethodInfo save = pngEncoderType.GetMethod("Save", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Stream) }, null);
                if (save == null)
                {
                    if (DebugPasteLogs)
                        Debug.Log("[AIChatWindow] PngBitmapEncoder.Save(Stream) not found.");
                    return;
                }

                save.Invoke(encoder, new object[] { ms });
                pngBytes = ms.ToArray();
                if (DebugPasteLogs)
                    Debug.Log($"[AIChatWindow] WPF clipboard image converted to PNG bytes. bytes={pngBytes.Length}");
            }
        }
        catch (Exception ex)
        {
            if (DebugPasteLogs)
                Debug.LogWarning($"[AIChatWindow] WPF clipboard fallback failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static Type ResolveType(string fullTypeName, string assemblyName)
    {
        Type t = Type.GetType($"{fullTypeName}, {assemblyName}");
        if (t != null) return t;

        try
        {
            Assembly asm = Assembly.Load(assemblyName);
            if (asm != null)
                t = asm.GetType(fullTypeName);
        }
        catch
        {
            // Return null below; caller handles fallback/logging.
        }

        return t;
    }
#endif

    private static bool IsImagePath(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" ||
               ext == ".tga" || ext == ".gif" || ext == ".tif" || ext == ".tiff" ||
               ext == ".webp";
    }

    private void HandleDragAndDropImages(Rect dropRect)
    {
        Event evt = Event.current;
        if (evt == null) return;
        if (!dropRect.Contains(evt.mousePosition)) return;

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        bool hasImageCandidate = false;

        if (DragAndDrop.paths != null)
        {
            foreach (string path in DragAndDrop.paths)
            {
                if (File.Exists(path) && IsImagePath(path))
                {
                    hasImageCandidate = true;
                    break;
                }
            }
        }

        if (!hasImageCandidate)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            return;
        }

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            int added = 0;

            foreach (string path in DragAndDrop.paths)
            {
                if (TryAddImageFromPath(path))
                    added++;
            }

            if (DebugPasteLogs)
                Debug.Log($"[AIChatWindow] Drag-and-drop processed. added={added}, totalPending={pendingAttachments.Count}");

            Repaint();
        }

        evt.Use();
    }

    private bool TryAddImageFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (!File.Exists(path))
        {
            if (DebugPasteLogs)
                Debug.Log("[AIChatWindow] Candidate is not an existing file path.");
            return false;
        }

        if (!IsImagePath(path))
        {
            if (DebugPasteLogs)
                Debug.Log("[AIChatWindow] Candidate file exists but is not a supported image extension.");
            return false;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, bytes, false))
            {
                if (DebugPasteLogs)
                    Debug.LogWarning($"[AIChatWindow] Failed to decode image bytes from path: {path}");
                DestroyImmediate(tex);
                return false;
            }

            pendingAttachments.Add(new ImageAttachment(tex, Path.GetFileName(path), isRuntime: true));
            if (DebugPasteLogs)
                Debug.Log($"[AIChatWindow] Added image from path: {path}");
            return true;
        }
        catch (Exception ex)
        {
            if (DebugPasteLogs)
                Debug.LogWarning($"[AIChatWindow] Failed reading image from path '{path}': {ex.Message}");
            return false;
        }
    }

    private static void DestroyRuntimeAttachments(List<ImageAttachment> attachments)
    {
        if (attachments == null) return;

        foreach (var attachment in attachments)
        {
            if (attachment == null || !attachment.IsRuntime || attachment.Texture == null) continue;
            DestroyImmediate(attachment.Texture);
        }
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

    // ── Data types ────────────────────────────────────────────────────────────
    private class AssetCard
    {
        public string Name;
        public string Path;
        public bool   AddedToScene;
        public AssetCard(string name, string path) { Name = name; Path = path; }
    }

    private class ImageAttachment
    {
        public Texture2D Texture;
        public string    Name;
        public bool      IsRuntime;

        public ImageAttachment(Texture2D texture, string name, bool isRuntime)
        {
            Texture = texture;
            Name = name;
            IsRuntime = isRuntime;
        }
    }

    // Mutable wrapper so we can update loading messages in-place
    private class ChatMessage
    {
        public string          Text;
        public bool            IsUser;
        public List<AssetCard> AssetCards; // non-null for asset result messages
        public List<ImageAttachment> ImageAttachments;
        public ChatMessage(string text, bool isUser, List<ImageAttachment> imageAttachments = null)
        {
            Text = text;
            IsUser = isUser;
            ImageAttachments = imageAttachments;
        }
    }
}
