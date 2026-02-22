using UnityEditor;
using UnityEngine;

public class ChatWindow : EditorWindow
{
    static string prompt = "";

    [MenuItem("DAMN/Chat Placeholder")]
    public static void Open()
    {
        GetWindow<ChatWindow>("DAMN Chat");
    }

    void OnGUI()
    {
        GUILayout.Label("Edit Instruction");

        prompt = EditorGUILayout.TextField("Prompt", prompt);

        if (GUILayout.Button("Send Selection + Prompt"))
        {
            SendContext();
        }
    }

    void SendContext()
    {
        var context = SelectionContextBuilder.Build();

        context.prompt = prompt;

        MCPClient client =
            Object.FindObjectOfType<MCPClient>();

        if (client != null)
            client.SendContext(context);
    }
}