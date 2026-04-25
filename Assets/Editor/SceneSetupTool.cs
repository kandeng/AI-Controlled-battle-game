using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs SetupScene automatically every time the Unity Editor starts or recompiles.
/// The Tools menu items are kept as a manual fallback.
/// </summary>
[InitializeOnLoad]
public static class SceneSetupTool
{
    const string SCENE_PATH = "Assets/FPS-Game/Scenes/MainScenes/Play Scene.unity";
    const string PREF_KEY   = "SceneSetupTool_LastSetup"; // avoids re-running mid-session

    // -------------------------------------------------------
    // Auto-run on Editor startup / recompile
    // -------------------------------------------------------

    static SceneSetupTool()
    {
        // Defer until the Editor is fully ready (not during domain reload)
        EditorApplication.delayCall += AutoSetup;
        EditorApplication.delayCall += ImportTMPEssentialsIfNeeded;
    }

    static void AutoSetup()
    {
        // Only run once per Editor session (session key resets when Unity restarts)
        string sessionKey = PREF_KEY + "_done";
        if (SessionState.GetBool(sessionKey, false)) return;
        SessionState.SetBool(sessionKey, true);

        // --- Auto-import TMP Essentials if not already present ---
        // (Removed from here — runs separately before Play mode)
        Debug.Log("[SceneSetupTool] Auto-setup triggered on Editor startup.");
        SetupScene(silent: true);
    }

    static void ImportTMPEssentialsIfNeeded()
    {
        // Must NOT run in Play mode — AssetDatabase.ImportPackage is forbidden there
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        const string tmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        // Case 1: TMP Settings exists but may be out of date (version mismatch triggers the dialog).
        // Stamp assetVersion = "2" (s_CurrentAssetVersion) via SerializedObject to silence the importer.
        if (AssetDatabase.AssetPathExists(tmpSettingsPath))
        {
            var settingsObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(tmpSettingsPath);
            if (settingsObj != null)
            {
                var so = new SerializedObject(settingsObj);
                var versionProp = so.FindProperty("assetVersion");
                if (versionProp != null && versionProp.stringValue != "2")
                {
                    versionProp.stringValue = "2";
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(settingsObj);
                    AssetDatabase.SaveAssetIfDirty(settingsObj);
                    Debug.Log("[SceneSetupTool] TMP Settings version stamped — importer dialog suppressed.");
                }
            }
            return; // Already imported
        }

        // Case 2: TMP Settings doesn't exist yet — import the package silently.
        string packagePath = null;

        // Locate .unitypackage inside com.unity.ugui package cache
        string[] dirs = System.IO.Directory.GetDirectories(
            System.IO.Path.Combine(Application.dataPath, "../Library/PackageCache"),
            "com.unity.ugui*", System.IO.SearchOption.TopDirectoryOnly);
        if (dirs.Length > 0)
        {
            string candidate = System.IO.Path.Combine(dirs[0],
                "Package Resources", "TMP Essential Resources.unitypackage");
            if (System.IO.File.Exists(candidate))
                packagePath = candidate;
        }

        if (packagePath != null)
        {
            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log("[SceneSetupTool] TMP Essentials imported silently.");
        }
        else
        {
            Debug.LogWarning("[SceneSetupTool] Could not locate TMP Essential Resources.unitypackage. " +
                             "Please import manually: Window > TextMeshPro > Import TMP Essential Resources");
        }
    }

    // -------------------------------------------------------
    // Setup Scene  (merged: open scene + prefabs + components)
    // -------------------------------------------------------

    [MenuItem("Tools/FPS Game/Setup Scene")]
    public static void SetupSceneMenu() => SetupScene(silent: false);

    public static void SetupScene(bool silent = false)
    {
        // --- 1. Open the Play scene if it exists and isn't already open ---
        if (System.IO.File.Exists(SCENE_PATH))
        {
            EditorSceneManager.OpenScene(SCENE_PATH);
            Debug.Log($"[SceneSetupTool] Opened scene: {SCENE_PATH}");
        }
        else
        {
            Debug.LogWarning($"[SceneSetupTool] Scene not found at {SCENE_PATH} — using currently open scene.");
        }

        // --- 1b. Pin Play Scene as the Play-mode start scene ---
        var playSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
        if (playSceneAsset != null)
        {
            EditorSceneManager.playModeStartScene = playSceneAsset;
            Debug.Log($"[SceneSetupTool] playModeStartScene set to: {SCENE_PATH}");
        }

        string results = "";

        // --- 2. NetworkManager prefab ---
        if (GameObject.Find("NetworkManager") != null)
        {
            results += "✓ NetworkManager already in scene\n";
        }
        else
        {
            string path = "Assets/FPS-Game/Prefabs/System/NetworkManager.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FPS-Game/Prefabs/NetworkManager.prefab");

            if (prefab != null)
            {
                (PrefabUtility.InstantiatePrefab(prefab) as GameObject).name = "NetworkManager";
                results += "✓ NetworkManager added\n";
            }
            else
            {
                results += "❌ NetworkManager prefab not found\n";
            }
        }

        // --- 3. InGameManager prefab ---
        if (GameObject.Find("InGameManager") != null)
        {
            results += "✓ InGameManager already in scene\n";
        }
        else
        {
            string path = "Assets/FPS-Game/Prefabs/System/InGameManager.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                (PrefabUtility.InstantiatePrefab(prefab) as GameObject).name = "InGameManager";
                results += "✓ InGameManager added\n";
            }
            else
            {
                results += "❌ InGameManager prefab not found\n";
            }
        }

        // --- 4. WebSocket system components on InGameManager (or new GameSystems GO) ---
        GameObject host = GameObject.Find("InGameManager") ?? new GameObject("GameSystems");
        if (host.name == "GameSystems")
            Undo.RegisterCreatedObjectUndo(host, "Create GameSystems");

        results += EnsureComponent<WebSocketServerManager>(host, "WebSocketServerManager");
        results += EnsureComponent<CommandDispatcher>(host, "CommandDispatcher");
        results += EnsureComponent<PlayerCommandAPI>(host, "PlayerCommandAPI");
        results += EnsureComponent<CoroutineManager>(host, "CoroutineManager");

        // --- 5. Save ---
        EditorSceneManager.MarkSceneDirty(host.scene);
        EditorSceneManager.SaveScene(host.scene);
        results += "✓ Scene saved\n";

        Selection.activeGameObject = host;

        if (!silent)
            EditorUtility.DisplayDialog("Setup Scene Complete", results, "OK");
        else
            Debug.Log("[SceneSetupTool] Auto-setup complete:\n" + results);
    }

    // -------------------------------------------------------
    // Verify
    // -------------------------------------------------------

    [MenuItem("Tools/FPS Game/Verify Scene Setup")]
    public static void VerifySceneSetup()
    {
        string results = "";
        bool allGood = true;

        Check("NetworkManager",          GameObject.Find("NetworkManager")          != null, ref results, ref allGood);
        Check("InGameManager",           GameObject.Find("InGameManager")           != null, ref results, ref allGood);
        Check("WebSocketServerManager",  FindComponent<WebSocketServerManager>()    != null, ref results, ref allGood);
        Check("CommandDispatcher",       FindComponent<CommandDispatcher>()         != null, ref results, ref allGood);
        Check("PlayerCommandAPI",        FindComponent<PlayerCommandAPI>()          != null, ref results, ref allGood);
        Check("CoroutineManager",        FindComponent<CoroutineManager>()          != null, ref results, ref allGood);

        // Check for missing (null) MonoBehaviour scripts on InGameManager
        GameObject igm = GameObject.Find("InGameManager");
        if (igm != null)
        {
            bool hasMissing = false;
            foreach (var c in igm.GetComponents<MonoBehaviour>())
                if (c == null) { hasMissing = true; break; }
            if (hasMissing)
            {
                results += "⚠️ InGameManager has missing scripts\n";
                allGood = false;
            }
        }

        EditorUtility.DisplayDialog(
            allGood ? "Verification Passed ✓" : "Verification Failed ❌",
            results,
            "OK");
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    static string EnsureComponent<T>(GameObject go, string label) where T : Component
    {
        if (go.GetComponent<T>() == null)
        {
            Undo.AddComponent<T>(go);
            return $"✓ {label} added\n";
        }
        return $"✓ {label} already present\n";
    }

    static T FindComponent<T>() where T : Component
        => UnityEngine.Object.FindAnyObjectByType<T>();

    static void Check(string label, bool condition, ref string results, ref bool allGood)
    {
        if (condition)
            results += $"✓ {label} found\n";
        else
        {
            results += $"❌ {label} NOT found\n";
            allGood = false;
        }
    }
}
