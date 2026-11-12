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
        SideRoomParts(scene);
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
        {
            BakeLabSystems(scene);
            NotebookRecord(scene);
        }
        if (scene.name == "Level03_Archives")
            HatchHingedNorth(scene);
        JournalOnCanvas(scene);
        ReaderParts(scene);
        JournalCatalog(scene);
        ApplyTheme(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        return true;
    }

    private static void ReaderParts(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var reader in root.GetComponentsInChildren<Enigma.UI.DocumentReaderUI>(true))
            {
                reader.EnsureParts();
                EditorUtility.SetDirty(reader);
            }
    }

    private const string CatalogPath = "Assets/ScriptableObjects/Documents/SO_MemoryCatalog.asset";

    // Todas las memorias del proyecto, para que «Continuar» reponga el journal desde los ids guardados
    private static Enigma.Data.MemoryCatalog MemoryCatalog()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<Enigma.Data.MemoryCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<Enigma.Data.MemoryCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        var memories = new List<Enigma.Data.DocumentData>();
        foreach (var guid in AssetDatabase.FindAssets("t:DocumentData"))
        {
            var doc = AssetDatabase.LoadAssetAtPath<Enigma.Data.DocumentData>(AssetDatabase.GUIDToAssetPath(guid));
            if (doc != null && doc.isMemory && !string.IsNullOrEmpty(doc.id))
                memories.Add(doc);
        }
        memories.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
        bool changed = memories.Count != catalog.memories.Count;
        for (int i = 0; !changed && i < memories.Count; i++)
            changed = memories[i] != catalog.memories[i];
        if (changed)
        {
            catalog.memories = memories;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
        return catalog;
    }

    private static void JournalCatalog(Scene scene)
    {
        var catalog = MemoryCatalog();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var journal in root.GetComponentsInChildren<Enigma.Memory.MemoryJournal>(true))
            {
                var so = new SerializedObject(journal);
                so.FindProperty("catalog").objectReferenceValue = catalog;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
    }

    // «Continuar» arriba de «Jugar», con su mismo estilo; MainMenuUI lo muestra solo si hay partida guardada
    private static void MainMenuContinue(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var menu in root.GetComponentsInChildren<Enigma.UI.MainMenuUI>(true))
            {
                var so = new SerializedObject(menu);
                var prop = so.FindProperty("continueButton");
                var play = FindDeep(root.transform, "BtnJugar");
                if (play == null)
                    continue;
                var existing = FindDeep(root.transform, "BtnContinue");
                GameObject go = existing != null ? existing.gameObject : Object.Instantiate(play.gameObject, play.parent);
                go.name = "BtnContinue";
                var rt = (RectTransform)go.transform;
                var playRt = (RectTransform)play;
                rt.anchoredPosition = playRt.anchoredPosition + new Vector2(0f, playRt.sizeDelta.y + ContinueGap);
                var label = go.GetComponentInChildren<UnityEngine.UI.Text>(true);
                if (label != null)
                    label.text = "Continuar";
                var button = go.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                        UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, menu.Continue);
                }
                prop.objectReferenceValue = go;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(go);
            }
    }

    private const float ContinueGap = 16f;

    private static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name)
            return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var f = FindDeep(t.GetChild(i), name);
            if (f != null)
                return f;
        }
        return null;
    }

    // Con las bisagras al norte la tapa se levanta por el lado sur, el del jugador, y deja ver el hueco
    private static void HatchHingedNorth(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var hatch in root.GetComponentsInChildren<Enigma.Interaction.ArrowLockInteractable>(true))
            {
                var so = new SerializedObject(hatch);
                var pitch = so.FindProperty("openPitch");
                pitch.floatValue = Mathf.Abs(pitch.floatValue);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    private const string TestRecordDoc = "Assets/ScriptableObjects/Documents/SO_Doc_TestRecord.asset";

    // El registro de la notebook se lee en el lector común de la escena
    private static void NotebookRecord(Scene scene)
    {
        var doc = AssetDatabase.LoadAssetAtPath<Enigma.Data.DocumentData>(TestRecordDoc);
        if (doc == null)
        {
            Debug.LogError("Kitbash bake: falta " + TestRecordDoc);
            return;
        }
        Enigma.UI.DocumentReaderUI reader = null;
        foreach (var root in scene.GetRootGameObjects())
            if (reader == null)
                reader = root.GetComponentInChildren<Enigma.UI.DocumentReaderUI>(true);
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var folders in root.GetComponentsInChildren<Enigma.UI.NotebookFolderUI>(true))
            {
                var so = new SerializedObject(folders);
                so.FindProperty("reader").objectReferenceValue = reader;
                so.FindProperty("record").objectReferenceValue = doc;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    // El journal escucha OnMemoryAdded mientras está prendido: si su objeto está apagado, pasa al Canvas
    private static void JournalOnCanvas(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var ui in root.GetComponentsInChildren<Enigma.Memory.MemoryJournalUI>(true))
            {
                if (ui.gameObject.activeSelf)
                    continue;
                var canvas = ui.GetComponentInParent<Canvas>(true);
                if (canvas == null || canvas.GetComponent<Enigma.Memory.MemoryJournalUI>() != null)
                    continue;
                var moved = canvas.gameObject.AddComponent<Enigma.Memory.MemoryJournalUI>();
                EditorUtility.CopySerialized(ui, moved);
                Repoint(scene, ui, moved);
                Object.DestroyImmediate(ui);
            }
        }
    }

    private static void Repoint(Scene scene, Object from, Object to)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null || mb == to)
                    continue;
                var so = new SerializedObject(mb);
                var it = so.GetIterator();
                bool changed = false;
                while (it.Next(true))
                {
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue == from)
                    {
                        it.objectReferenceValue = to;
                        changed = true;
                    }
                }
                if (changed)
                    so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    private const string SideDoorConfig = "Assets/ScriptableObjects/SO_Config_SideDoor.asset";
    private const string NotebookConfig = "Assets/ScriptableObjects/SO_Config_BathNotebook.asset";
    private const string NotebookDoc = "Assets/ScriptableObjects/Documents/SO_Doc_BathNotebook.asset";

    // Puertas de los cuartitos y cuaderno del baño: llevan assets, se arman una vez y el vestido los ubica
    private static void SideRoomParts(Scene scene)
    {
        if (scene.name == "Level01_Room")
        {
            SideDoor(scene, "Interactable_BathDoor", -90f);
            BathNotebook(scene);
        }
        else if (scene.name == "Level02_Lab")
            SideDoor(scene, "Interactable_OfficeDoor", 90f);
    }

    // N2 agrupa los interactuables; en N1 quedan en la raíz de la escena
    private static void PlaceInteractable(Scene scene, GameObject go)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Interactables")
            {
                go.transform.SetParent(root.transform, false);
                return;
            }
        SceneManager.MoveGameObjectToScene(go, scene);
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name)
                    return t.gameObject;
        return null;
    }

    private static void SideDoor(Scene scene, string name, float openYaw)
    {
        var config = AssetDatabase.LoadAssetAtPath<Enigma.Data.InteractableConfig>(SideDoorConfig);
        if (config == null)
        {
            Debug.LogError("Kitbash bake: falta " + SideDoorConfig);
            return;
        }
        var go = FindInScene(scene, name);
        if (go == null)
        {
            go = new GameObject(name);
            PlaceInteractable(scene, go);
        }
        go.layer = LayerMask.NameToLayer("Interactable");
        var box = go.GetComponent<BoxCollider>();
        if (box == null)
            box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        var pivot = go.transform.Find("DoorPivot");
        if (pivot == null)
        {
            pivot = new GameObject("DoorPivot").transform;
            pivot.SetParent(go.transform, false);
        }
        var door = go.GetComponent<Enigma.Interaction.ConditionalDoorInteractable>();
        if (door == null)
            door = go.AddComponent<Enigma.Interaction.ConditionalDoorInteractable>();
        var so = new SerializedObject(door);
        so.FindProperty("config").objectReferenceValue = config;
        so.FindProperty("doorPivot").objectReferenceValue = pivot;
        so.FindProperty("openYaw").floatValue = openYaw;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BathNotebook(Scene scene)
    {
        var config = AssetDatabase.LoadAssetAtPath<Enigma.Data.InteractableConfig>(NotebookConfig);
        var doc = AssetDatabase.LoadAssetAtPath<Enigma.Data.DocumentData>(NotebookDoc);
        if (config == null || doc == null)
        {
            Debug.LogError("Kitbash bake: faltan los assets del cuaderno del baño");
            return;
        }
        Enigma.UI.DocumentReaderUI reader = null;
        Object dialogue = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (reader == null)
                reader = root.GetComponentInChildren<Enigma.UI.DocumentReaderUI>(true);
            foreach (var other in root.GetComponentsInChildren<Enigma.Interaction.DocumentInteractable>(true))
                if (dialogue == null && other.name == "Interactable_Note")
                    dialogue = new SerializedObject(other).FindProperty("dialogue").objectReferenceValue;
        }
        var go = FindInScene(scene, "Interactable_BathNotebook");
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Interactable_BathNotebook";
            PlaceInteractable(scene, go);
        }
        go.layer = LayerMask.NameToLayer("Interactable");
        var rend = go.GetComponent<MeshRenderer>();
        if (rend != null)
            rend.sharedMaterial = Resources.Load<Material>("Mats/Mat_Kraft");
        var note = go.GetComponent<Enigma.Interaction.DocumentInteractable>();
        if (note == null)
            note = go.AddComponent<Enigma.Interaction.DocumentInteractable>();
        var so = new SerializedObject(note);
        so.FindProperty("config").objectReferenceValue = config;
        so.FindProperty("dialogue").objectReferenceValue = dialogue;
        so.FindProperty("document").objectReferenceValue = doc;
        so.FindProperty("readerUi").objectReferenceValue = reader;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool ThemeScene(Scene scene)
    {
        MainMenuContinue(scene);
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
