using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
static class PlayFromBuildSettings
{
    const string MenuPath = "Enigma/Play From Main Menu";
    const string PrefKey = "Enigma.PlayFromMainMenu";

    // Play usa la primera escena de File > Build Settings (el menú). Se apaga desde el menú Enigma.
    static PlayFromBuildSettings()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        ApplyFirstBuildScene();
    }

    static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        Enabled = !Enabled;
        ApplyFirstBuildScene();
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            ApplyFirstBuildScene();
    }

    static void ApplyFirstBuildScene()
    {
        // Tests (batchmode o Test Runner del Editor): el runner necesita su propia escena inicial
        if (!Enabled || UnityEngine.Application.isBatchMode
            || EditorSceneManager.GetActiveScene().name.StartsWith("InitTestScene"))
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

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
