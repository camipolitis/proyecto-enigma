using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
static class PlayFromBuildSettings
{
    // Play usa la primera escena de File > Build Settings (el menú).
    static PlayFromBuildSettings()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        ApplyFirstBuildScene();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            ApplyFirstBuildScene();
    }

    static void ApplyFirstBuildScene()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null)
            return;

        for (int i = 0; i < scenes.Length; i++)
        {
            var entry = scenes[i];
            if (!entry.enabled || string.IsNullOrEmpty(entry.path))
                continue;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(entry.path);
            if (sceneAsset == null)
                continue;

            EditorSceneManager.playModeStartScene = sceneAsset;
            return;
        }

        EditorSceneManager.playModeStartScene = null;
    }
}