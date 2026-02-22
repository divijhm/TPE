using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class MCPClient : MonoBehaviour
{
    const string MCP_URL = "http://localhost:3000/context";

    public void SendContext(SelectionContext context)
    {
        StartCoroutine(Post(context));
    }

    IEnumerator Post(SelectionContext context)
    {
        string json = JsonUtility.ToJson(context, true);

        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req =
            new UnityWebRequest(MCP_URL, "POST");

        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        Debug.Log("MCP RESPONSE:");
        Debug.Log(req.downloadHandler.text);
    }
}