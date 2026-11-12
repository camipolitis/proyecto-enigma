using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
static class PlayFromBuildSettings
{
    const string OldPrefKey = "Enigma.PlayFromMainMenu";
    const string TargetKey = "Enigma.PlayTarget";

    // Play arranca siempre por la primera escena de Build Settings (el menú), donde viven la música y los efectos.
    // Si la escena abierta es un nivel, el menú pasa directo a ese nivel; probarlo así no pisa la partida guardada.
    static PlayFromBuildSettings()
    {
        EditorPrefs.DeleteKey(OldPrefKey);
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        ApplyFirstBuildScene();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            SessionState.EraseString(TargetKey);
            var first = ApplyFirstBuildScene();
            string open = EditorSceneManager.GetActiveScene().path;
            if (first != null && !string.IsNullOrEmpty(open) && open != first && IsBuildScene(open))
                SessionState.SetString(TargetKey, open);
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            string target = SessionState.GetString(TargetKey, string.Empty);
            SessionState.EraseString(TargetKey);
            Enigma.Core.GameSave.Suspended = !string.IsNullOrEmpty(target) || IsTestRun();
            if (!string.IsNullOrEmpty(target))
                SceneManager.LoadScene(target);
        }
    }

    static bool IsTestRun()
    {
        return UnityEngine.Application.isBatchMode
            || EditorSceneManager.GetActiveScene().name.StartsWith("InitTestScene");
    }

    static bool IsBuildScene(string path)
    {
        foreach (var entry in EditorBuildSettings.scenes)
            if (entry.enabled && entry.path == path)
                return true;
        return false;
    }

    // Devuelve la ruta de la escena con la que arranca Play (null = la abierta)
    static string ApplyFirstBuildScene()
    {
        // Tests (batchmode o Test Runner del Editor): el runner necesita su propia escena inicial
        if (IsTestRun())
        {
            EditorSceneManager.playModeStartScene = null;
            return null;
        }

        var scenes = EditorBuildSettings.scenes;
        if (scenes != null)
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                var entry = scenes[i];
                if (!entry.enabled || string.IsNullOrEmpty(entry.path))
                    continue;

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(entry.path);
                if (sceneAsset == null)
                    continue;

                EditorSceneManager.playModeStartScene = sceneAsset;
                return entry.path;
            }
        }

        EditorSceneManager.playModeStartScene = null;
        return null;
    }
}
