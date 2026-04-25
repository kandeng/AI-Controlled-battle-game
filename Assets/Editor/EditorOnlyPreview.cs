#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Forces Unity to always start Play mode from Play Scene,
/// regardless of which scene is currently open in the Editor.
/// Uses EditorSceneManager.playModeStartScene — the correct Unity API for this purpose.
/// </summary>
[InitializeOnLoad]
public static class EditorOnlyPreview
{
    private const string PLAY_SCENE_PATH = "Assets/FPS-Game/Scenes/MainScenes/Play Scene.unity";

    static EditorOnlyPreview()
    {
        // Set the play-mode start scene immediately on Editor load/recompile
        SetPlayModeStartScene();

        // Re-apply whenever the Editor recompiles or domain reloads
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            SetPlayModeStartScene();
    }

    static void SetPlayModeStartScene()
    {
        SceneAsset playScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(PLAY_SCENE_PATH);
        if (playScene == null)
        {
            Debug.LogWarning($"[EditorOnlyPreview] Play Scene not found: {PLAY_SCENE_PATH}");
            return;
        }

        if (EditorSceneManager.playModeStartScene != playScene)
        {
            EditorSceneManager.playModeStartScene = playScene;
            Debug.Log($"[EditorOnlyPreview] Play mode start scene set to: {PLAY_SCENE_PATH}");
        }
    }
}
#endif
