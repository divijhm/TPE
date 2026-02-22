using UnityEngine;
using UnityEditor;

public class ApplyWhiteMaterial
{
    [MenuItem("Tools/Apply White Material to Init Children")]
    public static void Apply()
    {
        // Find or create the plain white material
        string matPath = "Assets/Materials/PlainWhite.mat";
        Material whiteMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (whiteMat == null)
        {
            whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMat.color = Color.white;
            AssetDatabase.CreateAsset(whiteMat, matPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created PlainWhite material at " + matPath);
        }

        // Find the "init" GameObject
        GameObject initObj = GameObject.Find("init");
        if (initObj == null)
        {
            Debug.LogError("Could not find GameObject named 'init' in the scene.");
            return;
        }

        // Get all Renderers in all children (including nested)
        Renderer[] renderers = initObj.GetComponentsInChildren<Renderer>(true);
        int count = 0;
        foreach (Renderer r in renderers)
        {
            // Replace ALL material slots with the white material
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = whiteMat;
            r.sharedMaterials = mats;
            EditorUtility.SetDirty(r);
            count++;
        }

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"Applied PlainWhite material to {count} renderers under 'init'.");
    }
}
