using UnityEditor;
using UnityEngine;

public class ChatWindow : EditorWindow
{
    static string prompt = "";
    static ChatAttachment attachment;

    [MenuItem("DAMN/Chat")]
    // public static void Open()
    // {
    //     GetWindow<ChatWindow>("DAMN Chat");
    // }
    public static void Open()
    {
        ChatWindow window =
            GetWindow<ChatWindow>("DAMN Chat");

        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Space(8);

        //-------------------------------------------------
        // ATTACHMENTS AREA
        //-------------------------------------------------
        DrawAttachmentArea();

        GUILayout.Space(10);

        //-------------------------------------------------
        // CHAT INPUT
        //-------------------------------------------------
        GUILayout.Label("Instruction");

        prompt = EditorGUILayout.TextArea(
            prompt,
            GUILayout.Height(position.height * 0.5f)
        );

        GUILayout.Space(10);

        //-------------------------------------------------
        // SEND BUTTON
        //-------------------------------------------------
        if (GUILayout.Button("Send", GUILayout.Height(40)))
        {
            SendMessage();
        }
    }

    //-------------------------------------------------
    // ATTACHMENT UI (like ChatGPT)
    //-------------------------------------------------
    void DrawAttachmentArea()
    {
        GUILayout.Label("Attachments");

        if (attachment == null)
        {
            GUILayout.Label("No selection attached");
            return;
        }

        var ctx = attachment.context;

        GUILayout.BeginVertical("box");

        GUILayout.Label($"Selected Objects: {ctx.selection.Count}");

        foreach (var obj in ctx.selection)
            GUILayout.Label("• " + obj.name);

        GUILayout.Label(
            $"Bounds Size: {ctx.bounds.size}");

        if (GUILayout.Button("Remove Attachment"))
            attachment = null;

        GUILayout.EndVertical();
    }

    //-------------------------------------------------
    // CALLED BY SELECTION LISTENER
    //-------------------------------------------------
    public static void AttachSelection(SelectionContext ctx)
    {
        attachment = new ChatAttachment
        {
            context = ctx
        };

        ChatWindow window = GetWindow<ChatWindow>();
        window.Repaint();
    }

    //-------------------------------------------------
    void SendMessage()
    {
        if (attachment == null)
        {
            Debug.LogWarning("No selection attached.");
            return;
        }

        attachment.context.prompt = prompt;

        MCPClient client =
            Object.FindObjectOfType<MCPClient>();

        if (client != null)
            client.SendContext(attachment.context);

        prompt = "";
        attachment = null;
    }
}