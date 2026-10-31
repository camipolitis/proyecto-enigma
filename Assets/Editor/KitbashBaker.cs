using System.Collections.Generic;
using Enigma.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Graba el kitbash en las escenas. No spawn en Play.
[InitializeOnLoad]
public static class KitbashBaker
{
    private static readonly string[] Paths =
    {
        "Assets/Scenes/Level01_Room.unity",
        "Assets/Scenes/Level02_Lab.unity",
        "Assets/Scenes/Level03_Archives.unity",
        "Assets/Scenes/Level03_Mezzanine.unity",
        "Assets/Scenes/Level04_Origen.unity"
    };

    static KitbashBaker()
    {
        EditorApplication.delayCall += DropBlendImports;
        EditorApplication.delayCall += BakeAll;
        EditorApplication.playModeStateChanged += OnPlay;
    }

    // Unity no tiene Blender. La batería en juego es Battery.fbx.
    private static void DropBlendImports()
    {
        const string batteryGuid = "c3acca651f2eed84b888951d328630cb";
        var known = AssetDatabase.GUIDToAssetPath(batteryGuid);
        if (!string.IsNullOrEmpty(known) && known.EndsWith(".blend", System.StringComparison.OrdinalIgnoreCase))
            AssetDatabase.DeleteAsset(known);

        const string batteryPath = "Assets/Art/Props/CarBattery/car battery.blend";
        if (AssetDatabase.LoadMainAssetAtPath(batteryPath) != null)
            AssetDatabase.DeleteAsset(batteryPath);

        // ambientCG y dumps: Unity intenta abrir .blend y explota sin Blender. Quedan los PNG.
        if (!System.IO.Directory.Exists("Assets/Art"))
            return;
        var extra = System.IO.Directory.GetFiles("Assets/Art", "*.blend", System.IO.SearchOption.AllDirectories);
        for (int i = 0; i < extra.Length; i++)
        {
            string p = extra[i].Replace('\\', '/');
            AssetDatabase.DeleteAsset(p);
        }
    }

    private static void OnPlay(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += BakeAll;
    }

    [MenuItem("Enigma/Bake Kitbash")]
    public static void BakeAllNow()
    {
        EditorPrefs.SetInt("EnigmaKitBakeForce", 1);
        EditorPrefs.DeleteKey("EnigmaKitBakeVer");
        EditorPrefs.DeleteKey("EnigmaKitBakeTries");
        BakeAll();
    }

    private static void BakeAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += BakeAll;
            return;
        }
        bool force = EditorPrefs.GetInt("EnigmaKitBakeForce", 0) == 1;
        if (!force && EditorPrefs.GetInt("EnigmaKitBakeVer", 0) == KitbashDressing.BakeId)
            return;
        if (Resources.Load<GameObject>("Kitbash/Chair_2") == null
            || (System.IO.File.Exists("Assets/Resources/Kitbash/ElectricalBox.fbx")
                && Resources.Load<GameObject>("Kitbash/ElectricalBox") == null))
        {
            int tries = EditorPrefs.GetInt("EnigmaKitBakeTries", 0);
            if (tries > 40)
            {
                Debug.LogError("Kitbash bake: faltan prefabs de Kitbash.");
                return;
            }
            EditorPrefs.SetInt("EnigmaKitBakeTries", tries + 1);
            EditorApplication.delayCall += BakeAll;
            return;
        }
        EditorPrefs.DeleteKey("EnigmaKitBakeTries");

        var had = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
            had.Add(SceneManager.GetSceneAt(i).path);

        bool ok = true;
        for (int p = 0; p < Paths.Length; p++)
        {
            var path = Paths[p];
            var scene = default(Scene);
            bool opened = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.path == path)
                {
                    scene = s;
                    break;
                }
            }
            if (!scene.IsValid())
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                opened = true;
            }
            if (!scene.IsValid())
            {
                ok = false;
                continue;
            }

            var prev = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            if (BakeScene(scene, force))
            {
                if (!EditorSceneManager.SaveScene(scene))
                    ok = false;
            }
            if (prev.IsValid() && prev.isLoaded)
                SceneManager.SetActiveScene(prev);
            if (opened && !had.Contains(path))
                EditorSceneManager.CloseScene(scene, true);
        }

        if (ok)
        {
            EditorPrefs.SetInt("EnigmaKitBakeVer", KitbashDressing.BakeId);
            EditorPrefs.DeleteKey("EnigmaKitBakeForce");
        }
    }

    private static bool BakeScene(Scene scene, bool force)
    {
        KitbashDressing dressing = null;
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            dressing = roots[i].GetComponentInChildren<KitbashDressing>(true);
            if (dressing != null)
                break;
        }
        if (!force && dressing != null && dressing.bakedId == KitbashDressing.BakeId && KitbashDressing.HasKit(scene))
            return false;

        if (dressing == null)
        {
            var go = new GameObject("KitDressing");
            SceneManager.MoveGameObjectToScene(go, scene);
            Transform env = null;
            roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == "Environment")
                {
                    env = roots[i].transform;
                    break;
                }
            }
            if (env != null)
                go.transform.SetParent(env, false);
            dressing = go.AddComponent<KitbashDressing>();
        }

        KitbashDressing.RestoreHidden(scene);
        ClearOld(scene, dressing);
        dressing.Bake(scene.name);
        UnpackKits(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        return true;
    }

    private static void ClearOld(Scene scene, KitbashDressing dressing)
    {
        var kill = new List<GameObject>();
        var roots = scene.GetRootGameObjects();
        for (int r = 0; r < roots.Length; r++)
            Collect(roots[r].transform, kill);
        for (int i = 0; i < kill.Count; i++)
        {
            if (kill[i] != null)
                Object.DestroyImmediate(kill[i]);
        }
        if (dressing == null)
            return;
        for (int i = dressing.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(dressing.transform.GetChild(i).gameObject);
    }

    private static void Collect(Transform t, List<GameObject> kill)
    {
        var n = t.name;
        if (n.StartsWith("Kit_") || n == "PaperScrap")
            kill.Add(t.gameObject);
        for (int i = 0; i < t.childCount; i++)
            Collect(t.GetChild(i), kill);
    }

    private static void UnpackKits(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        var list = new List<GameObject>();
        for (int r = 0; r < roots.Length; r++)
            FindKits(roots[r].transform, list);
        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (go == null)
                continue;
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (root != null)
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            if (go.name.IndexOf("ExitPad") < 0)
                KitbashDressing.StripColliders(go);
        }
    }

    private static void FindKits(Transform t, List<GameObject> list)
    {
        if (t.name.StartsWith("Kit_"))
            list.Add(t.gameObject);
        for (int i = 0; i < t.childCount; i++)
            FindKits(t.GetChild(i), list);
    }
}
