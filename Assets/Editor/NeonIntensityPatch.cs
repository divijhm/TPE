using UnityEngine;
using UnityEditor;

public class NeonIntensityPatch : MonoBehaviour
{
    [MenuItem("Tools/Neon Reduce Intensity x0.4")]
    public static void Run()
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] {
            "Assets/Materials", "Assets/blndrmaterials"
        });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) continue;
            if (!m.IsKeywordEnabled("_EMISSION")) continue;

            // IMPORTANT: Use Color multiplication to preserve HDR values above 1.
            // new Color(r,g,b) CLAMPS to [0,1] — that's why the old script stopped working.
            Color e = m.GetColor("_EmissionColor");
            Color reduced = e * 0.4f;   // multiply preserves HDR range
            reduced.a = 1f;
            m.SetColor("_EmissionColor", reduced);

            // Ensure emission is flagged as realtime so changes show immediately
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            EditorUtility.SetDirty(m);
            count++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();        // force reload so scene picks up changes
        Debug.Log("[Neon] Reduced intensity x0.4 on " + count + " emissive materials.");
    }
}
