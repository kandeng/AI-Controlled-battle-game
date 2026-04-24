using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script to automatically setup Play Scene.unity with NetworkManager and InGameManager
/// Run this once to fix the "No cameras rendering" and input not working issues
/// </summary>
public class SceneSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Play Scene")]
    public static void SetupPlayScene()
    {
        // Open the Play scene
        string scenePath = "Assets/FPS-Game/Scenes/MainScenes/Play Scene.unity";
        
        if (!System.IO.File.Exists(scenePath))
        {
            EditorUtility.DisplayDialog("Error", 
                $"Play Scene.unity not found at: {scenePath}\nPlease verify the scene exists.", 
                "OK");
            return;
        }
        
        // Open scene
        Scene scene = EditorSceneManager.OpenScene(scenePath);
        Debug.Log($"[SceneSetup] Opened scene: {scenePath}");
        
        string results = "";
        bool anyAdded = false;
        
        // --- Add NetworkManager ---
        GameObject existingNM = GameObject.Find("NetworkManager");
        if (existingNM != null)
        {
            results += "✓ NetworkManager already in scene\n";
            Debug.Log("[SceneSetup] NetworkManager already exists in scene");
        }
        else
        {
            string nmPrefabPath = "Assets/FPS-Game/Prefabs/System/NetworkManager.prefab";
            GameObject nmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(nmPrefabPath);
            
            if (nmPrefab == null)
            {
                // Try alternate path
                nmPrefabPath = "Assets/FPS-Game/Prefabs/NetworkManager.prefab";
                nmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(nmPrefabPath);
            }
            
            if (nmPrefab != null)
            {
                GameObject nm = PrefabUtility.InstantiatePrefab(nmPrefab) as GameObject;
                nm.name = "NetworkManager";
                results += "✓ NetworkManager added to scene\n";
                Debug.Log("[SceneSetup] ✓ Added NetworkManager to scene");
                anyAdded = true;
            }
            else
            {
                results += "❌ NetworkManager prefab not found!\n";
                Debug.LogError("[SceneSetup] NetworkManager prefab not found!");
            }
        }
        
        // --- Add InGameManager ---
        GameObject existingIGM = GameObject.Find("InGameManager");
        if (existingIGM != null)
        {
            results += "✓ InGameManager already in scene\n";
            Debug.Log("[SceneSetup] InGameManager already exists in scene");
        }
        else
        {
            string igmPrefabPath = "Assets/FPS-Game/Prefabs/System/InGameManager.prefab";
            GameObject igmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(igmPrefabPath);
            
            if (igmPrefab != null)
            {
                GameObject igm = PrefabUtility.InstantiatePrefab(igmPrefab) as GameObject;
                igm.name = "InGameManager";
                results += "✓ InGameManager added to scene\n";
                Debug.Log("[SceneSetup] ✓ Added InGameManager to scene");
                anyAdded = true;
            }
            else
            {
                results += "❌ InGameManager prefab not found!\n";
                Debug.LogError("[SceneSetup] InGameManager prefab not found!");
            }
        }
        
        // Save if anything was added
        if (anyAdded)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            results += "✓ Scene saved\n";
            Debug.Log("[SceneSetup] ✓ Scene saved");
        }
        
        // Show result
        EditorUtility.DisplayDialog("Scene Setup Complete", 
            results + "\nNext steps:\n" +
            "1. Select InGameManager in Hierarchy\n" +
            "2. In Inspector, set Game Mode to 'SinglePlayer'\n" +
            "3. Press Play (▶️) to test", 
            "OK");
    }
    
    [MenuItem("Tools/Verify Scene Setup")]
    public static void VerifySceneSetup()
    {
        string results = "";
        bool allGood = true;
        
        // Check NetworkManager
        GameObject nm = GameObject.Find("NetworkManager");
        if (nm == null)
        {
            results += "❌ NetworkManager NOT found in scene!\n";
            allGood = false;
        }
        else
        {
            results += "✓ NetworkManager found in scene\n";
        }
        
        // Check InGameManager
        GameObject igm = GameObject.Find("InGameManager");
        if (igm == null)
        {
            results += "❌ InGameManager NOT found in scene!\n";
            allGood = false;
        }
        else
        {
            results += "✓ InGameManager found in scene\n";
        }
        
        // Check for missing scripts on InGameManager
        if (igm != null)
        {
            var components = igm.GetComponents<MonoBehaviour>();
            bool hasMissing = false;
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    hasMissing = true;
                    break;
                }
            }
            if (hasMissing)
            {
                results += "⚠️ InGameManager has missing scripts!\n";
                allGood = false;
            }
            else
            {
                results += "✓ No missing scripts on InGameManager\n";
            }
        }
        
        EditorUtility.DisplayDialog(
            allGood ? "Verification Passed" : "Verification Failed", 
            results, 
            "OK");
    }
}
