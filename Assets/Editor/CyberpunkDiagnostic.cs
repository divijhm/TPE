using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CyberpunkDiagnostic : MonoBehaviour
{
    [MenuItem("Tools/Cyberpunk Diagnostic")]
    public static void RunDiagnostic()
    {
        // Find every unique (materialName -> sharedMesh + GO name) to understand scene layout
        var matToObjects = new Dictionary<string, List<string>>();
        foreach (var mr in GameObject.FindObjectsOfType<MeshRenderer>())
        {
            foreach (var mat in mr.sharedMaterials)
            {
                if (mat == null) continue;
                string nm = mat.name;
                if (!matToObjects.ContainsKey(nm)) matToObjects[nm] = new List<string>();
                matToObjects[nm].Add(mr.gameObject.name);
            }
        }
        foreach (var kv in matToObjects)
            Debug.Log("[DIAG] MAT=" + kv.Key + "  OBJECTS=" + string.Join(", ", kv.Value.GetRange(0, Mathf.Min(3, kv.Value.Count))));

        // Log all lights
        foreach (var L in GameObject.FindObjectsOfType<Light>())
            Debug.Log("[DIAG] LIGHT=" + L.gameObject.name + " type=" + L.type + " intensity=" + L.intensity + " color=" + ColorUtility.ToHtmlStringRGBA(L.color) + " pos=" + L.transform.position);

        Debug.Log("[DIAG] Done. Total MeshRenderers: " + GameObject.FindObjectsOfType<MeshRenderer>().Length);
    }
}
