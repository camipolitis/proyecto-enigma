using System.Collections.Generic;
using Enigma.Environment;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
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

    // Escenas sin vestido: solo reciben el tema de la UI
    private static readonly string[] ThemeOnlyPaths =
    {
        "Assets/Scenes/MainMenu.unity"
    };

    private const string ThemePath = "Assets/ScriptableObjects/UI/SO_UiTheme.asset";
    private const string TitleFontPath = "Assets/Art/Fonts/StardosStencil/StardosStencil-Bold.ttf";
    private const string BodyFontPath = "Assets/Art/Fonts/SpecialElite/SpecialElite-Regular.ttf";

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

        const string batteryPath = "Assets/Art/Models/Props/CarBattery/car battery.blend";
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
        var all = new List<string>(Paths);
        all.AddRange(ThemeOnlyPaths);
        for (int p = 0; p < all.Count; p++)
        {
            var path = all[p];
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
            bool baked = p < Paths.Length ? BakeScene(scene, force) : ThemeScene(scene);
            if (baked)
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
            BakeIcons();
            EditorPrefs.SetInt("EnigmaKitBakeVer", KitbashDressing.BakeId);
            EditorPrefs.DeleteKey("EnigmaKitBakeForce");
        }
    }

    private const string IconDir = "Assets/Art/Textures/Icons";
    private const int IconSize = 128;

    // Miniaturas del hotbar: el modelo que se inspecciona, en tres cuartos y con fondo transparente
    private static void BakeIcons()
    {
        var guids = AssetDatabase.FindAssets("t:InventoryItem");
        if (guids.Length == 0)
            return;
        System.IO.Directory.CreateDirectory(IconDir);
        var preview = EditorSceneManager.NewPreviewScene();
        var camGo = new GameObject("IconCamera");
        SceneManager.MoveGameObjectToScene(camGo, preview);
        var cam = camGo.AddComponent<Camera>();
        cam.scene = preview;
        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        cam.fieldOfView = 25f;
        AddIconLight(preview, Quaternion.Euler(40f, -35f, 0f), 1.3f);
        AddIconLight(preview, Quaternion.Euler(-15f, 150f, 0f), 0.6f);
        var rt = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
        cam.targetTexture = rt;
        var tex = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
        var written = new List<(Enigma.Data.InventoryItem item, string path)>();

        foreach (var guid in guids)
        {
            var item = AssetDatabase.LoadAssetAtPath<Enigma.Data.InventoryItem>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || string.IsNullOrEmpty(item.id))
                continue;
            var model = Enigma.UI.ItemInspectUI.BuildModel(item);
            if (model == null)
                continue;
            SceneManager.MoveGameObjectToScene(model, preview);
            var rends = model.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0)
            {
                Object.DestroyImmediate(model);
                continue;
            }
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            Vector3 dir = new Vector3(0.55f, 0.6f, -1f).normalized;
            float dist = b.extents.magnitude / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            cam.transform.position = b.center + dir * dist;
            cam.transform.LookAt(b.center);
            cam.nearClipPlane = Mathf.Max(0.01f, dist - b.extents.magnitude * 2f);
            cam.farClipPlane = dist + b.extents.magnitude * 2f;
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            string path = IconDir + "/Icon_" + item.id + ".png";
            // Solo se reescribe si el ícono cambió, para no ensuciar el control de versiones
            byte[] png = tex.EncodeToPNG();
            if (!System.IO.File.Exists(path) || !System.Linq.Enumerable.SequenceEqual(System.IO.File.ReadAllBytes(path), png))
                System.IO.File.WriteAllBytes(path, png);
            written.Add((item, path));
            Object.DestroyImmediate(model);
        }

        cam.targetTexture = null;
        rt.Release();
        Object.DestroyImmediate(tex);
        EditorSceneManager.ClosePreviewScene(preview);
        AssetDatabase.Refresh();

        foreach (var (item, path) in written)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter imp
                && (imp.textureType != TextureImporterType.Sprite || imp.spriteImportMode != SpriteImportMode.Single
                    || !imp.alphaIsTransparency || imp.mipmapEnabled))
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                imp.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && item.icon != sprite)
            {
                item.icon = sprite;
                EditorUtility.SetDirty(item);
            }
        }
        AssetDatabase.SaveAssets();
    }

    private static void AddIconLight(Scene scene, Quaternion rot, float intensity)
    {
        var go = new GameObject("IconLight");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.rotation = rot;
        var l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = intensity;
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
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var slot in root.GetComponentsInChildren<Enigma.Inventory.InventorySlotView>(true))
                slot.EnsureParts();
            foreach (var inv in root.GetComponentsInChildren<Enigma.Inventory.InventoryUI>(true))
            {
                var inspect = inv.GetComponent<Enigma.UI.ItemInspectUI>();
                if (inspect == null)
                    inspect = inv.gameObject.AddComponent<Enigma.UI.ItemInspectUI>();
                var so = new SerializedObject(inv);
                so.FindProperty("inspectUi").objectReferenceValue = inspect;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (var inspect in root.GetComponentsInChildren<Enigma.UI.ItemInspectUI>(true))
            {
                inspect.EnsureBlur();
                EditorUtility.SetDirty(inspect);
            }
            foreach (var orbit in root.GetComponentsInChildren<Enigma.CameraSystem.ThirdPersonCamera>(true))
            {
                var director = orbit.GetComponent<Enigma.Cinematics.CinematicDirector>();
                if (director == null)
                    director = orbit.gameObject.AddComponent<Enigma.Cinematics.CinematicDirector>();
                director.EnsureRig();
                EditorUtility.SetDirty(director);
            }
        }
        if (scene.name == "Level02_Lab")
            BakeLabSystems(scene);
        ApplyTheme(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        return true;
    }

    private static bool ThemeScene(Scene scene)
    {
        ApplyTheme(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        return true;
    }

    private static void ApplyTheme(Scene scene)
    {
        var theme = LoadTheme();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                bool nested = canvas.transform.parent != null && canvas.transform.parent.GetComponentInParent<Canvas>(true) != null;
                if (!nested && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    theme.Apply(canvas);
            }
            foreach (var prompt in root.GetComponentsInChildren<Enigma.UI.InteractionPromptUI>(true))
                prompt.SetKeyColor(theme.accentColor);
        }
        PanelsBelowFade(scene);
    }

    // Las ventanas quedan debajo del fundido para que el negro las tape; el fin va encima a propósito
    private static void PanelsBelowFade(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var fade in root.GetComponentsInChildren<Enigma.UI.ScreenFade>(true))
            {
                var parent = fade.transform.parent;
                if (parent == null)
                    continue;
                for (int i = fade.transform.GetSiblingIndex() + 1; i < parent.childCount; i++)
                {
                    var t = parent.GetChild(i);
                    bool window = (t.name.EndsWith("Panel") || t.name == "NotebookFolders") && t.name != "EndPanel";
                    if (window)
                        t.SetSiblingIndex(fade.transform.GetSiblingIndex());
                }
            }
        }
    }

    private static Enigma.UI.UiTheme LoadTheme()
    {
        var theme = AssetDatabase.LoadAssetAtPath<Enigma.UI.UiTheme>(ThemePath);
        if (theme != null)
            return theme;
        theme = ScriptableObject.CreateInstance<Enigma.UI.UiTheme>();
        theme.titleFont = AssetDatabase.LoadAssetAtPath<Font>(TitleFontPath);
        theme.bodyFont = AssetDatabase.LoadAssetAtPath<Font>(BodyFontPath);
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/UI"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "UI");
        AssetDatabase.CreateAsset(theme, ThemePath);
        AssetDatabase.SaveAssets();
        return theme;
    }

    private static void BakeLabSystems(Scene scene)
    {
        Enigma.AI.LabSystems systems = null;
        Transform player = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (systems == null)
                systems = root.GetComponentInChildren<Enigma.AI.LabSystems>(true);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (player == null && t.CompareTag("Player"))
                    player = t;
        }
        if (systems == null)
        {
            var go = new GameObject("LevelSystems");
            SceneManager.MoveGameObjectToScene(go, scene);
            systems = go.AddComponent<Enigma.AI.LabSystems>();
        }
        systems.BuildWatcher(player, Resources.Load<Material>("Mats/Mat_Steel"), Resources.Load<Material>("Mats/Mat_Rubber"));
        SaveNavMesh(scene, systems.BuildSurface());
        EditorUtility.SetDirty(systems);
    }

    // Los datos del NavMesh viven en un asset junto a la escena; se reescribe el mismo para no cambiar de GUID
    private static void SaveNavMesh(Scene scene, NavMeshSurface surface)
    {
        var data = surface.navMeshData;
        if (data == null)
            return;
        string parent = System.IO.Path.GetDirectoryName(scene.path).Replace('\\', '/');
        string folder = parent + "/" + scene.name;
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(parent, scene.name);
        string path = folder + "/NavMesh-" + surface.name + ".asset";
        var saved = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if (saved == null)
        {
            AssetDatabase.CreateAsset(data, path);
        }
        else
        {
            EditorUtility.CopySerialized(data, saved);
            surface.RemoveData();
            surface.navMeshData = saved;
            surface.AddData();
            EditorUtility.SetDirty(saved);
        }
        EditorUtility.SetDirty(surface);
        AssetDatabase.SaveAssets();
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
            // Los _Solid son los bloqueos armados por el vestido: quedan grabados
            if (go.name.IndexOf("ExitPad") < 0 && !go.name.EndsWith("_Solid"))
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
