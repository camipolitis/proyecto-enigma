using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Enigma.Environment
{
    // Kenney en escena. Colliders del greybox se quedan.
    public class KitbashDressing : MonoBehaviour
    {
        public const int BakeId = 17;
        public int bakedId;

        private Transform _root;
        private Material _wood;
        private Material _rust;
        private Material _paper;
        private Material _paperBurn;
        private Material _plaster;
        private Material _fuseNeon;
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
            OverlayNamed();
            Extra(sceneName);
            Lights(sceneName);
            CapShadows();
            Papers(sceneName);
            PaintGrey();
            PlacePrints(sceneName);
            WireMagnet(sceneName);
            PlaceRoofs(sceneName);
            DressRuntime(sceneName);
            bakedId = BakeId;
        }

        private void OverlayNamed()
        {
            Overlay("Decor_Sink", "Bathroom_Sink", _rust);

            OverlayChild("Metal_A", "metal-panel", _rust);
            OverlayChild("Metal_B", "metal-panel-screws", _rust);
            OverlayChild("Metal_C", "metal-panel-narrow", _rust);
            OverlayChild("Interactable_ElectricalBox", "ElectricalBox", null, false);
            OverlayChild("Interactable_Credential", "Pickup_KeyCard", null, false);

            OverlayDoor("Interactable_Door");
            OverlayDoor("Interactable_DoorN3");
            OverlayDoor("Interactable_OrigenDoor");
            OverlayPapers();

            PaintPrefix("Crate_", _wood);
            PaintPrefix("Debris_", _rust);
        }

        private void Extra(string sceneName)
        {
            if (sceneName == "Level01_Room")
                ExtraRoom();
            else if (sceneName == "Level02_Lab")
                ExtraLab();
            else if (sceneName == "Level03_Archives")
                ExtraArchives();
            else if (sceneName == "Level03_Mezzanine")
                ExtraMezz();
            else if (sceneName == "Level04_Origen")
                ExtraOrigen();
        }

        private void ExtraRoom()
        {
            // Rincón SO: mesita lejos de la camilla.
            Put("NightStand_2", new Vector3(-8.6f, 0f, -6.35f), 20f, _wood, 0.7f);
            Put("Details_Cylinder", new Vector3(-8.85f, 0.22f, -6.55f), 40f, _rust);
            Put("Trashcan_Small2", new Vector3(8.7f, 0f, -6.5f), 18f, _rust);
            Put("container-flat-open", new Vector3(8.55f, 0.02f, 6.6f), -25f, _rust);
            Put("metal-panel", new Vector3(-8.7f, 0.02f, 7.4f), 12f, _rust);
            Put("metal-panel-narrow", new Vector3(-8.4f, 0.02f, 7.15f), -30f, _rust);

            Put("RoofTile_Vents", new Vector3(-3.4f, 7.35f, -3.6f), 0f, _rust);
            Put("RoofTile_Vents", new Vector3(4.2f, 7.35f, 4.8f), 90f, _rust);
            Put("pipe-large-long", new Vector3(-9.55f, 7.1f, 0f), 90f, _rust, 0.32f);
            Put("pipe-large-bend", new Vector3(-9.4f, 7.1f, 7.6f), 90f, _rust, 0.32f);
            Put("pipe-end", new Vector3(-9.55f, 7.05f, -7.1f), 90f, _rust, 0.28f);

            Put("Light_CeilingSingle", new Vector3(0f, 7.35f, 0f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(4.8f, 7.35f, 3.4f), 0f, _rust);

            Put("computerScreen", new Vector3(5.52f, 0.82f, 3.05f), 180f, _wood, 0.28f);
            Put("computerKeyboard", new Vector3(5.50f, 0.81f, 2.72f), 180f, _wood, 0.04f);
            Put("computerMouse", new Vector3(5.78f, 0.81f, 2.70f), 10f, _wood, 0.03f);
            Put("radio", new Vector3(-8.55f, 0.72f, -6.2f), 25f, _wood, 0.16f);
            Put("books", new Vector3(5.2f, 0.82f, 2.55f), 12f, _wood, 0.14f);
            Put("sideTable", new Vector3(-7.35f, 0f, 7.15f), -18f, _wood, 0.75f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N1a", new Vector3(8.15f, 0.02f, -6.05f), 15f, _wood, 0.48f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N1b", new Vector3(8.45f, 0.02f, -5.55f), -22f, _wood, 0.38f);

            ExitPad(new Vector3(0f, 1.1f, 10.55f), new Vector3(2.4f, 2.2f, 1.2f));
        }

        private void ExtraLab()
        {
            Put("workbench", new Vector3(-6.6f, 0f, -4.2f), 90f, _rust, 0.9f);
            Put("table-display", new Vector3(6.35f, 0f, -3.35f), -90f, _rust, 0.9f);
            Put("table-display-small", new Vector3(-6.55f, 0f, -0.4f), 90f, _rust, 0.85f);
            Put("Table_RoundSmall", new Vector3(6.55f, 0f, 0.6f), 8f, _wood, 0.85f);
            Put("Chair_3", new Vector3(5.15f, 0f, -4.15f), 40f, _wood, 0.95f);

            Put("pipe-glass-large-long", new Vector3(-7.45f, 1.05f, 0.6f), 90f, _rust);
            Put("pipe-glass-large-valve", new Vector3(-7.45f, 1.05f, 3.6f), 90f, _rust);
            Put("pipe-large-long", new Vector3(-7.55f, 2.15f, 0.2f), 90f, _rust);
            Put("pipe-large-valve", new Vector3(7.45f, 2.15f, -0.6f), 90f, _rust);
            Put("pipe-end", new Vector3(7.45f, 0.9f, -5.15f), -90f, _rust);
            Put("pipe-end", new Vector3(-7.45f, 0.35f, -5.0f), 90f, _rust);
            Put("Details_Cylinder", new Vector3(-6.5f, 0.38f, -4.15f), 18f, _rust);
            Put("Details_Cylinder", new Vector3(-6.25f, 0.38f, -4.35f), -22f, _rust);

            Put("warning-orange", new Vector3(-7.55f, 1.7f, 5.35f), 180f, _rust);
            Put("warning-orange", new Vector3(7.55f, 1.7f, -5.35f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(-3.2f, 7.35f, -2.2f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(3.4f, 7.35f, 2.2f), 0f, _rust);

            Put("container-tall", new Vector3(-7.15f, 0f, 4.55f), 18f, _rust, 1.4f);
            Put("container-flat-open", new Vector3(7.05f, 0f, 4.55f), -22f, _rust);
            Put("container-flat", new Vector3(-7.2f, 0.02f, -5.25f), 30f, _rust);
            Put("metal-panel", new Vector3(7.2f, 0.02f, -5.15f), -15f, _rust);
            Put("metal-panel-screws", new Vector3(7.05f, 0.02f, -4.85f), 40f, _rust);
            Put("Details_Pipes_Long", new Vector3(7.45f, 1.4f, 1.6f), 0f, _rust);
            Put("Trashcan_Small2", new Vector3(-7.35f, 0f, 5.15f), 25f, _rust);

            Put("container-flat", new Vector3(0.95f, 0.02f, 0.15f), 8f, _rust, 0.16f);
            Put("Battery", new Vector3(1.2f, 0.18f, -0.2f), 90f, null, 0.22f, false);
            Put("ElectricalBox02", new Vector3(7.45f, 1.15f, 3.2f), -90f, null, 0.7f, false);
            Put("WorkLight", new Vector3(-7.2f, 0f, -5.15f), 25f, null, 1.1f, false);
            Put("GasCylinder", new Vector3(7.25f, 0f, -5.2f), -20f, null, 0.95f, false);
            Put("CableDrum", new Vector3(-7.25f, 0f, 4.85f), 30f, null, 0.7f, false);
            Put("radio", new Vector3(-6.35f, 0.92f, -4.05f), 8f, _wood, 0.14f);
            Put("books", new Vector3(6.15f, 0.92f, -3.15f), -12f, _wood, 0.16f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N2a", new Vector3(-6.85f, 0.02f, 5.05f), 18f, _wood, 0.45f);

            ExitPad(new Vector3(0f, 1.1f, 6.55f), new Vector3(2.4f, 2.2f, 1.2f));
        }

        private void ExtraArchives()
        {
            Put("Shelf_Small1", new Vector3(-7.15f, 0f, -4.4f), 90f, _wood, 1.4f);
            Put("Shelf_Small1", new Vector3(7.2f, 0f, -4.6f), -90f, _wood, 1.4f);
            Put("Shelf_Small1", new Vector3(-7.35f, 0f, 4.55f), 90f, _wood, 1.4f);

            Put("metal-panel", new Vector3(-7.1f, 0.02f, 4.15f), 22f, _rust);
            Put("metal-panel-screws", new Vector3(-6.85f, 0.02f, 3.85f), -18f, _rust);
            Put("metal-panel-narrow", new Vector3(7.15f, 0.02f, 4.35f), 35f, _rust);
            Put("container-flat", new Vector3(7.05f, 0.02f, 4.05f), -12f, _rust);
            Put("container-flat-open", new Vector3(-7.2f, 0.02f, -4.85f), 28f, _rust);
            Put("Trashcan_Small2", new Vector3(7.15f, 0f, 0.15f), 8f, _rust);

            Put("pipe-large-long", new Vector3(-7.7f, 7.1f, 0.4f), 90f, _rust);
            Put("pipe-large-long", new Vector3(7.7f, 7.1f, -1.2f), 90f, _rust);

            Put("Light_CeilingSingle", new Vector3(-3.4f, 7.35f, -1.6f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(3.2f, 7.35f, 1.4f), 0f, _rust);

            Put("computerScreen", new Vector3(2.72f, 0.92f, -1.72f), 180f, _wood, 0.26f);
            Put("books", new Vector3(-7.15f, 1.15f, -4.4f), 90f, _wood, 0.22f);
            Put("cardboardBoxOpen", new Vector3(7.15f, 0.02f, -4.95f), 0f, _wood, 0.45f);
            Put("radio", new Vector3(3.15f, 0.62f, -1.55f), -8f, _wood, 0.14f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N3a", new Vector3(-6.55f, 0.02f, 4.85f), 12f, _wood, 0.42f);
            Put("ElectricalBox02", new Vector3(-7.55f, 1.05f, 1.6f), 90f, null, 0.85f, false);
            StandUpright(FindLocal("Kit_ElectricalBox02"), 1.05f, 0.85f);
            ExitPad(new Vector3(0f, 0.7f, 2.4f), new Vector3(1.15f, 1.4f, 1.15f));
            PlaceWireBox();
        }

        private void ExtraMezz()
        {
            Put("Light_CeilingSingle", new Vector3(0f, 7.35f, 0f), 0f, _rust);
            Put("Trashcan_Small2", new Vector3(-7.2f, 0f, -3.4f), 40f, _rust);
            Put("container-flat-open", new Vector3(7.0f, 0.02f, -3.35f), -30f, _rust);
            Put("metal-panel", new Vector3(-7.15f, 0.02f, 4.6f), 16f, _rust);
            Put("metal-panel-narrow", new Vector3(7.1f, 0.02f, 4.45f), -22f, _rust);
            Put("container-flat", new Vector3(6.95f, 0.02f, 4.15f), 40f, _rust);
            Put("books", new Vector3(-6.4f, 0.02f, -3.15f), 22f, _wood, 0.12f);
            PutNamed("cardboardBoxClosed", "Kit_Box_Mezz", new Vector3(6.55f, 0.02f, -2.85f), 8f, _wood, 0.4f);

            Put("metal-panel-narrow", new Vector3(-3.4f, 0.08f, 5.22f), 25f, _rust, 0.08f);
            Put("metal-panel-narrow", new Vector3(-0.95f, 0.08f, 5.18f), -18f, _rust, 0.07f);
            Put("metal-panel-narrow", new Vector3(-2.55f, 0.08f, 5.12f), 50f, _rust, 0.07f);
            Put("metal-panel-narrow", new Vector3(-1.6f, 0.08f, 5.24f), -35f, _rust, 0.06f);
            Put("metal-panel-narrow", new Vector3(-3.9f, 0.08f, 5.15f), 8f, _rust, 0.07f);
            Put("metal-panel-narrow", new Vector3(-0.55f, 0.08f, 5.08f), 42f, _rust, 0.06f);

            ExitPadAtDoor("Interactable_DoorN3", 0.9f);
            PlaceMezzHatch();
        }

        private void ExtraOrigen()
        {
            Put("pipe-large-long", new Vector3(7.45f, -1.5f, 0f), 90f, _rust);
            Put("pipe-large-valve", new Vector3(7.4f, -1.55f, 1.8f), 90f, _rust);
            Put("metal-panel", new Vector3(7.55f, -1.35f, 0f), -90f, _rust);

            Put("rail", new Vector3(0f, 0.13f, 3.15f), 0f, _rust);
            Put("rail", new Vector3(3.35f, 0.13f, 0.4f), 90f, _rust);
            Put("Light_CeilingSingle", new Vector3(0f, 7.35f, 4.2f), 0f, _rust);
            Put("container-flat", new Vector3(-6.6f, -2.55f, 3.4f), 40f, _rust);
            Put("pipe-end", new Vector3(-7.4f, -1.5f, -1.2f), 90f, _rust);
            Put("metal-panel-narrow", new Vector3(-7.2f, -2.52f, -3.6f), 22f, _rust);
            Put("container-flat-open", new Vector3(6.8f, -2.55f, -3.5f), 0f, _rust);
            Put("books", new Vector3(3.4f, 0.72f, -3.45f), -15f, _wood, 0.14f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N4a", new Vector3(-6.05f, 0.02f, -3.55f), 14f, _wood, 0.42f);

            ExitPad(new Vector3(0f, 1.1f, 6.45f), new Vector3(2.4f, 2.2f, 1.2f));
            CoverN4Pit();
        }

        private void Lights(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                var fill = Point("Fill", new Vector3(0f, 6.8f, 0f), new Color(1f, 0.84f, 0.62f), 6.5f, 16f);
                var flicker = fill.gameObject.AddComponent<LightFlicker>();
                flicker.baseIntensity = 6.5f;
                Point("DeskLamp", new Vector3(5.5f, 1.6f, 2.5f), new Color(1f, 0.72f, 0.45f), 3.2f, 5f);
            }
            else if (sceneName == "Level02_Lab")
            {
                Point("Fill", new Vector3(0f, 6.6f, 0f), new Color(0.45f, 0.55f, 0.5f), 1.8f, 16f);
                Point("WarnA", new Vector3(7.2f, 2.1f, 4.4f), new Color(1f, 0.22f, 0.1f), 2.6f, 7f);
                Point("WarnB", new Vector3(-6.8f, 2.15f, -4.8f), new Color(1f, 0.28f, 0.12f), 2.2f, 6.5f);
            }
            else if (sceneName == "Level03_Archives")
            {
                Point("EmergA", new Vector3(-3.4f, 6.8f, -1.5f), new Color(1f, 0.32f, 0.12f), 2.4f, 14f);
                Point("EmergB", new Vector3(3.2f, 6.8f, 1.3f), new Color(1f, 0.3f, 0.1f), 2.3f, 14f);
                Point("Fire", new Vector3(3.8f, 0.7f, 1.6f), new Color(1f, 0.38f, 0.08f), 2.4f, 4.5f);
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                Point("Fill", new Vector3(0f, 6.8f, 0f), new Color(0.9f, 0.82f, 0.62f), 5.2f, 16f);
            }
            else if (sceneName == "Level04_Origen")
            {
                Point("Gallery", new Vector3(0f, 6.5f, 4.2f), new Color(0.85f, 0.7f, 0.5f), 4.8f, 16f);
                Point("Pit", new Vector3(0f, -1.4f, 0f), new Color(1f, 0.38f, 0.16f), 5.5f, 10f);
            }
        }

        // URP 2048 no entra tantas sombras puntuales. Solo la direccional proyecta.
        private void CapShadows()
        {
            var scene = gameObject.scene;
            if (!scene.IsValid())
                return;
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                var lights = roots[r].GetComponentsInChildren<Light>(true);
                for (int i = 0; i < lights.Length; i++)
                {
                    var light = lights[i];
                    if (light == null || light.type == LightType.Directional)
                        continue;
                    light.shadows = LightShadows.None;
                }
            }
        }

        private void Papers(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                Scraps(new Vector3(-8.4f, 0.72f, -6.2f), 4, 0.22f, _paper);
            }
            else if (sceneName == "Level02_Lab")
            {
                Scraps(new Vector3(-6.2f, 0.42f, -4.8f), 5, 0.32f, _paper);
                Scraps(new Vector3(7.0f, 0.04f, 4.4f), 3, 0.28f, _paper);
            }
            else if (sceneName == "Level03_Archives")
            {
                Scraps(new Vector3(-7.0f, 0.04f, 4.2f), 5, 0.38f, _paper);
                Scraps(new Vector3(7.05f, 0.04f, 4.15f), 3, 0.35f, _paper);
                Scraps(new Vector3(3.7f, 0.04f, 1.35f), 4, 0.4f, _paperBurn);
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                Scraps(new Vector3(-3.2f, 0.02f, 2.15f), 3, 0.22f, _paperBurn);
            }
            else if (sceneName == "Level04_Origen")
            {
                Scraps(new Vector3(3.8f, 0.68f, -3.6f), 5, 0.32f, _paper);
            }
        }

        private void PaintGrey()
        {
            PaintGoNamed("Generator", _rust);
            PaintGoNamed("Lockers", _rust);
            PaintGoNamed("BurnMark", _rust);
            PaintGoNamed("HatchLid", _rust);
            PaintGoNamed("Chair_Origen", _rust);
            PaintGoNamed("ArchiveDesk", _wood);
            PaintGoNamed("Console", _rust);
            PaintGoNamed("Decor_Stretcher", _wood);
            PaintGoNamed("Decor_Desk", _wood);
            PaintGoNamed("Desk", _wood);
            PaintGoNamed("Decor_FallenFurniture", _wood);
            PaintGoNamed("Decor_Shelf", _wood);
            PaintGoNamed("CabinetRow_L", _wood);
            PaintGoNamed("CabinetRow_R", _rust);
            PaintProps();
        }

        private void Overlay(string goName, string kit, Material mat)
        {
            var src = FindLocal(goName);
            if (src == null)
                return;
            var rend = src.GetComponent<MeshRenderer>();
            if (rend != null && rend.bounds.size.y < 0.25f)
            {
                Paint(src, mat);
                return;
            }
            float minH = goName.IndexOf("Chair") >= 0 ? 0.9f : 0.35f;
            float h = 1.1f;
            if (rend != null)
                h = Mathf.Clamp(rend.bounds.size.y, minH, 2.2f);
            if (!Put(kit, FloorPos(src), src.transform.eulerAngles.y, mat, h))
                return;
            HideVisual(src);
        }

        private void PaintPrefix(string prefix, Material mat)
        {
            var scene = gameObject.scene;
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.gameObject.scene != scene)
                    continue;
                if (!t.name.StartsWith(prefix) || t.name.StartsWith("Kit_"))
                    continue;
                Paint(t.gameObject, mat);
            }
        }

        private void OverlayChild(string goName, string kit, Material mat, bool paint = true)
        {
            var src = FindLocal(goName);
            if (src == null)
                return;
            OverlayChildOn(src, kit, mat, paint, 0f);
        }

        private void OverlayChildOn(GameObject src, string kit, Material mat, bool paint, float doorH)
        {
            var already = src.transform.Find("Kit_" + kit);
            if (already != null && already.gameObject.activeSelf)
            {
                if (kit == "ElectricalBox")
                    HideProto(src);
                else
                    HideSelf(src);
                return;
            }
            if (already != null)
            {
                already.name = "Kit_" + kit + "_Dead";
                already.gameObject.SetActive(false);
                DropGo(already.gameObject);
            }

            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
            {
                if (kit == "ElectricalBox")
                    HideProto(src);
                else
                {
                    var mesh = src.GetComponent<MeshRenderer>();
                    if (mesh != null)
                        mesh.enabled = true;
                }
                return;
            }
            var go = Object.Instantiate(prefab, src.transform);
            go.name = "Kit_" + kit;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = prefab.transform.rotation;
            var s = src.transform.lossyScale;
            go.transform.localScale = new Vector3(
                s.x > 0.01f ? 1f / s.x : 1f,
                s.y > 0.01f ? 1f / s.y : 1f,
                s.z > 0.01f ? 1f / s.z : 1f);
            StripColliders(go);
            if (paint && mat != null)
                Paint(go, mat);
            var rend = src.GetComponent<MeshRenderer>();
            float floorY = rend != null ? rend.bounds.min.y : src.transform.position.y;
            float h = rend != null ? Mathf.Max(0.2f, rend.bounds.size.y) : 1f;
            if (doorH > 0.01f)
            {
                floorY = src.transform.position.y - doorH * 0.5f;
                h = doorH;
            }
            FitToHeight(go, floorY, h);
            if (kit == "ElectricalBox")
            {
                ClampBox(go, 0.52f, 0.22f);
                HideProto(src);
            }
            else
                HideSelf(src);
        }

        private void OverlayDoor(string doorName)
        {
            KillNamed("Kit_doorwayFront_" + doorName);
            var door = FindLocal(doorName);
            if (door == null)
                return;

            if (doorName == "Interactable_OrigenDoor")
            {
                Transform host = FindChildNamed(door.transform, "DoorLeaf");
                if (host == null)
                    host = door.transform;
                var paneled = FindChildNamed(door.transform, "Kit_door_B");
                if (paneled != null)
                    DropGo(paneled.gameObject);
                OverlayOrigenLeaf(host.gameObject);
                HideNonKit(door);
                return;
            }

            Transform pivot = FindChildNamed(door.transform, "DoorPivot");
            if (pivot == null)
                pivot = FindChildNamed(door.transform, "DoorPivotN3");
            if (pivot == null && door.transform.parent != null &&
                door.transform.parent.name.IndexOf("DoorPivot") >= 0)
                pivot = door.transform.parent;
            if (pivot == null)
                pivot = door.transform;

            Transform leaf = FindChildNamed(door.transform, "DoorLeaf");
            if (leaf == null)
                leaf = door.transform;

            float width = 1.2f;
            float height = 2.4f;
            float thick = 0.14f;
            var leafRend = leaf.GetComponent<MeshRenderer>();
            if (leafRend != null && leafRend.bounds.size.x > 0.2f)
            {
                width = Mathf.Max(0.85f, leafRend.bounds.size.x);
                height = Mathf.Max(1.8f, leafRend.bounds.size.y);
                thick = Mathf.Clamp(leafRend.bounds.size.z, 0.08f, 0.22f);
            }
            else
            {
                var ls = leaf.lossyScale;
                width = Mathf.Max(0.85f, ls.x);
                height = Mathf.Max(1.8f, ls.y);
                thick = Mathf.Clamp(ls.z, 0.08f, 0.22f);
            }

            GameObject go = null;
            var existing = FindChildNamed(door.transform, "Kit_door_B");
            if (existing == null)
                existing = FindChildNamed(pivot, "Kit_door_B");
            if (existing != null)
                go = existing.gameObject;

            if (go == null)
            {
                var prefab = Resources.Load<GameObject>("Kitbash/door_B");
                if (prefab == null)
                {
                    HideNonKit(door);
                    return;
                }
                go = Object.Instantiate(prefab, pivot);
                go.name = "Kit_door_B";
                StripColliders(go);
            }

            go.transform.SetParent(pivot, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            float floorY = pivot.position.y - height * 0.5f;
            FitToBox(go, floorY, width, height, thick);
            AlignHinge(go, pivot);
            HideNonKit(door);
            HideSelf(leaf.gameObject);
            // N3: la hoja ES el interactable (solo trigger). N1: DoorLeaf ya es sólido.
            EnsureWalkCollider(leaf.gameObject);
        }

        private static void AlignHinge(GameObject go, Transform pivot)
        {
            if (go == null || pivot == null)
                return;
            var b = Encapsulate(go);
            Vector3 minLocal = pivot.InverseTransformPoint(new Vector3(b.min.x, b.center.y, b.center.z));
            go.transform.position += pivot.TransformVector(new Vector3(-minLocal.x, 0f, 0f));
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

        private void OverlayPapers()
        {
            var scene = gameObject.scene;
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.gameObject.scene != scene)
                    continue;
                if (!t.name.StartsWith("Doc_"))
                    continue;
                if (t.name.IndexOf("Cassette") >= 0)
                    continue;
                OverlayChildOn(t.gameObject, "UI_Paper", _paper, false, 0f);
            }
        }

        private static void HideNonKit(GameObject src)
        {
            var rends = src.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (IsKitBranch(r.transform, src.transform))
                    continue;
                if (KeepVisual(r.gameObject.name) || r.gameObject.name.StartsWith("Kit_"))
                    continue;
                if (r.GetComponent<Light>() != null)
                    continue;
                r.enabled = false;
            }
        }

        private bool PutNamed(string kit, string name, Vector3 pos, float yaw, Material mat, float maxH = 0f, bool paint = true)
        {
            if (FindLocal(name) != null)
                return true;
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return false;
            var go = Object.Instantiate(prefab, _root);
            go.name = name;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * prefab.transform.rotation;
            StripColliders(go);
            if (paint && mat != null)
                Paint(go, mat);
            StandUpright(go, pos.y, maxH);
            if (ShouldBlock(kit))
                SolidShell(go);
            return true;
        }

        private bool Put(string kit, Vector3 pos, float yaw, Material mat, float maxH = 0f, bool paint = true)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return false;
            var go = Object.Instantiate(prefab, _root);
            go.name = "Kit_" + kit;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * prefab.transform.rotation;
            StripColliders(go);
            if (paint && mat != null)
                Paint(go, mat);
            if (maxH > 0.01f)
                FitToHeight(go, pos.y, maxH);
            if (ShouldBlock(kit))
                SolidShell(go);
            return true;
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

        private void OverlayOrigenLeaf(GameObject src)
        {
            var existing = src.transform.Find("Kit_wall-window-shutters");
            if (existing != null)
            {
                FitToBox(existing.gameObject, src.transform.position.y - 1.2f, 1.55f, 2.35f, 0.14f);
                HideSelf(src);
                return;
            }

            var prefab = Resources.Load<GameObject>("Kitbash/wall-window-shutters");
            if (prefab == null)
            {
                HideSelf(src);
                return;
            }

            var go = Object.Instantiate(prefab, src.transform);
            go.name = "Kit_wall-window-shutters";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = prefab.transform.rotation;
            var ls = src.transform.lossyScale;
            go.transform.localScale = new Vector3(
                ls.x > 0.01f ? 1f / ls.x : 1f,
                ls.y > 0.01f ? 1f / ls.y : 1f,
                ls.z > 0.01f ? 1f / ls.z : 1f);
            StripColliders(go);
            Paint(go, ShutterMat());
            float floorY = src.transform.position.y - 1.2f;
            FitToBox(go, floorY, 1.55f, 2.35f, 0.14f);
            HideSelf(src);
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

        private void Scraps(Vector3 center, int n, float spread, Material mat)
        {
            for (int i = 0; i < n; i++)
            {
                float x = center.x + (float)(_rng.NextDouble() * 2.0 - 1.0) * spread;
                float z = center.z + (float)(_rng.NextDouble() * 2.0 - 1.0) * spread;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "PaperScrap";
                go.transform.SetParent(_root, false);
                go.transform.position = new Vector3(x, center.y, z);
                go.transform.rotation = Quaternion.Euler(0f, (float)_rng.Next(0, 360), (float)_rng.Next(-8, 9));
                go.transform.localScale = new Vector3(0.22f, 0.008f, 0.16f);
                var col = go.GetComponent<Collider>();
                if (col != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(col);
                    else
                        Object.DestroyImmediate(col);
                }
                var rend = go.GetComponent<MeshRenderer>();
                if (rend != null)
                {
                    rend.sharedMaterial = mat;
                    rend.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
        }

        private static Vector3 FloorPos(GameObject src)
        {
            var rend = src.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                var b = rend.bounds;
                return new Vector3(b.center.x, b.min.y, b.center.z);
            }
            var p = src.transform.position;
            return new Vector3(p.x, p.y, p.z);
        }

        private static bool KeepVisual(string n)
        {
            return n.StartsWith("Sign_")
                || n.StartsWith("Interactable_")
                || n.StartsWith("Doc_")
                || n.StartsWith("LockerDrawer")
                || n.StartsWith("Inserted")
                || n.StartsWith("FuseDecor")
                || n.StartsWith("ZoomAnchor")
                || n.StartsWith("PcScreen")
                || n.StartsWith("Hatch")
                || n.StartsWith("PowerLight")
                || n.StartsWith("Panel_");
        }

        private static void HideVisual(GameObject src)
        {
            var rends = src.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (r.gameObject != src && KeepVisual(r.gameObject.name))
                    continue;
                r.enabled = false;
            }
        }

        private static void HideSelf(GameObject src)
        {
            var r = src.GetComponent<MeshRenderer>();
            if (r != null)
                r.enabled = false;
        }

        // Apaga el proto (hijos incluidos, también Panel_*). Deja Kit_ y FuseDecor/Inserted.
        private static void HideProto(GameObject src)
        {
            var rends = src.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (IsKitBranch(r.transform, src.transform))
                    continue;
                if (r.gameObject != src && KeepSlotChild(r.gameObject.name))
                    continue;
                r.enabled = false;
            }
        }

        private static bool KeepSlotChild(string n)
        {
            return n.StartsWith("Inserted")
                || n.StartsWith("FuseDecor")
                || n.StartsWith("ZoomAnchor")
                || n.StartsWith("Kit_");
        }

        private static bool IsKitBranch(Transform t, Transform stop)
        {
            while (t != null && t != stop)
            {
                if (t.name.StartsWith("Kit_"))
                    return true;
                t = t.parent;
            }
            return false;
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
                var mesh = mag.GetComponent<MeshRenderer>();
                if (mesh != null)
                    mesh.enabled = true;
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
                StandUpright(FindLocal("Kit_ElectricalBox02"), 1.05f, 0.85f);
                if (FindLocal("Kit_ExitPad") == null)
                    ExitPad(new Vector3(0f, 0.7f, 2.4f), new Vector3(1.15f, 1.4f, 1.15f));
            }
            PlaceRoofs(gameObject.scene.name);
            DressRuntime(gameObject.scene.name);
            WireExitPads();
            SnapDoorExit();

            if (gameObject.scene.name == "Level03_Mezzanine")
            {
                PlacePuzzleMetals();
                PlaceMagnetByMetals();
                WireMagnet("Level03_Mezzanine");
                PlaceMezzHatch();
            }

            FixOverrideKey();
            PlaceWireBox();
        }

        private void PlaceWireBox()
        {
            var src = FindLocal("Interactable_Wires");
            if (src == null)
                return;

            src.transform.position = new Vector3(-7.85f, 1.28f, -1.15f);
            src.transform.rotation = Quaternion.identity;
            src.transform.localScale = new Vector3(0.22f, 0.9f, 0.55f);

            var col = src.GetComponent<BoxCollider>();
            if (col != null)
            {
                col.isTrigger = true;
                col.center = Vector3.zero;
                col.size = Vector3.one;
            }

            if (src.transform.Find("Kit_ElectricalBox") == null)
            {
                var prefab = Resources.Load<GameObject>("Kitbash/ElectricalBox");
                if (prefab != null)
                {
                    var go = Object.Instantiate(prefab, src.transform);
                    go.name = "Kit_ElectricalBox";
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up) * prefab.transform.rotation;
                    var s = src.transform.lossyScale;
                    go.transform.localScale = new Vector3(
                        s.x > 0.01f ? 1f / s.x : 1f,
                        s.y > 0.01f ? 1f / s.y : 1f,
                        s.z > 0.01f ? 1f / s.z : 1f);
                    StripColliders(go);
                    var rend = src.GetComponent<MeshRenderer>();
                    float floorY = rend != null ? rend.bounds.min.y : src.transform.position.y - 0.45f;
                    float h = rend != null ? Mathf.Max(0.35f, rend.bounds.size.y) : 0.9f;
                    FitToHeight(go, floorY, h);
                    ClampBox(go, 0.32f, 0.58f);
                }
            }

            HideProto(src);
        }

        public static void RestoreHidden(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                var rends = roots[r].GetComponentsInChildren<MeshRenderer>(true);
                for (int i = 0; i < rends.Length; i++)
                {
                    var mesh = rends[i];
                    if (KeepVisual(mesh.gameObject.name))
                        continue;
                    if (mesh.gameObject.name.StartsWith("Kit_"))
                        continue;
                    if (UnderPlayer(mesh.transform))
                        continue;
                    mesh.enabled = true;
                }
            }
        }

        private static bool UnderPlayer(Transform t)
        {
            while (t != null)
            {
                if (t.name == "Player" || t.name == "VisualRoot" || t.name == "MixamoRoot")
                    return true;
                t = t.parent;
            }
            return false;
        }

        public static bool HasKit(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                var ts = roots[r].GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < ts.Length; i++)
                {
                    if (ts[i].name.StartsWith("Kit_"))
                        return true;
                }
            }
            return false;
        }

        public static void StripColliders(GameObject go)
        {
            var cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                if (Application.isPlaying)
                    Object.Destroy(cols[i]);
                else
                    Object.DestroyImmediate(cols[i]);
            }
        }

        private static bool ShouldBlock(string kit)
        {
            if (string.IsNullOrEmpty(kit))
                return false;
            return kit.IndexOf("cardboard") >= 0
                || kit.IndexOf("container") >= 0
                || kit.IndexOf("Chair") >= 0
                || kit.IndexOf("officeChair") >= 0
                || kit.IndexOf("NightStand") >= 0
                || kit.IndexOf("Trashcan") >= 0
                || kit.IndexOf("GasCylinder") >= 0
                || kit.IndexOf("CableDrum") >= 0
                || kit.IndexOf("ElectricalBox") >= 0
                || kit.IndexOf("metal-panel") >= 0
                || kit.IndexOf("HospitalBed") >= 0
                || kit.IndexOf("Bathroom_Sink") >= 0
                || kit.IndexOf("rail") >= 0
                || kit.IndexOf("books") >= 0
                || kit.IndexOf("workbench") >= 0
                || kit.IndexOf("table") >= 0
                || kit.IndexOf("locker") >= 0;
        }

        private void SolidShell(GameObject visual)
        {
            if (visual == null)
                return;
            if (_root == null)
                _root = transform;
            var b = Encapsulate(visual);
            if (b.size.x < 0.04f && b.size.z < 0.04f)
                return;

            string name = visual.name + "_Solid";
            var existing = FindLocal(name);
            GameObject go = existing;
            if (go == null)
            {
                go = new GameObject(name);
                go.AddComponent<BoxCollider>();
            }
            go.transform.SetParent(_root, true);
            go.transform.position = b.center;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                box = go.AddComponent<BoxCollider>();
            box.isTrigger = false;
            box.center = Vector3.zero;
            box.size = b.size;
        }

        private static void FitColliderTo(GameObject grey, GameObject visual)
        {
            if (grey == null || visual == null)
                return;
            var b = Encapsulate(visual);
            var col = grey.GetComponent<BoxCollider>();
            if (col == null)
                col = grey.AddComponent<BoxCollider>();
            col.isTrigger = false;
            col.center = grey.transform.InverseTransformPoint(b.center);
            Vector3 local = grey.transform.InverseTransformVector(b.size);
            col.size = new Vector3(Mathf.Abs(local.x), Mathf.Abs(local.y), Mathf.Abs(local.z));
        }

        private static void EnsureWalkCollider(GameObject go)
        {
            if (go == null)
                return;
            var cols = go.GetComponents<BoxCollider>();
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null && !cols[i].isTrigger)
                    return;
            }
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = false;
            box.center = Vector3.zero;
            box.size = Vector3.one;
            Vector3 ls = go.transform.lossyScale;
            const float minY = 0.45f;
            if (ls.y > 0.001f && ls.y < minY)
            {
                float h = minY / ls.y;
                box.size = new Vector3(box.size.x, h, box.size.z);
                box.center = new Vector3(0f, (h - 1f) * 0.5f, 0f);
            }
        }

        private static void Paint(GameObject go, Material mat)
        {
            if (mat == null)
                return;
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (r.gameObject != go && KeepVisual(r.gameObject.name))
                    continue;
                var arr = r.sharedMaterials;
                for (int m = 0; m < arr.Length; m++)
                    arr[m] = mat;
                r.sharedMaterials = arr;
            }
        }

        private void PaintGoNamed(string name, Material mat)
        {
            var src = FindLocal(name);
            if (src == null)
                return;
            Paint(src, mat);
        }

        private void PlaceGrey(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                var chair = FindLocal("Decor_Chair");
                if (chair != null)
                    chair.transform.position = new Vector3(7.05f, 0.314f, 1.45f);
                var box = FindLocal("Interactable_ElectricalBox");
                if (box != null)
                    HideProto(box);
            }
            else if (sceneName == "Level02_Lab")
            {
                var chair = FindLocal("Chair");
                if (chair != null)
                    chair.transform.position = new Vector3(-1.55f, 0.45f, 0.35f);
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                var paint = FindLocal("Interactable_Painting");
                if (paint != null)
                {
                    paint.SetActive(true);
                    var col = paint.GetComponent<Collider>();
                    if (col != null)
                        col.enabled = false;
                }

                var cred = FindLocal("Interactable_Credential");
                if (cred != null)
                {
                    Transform env = cred.transform;
                    while (env.parent != null)
                        env = env.parent;
                    var sceneRoots = env.gameObject.scene.GetRootGameObjects();
                    Transform parent = null;
                    for (int i = 0; i < sceneRoots.Length; i++)
                    {
                        if (sceneRoots[i].name == "Environment")
                        {
                            parent = sceneRoots[i].transform;
                            break;
                        }
                    }
                    cred.transform.SetParent(parent, true);
                    Vector3 into = IntoRoomFrom(paint != null ? paint.transform : cred.transform);
                    Vector3 cp = paint != null ? paint.transform.position + into * 0.16f : cred.transform.position;
                    if (paint != null)
                        cp.y = paint.transform.position.y - 0.42f;
                    cred.transform.position = cp;
                    cred.transform.rotation = Quaternion.identity;
                    cred.transform.localScale = new Vector3(0.42f, 0.24f, 0.06f);
                    cred.SetActive(false);
                    var box = cred.GetComponent<BoxCollider>();
                    if (box != null)
                    {
                        box.isTrigger = true;
                        box.size = new Vector3(1.6f, 1.8f, 2.2f);
                    }
                }

                PlacePuzzleMetals();
                PlaceMagnetByMetals();
            }
        }

        private void PlaceMetal(string name, Vector3 pos, Vector3 scale)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            go.transform.position = pos;
            go.transform.localScale = scale;
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                box = go.AddComponent<BoxCollider>();
            box.isTrigger = false;
            box.size = Vector3.one;
            box.center = Vector3.zero;
            EnsureWalkCollider(go);
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

        private void PlacePuzzleMetals()
        {
            var paint = FindLocal("Interactable_Painting");
            if (paint == null)
                return;
            Vector3 p = paint.transform.position;
            Vector3 into = IntoRoomFrom(paint.transform);
            Vector3 right = Vector3.Cross(Vector3.up, into);
            if (right.sqrMagnitude < 0.01f)
                right = paint.transform.right;
            right.y = 0f;
            right.Normalize();

            PlaceMetal("Metal_A", p + into * 0.42f + right * 0.80f + Vector3.up * -0.15f, new Vector3(1.6f, 2.35f, 0.12f));
            PlaceMetal("Metal_B", p + into * 0.42f + right * -0.40f + Vector3.up * -0.15f, new Vector3(1.4f, 2.35f, 0.12f));
            PlaceMetal("Metal_C", p + into * 0.66f + Vector3.up * -0.65f, new Vector3(1.5f, 1.50f, 0.12f));
        }

        private void PlaceMagnetByMetals()
        {
            var mag = FindLocal("Interactable_Magnet");
            if (mag == null)
                return;

            Vector3 avg = Vector3.zero;
            int n = 0;
            var a = FindLocal("Metal_A");
            var b = FindLocal("Metal_B");
            var c = FindLocal("Metal_C");
            if (a != null) { avg += a.transform.position; n++; }
            if (b != null) { avg += b.transform.position; n++; }
            if (c != null) { avg += c.transform.position; n++; }
            if (n == 0)
                return;
            avg /= n;

            var paint = FindLocal("Interactable_Painting");
            Vector3 into = paint != null ? IntoRoomFrom(paint.transform) : Vector3.back;
            Vector3 fromPaint = avg;
            if (paint != null)
            {
                fromPaint = avg - paint.transform.position;
                fromPaint.y = 0f;
                if (fromPaint.sqrMagnitude > 0.01f)
                    into = fromPaint.normalized;
            }

            Vector3 pos = avg + into * 1.55f;
            pos.y = 0.82f;
            mag.transform.position = pos;
            mag.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
            EnsureWalkCollider(mag);
        }

        private void PlacePrints(string sceneName)
        {
            if (sceneName != "Level03_Archives")
                return;
            PlacePrint("Print_10", new Vector3(3.05f, 0.045f, -0.35f), 12f);
            PlacePrint("Print_109", new Vector3(3.55f, 0.05f, -0.18f), -8f);
            PlacePrint("Print_3", new Vector3(4.05f, 0.045f, -0.42f), 20f);
        }

        private void PlacePrint(string name, Vector3 pos, float yaw)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = new Vector3(0.42f, 0.025f, 0.55f);
            Paint(go, _paper);
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.15f, 4f, 1.15f);
            box.center = Vector3.zero;
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0)
                go.layer = layer;
            go.SetActive(false);
        }

        private void WireMagnet(string sceneName)
        {
            if (sceneName != "Level03_Mezzanine")
                return;
            var mag = FindLocal("Interactable_Magnet");
            if (mag == null)
                return;
            var control = mag.GetComponent<MagnetControlInteractable>();
            if (control == null)
                return;

            var metals = new Transform[3];
            var a = FindLocal("Metal_A");
            var b = FindLocal("Metal_B");
            var c = FindLocal("Metal_C");
            if (a != null) metals[0] = a.transform;
            if (b != null) metals[1] = b.transform;
            if (c != null) metals[2] = c.transform;

            var paint = FindLocal("Interactable_Painting");
            Vector3 origin = paint != null ? paint.transform.position : mag.transform.position;

            var bits = new System.Collections.Generic.List<Transform>();
            var kids = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < kids.Length; i++)
            {
                if (kids[i].name != "Kit_metal-panel-narrow")
                    continue;
                var p = kids[i].parent;
                if (p != null && (p.name == "Metal_A" || p.name == "Metal_B" || p.name == "Metal_C"))
                    continue;
                Vector3 d = kids[i].position - origin;
                d.y = 0f;
                if (kids[i].position.y < origin.y && d.sqrMagnitude < 16f)
                    bits.Add(kids[i]);
            }

            control.BakerSetup(paint, metals, bits.ToArray());
        }

        private void ExitPad(Vector3 pos, Vector3 scale)
        {
            if (_root == null)
                _root = transform;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Kit_ExitPad";
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            DressExitPad(go);
        }

        private void ExitPadAtDoor(string doorName, float outDist)
        {
            var door = FindLocal(doorName);
            if (door == null)
                return;
            Vector3 p = door.transform.position;
            Vector3 fwd = door.transform.forward;
            if (Mathf.Abs(fwd.y) > 0.65f)
                fwd = Vector3.forward;
            p = p + fwd * outDist;
            p.y = 1.1f;
            ExitPad(p, new Vector3(2.4f, 2.2f, 1.15f));
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

        private void PaintProps()
        {
            var neon = FuseNeon();
            PaintPrefix("FuseDecor", neon);
            PaintPrefix("Drawer", _wood);
            PaintGoNamed("InsertedFuseVisual", neon);
            PaintGoNamed("Interactable_Fuse", neon);
            PaintGoNamed("Interactable_Drawer", _wood);
        }

        private void PaintOrigenDoor()
        {
            var door = FindLocal("Interactable_OrigenDoor");
            if (door == null)
                return;
            var mat = ShutterMat();
            Paint(door, mat);
            var pivot = door.transform.parent;
            if (pivot != null)
                Paint(pivot.gameObject, mat);
        }

        private void PlaceOrigenStencil()
        {
            var sign = FindLocal("Sign_ORIGEN");
            if (sign == null)
                return;

            // Fuera del vano: sobre el dintel, no en el hueco de la puerta.
            sign.transform.position = new Vector3(0f, 3.22f, 5.72f);
            sign.transform.localScale = new Vector3(2.2f, 0.38f, 0.06f);
            Paint(sign, ShutterMat());

            var existingStencil = FindLocal("Kit_OrigenStencil");
            if (existingStencil != null)
            {
                var s = existingStencil.transform.localScale;
                existingStencil.transform.localScale = new Vector3(-Mathf.Abs(s.x < 0.01f ? 1f : s.x), s.y, s.z);
                existingStencil.transform.rotation = Quaternion.LookRotation(Vector3.back);
                return;
            }

            var font = Resources.Load<Font>("Fonts/StardosStencil-Bold");
            var go = new GameObject("Kit_OrigenStencil");
            go.transform.SetParent(transform, false);
            Vector3 p = sign.transform.position;
            go.transform.position = new Vector3(p.x, p.y, p.z - 0.05f);
            // TextMesh mira +Z local; el jugador mira la pared norte (+Z),
            // así que el texto tiene que apuntar al cuarto (−Z).
            go.transform.rotation = Quaternion.LookRotation(Vector3.back);
            go.transform.localScale = new Vector3(-1f, 1f, 1f);

            var tm = go.AddComponent<TextMesh>();
            tm.text = "ORIGEN";
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.12f;
            tm.fontSize = 64;
            tm.color = new Color(0.08f, 0.07f, 0.05f, 1f);
            if (font != null)
            {
                tm.font = font;
                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.sharedMaterial = font.material;
            }
        }

        private Material ShutterMat()
        {
            if (_shutter != null)
                return _shutter;
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            _shutter = lit != null ? new Material(lit) : new Material(Shader.Find("Sprites/Default"));
            _shutter.name = "Mat_WornShutter";
            var diff = Resources.Load<Texture2D>("Mats/worn_shutter_diff_2k");
            var nrm = Resources.Load<Texture2D>("Mats/worn_shutter_nor_gl_2k");
            if (diff != null)
            {
                _shutter.SetTexture("_BaseMap", diff);
                _shutter.SetTexture("_MainTex", diff);
            }
            if (nrm != null)
            {
                _shutter.SetTexture("_BumpMap", nrm);
                _shutter.EnableKeyword("_NORMALMAP");
            }
            _shutter.SetColor("_BaseColor", Color.white);
            if (_shutter.HasProperty("_Smoothness"))
                _shutter.SetFloat("_Smoothness", 0.22f);
            if (_shutter.HasProperty("_Metallic"))
                _shutter.SetFloat("_Metallic", 0.35f);
            return _shutter;
        }

        private void DressStretcher()
        {
            var src = FindLocal("Decor_Stretcher");
            if (src == null)
                return;
            Vector3 floor = FloorPos(src);
            float yaw = src.transform.eulerAngles.y;
            var bed = FindLocal("Kit_HospitalBed");
            if (bed == null)
            {
                var prefab = Resources.Load<GameObject>("Kitbash/HospitalBed");
                if (prefab == null)
                    return;
                bed = Object.Instantiate(prefab, _root);
                bed.name = "Kit_HospitalBed";
                bed.transform.position = floor;
                bed.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * prefab.transform.rotation;
                StripColliders(bed);
                Paint(bed, _rust);
            }
            else
            {
                // Por si un bake viejo la dejó de canto.
                bed.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up);
            }
            FitLongOnFloor(bed, floor.y, 2.1f, 0.72f);
            HideVisual(src);
            FitColliderTo(src, bed);
            SolidShell(bed);
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

        private void DressOfficeChairs()
        {
            for (int i = 0; i < 4; i++)
            {
                var old = FindLocal("Kit_Chair_2");
                if (old == null)
                    break;
                DropGo(old);
            }
            SwapChair("Decor_Chair");
            SwapChair("Chair");
            SwapChair("Chair_Origen");
        }

        private void SwapChair(string goName)
        {
            var src = FindLocal(goName);
            if (src == null)
                return;
            string kitName = "Kit_OfficeChair_" + goName;
            if (PutNamed("officeChair", kitName, FloorPos(src), src.transform.eulerAngles.y, PlasticMat(), 0.95f))
                HideVisual(src);
        }

        private void PaintComputers()
        {
            var off = ScreenOffMat();
            var plastic = PlasticMat();
            PaintGoNamed("Kit_computerScreen", off);
            PaintGoNamed("computerScreen", off);
            PaintGoNamed("Kit_computerKeyboard", plastic);
            PaintGoNamed("computerKeyboard", plastic);
            PaintGoNamed("Kit_computerMouse", plastic);
            PaintGoNamed("computerMouse", plastic);
        }

        private void PlaceLeaks(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                PlaceDecal("Kit_Leak_N1a", "Docs/Leaking001_Color", "Docs/Leaking001_Opacity", new Vector3(-9.88f, 2.1f, -2.4f), new Vector3(0f, 90f, 0f), new Vector3(1.8f, 2.4f, 1f));
                PlaceDecal("Kit_Rust_N1a", "Docs/Rust001_Color", "Docs/Rust001_Opacity", new Vector3(9.88f, 1.4f, 3.2f), new Vector3(0f, -90f, 0f), new Vector3(1.5f, 1.8f, 1f));
            }
            else if (sceneName == "Level02_Lab")
            {
                PlaceDecal("Kit_Leak_N2a", "Docs/Leaking003_Color", "Docs/Leaking003_Opacity", new Vector3(7.58f, 2.2f, 1.1f), new Vector3(0f, -90f, 0f), new Vector3(1.6f, 2.2f, 1f));
                PlaceDecal("Kit_Rust_N2a", "Docs/Rust001_Color", "Docs/Rust001_Opacity", new Vector3(-7.58f, 1.5f, -2.4f), new Vector3(0f, 90f, 0f), new Vector3(1.4f, 1.7f, 1f));
            }
            else if (sceneName == "Level03_Archives")
            {
                PlaceDecal("Kit_Leak_N3a", "Docs/Leaking005_Color", "Docs/Leaking005_Opacity", new Vector3(-7.68f, 2.3f, 0.4f), new Vector3(0f, 90f, 0f), new Vector3(1.7f, 2.3f, 1f));
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                PlaceDecal("Kit_Rust_Mezz", "Docs/Rust001_Color", "Docs/Rust001_Opacity", new Vector3(7.58f, 1.3f, 1.8f), new Vector3(0f, -90f, 0f), new Vector3(1.3f, 1.6f, 1f));
            }
            else if (sceneName == "Level04_Origen")
            {
                PlaceDecal("Kit_Leak_N4a", "Docs/Leaking001_Color", "Docs/Leaking001_Opacity", new Vector3(-2.55f, 2.15f, 5.88f), new Vector3(0f, 180f, 0f), new Vector3(1.5f, 2.0f, 1f));
                PlaceDecal("Kit_Rust_N4a", "Docs/Rust001_Color", "Docs/Rust001_Opacity", new Vector3(7.48f, 1.2f, -1.4f), new Vector3(0f, -90f, 0f), new Vector3(1.4f, 1.8f, 1f));
            }
        }

        private void PlaceDecal(string name, string colorPath, string opacityPath, Vector3 pos, Vector3 euler, Vector3 scale)
        {
            if (FindLocal(name) != null)
                return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            StripColliders(go);
            var rend = go.GetComponent<MeshRenderer>();
            if (rend == null)
                return;
            rend.sharedMaterial = LeakMat(colorPath, opacityPath);
            rend.shadowCastingMode = ShadowCastingMode.Off;
        }

        private void PlaceBrokenCams(string sceneName)
        {
            if (sceneName == "Level02_Lab")
                HangBrokenCam("Kit_CamBroken_N2", new Vector3(-6.85f, 6.85f, -4.1f), new Vector3(28f, 40f, 12f));
            else if (sceneName == "Level03_Archives")
                HangBrokenCam("Kit_CamBroken_N3", new Vector3(6.7f, 6.8f, 3.15f), new Vector3(32f, -50f, 18f));
            else if (sceneName == "Level03_Mezzanine")
                HangBrokenCam("Kit_CamBroken_Mezz", new Vector3(5.4f, 6.85f, -2.2f), new Vector3(22f, 200f, -15f));
            else if (sceneName == "Level04_Origen")
                HangBrokenCam("Kit_CamBroken_N4", new Vector3(-6.4f, 6.75f, 3.8f), new Vector3(35f, 110f, 20f));
        }

        private void HangBrokenCam(string name, Vector3 pos, Vector3 euler)
        {
            if (FindLocal(name) != null)
                return;
            if (!PutNamed("scanner-high", name, pos, euler.y, _rust, 0.22f))
                return;
            var go = FindLocal(name);
            if (go != null)
                go.transform.rotation = Quaternion.Euler(euler);

            string cable = name + "_Cable";
            if (FindLocal(cable) != null)
                return;
            var rod = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rod.name = cable;
            rod.transform.SetParent(_root, false);
            rod.transform.position = new Vector3(pos.x, pos.y + 0.45f, pos.z);
            rod.transform.localScale = new Vector3(0.03f, 0.9f, 0.03f);
            StripColliders(rod);
            Paint(rod, _rust);
        }

        private void ConnectPipes(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                PutNamed("pipe-large-long", "Kit_PipeRun_N1e", new Vector3(9.55f, 7.1f, 0f), 90f, _rust, 0.32f);
                PutNamed("pipe-large-bend", "Kit_PipeBend_N1e", new Vector3(9.4f, 7.1f, -7.55f), -90f, _rust, 0.32f);
                PutNamed("pipe-end", "Kit_PipeEnd_N1e", new Vector3(9.55f, 7.05f, 7.1f), -90f, _rust, 0.28f);
            }
            else if (sceneName == "Level02_Lab")
            {
                PutNamed("pipe-large-bend", "Kit_PipeBend_N2a", new Vector3(7.45f, 2.15f, 2.15f), 0f, _rust, 0.35f);
                PutNamed("pipe-large-bend", "Kit_PipeBend_N2b", new Vector3(-7.55f, 2.15f, 3.4f), 180f, _rust, 0.35f);
            }
            else if (sceneName == "Level03_Archives")
            {
                PutNamed("pipe-large-bend", "Kit_PipeBend_N3a", new Vector3(-7.7f, 7.1f, 4.2f), 0f, _rust, 0.32f);
                PutNamed("pipe-large-bend", "Kit_PipeBend_N3b", new Vector3(7.7f, 7.1f, 4.0f), 180f, _rust, 0.32f);
            }
            else if (sceneName == "Level04_Origen")
            {
                PutNamed("pipe-large-long", "Kit_PipeRun_N4c", new Vector3(0f, 7.1f, -2.2f), 0f, _rust, 0.32f);
                PutNamed("pipe-large-bend", "Kit_PipeBend_N4c", new Vector3(7.2f, 7.1f, -2.2f), 90f, _rust, 0.32f);
            }
        }

        private Material PlasticMat()
        {
            if (_plastic != null)
                return _plastic;
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            _plastic = lit != null ? new Material(lit) : new Material(Shader.Find("Sprites/Default"));
            _plastic.name = "Mat_Plastic001";
            var tex = Resources.Load<Texture2D>("Mats/Plastic001_Color");
            if (tex != null)
            {
                _plastic.SetTexture("_BaseMap", tex);
                _plastic.SetTexture("_MainTex", tex);
            }
            _plastic.SetColor("_BaseColor", Color.white);
            if (_plastic.HasProperty("_Smoothness"))
                _plastic.SetFloat("_Smoothness", 0.45f);
            if (_plastic.HasProperty("_Metallic"))
                _plastic.SetFloat("_Metallic", 0.05f);
            return _plastic;
        }

        private Material ScreenOffMat()
        {
            if (_screenOff != null)
                return _screenOff;
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            _screenOff = lit != null ? new Material(lit) : new Material(Shader.Find("Sprites/Default"));
            _screenOff.name = "Mat_ScreenOff";
            var tex = Resources.Load<Texture2D>("Mats/Screen_Off_001");
            if (tex != null)
            {
                _screenOff.SetTexture("_BaseMap", tex);
                _screenOff.SetTexture("_MainTex", tex);
            }
            _screenOff.SetColor("_BaseColor", new Color(0.04f, 0.045f, 0.05f, 1f));
            if (_screenOff.HasProperty("_Smoothness"))
                _screenOff.SetFloat("_Smoothness", 0.72f);
            return _screenOff;
        }

        private Material LeakMat(string colorPath, string opacityPath)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var mat = lit != null ? new Material(lit) : new Material(Shader.Find("Sprites/Default"));
            mat.name = "Mat_LeakDecal";
            var color = Resources.Load<Texture2D>(colorPath);
            var opacity = Resources.Load<Texture2D>(opacityPath);
            if (color != null)
            {
                mat.SetTexture("_BaseMap", color);
                mat.SetTexture("_MainTex", color);
            }
            else if (opacity != null)
            {
                mat.SetTexture("_BaseMap", opacity);
                mat.SetTexture("_MainTex", opacity);
            }
            mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Cutoff"))
                mat.SetFloat("_Cutoff", 0.18f);
            mat.EnableKeyword("_ALPHATEST_ON");
            if (mat.HasProperty("_AlphaClip"))
                mat.SetFloat("_AlphaClip", 1f);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.12f);
            mat.renderQueue = 2450;
            return mat;
        }

        private Material FuseNeon()
        {
            if (_fuseNeon != null)
                return _fuseNeon;
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            _fuseNeon = lit != null ? new Material(lit) : new Material(Shader.Find("Sprites/Default"));
            _fuseNeon.name = "Mat_FuseNeon";
            var green = new Color(0.18f, 1f, 0.22f, 1f);
            _fuseNeon.SetColor("_BaseColor", green);
            _fuseNeon.SetColor("_Color", green);
            _fuseNeon.EnableKeyword("_EMISSION");
            _fuseNeon.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            _fuseNeon.SetColor("_EmissionColor", new Color(0.45f, 4.2f, 0.55f));
            if (_fuseNeon.HasProperty("_Smoothness"))
                _fuseNeon.SetFloat("_Smoothness", 0.62f);
            return _fuseNeon;
        }

        private const float RoofYGround = 7.8f;
        private const float RoofYMezz = 7.8f;
        private const float RoofThick = 0.5f;

        private static float RoofYOf(string sceneName)
        {
            return sceneName == "Level03_Mezzanine" ? RoofYMezz : RoofYGround;
        }

        private void PlaceRoofs(string sceneName)
        {
            float y = RoofYOf(sceneName);
            float minKeep = y - 1f;
            DropIfLow("Kit_Ceiling", minKeep);
            DropIfLow("Kit_Ceiling_N", minKeep);
            DropIfLow("Kit_Ceiling_S", minKeep);
            DropIfLow("Kit_Ceiling_W", minKeep);
            DropIfLow("Kit_Ceiling_E", minKeep);

            if (sceneName == "Level03_Mezzanine")
            {
                PlaceMezzRoof();
            }
            else
            {
                var scale = new Vector3(20f, RoofThick, 22f);
                if (sceneName == "Level02_Lab")
                    scale = new Vector3(17f, RoofThick, 14f);
                else if (sceneName == "Level03_Archives")
                    scale = new Vector3(17f, RoofThick, 13f);
                else if (sceneName == "Level04_Origen")
                    scale = new Vector3(17f, RoofThick, 14f);
                if (FindLocal("Kit_Ceiling") == null)
                    Slab("Kit_Ceiling", new Vector3(0f, y, 0f), scale, _plaster);
            }

            RaiseWalls(y - RoofThick * 0.5f);
            StretchN4Lintel(y - RoofThick * 0.5f);
            LiftCeilingDressing(y);
        }

        private void PlaceMezzRoof()
        {
            float y = RoofYMezz;
            float t = RoofThick;
            PlaceOrMoveSlab("Kit_Ceiling_N", new Vector3(0f, y, 5.75f), new Vector3(16.5f, t, 5.8f), _plaster);
            PlaceOrMoveSlab("Kit_Ceiling_S", new Vector3(0f, y, -2.15f), new Vector3(16.5f, t, 8.2f), _plaster);
            PlaceOrMoveSlab("Kit_Ceiling_W", new Vector3(-4.85f, y, 2.4f), new Vector3(8.8f, t, 1.0f), _plaster);
            PlaceOrMoveSlab("Kit_Ceiling_E", new Vector3(4.85f, y, 2.4f), new Vector3(8.8f, t, 1.0f), _plaster);
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

        private void DropIfLow(string name, float minY)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            if (go.transform.position.y >= minY)
                return;
            go.name = name + "_Old";
            var col = go.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
            go.SetActive(false);
            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }

        private void RaiseWalls(float topY)
        {
            var scene = gameObject.scene;
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.gameObject.scene != scene)
                    continue;
                if (!t.name.StartsWith("Wall_"))
                    continue;
                if (t.name.IndexOf("Cap") >= 0)
                    continue;
                var p = t.position;
                var s = t.localScale;
                float minY = p.y - s.y * 0.5f;
                float maxY = p.y + s.y * 0.5f;
                if (maxY >= topY - 0.05f)
                    continue;
                float newH = topY - minY;
                if (newH < 0.2f)
                    continue;
                t.position = new Vector3(p.x, minY + newH * 0.5f, p.z);
                t.localScale = new Vector3(s.x, newH, s.z);
            }
        }

        private void StretchN4Lintel(float topY)
        {
            var lintel = FindLocal("Kit_DoorLintel");
            if (lintel == null)
                return;
            float bot = 2.55f;
            float h = topY - bot;
            if (h < 0.4f)
                return;
            var p = lintel.transform.position;
            var s = lintel.transform.localScale;
            lintel.transform.position = new Vector3(p.x, bot + h * 0.5f, p.z);
            lintel.transform.localScale = new Vector3(s.x, h, s.z);
        }

        private void LiftCeilingDressing(float roofY)
        {
            float target = roofY - 0.45f;
            var scene = gameObject.scene;
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.gameObject.scene != scene)
                    continue;
                string n = t.name;
                bool kit = n.StartsWith("Kit_Light_CeilingSingle") || n.StartsWith("Kit_RoofTile_Vents");
                bool light = n == "Light_Fill" || n == "Light_EmergA" || n == "Light_EmergB" || n == "Light_Gallery";
                bool pipeHigh = (n.StartsWith("Kit_pipe-large") || n.StartsWith("Kit_pipe-end")) && t.position.y >= 2.35f;
                if (!kit && !light && !pipeHigh)
                    continue;
                if (t.position.y >= target - 0.05f)
                    continue;
                if (t.position.y < 2.35f)
                    continue;
                t.position = new Vector3(t.position.x, target, t.position.z);
            }
        }

        private void DressRuntime(string sceneName)
        {
            FixOverrideKey();
            OverlayChild("Interactable_Credential", "Pickup_KeyCard", null, false);
            OverlayDoor("Interactable_Door");
            OverlayDoor("Interactable_DoorN3");
            OverlayDoor("Interactable_OrigenDoor");
            PaintOrigenDoor();
            PlaceOrigenStencil();
            OverlayPapers();
            OverlayCrate("Crate_R0", "Kit_CrateBox_0");
            OverlayCrate("Crate_R1", "Kit_CrateBox_1");
            OverlayCrate("Crate_R2", "Kit_CrateBox_2");
            OverlayCrate("Crate_R3", "Kit_CrateBox_3");
            HangCables(sceneName);
            ScatterBoxes(sceneName);
            StampNewspapers(sceneName);
            PaintWright();
            DressStretcher();
            DressOfficeChairs();
            PaintComputers();
            PlaceLeaks(sceneName);
            PlaceBrokenCams(sceneName);
            ConnectPipes(sceneName);
            UprightPass();
            PaintProps();
            FixOverrideKey();
            PropSolidPass();
        }

        private void FixOverrideKey()
        {
            var key = FindLocal("Interactable_OverrideKey");
            if (key == null)
                return;
            key.SetActive(true);

            var books = FindLocal("Kit_books");
            if (books == null)
                books = FindLocal("Kit_books_N4");
            var locker = FindLocal("Interactable_Locker");

            if (books != null)
            {
                var b = Encapsulate(books);
                Vector3 pos = new Vector3(b.center.x, b.max.y + 0.05f, b.center.z);
                if (locker != null)
                {
                    Vector3 toward = locker.transform.position - pos;
                    toward.y = 0f;
                    if (toward.sqrMagnitude > 0.01f)
                        pos += toward.normalized * 0.18f;
                }
                key.transform.position = pos;
            }
            else if (locker != null)
            {
                Vector3 p = locker.transform.position;
                Vector3 inward = new Vector3(-p.x, 0f, -p.z);
                if (inward.sqrMagnitude < 0.01f)
                    inward = Vector3.back;
                inward.Normalize();
                key.transform.position = new Vector3(p.x, 0.08f, p.z) + inward * 0.55f;
            }

            key.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            // El modelo de la llave lo pone PickupInteractable (ItemVisuals) a escala real
        }

        private void PropSolidPass()
        {
            EnsureWalkCollider(FindLocal("Interactable_Magnet"));
            EnsureWalkCollider(FindLocal("Metal_A"));
            EnsureWalkCollider(FindLocal("Metal_B"));
            EnsureWalkCollider(FindLocal("Metal_C"));
            EnsureWalkCollider(FindLocal("Interactable_Locker"));
            EnsureWalkCollider(FindLocal("Lockers"));
            EnsureWalkCollider(FindLocal("Interactable_Cassette"));
            EnsureWalkCollider(FindLocal("Chair_Origen"));
            SolidShell(FindLocal("Interactable_Locker"));
            SolidShell(FindLocal("Lockers"));
            SolidShell(FindLocal("Kit_HospitalBed"));
            SolidShell(FindLocal("Kit_Box_N1a"));
            SolidShell(FindLocal("Kit_Box_N1b"));
            SolidShell(FindLocal("Kit_Box_N2a"));
            SolidShell(FindLocal("Kit_Box_N3a"));
            SolidShell(FindLocal("Kit_Box_Mezz"));
            SolidShell(FindLocal("Kit_Box_N4a"));
            SolidShell(FindLocal("Kit_CrateBox_0"));
            SolidShell(FindLocal("Kit_CrateBox_1"));
            SolidShell(FindLocal("Kit_CrateBox_2"));
            SolidShell(FindLocal("Kit_CrateBox_3"));
            SolidShell(FindLocal("Kit_OfficeChair_Decor_Chair"));
            SolidShell(FindLocal("Kit_OfficeChair_Chair"));
            SolidShell(FindLocal("Kit_OfficeChair_Chair_Origen"));
            SolidShell(FindLocal("Kit_Trashcan_Small2"));
            SolidShell(FindLocal("Kit_NightStand_2"));
            SolidShell(FindLocal("Kit_GasCylinder"));
            SolidShell(FindLocal("Kit_CableDrum"));
            SolidShell(FindLocal("Kit_container-tall"));
            SolidShell(FindLocal("Kit_ElectricalBox02"));
            SolidShell(FindLocal("Kit_books"));
            SolidShell(FindLocal("Kit_books_N4"));
            SolidShell(FindLocal("Kit_rail"));
            var src = FindLocal("Decor_Stretcher");
            var bed = FindLocal("Kit_HospitalBed");
            if (src != null && bed != null)
                FitColliderTo(src, bed);
            WalkNamedProps();
        }

        private static bool SkipWalkSolid(string n)
        {
            if (string.IsNullOrEmpty(n))
                return true;
            if (n.IndexOf("Door") >= 0)
                return true;
            if (n.IndexOf("Hatch") >= 0)
                return true;
            if (n.IndexOf("Exit") >= 0)
                return true;
            if (n.StartsWith("Doc_"))
                return true;
            if (n.StartsWith("Print_"))
                return true;
            return false;
        }

        private void WalkNamedProps()
        {
            var scene = gameObject.scene;
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.gameObject.scene != scene)
                    continue;
                string n = t.name;
                if (SkipWalkSolid(n))
                    continue;
                bool kit = n.StartsWith("Kit_") && ShouldBlock(n);
                bool furniture = n == "Lockers" || n == "CabinetRow_L" || n == "CabinetRow_R"
                    || n == "ArchiveDesk" || n == "Console" || n == "Generator"
                    || n == "Decor_Desk" || n == "Decor_Shelf" || n == "Decor_FallenFurniture"
                    || n == "Decor_Stretcher" || n == "Chair_Origen" || n == "Decor_Chair" || n == "Chair";
                bool interact = n == "Interactable_Locker" || n == "Interactable_Cassette"
                    || n == "Interactable_Magnet"
                    || n == "Interactable_ElectricalBox" || n == "Interactable_Wires"
                    || n.StartsWith("Metal_");
                if (!kit && !furniture && !interact)
                    continue;
                // Los modelos Kit_ vienen con escalas de importación enormes: un box unitario los vuelve gigantes. Solo usan el _Solid
                if (!kit)
                    EnsureWalkCollider(t.gameObject);
                if (kit || furniture || n == "Interactable_Locker")
                    SolidShell(t.gameObject);
            }
        }

        private void UprightPass()
        {
            UprightOne("Kit_cardboardBoxClosed", 0.45f);
            UprightOne("Kit_cardboardBoxOpen", 0.45f);
            UprightOne("Kit_ElectricalBox02", 0.85f);
            UprightOne("Kit_container-tall", 1.35f);
            UprightOne("Kit_CableDrum", 0.7f);
            UprightOne("Kit_GasCylinder", 0.95f);
            UprightOne("Kit_NightStand_2", 0.7f);
            UprightOne("Kit_Chair_2", 0.95f);
            UprightOne("Kit_Chair_3", 0.95f);
            UprightOne("Kit_OfficeChair_Decor_Chair", 0.95f);
            UprightOne("Kit_OfficeChair_Chair", 0.95f);
            UprightOne("Kit_OfficeChair_Chair_Origen", 0.95f);
            UprightOne("Kit_Trashcan_Small2", 0.48f);
            UprightOne("Kit_Box_N1a", 0.48f);
            UprightOne("Kit_Box_N1b", 0.38f);
            UprightOne("Kit_Box_N2a", 0.45f);
            UprightOne("Kit_Box_N3a", 0.42f);
            UprightOne("Kit_Box_Mezz", 0.4f);
            UprightOne("Kit_Box_N4a", 0.42f);
            UprightOne("Kit_CrateBox_0", 0.5f);
            UprightOne("Kit_CrateBox_1", 0.5f);
            UprightOne("Kit_CrateBox_2", 0.5f);
            UprightOne("Kit_CrateBox_3", 0.5f);
        }

        private void OverlayCrate(string crateName, string kitName)
        {
            var src = FindLocal(crateName);
            if (src == null)
                return;
            if (FindLocal(kitName) != null)
            {
                HideVisual(src);
                return;
            }
            PutNamed("cardboardBoxClosed", kitName, FloorPos(src), src.transform.eulerAngles.y, _wood, 0.5f);
            HideVisual(src);
        }

        private void UprightOne(string name, float h)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            float floor = go.transform.position.y;
            var b = Encapsulate(go);
            floor = b.min.y;
            StandUpright(go, floor, h);
        }

        private void HangCables(string sceneName)
        {
            float y = RoofYOf(sceneName) - 0.9f;
            if (FindLocal("Kit_Cable_Long") == null)
            {
                Vector3 p = new Vector3(-4.2f, y, -2.4f);
                if (sceneName == "Level02_Lab")
                    p = new Vector3(-5.4f, y, 1.2f);
                else if (sceneName == "Level03_Archives")
                    p = new Vector3(4.1f, y, -1.8f);
                else if (sceneName == "Level03_Mezzanine")
                    p = new Vector3(-2.2f, y, 0.6f);
                else if (sceneName == "Level04_Origen")
                    p = new Vector3(3.4f, y, 2.2f);
                Put("Cable_Long", p, 12f, _rust);
            }

            if (FindLocal("Kit_Cable_Small") == null)
            {
                Vector3 p = new Vector3(3.6f, y, 3.1f);
                if (sceneName == "Level03_Archives")
                    p = new Vector3(-3.8f, y, 2.1f);
                Put("Cable_Small", p, -18f, _rust);
            }
        }

        private void ScatterBoxes(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                PutNamed("cardboardBoxClosed", "Kit_Box_N1a", new Vector3(8.15f, 0.02f, -6.05f), 15f, _wood, 0.48f);
                PutNamed("cardboardBoxClosed", "Kit_Box_N1b", new Vector3(8.45f, 0.02f, -5.55f), -22f, _wood, 0.38f);
                PutNamed("radio", "Kit_radio_N1", new Vector3(-8.55f, 0.72f, -6.2f), 25f, _wood, 0.16f);
                PutNamed("books", "Kit_books_N1", new Vector3(5.2f, 0.82f, 2.55f), 12f, _wood, 0.14f);
                PutNamed("sideTable", "Kit_sideTable_N1", new Vector3(-7.35f, 0f, 7.15f), -18f, _wood, 0.75f);
            }
            else if (sceneName == "Level02_Lab")
            {
                PutNamed("cardboardBoxClosed", "Kit_Box_N2a", new Vector3(-6.85f, 0.02f, 5.05f), 18f, _wood, 0.45f);
                PutNamed("radio", "Kit_radio_N2", new Vector3(-6.35f, 0.92f, -4.05f), 8f, _wood, 0.14f);
                PutNamed("books", "Kit_books_N2", new Vector3(6.15f, 0.92f, -3.15f), -12f, _wood, 0.16f);
            }
            else if (sceneName == "Level03_Archives")
            {
                PutNamed("cardboardBoxClosed", "Kit_Box_N3a", new Vector3(-6.55f, 0.02f, 4.85f), 12f, _wood, 0.42f);
                PutNamed("radio", "Kit_radio_N3", new Vector3(3.15f, 0.62f, -1.55f), -8f, _wood, 0.14f);
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                PutNamed("cardboardBoxClosed", "Kit_Box_Mezz", new Vector3(6.55f, 0.02f, -2.85f), 8f, _wood, 0.4f);
                PutNamed("books", "Kit_books_Mezz", new Vector3(-6.4f, 0.02f, -3.15f), 22f, _wood, 0.12f);
            }
            else if (sceneName == "Level04_Origen")
            {
                PutNamed("cardboardBoxClosed", "Kit_Box_N4a", new Vector3(-6.05f, 0.02f, -3.55f), 14f, _wood, 0.42f);
                PutNamed("books", "Kit_books_N4", new Vector3(3.4f, 0.72f, -3.45f), -15f, _wood, 0.14f);
            }
        }

        private void StampNewspapers(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                StampNews("PaperNews_6", "Docs/paper_6", new Vector3(4.15f, 0.03f, -4.55f));
            }
            else if (sceneName == "Level02_Lab")
            {
                StampNews("PaperNews_7", "Docs/paper_7", new Vector3(1.15f, 0.03f, -3.85f));
            }
            else if (sceneName == "Level03_Archives")
            {
                StampNews("PaperNews_6", "Docs/paper_6", new Vector3(3.55f, 0.03f, 1.55f));
                StampNews("PaperNews_7", "Docs/paper_7", new Vector3(3.95f, 0.03f, 1.15f));
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                StampNews("PaperNews_6", "Docs/paper_6", new Vector3(-2.85f, 0.03f, 2.05f));
            }
            else if (sceneName == "Level04_Origen")
            {
                StampNews("PaperNews_7", "Docs/paper_7", new Vector3(3.55f, 0.68f, -3.35f));
            }
        }

        private void StampNews(string name, string texPath, Vector3 pos)
        {
            if (FindLocal(name) != null)
                return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            go.transform.localScale = new Vector3(0.46f, 0.01f, 0.62f);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(col);
                else
                    Object.DestroyImmediate(col);
            }
            var rend = go.GetComponent<MeshRenderer>();
            if (rend == null)
                return;
            var tex = Resources.Load<Texture2D>(texPath);
            if (_paper != null)
            {
                var mat = new Material(_paper);
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTexture("_MainTex", tex);
                }
                rend.sharedMaterial = mat;
            }
            rend.shadowCastingMode = ShadowCastingMode.Off;
        }

        private void PaintWright()
        {
            var paint = FindLocal("Interactable_Painting");
            if (paint == null)
                return;
            var rend = paint.GetComponent<MeshRenderer>();
            if (rend == null)
                rend = paint.GetComponentInChildren<MeshRenderer>();
            if (rend == null)
                return;
            var tex = Resources.Load<Texture2D>("Docs/wright");
            if (tex == null)
                return;
            var mat = new Material(_paper != null ? _paper : rend.sharedMaterial);
            mat.SetTexture("_BaseMap", tex);
            mat.SetTexture("_MainTex", tex);
            // Recorte del margen del canvas.
            mat.SetTextureScale("_BaseMap", new Vector2(0.92f, 0.94f));
            mat.SetTextureOffset("_BaseMap", new Vector2(0.04f, 0.03f));
            mat.SetTextureScale("_MainTex", new Vector2(0.92f, 0.94f));
            mat.SetTextureOffset("_MainTex", new Vector2(0.04f, 0.03f));
            rend.material = mat;
        }

        private void PlaceMezzHatch()
        {
            var down = FindLocal("Interactable_HatchDown");
            if (down != null)
                down.SetActive(false);

            float y = RoofYMezz - 0.22f;
            PlaceOrMoveRim("Kit_HatchRim_N", new Vector3(0f, y, 2.85f), new Vector3(0.98f, 0.1f, 0.1f));
            PlaceOrMoveRim("Kit_HatchRim_S", new Vector3(0f, y, 1.95f), new Vector3(0.98f, 0.1f, 0.1f));
            PlaceOrMoveRim("Kit_HatchRim_E", new Vector3(0.45f, y, 2.4f), new Vector3(0.1f, 0.1f, 0.9f));
            PlaceOrMoveRim("Kit_HatchRim_W", new Vector3(-0.45f, y, 2.4f), new Vector3(0.1f, 0.1f, 0.9f));

            PlaceOrMoveSlab("Kit_HatchLid", new Vector3(0f, y - 0.12f, 2.78f), new Vector3(0.9f, 0.05f, 0.72f), _rust);
            var lid = FindLocal("Kit_HatchLid");
            if (lid != null)
                lid.transform.rotation = Quaternion.Euler(70f, 0f, 0f);

            var ladder = FindLocal("Kit_oliveladderpoly");
            if (ladder == null)
                Put("oliveladderpoly", new Vector3(0.18f, 0f, 2.08f), 8f, _rust, y - 0.25f);
            else
            {
                ladder.transform.position = new Vector3(0.18f, ladder.transform.position.y, 2.08f);
                FitToHeight(ladder, 0f, y - 0.25f);
            }
        }

        private void PlaceOrMoveRim(string name, Vector3 pos, Vector3 scale)
        {
            var go = FindLocal(name);
            if (go == null)
            {
                Slab(name, pos, scale, _rust);
                return;
            }
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        private void CoverN4Pit()
        {
            var cap = FindLocal("Wall_N_Cap");
            if (cap == null)
                return;
            cap.SetActive(true);
            cap.transform.position = new Vector3(0f, -1.75f, 6.1f);
            cap.transform.localScale = new Vector3(2.2f, 3.5f, 0.4f);
            if (_plaster != null)
                Paint(cap, _plaster);

            if (FindLocal("Kit_DoorLintel") == null)
                Slab("Kit_DoorLintel", new Vector3(0f, 2.85f, 6.1f), new Vector3(2.2f, 0.5f, 0.4f), _plaster);
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

        private void WireExitPads()
        {
            var scene = gameObject.scene;
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
                WireExitPadsUnder(roots[r].transform);
        }

        private static void WireExitPadsUnder(Transform t)
        {
            if (t.name == "Kit_ExitPad")
                DressExitPad(t.gameObject);
            for (int i = 0; i < t.childCount; i++)
                WireExitPadsUnder(t.GetChild(i));
        }

        private static void DressExitPad(GameObject go)
        {
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null)
                rend.enabled = false;

            var col = go.GetComponent<BoxCollider>();
            if (col == null)
                col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;

            var p = go.transform.position;
            var s = go.transform.localScale;
            if (s.y < 1.2f)
            {
                go.transform.position = new Vector3(p.x, 1.1f, p.z);
                go.transform.localScale = new Vector3(Mathf.Max(2.2f, s.x), 2.2f, Mathf.Max(1.4f, s.z));
            }

            if (go.GetComponent<Rigidbody>() == null)
            {
                var rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            if (go.GetComponent<ExitSceneTrigger>() == null)
                go.AddComponent<ExitSceneTrigger>();
        }

        private void SnapDoorExit()
        {
            if (gameObject.scene.name == "Level03_Archives")
                return;

            var door = FindLocal("Interactable_DoorN3");
            if (door == null)
                door = FindLocal("Interactable_OrigenDoor");
            if (door == null)
                door = FindLocal("Interactable_Door");
            if (door == null)
                return;

            var pad = FindLocal("Kit_ExitPad");
            if (pad == null)
                return;

            Vector3 fwd = door.transform.forward;
            if (Mathf.Abs(fwd.y) > 0.65f)
                fwd = Vector3.forward;
            Vector3 p = door.transform.position + fwd * 0.9f;
            p.y = 1.1f;
            pad.transform.position = p;
            pad.transform.localScale = new Vector3(2.4f, 2.2f, 1.15f);
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
