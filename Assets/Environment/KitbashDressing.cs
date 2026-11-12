using Enigma.Data;
using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Enigma.Environment
{
    // Kenney en escena. Colliders del greybox se quedan.
    public partial class KitbashDressing : MonoBehaviour
    {
        public const int BakeId = 44;
        public int bakedId;

        [SerializeField] private DocumentData newspaper;
        // El diario del nivel se lee y entra al journal; los demás diarios del piso son decorado.
        [SerializeField] private Material newspaperMaterial;
        [SerializeField] private InteractableConfig newspaperConfig;

        private Transform _root;
        private Material _wood;
        private Material _rust;
        private Material _paper;
        private Material _paperBurn;
        private Material _plaster;
        private Material _shutter;
        private Material _plastic;
        private Material _screenOff;
        private System.Random _rng;

        public void Bake(string sceneName)
        {
            _root = transform;
            _rng = new System.Random(sceneName.GetHashCode());
            _wood = Resources.Load<Material>("Mats/Mat_WoodFloor");
            _rust = Resources.Load<Material>("Mats/Mat_RustMetal");
            _paper = Resources.Load<Material>("Mats/Mat_PaperScrap");
            _paperBurn = Resources.Load<Material>("Mats/Mat_PaperBurn");
            _plaster = Resources.Load<Material>("Mats/Mat_PlasterWall");

            PlaceGrey(sceneName);
            ArrangeRooms(sceneName);
            OverlayNamed();
            Extra(sceneName);
            SideRooms(sceneName);
            Lights(sceneName);
            PowerTubes(sceneName);
            CapShadows();
            Underground();
            Papers(sceneName);
            PaintGrey();
            if (sceneName == "Level03_Archives")
                DressArchiveHatch();
            if (sceneName == "Level02_Lab")
                DressGenerator();
            SlabLegs("ArchiveDesk");
            SlabLegs("Console");
            WireMagnet(sceneName);
            PlaceRoofs(sceneName);
            PowerBulbs(sceneName);
            DressRuntime(sceneName);
            PlaceArrival(sceneName);
            bakedId = BakeId;
        }

        private void KillNamed(string name)
        {
            var go = FindLocal(name);
            if (go != null)
                DropGo(go);
        }

        private static void DropGo(GameObject go)
        {
            if (go == null)
                return;
            go.SetActive(false);
            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }

        private static Transform FindChildNamed(Transform t, string name)
        {
            if (t.name == name)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var f = FindChildNamed(t.GetChild(i), name);
                if (f != null)
                    return f;
            }
            return null;
        }

        private static void ClampBox(GameObject go, float maxX, float maxZ)
        {
            var b = Encapsulate(go);
            float sx = b.size.x > maxX && b.size.x > 0.01f ? maxX / b.size.x : 1f;
            float sz = b.size.z > maxZ && b.size.z > 0.01f ? maxZ / b.size.z : 1f;
            float s = Mathf.Min(sx, sz);
            if (s < 0.999f)
                go.transform.localScale *= s;
        }

        private static void FitToHeight(GameObject go, float floorY, float maxH)
        {
            var b = Encapsulate(go);
            if (b.size.y > 0.01f)
                go.transform.localScale *= maxH / b.size.y;
            b = Encapsulate(go);
            var p = go.transform.position;
            go.transform.position = new Vector3(p.x, p.y + (floorY - b.min.y), p.z);
        }

        private static void FitToBox(GameObject go, float floorY, float width, float height, float thick)
        {
            var b = Encapsulate(go);
            Vector3 s = go.transform.localScale;
            if (b.size.x > 0.01f)
                s.x *= width / b.size.x;
            if (b.size.y > 0.01f)
                s.y *= height / b.size.y;
            if (b.size.z > 0.01f)
                s.z *= thick / b.size.z;
            go.transform.localScale = s;
            b = Encapsulate(go);
            var p = go.transform.position;
            go.transform.position = new Vector3(p.x, p.y + (floorY - b.min.y), p.z);
        }

        private static Bounds Encapsulate(GameObject go)
        {
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            if (rends.Length == 0)
                return new Bounds(go.transform.position, Vector3.one * 0.01f);
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            return b;
        }

        private void Start()
        {
            var box = FindLocal("Interactable_ElectricalBox");
            if (box != null)
                HideProto(box);

            var oldFlicker = FindLocal("Light_Flickering");
            if (oldFlicker != null)
                oldFlicker.SetActive(false);

            var mag = FindLocal("Interactable_Magnet");
            if (mag != null)
            {
                for (int i = 0; i < mag.transform.childCount; i++)
                {
                    var c = mag.transform.GetChild(i);
                    if (c.name.StartsWith("Kit_"))
                        c.gameObject.SetActive(false);
                }
            }

            EnsureMats();
            _root = transform;
            PaintProps();
            CoverN4Pit();
            if (gameObject.scene.name == "Level03_Archives")
            {
                if (FindLocal("Kit_ExitPad") == null)
                    ExitPad(new Vector3(0f, 0.7f, 2.4f), new Vector3(1.15f, 1.4f, 1.15f));
                // Las letras del piso no se ven a través de los archiveros
                foreach (var doc in FindObjectsByType<DocumentInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (doc.gameObject.scene == gameObject.scene)
                        ItemVisuals.WorldText(doc.gameObject);
            }
            PlaceRoofs(gameObject.scene.name);
            DressRuntime(gameObject.scene.name);
            WireExitPads();
            SnapDoorExit();
            SideRoomText();

            if (gameObject.scene.name == "Level03_Mezzanine")
            {
                PlacePuzzleMetals();
                PlaceMagnetByMetals();
                WireMagnet("Level03_Mezzanine");
                PlaceMezzHatch();
                DressMezzanine();
                ItemVisuals.WorldText(FindLocal("Kit_PaintingFrame"));
            }

            PlaceWireBox();
        }

        private static void Paint(GameObject go, Material mat)
        {
            if (mat == null)
                return;
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (r.gameObject != go && (KeepVisual(r.gameObject.name) || InOtherInteractable(r.transform, go.transform)))
                    continue;
                var arr = r.sharedMaterials;
                for (int m = 0; m < arr.Length; m++)
                    arr[m] = mat;
                r.sharedMaterials = arr;
            }
        }

        // Un material por parte: el slot i toma slots[i]; null o fuera del arreglo deja el importado
        private static void PaintSlots(GameObject go, Material[] slots)
        {
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (r.gameObject != go && (KeepVisual(r.gameObject.name) || InOtherInteractable(r.transform, go.transform)))
                    continue;
                var arr = r.sharedMaterials;
                for (int m = 0; m < arr.Length && m < slots.Length; m++)
                {
                    if (slots[m] != null)
                        arr[m] = slots[m];
                }
                r.sharedMaterials = arr;
            }
        }

        // Kits con partes: orden de slots según el modelo importado. Arreglo vacío = colores propios del modelo
        private static Material[] SlotsFor(string kit)
        {
            switch (kit)
            {
                case "HospitalBed":
                    return new[] { Mat("Mat_BedFrame"), Mat("Mat_Mattress"), Mat("Mat_Pillow"), Mat("Mat_BedBoard"), Mat("Mat_Rubber") };
                case "Armchair":
                    return new[] { Mat("Mat_DarkWood"), Mat("Mat_Leather") };
                case "radio":
                    return new[] { Mat("Mat_Steel"), Mat("Mat_DarkWood"), Mat("Mat_Steel") };
                case "sideTable":
                    return new[] { Mat("Mat_DarkWood"), Mat("Mat_DarkWood") };
                case "NightStand_2":
                    return new[] { Mat("Mat_DarkWood"), Mat("Mat_Steel") };
                case "Chair_3":
                    return new[] { Mat("Mat_DarkWood"), Mat("Mat_Leather") };
                case "books":
                case "cardboardBoxClosed":
                case "cardboardBoxOpen":
                    return new Material[0];
            }
            return null;
        }

        private static Material Mat(string name) => Resources.Load<Material>("Mats/" + name);

        // Pinta un kit recién ubicado: por partes si las tiene, si no entero con mat (null = sin pintar)
        private static void PaintKit(GameObject go, string kit, Material mat)
        {
            var slots = SlotsFor(kit);
            if (slots != null)
                PaintSlots(go, slots);
            else if (mat != null)
                Paint(go, mat);
        }

        // Un interactable anidado (la llave dentro del cajón) conserva su material
        private static bool InOtherInteractable(Transform t, Transform root)
        {
            for (; t != null && t != root; t = t.parent)
            {
                if (t.GetComponent<InteractableBase>() != null)
                    return true;
            }
            return false;
        }

        private void PaintGoNamed(string name, Material mat)
        {
            var src = FindLocal(name);
            if (src == null)
                return;
            Paint(src, mat);
        }

        private void SupportCollider(string name)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                box = go.AddComponent<BoxCollider>();
            box.isTrigger = false;
            box.center = Vector3.zero;
            box.size = Vector3.one;
        }

        private static Vector3 IntoRoomFrom(Transform t)
        {
            if (t == null)
                return Vector3.back;
            Vector3 into = -t.forward;
            into.y = 0f;
            if (into.sqrMagnitude < 0.01f)
                into = Vector3.back;
            return into.normalized;
        }

        private void EnsureMats()
        {
            if (_wood == null)
                _wood = Resources.Load<Material>("Mats/Mat_WoodFloor");
            if (_rust == null)
                _rust = Resources.Load<Material>("Mats/Mat_RustMetal");
            if (_paper == null)
                _paper = Resources.Load<Material>("Mats/Mat_PaperScrap");
            if (_paperBurn == null)
                _paperBurn = Resources.Load<Material>("Mats/Mat_PaperBurn");
            if (_plaster == null)
                _plaster = Resources.Load<Material>("Mats/Mat_PlasterWall");
        }

        private static void FitLongOnFloor(GameObject go, float floorY, float length, float height)
        {
            var b = Encapsulate(go);
            float horiz = Mathf.Max(b.size.x, b.size.z);
            if (horiz > 0.01f)
                go.transform.localScale *= length / horiz;
            b = Encapsulate(go);
            if (b.size.y > height && b.size.y > 0.01f)
                go.transform.localScale *= height / b.size.y;
            b = Encapsulate(go);
            var p = go.transform.position;
            go.transform.position = new Vector3(p.x, p.y + (floorY - b.min.y), p.z);
        }

        private void PlaceOrMoveSlab(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = FindLocal(name);
            if (go == null)
            {
                Slab(name, pos, scale, mat);
                return;
            }
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (mat != null)
                Paint(go, mat);
        }

        private void DressRuntime(string sceneName)
        {
            DressCredential();
            FrameOrigenDoor();
            OverlayDoor("Interactable_Door");
            OverlayDoor("Interactable_DoorN3");
            OverlayDoor("Interactable_OrigenDoor");
            PaintOrigenDoor();
            LockOrigenDoor();
            DressLockerBank();
            PlaceOrigenStencil();
            OverlayPapers();
            OverlayCrate("Crate_R0", "Kit_CrateBox_0");
            OverlayCrate("Crate_R1", "Kit_CrateBox_1");
            OverlayCrate("Crate_R2", "Kit_CrateBox_2");
            OverlayCrate("Crate_R3", "Kit_CrateBox_3");
            HangCables(sceneName);
            ScatterBoxes(sceneName);
            DressDebris();
            StampNewspapers(sceneName);
            DressStretcher();
            DressOfficeChairs();
            PaintComputers();
            DressArchivePc(sceneName);
            PlaceLeaks(sceneName);
            PlaceBrokenCams(sceneName);
            WallPipes(sceneName);
            DressGreyProps(sceneName);
            DressAttempts();
            UprightPass();
            PaintProps();
            PlaceOverrideKey();
            SettleProps();
            FitPcScreen(sceneName);
            PropSolidPass();
        }

        private void Slab(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            if (_root == null)
                _root = transform;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null && mat != null)
            {
                rend.sharedMaterial = mat;
                rend.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void StandUpright(GameObject go, float floorY, float height)
        {
            if (go == null)
                return;
            for (int i = 0; i < 2; i++)
            {
                var b = Encapsulate(go);
                if (b.size.y >= b.size.x * 0.9f && b.size.y >= b.size.z * 0.9f)
                    break;
                if (b.size.x >= b.size.z)
                    go.transform.Rotate(0f, 0f, 90f, Space.Self);
                else
                    go.transform.Rotate(90f, 0f, 0f, Space.Self);
            }
            if (height > 0.01f)
                FitToHeight(go, floorY, height);
            else
            {
                var b2 = Encapsulate(go);
                var p = go.transform.position;
                go.transform.position = new Vector3(p.x, p.y + (floorY - b2.min.y), p.z);
            }
        }

        private GameObject FindLocal(string name)
        {
            var scene = gameObject.scene;
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var t = FindNamed(roots[i].transform, name);
                if (t != null)
                    return t.gameObject;
            }
            return null;
        }

        private static Transform FindNamed(Transform t, string name)
        {
            if (t.name == name)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var f = FindNamed(t.GetChild(i), name);
                if (f != null)
                    return f;
            }
            return null;
        }

        private Light Point(string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = new GameObject("Light_" + name);
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }
    }
}
