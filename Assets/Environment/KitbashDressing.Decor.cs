using Enigma.Cinematics;
using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Enigma.Environment
{
    // Decorado: luces, techos, manchas, cables, cajas y materiales
    public partial class KitbashDressing
    {
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
            if (sceneName == "Level02_Lab" || sceneName == "Level03_Archives" || sceneName == "Level04_Origen")
                ArrivalDoor();
        }

        private static readonly Vector3 ArrivalLeaf = new Vector3(1.3f, 2.4f, 0.08f);
        private const float ArrivalJamb = 0.12f;
        private const float ArrivalDepth = 0.14f;

        // Puerta cerrada por la que se llega desde el nivel anterior, en la pared detrás de la aparición
        private void ArrivalDoor()
        {
            var wall = FindLocal("Wall_S");
            if (wall == null)
                return;
            float face = wall.transform.position.z + wall.transform.lossyScale.z * 0.5f;
            float x = wall.transform.position.x;
            Physics.SyncTransforms();
            float floor = 0f;
            if (Physics.Raycast(new Vector3(x, 3f, face + 0.6f), Vector3.down, out var hit, 6f, ~0, QueryTriggerInteraction.Ignore))
                floor = hit.point.y;

            if (!PutNamed("door_B", "Kit_ArrivalDoor", new Vector3(x, floor, face), 0f, null, ArrivalLeaf.y, false))
                return;
            var leaf = FindLocal("Kit_ArrivalDoor");
            FitToBox(leaf, floor, ArrivalLeaf.x, ArrivalLeaf.y, ArrivalLeaf.z);
            var b = Encapsulate(leaf);
            leaf.transform.position += new Vector3(x - b.center.x, 0f, face + ArrivalLeaf.z * 0.5f + 0.005f - b.center.z);

            var steel = Resources.Load<Material>("Mats/Mat_Steel");
            float jx = ArrivalLeaf.x * 0.5f + ArrivalJamb * 0.5f;
            float h = ArrivalLeaf.y + ArrivalJamb;
            float z = face + ArrivalDepth * 0.5f;
            Slab("Kit_ArrivalDoor_JambL", new Vector3(x - jx, floor + h * 0.5f, z), new Vector3(ArrivalJamb, h, ArrivalDepth), steel);
            Slab("Kit_ArrivalDoor_JambR", new Vector3(x + jx, floor + h * 0.5f, z), new Vector3(ArrivalJamb, h, ArrivalDepth), steel);
            Slab("Kit_ArrivalDoor_Lintel", new Vector3(x, floor + ArrivalLeaf.y + ArrivalJamb * 0.5f, z), new Vector3(ArrivalLeaf.x + ArrivalJamb * 2f, ArrivalJamb, ArrivalDepth), steel);
        }

        private void ExtraRoom()
        {
            PlaceLegDebris();
            // Mesa de luz al costado de la cabecera, del lado contrario al camino
            Put("NightStand_2", new Vector3(-4.05f, 0f, -0.95f), 35f, _wood, 0.7f);
            Put("Details_Cylinder", new Vector3(-8.85f, 0.22f, -6.55f), 40f, _rust);
            Put("Trashcan_Small2", new Vector3(8.7f, 0f, -6.5f), 18f, _rust);
            Put("container-flat-open", new Vector3(8.55f, 0.02f, 6.6f), -25f, _rust);
            Put("metal-panel", new Vector3(-8.7f, 0.02f, 7.4f), 12f, _rust);
            Put("metal-panel-narrow", new Vector3(-8.4f, 0.02f, 7.15f), -30f, _rust);

            Put("RoofTile_Vents", new Vector3(-3.4f, 7.35f, -3.6f), 0f, _rust);
            Put("RoofTile_Vents", new Vector3(4.2f, 7.35f, 4.8f), 90f, _rust);
            Put("Light_CeilingSingle", new Vector3(0f, 7.35f, 0f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(4.8f, 7.35f, 3.4f), 0f, _rust);

            // Quien se sienta mira a −X: monitor atrás, teclado adelante, mouse a su derecha
            // El pivote de estos modelos no está en su centro: las posiciones ya lo compensan
            Put("computerScreen", new Vector3(5.25f, 0.82f, 2.94f), 90f, _wood, 0.28f);
            Put("computerKeyboard", new Vector3(5.49f, 0.81f, 2.95f), 90f, _wood, 0.04f);
            Put("computerMouse", new Vector3(5.72f, 0.81f, 3.03f), 280f, _wood, 0.03f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N1a", new Vector3(8.15f, 0.02f, -6.05f), 15f, _wood, 0.48f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N1b", new Vector3(8.45f, 0.02f, -5.55f), -22f, _wood, 0.38f);

            ExitPad(new Vector3(0f, 1.1f, 10.55f), new Vector3(2.4f, 2.2f, 1.2f));
            FurnishRoom();
        }

        // Muebles contra las paredes, lejos de la puerta y de donde despierta Megan
        private void FurnishRoom()
        {
            PlaceProp("ContainerShelf", "Kit_Furniture_Shelf_N1", new Vector3(-6.2f, 0f, 9.3f), 0f, 2.0f, Vector3.back, _rust);
            var cabinet = PlaceProp("FileCabinet", "Kit_Furniture_FileCabinet_N1", new Vector3(5.6f, 0f, 9.3f), -90f, 1.35f, Vector3.back);
            PlaceProp("OpenLocker", "Kit_Furniture_Locker_N1", new Vector3(-9.3f, 0f, 4.2f), 180f, 1.95f, Vector3.right);
            // Sillón de acompañante mirando la camilla
            PlaceProp("Armchair", "Kit_Furniture_Armchair_N1", new Vector3(-3.5f, 0f, 0.85f), 140f, 1.0f, Vector3.zero);
            PlaceProp("Cage", "Kit_Furniture_Cage_N1", new Vector3(8.5f, 0f, -1.4f), -90f, 1.9f, Vector3.left);
            PlaceProp("WarehouseShelving", "Kit_Furniture_WarehouseShelf_N1", new Vector3(4.6f, 0f, -9.3f), 90f, 2.2f, Vector3.forward);
            if (cabinet != null)
            {
                var top = Encapsulate(cabinet);
                PlaceProp("ShelfClutter", "Kit_Clutter_N1", new Vector3(top.center.x, top.max.y, top.center.z), 0f, 0.3f, Vector3.zero);
            }
            DropFlat("Binder", "Kit_Binder_N1a", new Vector3(3.5f, 0f, -8.4f), 25f, 0.32f);
            DropFlat("Binder", "Kit_Binder_N1b", new Vector3(3.9f, 0f, -8.1f), -50f, 0.32f);
        }

        private const float WallGap = 0.03f;
        private const float WallReach = 4f;

        // Mueble apoyado en pos con el alto pedido; con inward distinto de cero queda de lomo contra la pared
        private GameObject PlaceProp(string kit, string name, Vector3 pos, float yaw, float height, Vector3 inward, Material mat = null)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return null;
            var go = Object.Instantiate(prefab, _root);
            go.name = name;
            go.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * prefab.transform.rotation;
            StripColliders(go);
            PaintKit(go, kit, mat);
            FitToHeight(go, pos.y, height);
            var b = Encapsulate(go);
            go.transform.position += new Vector3(pos.x - b.center.x, 0f, pos.z - b.center.z);
            if (inward != Vector3.zero)
                BackToWall(go, inward);
            if (ShouldBlock(name))
                SolidShell(go);
            return go;
        }

        private static void BackToWall(GameObject go, Vector3 inward)
        {
            Physics.SyncTransforms();
            var b = Encapsulate(go);
            float half = Mathf.Abs(Vector3.Dot(b.extents, new Vector3(Mathf.Abs(inward.x), 0f, Mathf.Abs(inward.z))));
            if (!Physics.Raycast(b.center, -inward, out var hit, WallReach, ~0, QueryTriggerInteraction.Ignore))
                return;
            float gap = Vector3.Dot(b.center - hit.point, inward) - half;
            go.transform.position -= inward * (gap - WallGap);
        }

        // Algo tirado en el piso, acostado sobre su cara más ancha
        private void DropFlat(string kit, string name, Vector3 pos, float yaw, float length, Material mat = null)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return;
            var go = Object.Instantiate(prefab, _root);
            go.name = name;
            go.transform.rotation = prefab.transform.rotation;
            StripColliders(go);
            PaintKit(go, kit, mat);
            LayFlat(go);
            go.transform.Rotate(0f, yaw, 0f, Space.World);
            var b = Encapsulate(go);
            float side = Mathf.Max(b.size.x, b.size.z);
            if (side > 0.001f)
                go.transform.localScale *= length / side;
            b = Encapsulate(go);
            go.transform.position += new Vector3(pos.x - b.center.x, pos.y - b.min.y, pos.z - b.center.z);
        }

        private void ExtraLab()
        {
            Put("workbench", new Vector3(-6.6f, 0f, -4.2f), 90f, _rust, 0.9f);
            Put("table-display", new Vector3(6.35f, 0f, -3.35f), -90f, _rust, 0.9f);
            Put("table-display-small", new Vector3(-6.55f, 0f, -0.4f), 90f, _rust, 0.85f);
            Put("Chair_3", new Vector3(5.15f, 0f, -4.15f), 40f, _wood, 0.95f);

            Put("Details_Cylinder", new Vector3(-6.5f, 0.38f, -4.15f), 18f, _rust);
            Put("Details_Cylinder", new Vector3(-6.25f, 0.38f, -4.35f), -22f, _rust);

            Put("warning-orange", new Vector3(-7.55f, 1.7f, 5.35f), 180f, _rust);
            Put("warning-orange", new Vector3(7.55f, 1.7f, -5.35f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(-3.2f, 7.35f, -2.2f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(3.4f, 7.35f, 2.2f), 0f, _rust);

            Put("container-tall", new Vector3(-7.15f, 0f, 4.55f), 18f, _rust, 1.4f);
            Put("container-flat-open", new Vector3(7.05f, 0f, 4.55f), -22f, _rust);
            Put("metal-panel", new Vector3(7.2f, 0.02f, -5.15f), -15f, _rust);
            Put("metal-panel-screws", new Vector3(7.05f, 0.02f, -4.85f), 40f, _rust);
            Put("Trashcan_Small2", new Vector3(-7.35f, 0f, 5.15f), 25f, _rust);

            Put("container-flat", new Vector3(0.95f, 0.02f, 0.15f), 8f, _rust, 0.16f);
            // La puerta del modelo mira a su −X: de espalda a la pared este
            Put("ElectricalBox02", new Vector3(7.57f, 1.15f, 3.2f), 0f, null, 0.7f, false);
            Put("WorkLight", new Vector3(-7.2f, 0f, -5.15f), 25f, null, 1.1f, false);
            Put("GasCylinder", new Vector3(7.25f, 0f, -5.2f), -20f, null, 0.95f, false);
            Put("CableDrum", new Vector3(-7.25f, 0f, 4.85f), 30f, null, 0.7f, false);
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

            Put("Light_CeilingSingle", new Vector3(-3.4f, 7.35f, -1.6f), 0f, _rust);
            Put("Light_CeilingSingle", new Vector3(3.2f, 7.35f, 1.4f), 0f, _rust);

            Put("books", new Vector3(-7.15f, 1.15f, -4.4f), 90f, _wood, 0.22f);
            Put("cardboardBoxOpen", new Vector3(7.15f, 0.02f, -4.95f), 0f, _wood, 0.45f);
            PutNamed("cardboardBoxClosed", "Kit_Box_N3a", new Vector3(-6.55f, 0.02f, 4.85f), 12f, _wood, 0.42f);
            Put("ElectricalBox02", new Vector3(-7.68f, 1.05f, 1.6f), 180f, null, 0.85f, false);
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
            PutNamed("cardboardBoxClosed", "Kit_Box_Mezz", new Vector3(6.55f, 0.02f, -2.85f), 8f, _wood, 0.4f);

            Put("metal-panel-narrow", new Vector3(-3.4f, 0.08f, 5.22f), 25f, _rust, 0.08f);
            Put("metal-panel-narrow", new Vector3(-0.95f, 0.08f, 5.18f), -18f, _rust, 0.07f);
            Put("metal-panel-narrow", new Vector3(-2.55f, 0.08f, 5.12f), 50f, _rust, 0.07f);
            Put("metal-panel-narrow", new Vector3(-1.6f, 0.08f, 5.24f), -35f, _rust, 0.06f);
            Put("metal-panel-narrow", new Vector3(-3.9f, 0.08f, 5.15f), 8f, _rust, 0.07f);
            Put("metal-panel-narrow", new Vector3(-0.55f, 0.08f, 5.08f), 42f, _rust, 0.06f);

            ExitPadAtDoor("Interactable_DoorN3", 0.9f);
            PlaceMezzHatch();
            DressMezzanine();
        }

        // Entrepiso: marco de la puerta, pasillo detrás y depósito contra las paredes.
        // Se graba en la escena; en Play solo se reponen colisión, posiciones y material del texto.
        private void DressMezzanine()
        {
            if (_root == null)
                _root = transform;
            EnsureMats();
            var wallL = FindLocal("Wall_N_L");
            var wallMat = wallL != null && wallL.GetComponent<MeshRenderer>() != null
                ? wallL.GetComponent<MeshRenderer>().sharedMaterial
                : _plaster;
            var dark = Resources.Load<Material>("Mats/Mat_Corridor");

            // Estructura: sin prefijo Kit_ para conservar la colisión al grabar
            Block("Wall_N_Lintel", new Vector3(0f, 4.925f, 6.1f), new Vector3(1.8f, 5.25f, 0.28f), wallMat);
            Block("Wall_N_JambL", new Vector3(-0.825f, 1.15f, 6.1f), new Vector3(0.15f, 2.3f, 0.28f), wallMat);
            Block("Wall_N_JambR", new Vector3(0.825f, 1.15f, 6.1f), new Vector3(0.15f, 2.3f, 0.28f), wallMat);
            Block("Hall_Floor", new Vector3(0f, -0.1f, 7.425f), new Vector3(1.8f, 0.2f, 2.65f), dark);
            Block("Hall_Roof", new Vector3(0f, 2.4f, 7.5f), new Vector3(1.8f, 0.2f, 2.5f), dark);
            Block("Hall_Back", new Vector3(0f, 1.15f, 8.75f), new Vector3(1.8f, 2.5f, 0.2f), dark);
            Block("Hall_L", new Vector3(-1f, 1.15f, 7.5f), new Vector3(0.2f, 2.5f, 2.5f), dark);
            Block("Hall_R", new Vector3(1f, 1.15f, 7.5f), new Vector3(0.2f, 2.5f, 2.5f), dark);

            Prop("workbench", "Kit_Mezz_Workbench", new Vector3(7.4f, 0f, 0.6f), 0f, _wood, 0.95f, true);
            Prop("WorkLight", "Kit_Mezz_BenchLamp", new Vector3(7.3f, 0f, 1.85f), -120f, null, 1.2f, false);
            Prop("Shelf_2", "Kit_Mezz_ShelfE", new Vector3(7.6f, 0f, -1.7f), -90f, _rust, 1.9f, true);
            Prop("cardboardBoxOpen", "Kit_Mezz_BoxE", new Vector3(6.95f, 0f, -0.95f), 15f, _wood, 0.42f, true);

            Prop("Shelf_2", "Kit_Mezz_ShelfW", new Vector3(-7.6f, 0f, 0.6f), 90f, _rust, 1.9f, true);
            Prop("container-tall", "Kit_Mezz_Cabinet", new Vector3(-7.3f, 0f, -1.6f), 90f, _rust, 1.9f, true);
            Prop("barrel", "Kit_Mezz_Barrel", new Vector3(-7.2f, 0f, 2.4f), 0f, _rust, 0.95f, true);
            Prop("barrel-open", "Kit_Mezz_BarrelOpen", new Vector3(-6.55f, 0f, 2.95f), 40f, _rust, 0.9f, true);

            var desk = Prop("desk", "Kit_Mezz_Desk", new Vector3(3.0f, 0f, -5.45f), 180f, _wood, 0.78f, true);
            float deskTop = desk != null ? Encapsulate(desk).max.y : 0.78f;
            Prop("chairDesk", "Kit_Mezz_Chair", new Vector3(3.0f, 0f, -4.6f), 160f, _rust, 0.95f, true);
            Prop("books", "Kit_Mezz_DeskBooks", new Vector3(2.5f, deskTop, -5.5f), 12f, null, 0.12f, false);
            Prop("radio", "Kit_Mezz_DeskRadio", new Vector3(3.6f, deskTop, -5.55f), 170f, null, 0.22f, false);
            Prop("Kitchen_3Drawers", "Kit_Mezz_Drawers", new Vector3(5.6f, 0f, -5.6f), 0f, _rust, 0.95f, true);
            Prop("GasCylinder", "Kit_Mezz_Gas", new Vector3(-2.6f, 0f, -5.55f), 0f, null, 1.3f, true);
            Prop("CableDrum", "Kit_Mezz_CableDrum", new Vector3(-4.6f, 0f, -5.2f), 20f, null, 0.8f, true);
            Prop("WorkLight", "Kit_Mezz_FloorLamp", new Vector3(-6.4f, 0f, -5.3f), 200f, null, 1.5f, false);

            if (!Application.isPlaying)
            {
                Logbook(new Vector3(3.0f, deskTop, -5.32f));
                BenchLight(FindLocal("Kit_Mezz_BenchLamp"));
                NoEntrySign(new Vector3(0f, 1.45f, 8.64f));
                Footprints(new Vector3(2.7f, 0f, -4.1f), new Vector3(1.4f, 0f, 2.3f), new Vector3(-1.7f, 0f, 4.85f));
            }
            AlignTo("Interactable_Logbook", "Kit_Mezz_Logbook");
            AlignTo("Interactable_Radio", "Kit_Mezz_DeskRadio");
            WorldTextMat(FindLocal("Kit_Mezz_SignText"));
        }

        // El interactable sigue al modelo que representa
        private void AlignTo(string interactable, string visual)
        {
            var it = FindLocal(interactable);
            var vis = FindLocal(visual);
            if (it == null || vis == null)
                return;
            it.transform.position = Encapsulate(vis).center;
        }

        // En Play el texto queda tapado por las paredes; grabado usa el material de la fuente
        private static void WorldTextMat(GameObject go)
        {
            if (go == null || !Application.isPlaying)
                return;
            var tm = go.GetComponent<TextMesh>();
            var mr = go.GetComponent<MeshRenderer>();
            if (tm != null && tm.font != null && mr != null)
                mr.sharedMaterial = Enigma.Interaction.ItemVisuals.TextMat(tm.font);
        }

        private void Block(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = FindLocal(name);
            if (go == null)
            {
                if (Application.isPlaying)
                    return;
                Slab(name, pos, scale, mat);
                return;
            }
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        // Libro de guardia abierto sobre el escritorio
        private void Logbook(Vector3 top)
        {
            if (FindLocal("Kit_Mezz_Logbook") != null)
                return;
            var cover = Resources.Load<Material>("Mats/Mat_Corridor");
            Slab("Kit_Mezz_Logbook", top + new Vector3(0f, 0.012f, 0f), new Vector3(0.46f, 0.024f, 0.32f), cover);
            Slab("Kit_Mezz_LogbookPages", top + new Vector3(0f, 0.03f, 0f), new Vector3(0.43f, 0.012f, 0.29f), _paper);
            var book = FindLocal("Kit_Mezz_Logbook");
            var pages = FindLocal("Kit_Mezz_LogbookPages");
            if (book != null && pages != null)
            {
                book.transform.rotation = Quaternion.Euler(0f, 8f, 0f);
                pages.transform.rotation = book.transform.rotation;
            }
        }

        // Luz de trabajo sobre el banco: la lámpara titila
        private void BenchLight(GameObject lamp)
        {
            if (lamp == null || FindLocal("Kit_Mezz_BenchLight") != null)
                return;
            var b = Encapsulate(lamp);
            var go = new GameObject("Kit_Mezz_BenchLight");
            go.transform.SetParent(_root, false);
            go.transform.position = new Vector3(b.center.x - 0.25f, b.max.y - 0.05f, b.center.z - 0.35f);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 5.5f;
            l.color = new Color(1f, 0.82f, 0.58f);
            l.shadows = LightShadows.None;
            var flick = go.AddComponent<FlickeringLight>();
            flick.Configure(4.5f, 2.2f);
        }

        // Cartel al fondo del pasillo, de cara a la puerta
        private void NoEntrySign(Vector3 pos)
        {
            if (FindLocal("Kit_Mezz_Sign") != null)
                return;
            Slab("Kit_Mezz_Sign", pos, new Vector3(1.2f, 0.62f, 0.03f), _plaster);
            var lampGo = new GameObject("Kit_Mezz_HallLight");
            lampGo.transform.SetParent(_root, false);
            lampGo.transform.position = new Vector3(pos.x, 2.15f, pos.z - 1.1f);
            var lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.range = 3.2f;
            lamp.intensity = 4.5f;
            lamp.color = new Color(0.72f, 0.8f, 0.9f);
            lamp.shadows = LightShadows.None;
            var font = Resources.Load<Font>("Fonts/StardosStencil-Bold");
            var go = new GameObject("Kit_Mezz_SignText");
            go.transform.SetParent(_root, false);
            go.transform.position = pos + new Vector3(0f, 0f, -0.02f);
            // TextMesh se lee desde -Z local: mira hacia la puerta
            go.transform.rotation = Quaternion.identity;
            var tm = go.AddComponent<TextMesh>();
            tm.text = "PROHIBIDO\nEL PASO";
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.032f;
            tm.fontSize = 64;
            tm.color = new Color(0.55f, 0.06f, 0.05f, 1f);
            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.Off;
            if (font != null)
            {
                tm.font = font;
                mr.sharedMaterial = font.material;
            }
        }

        // Pisadas en el polvo, del escritorio hacia el cuadro
        private void Footprints(Vector3 from, Vector3 mid, Vector3 to)
        {
            if (FindLocal("Kit_Mezz_Steps") != null)
                return;
            var mat = Resources.Load<Material>("Mats/Mat_Footprint");
            if (mat == null)
                return;
            var root = new GameObject("Kit_Mezz_Steps");
            root.transform.SetParent(_root, false);
            var rng = new System.Random(129);
            const float stride = 0.62f;
            float len = Vector3.Distance(from, mid) + Vector3.Distance(mid, to);
            int count = Mathf.Max(1, Mathf.FloorToInt(len / stride));
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                // Bezier cuadrática: el recorrido esquiva la escalera
                Vector3 p = (1 - t) * (1 - t) * from + 2 * (1 - t) * t * mid + t * t * to;
                Vector3 dir = 2 * (1 - t) * (mid - from) + 2 * t * (to - mid);
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-6f)
                    dir = to - from;
                dir.Normalize();
                Vector3 side = Vector3.Cross(Vector3.up, dir);
                bool left = i % 2 == 0;
                Vector3 at = p + side * (left ? -0.11f : 0.11f);
                float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + (float)(rng.NextDouble() * 10.0 - 5.0);
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                q.name = "Kit_Mezz_Step";
                q.transform.SetParent(root.transform, false);
                q.transform.position = new Vector3(at.x, 0.004f, at.z);
                q.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
                q.transform.localScale = new Vector3(left ? -0.13f : 0.13f, 0.3f, 1f);
                StripColliders(q);
                var r = q.GetComponent<MeshRenderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private GameObject Prop(string kit, string name, Vector3 pos, float yaw, Material mat, float height, bool solid)
        {
            var existing = FindLocal(name);
            if (existing != null)
            {
                if (solid)
                    SolidShell(existing);
                return existing;
            }
            if (Application.isPlaying)
                return null;
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return null;
            var go = Object.Instantiate(prefab, _root);
            go.name = name;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * prefab.transform.rotation;
            StripColliders(go);
            PaintKit(go, kit, mat);
            FitToHeight(go, pos.y, height);
            var b = Encapsulate(go);
            Vector3 p = go.transform.position;
            go.transform.position = new Vector3(p.x + (pos.x - b.center.x), p.y, p.z + (pos.z - b.center.z));
            if (solid)
                SolidShell(go);
            return go;
        }

        private void ExtraOrigen()
        {
            Put("metal-panel", new Vector3(7.55f, -1.35f, 0f), -90f, _rust);

            Put("Light_CeilingSingle", new Vector3(0f, 7.35f, 4.2f), 0f, _rust);
            Put("container-flat", new Vector3(-6.6f, -2.55f, 3.4f), 40f, _rust);
            Put("metal-panel-narrow", new Vector3(-7.2f, -2.52f, -3.6f), 22f, _rust);
            Put("container-flat-open", new Vector3(6.8f, -2.55f, -3.5f), 0f, _rust);
            PutNamed("cardboardBoxClosed", "Kit_Box_N4a", new Vector3(-6.05f, 0.02f, -3.55f), 14f, _wood, 0.42f);

            ExitPad(new Vector3(0f, 1.1f, 6.45f), new Vector3(2.4f, 2.2f, 1.2f));
            CoverN4Pit();
            StairRamp("Ramp_Pit_", "PitStairRamp");
            PitRailing();
            DressOrigen();
        }

        private const float RailHeight = 1.0f;
        private const float RailPostGap = 1.2f;

        // Baranda de caño alrededor del foso y al costado de la escalera; la entrada desde el sur queda abierta
        private void PitRailing()
        {
            var gallery = FindLocal("Gallery_S");
            var gr = gallery != null ? gallery.GetComponent<Renderer>() : null;
            float top = gr != null ? gr.bounds.max.y : 0.13f;
            var root = NewRoot("Kit_PitRailing", Vector3.zero, Quaternion.identity).transform;
            var solid = NewRoot("Kit_PitRailing_Solid", Vector3.zero, Quaternion.identity).transform;
            RailRun(root, solid, "PitRail_S", new Vector3(-2.65f, top, -2.65f), new Vector3(2.95f, top, -2.65f), RailHeight);
            RailRun(root, solid, "PitRail_E", new Vector3(2.95f, top, -2.65f), new Vector3(2.95f, top, 2.65f), RailHeight);
            RailRun(root, solid, "PitRail_N", new Vector3(-4.55f, top, 2.65f), new Vector3(2.95f, top, 2.65f), RailHeight);
            RailRun(root, solid, "PitRail_W", new Vector3(-4.55f, top, -2.6f), new Vector3(-4.55f, top, 2.65f), RailHeight);

            // Pasamanos sobre el muro entre escalera y foso: acompaña las narices hasta que el muro ya pasa el metro
            var wall = FindLocal("StairRail_E");
            var steps = new System.Collections.Generic.List<Bounds>();
            foreach (var r in gameObject.scene.GetRootGameObjects())
                foreach (var rend in r.GetComponentsInChildren<MeshRenderer>(true))
                    if (rend.name.StartsWith("Ramp_Pit_"))
                        steps.Add(rend.bounds);
            if (wall == null || steps.Count < 2)
                return;
            var wb = wall.GetComponent<Renderer>().bounds;
            steps.Sort((a, b) => b.max.y.CompareTo(a.max.y));
            Bounds hi = steps[0], lo = steps[steps.Count - 1];
            float slope = (lo.max.y - hi.max.y) / (lo.center.z - hi.center.z);
            float NoseY(float z) => hi.max.y + (z - hi.center.z) * slope;
            float z0 = wb.min.z;
            float z1 = hi.center.z + (wb.max.y + 0.1f - RailHeight - hi.max.y) / slope;
            var a = new Vector3(wb.center.x, NoseY(z0) + RailHeight, z0);
            var b = new Vector3(wb.center.x, NoseY(z1) + RailHeight, z1);
            RailRun(root, solid, "PitRail_Stair", a, b, 0f, wb.max.y);
        }

        // Tramo recto: postes, pasamanos y travesaño. Sin height, a y b son el pasamanos y los postes bajan hasta floorY
        private void RailRun(Transform root, Transform solid, string name, Vector3 a, Vector3 b, float height, float floorY = 0f)
        {
            var steel = Mat("Mat_Steel");
            const float post = 0.05f, bar = 0.04f;
            Vector3 railA = a + Vector3.up * height, railB = b + Vector3.up * height;
            Vector3 along = railB - railA;
            float len = along.magnitude;
            if (len < 0.1f)
                return;
            var run = new GameObject(name).transform;
            run.SetParent(root, false);
            run.position = (railA + railB) * 0.5f;
            run.rotation = Quaternion.LookRotation(along.normalized, Vector3.up);
            Part(run, "RailTop", Vector3.zero, new Vector3(bar, bar, len + post), steel);

            int posts = Mathf.CeilToInt(len / RailPostGap) + 1;
            for (int i = 0; i < posts; i++)
            {
                Vector3 p = Vector3.Lerp(railA, railB, i / (float)(posts - 1));
                float baseY = height > 0f ? p.y - height : floorY;
                float h = p.y - baseY;
                if (h < 0.05f)
                    continue;
                var pg = Part(run, "RailPost", Vector3.zero, new Vector3(post, h, post), steel);
                pg.transform.SetPositionAndRotation(new Vector3(p.x, baseY + h * 0.5f, p.z), Quaternion.LookRotation(new Vector3(along.x, 0f, along.z), Vector3.up));
            }
            if (height > 0f)
            {
                var mid = Part(run, "RailMid", Vector3.zero, new Vector3(bar, bar, len), steel);
                mid.transform.position = run.position - Vector3.up * height * 0.5f;
            }

            // El sólido va en Ignore Raycast: frena al jugador sin empujar la cámara
            float colH = height > 0f ? height + 0.1f : RailHeight;
            var block = new GameObject(name + "_Block");
            block.transform.SetParent(solid, false);
            block.layer = LayerMask.NameToLayer("Ignore Raycast");
            var box = block.AddComponent<BoxCollider>();
            if (height > 0f)
            {
                block.transform.SetPositionAndRotation(run.TransformPoint(new Vector3(0f, -colH * 0.5f + 0.05f, 0f)), run.rotation);
                box.size = new Vector3(0.12f, colH, len);
            }
            else
            {
                // En la escalera el sólido queda derecho, del muro al punto más alto del pasamanos
                Vector3 flat = new Vector3(along.x, 0f, along.z);
                float high = Mathf.Max(railA.y, railB.y);
                Vector3 mid = (railA + railB) * 0.5f;
                block.transform.SetPositionAndRotation(new Vector3(mid.x, (floorY + high) * 0.5f, mid.z), Quaternion.LookRotation(flat, Vector3.up));
                box.size = new Vector3(0.12f, high - floorY, flat.magnitude);
            }
        }

        // Rampa invisible apoyada en las narices de los escalones: el capsule baja sin caer escalón por escalón
        private void StairRamp(string stepPrefix, string name)
        {
            var steps = new System.Collections.Generic.List<Bounds>();
            var roots = gameObject.scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
                foreach (var rend in roots[r].GetComponentsInChildren<MeshRenderer>(true))
                    if (rend.name.StartsWith(stepPrefix))
                        steps.Add(rend.bounds);
            if (steps.Count < 2)
                return;
            steps.Sort((a, b) => b.max.y.CompareTo(a.max.y));

            Bounds top = steps[0], low = steps[steps.Count - 1];
            Vector3 dir = Vector3.ProjectOnPlane(low.center - top.center, Vector3.up).normalized;
            Vector3 a = Nose(top, dir);
            Vector3 b = Nose(low, dir);
            b += (b - a) / (steps.Count - 1);
            Vector3 along = b - a;
            Vector3 normal = Vector3.Cross(along, Vector3.Cross(Vector3.up, along)).normalized;
            float width = Mathf.Abs(Vector3.Dot(top.size, Vector3.Cross(Vector3.up, dir)));
            const float thick = 0.1f;

            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = (a + b) * 0.5f - normal * (thick * 0.5f);
            go.transform.rotation = Quaternion.LookRotation(along, normal);
            go.AddComponent<BoxCollider>().size = new Vector3(width, thick, along.magnitude);
        }

        private static Vector3 Nose(Bounds step, Vector3 dir)
        {
            float reach = Mathf.Abs(Vector3.Dot(step.extents, new Vector3(Mathf.Abs(dir.x), 0f, Mathf.Abs(dir.z))));
            Vector3 c = step.center + dir * reach;
            return new Vector3(c.x, step.max.y, c.z);
        }

        private void Lights(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                var fill = Point("Fill", new Vector3(0f, 6.8f, 0f), new Color(1f, 0.84f, 0.62f), 16f, 16f);
                var flicker = fill.gameObject.AddComponent<LightFlicker>();
                flicker.baseIntensity = 16f;
                Point("DeskLamp", new Vector3(5.5f, 1.6f, 2.5f), new Color(1f, 0.72f, 0.45f), 8f, 5f);
            }
            else if (sceneName == "Level02_Lab")
            {
                Point("Fill", new Vector3(0f, 6.6f, 0f), new Color(0.45f, 0.55f, 0.5f), 7f, 16f);
                Point("WarnA", new Vector3(7.2f, 2.1f, 4.4f), new Color(1f, 0.22f, 0.1f), 10f, 7f);
                Point("WarnB", new Vector3(-6.8f, 2.15f, -4.8f), new Color(1f, 0.28f, 0.12f), 9f, 6.5f);
            }
            else if (sceneName == "Level03_Archives")
            {
                Point("EmergA", new Vector3(-3.4f, 6.8f, -1.5f), new Color(1f, 0.32f, 0.12f), 9.5f, 14f);
                Point("EmergB", new Vector3(3.2f, 6.8f, 1.3f), new Color(1f, 0.3f, 0.1f), 9f, 14f);
                Point("Fire", new Vector3(3.8f, 0.7f, 1.6f), new Color(1f, 0.38f, 0.08f), 9.5f, 4.5f);
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                Point("Fill", new Vector3(0f, 6.8f, 0f), new Color(0.9f, 0.82f, 0.62f), 13f, 16f);
            }
            else if (sceneName == "Level04_Origen")
            {
                Point("Gallery", new Vector3(0f, 6.5f, 4.2f), new Color(0.85f, 0.7f, 0.5f), 19f, 16f);
                Point("Pit", new Vector3(0f, -1.4f, 0f), new Color(1f, 0.38f, 0.16f), 22f, 10f);
            }
        }

        // Tubos colgados: el artefacto siempre se ve; brillo y luz viven en PoweredLights, que prende la flag de la energía
        private void PowerTubes(string sceneName)
        {
            Vector3[] spots;
            string flag;
            if (sceneName == "Level02_Lab")
            {
                spots = new[] { new Vector3(-3.6f, 4.2f, 1.2f), new Vector3(0.4f, 4.2f, -1.0f), new Vector3(4.0f, 4.2f, 1.6f) };
                flag = "generator_on";
            }
            else if (sceneName == "Level03_Archives")
            {
                spots = new[] { new Vector3(-4.2f, 4.2f, -2.2f), new Vector3(0.6f, 4.2f, -0.4f), new Vector3(4.6f, 4.2f, -2.4f) };
                flag = "power_restored";
            }
            else
                return;
            var powered = PoweredRoot(flag);
            if (powered == null)
                return;
            var steel = Resources.Load<Material>("Mats/Mat_Steel");
            var glow = Resources.Load<Material>("Mats/Mat_TubeGlow");
            float roof = RoofYOf(sceneName) - RoofThick * 0.5f;
            const float length = 1.25f, width = 0.22f, lamp = 0.035f;
            for (int i = 0; i < spots.Length; i++)
            {
                Vector3 p = spots[i];
                var fixture = new GameObject("Kit_Tube_" + i).transform;
                fixture.SetParent(_root, false);
                fixture.position = p;
                Part(fixture, "Kit_TubeHousing", Vector3.zero, new Vector3(length, 0.05f, width), steel);
                for (int s = -1; s <= 1; s += 2)
                {
                    float hang = roof - p.y - 0.025f;
                    Part(fixture, "Kit_TubeCable", new Vector3(s * length * 0.35f, 0.025f + hang * 0.5f, 0f), new Vector3(0.012f, hang, 0.012f), steel);
                    Lamp(fixture, "Kit_TubeLamp", new Vector3(0f, -0.045f, s * width * 0.22f), lamp, length - 0.1f, steel);
                }

                var on = new GameObject("Kit_TubeOn_" + i).transform;
                on.SetParent(powered.transform, false);
                on.position = p;
                for (int s = -1; s <= 1; s += 2)
                    Lamp(on, "Kit_TubeGlow", new Vector3(0f, -0.045f, s * width * 0.22f), lamp * 1.25f, length - 0.09f, glow);
                var lightGo = new GameObject("Kit_TubeLight");
                lightGo.transform.SetParent(on, false);
                lightGo.transform.localPosition = new Vector3(0f, -0.25f, 0f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.9f, 0.95f, 1f);
                light.intensity = 9f;
                light.range = 10f;
                light.shadows = LightShadows.None;
            }
            LightsShot(powered, flag, spots, length * 0.5f);
        }

        // N1: el segundo artefacto del techo está muerto hasta que entra el fusible
        private void PowerBulbs(string sceneName)
        {
            if (sceneName != "Level01_Room")
                return;
            const string flag = "fuse_installed";
            Vector3 spot = new Vector3(4.8f, 0f, 3.4f);
            Transform dead = null, live = null;
            foreach (var t in _root.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Kit_Light_CeilingSingle"))
                    continue;
                if (dead == null || Flat(t.position - spot) < Flat(dead.position - spot))
                    dead = t;
                if (live == null || Flat(t.position) < Flat(live.position))
                    live = t;
            }
            if (dead == null || live == null || dead == live)
                return;
            var powered = PoweredRoot(flag);
            if (powered == null)
                return;
            var old = powered.transform.Find("Kit_BulbOn");
            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            var b = Encapsulate(dead.gameObject);
            var on = new GameObject("Kit_BulbOn").transform;
            on.SetParent(powered.transform, false);
            on.position = new Vector3(b.center.x, b.min.y, b.center.z);
            var glow = Part(on, "Kit_BulbGlow", new Vector3(0f, -0.03f, 0f), Vector3.one * 0.08f, Resources.Load<Material>("Mats/Mat_TubeGlow"), PrimitiveType.Sphere);
            glow.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            var lightGo = new GameObject("Kit_BulbLight");
            lightGo.transform.SetParent(on, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.12f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.82f, 0.58f);
            light.intensity = 12f;
            light.range = 12f;
            light.shadows = LightShadows.None;

            var lb = Encapsulate(live.gameObject);
            LightsShot(powered, flag, new[] { lb.center, b.center }, Mathf.Max(b.extents.x, b.extents.z));
        }

        private static float Flat(Vector3 v)
        {
            return v.x * v.x + v.z * v.z;
        }

        private const float LightsShotSeconds = 2.5f;

        // Al prenderse, la cámara sube la vista al centro de las luces
        private static void LightsShot(GameObject powered, string flag, Vector3[] spots, float halfTube)
        {
            var host = powered.transform.parent;
            Vector3 center = Vector3.zero;
            foreach (var p in spots)
                center += p;
            center /= spots.Length;
            float reach = 0f;
            foreach (var p in spots)
                reach = Mathf.Max(reach, Vector3.Distance(p, center) + halfTube);
            var focus = host.Find("LightsFocus");
            if (focus == null)
            {
                focus = new GameObject("LightsFocus").transform;
                focus.SetParent(host, false);
            }
            focus.position = center;
            var shot = host.GetComponent<FlagShot>();
            if (shot == null)
                shot = host.gameObject.AddComponent<FlagShot>();
            shot.Configure(flag, focus, LightsShotSeconds, reach);
        }

        // Raíz apagada de lo que prende la energía; si la escena no la trae se arma con su interruptor de flag
        private GameObject PoweredRoot(string flag)
        {
            var powered = FindLocal("PoweredLights");
            if (powered != null)
                return powered;
            var host = FindLocal("Systems");
            if (host == null)
                return null;
            powered = new GameObject("PoweredLights");
            powered.transform.SetParent(host.transform, false);
            powered.SetActive(false);
            host.AddComponent<FlagToggleRoot>().Configure(powered, flag);
            return powered;
        }

        // Tubo acostado a lo largo de X
        private static void Lamp(Transform parent, string name, Vector3 local, float diameter, float length, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            StripColliders(go);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            go.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        // URP 2048 no entra tantas sombras puntuales: las apaga y Underground elige una.
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

        // Subsuelo: sin cielo ni sol; el ambiente casi negro deja que las lámparas (y su titileo) manden
        private void Underground()
        {
            var scene = gameObject.scene;
            if (!scene.IsValid() || Application.isPlaying)
                return;
            var murk = new Color(0.012f, 0.012f, 0.015f);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.05f, 0.055f, 0.065f);
            RenderSettings.ambientEquatorColor = new Color(0.03f, 0.03f, 0.035f);
            RenderSettings.ambientGroundColor = new Color(0.012f, 0.011f, 0.01f);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;
            RenderSettings.reflectionIntensity = 0.2f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = murk;
            RenderSettings.fogDensity = 0.05f;

            Light key = null;
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                foreach (var light in roots[r].GetComponentsInChildren<Light>(true))
                {
                    if (light.type == LightType.Directional)
                        light.enabled = false;
                    else if (light.transform.IsChildOf(_root) && (key == null || light.intensity * light.range > key.intensity * key.range))
                        key = light;
                }
                foreach (var cam in roots[r].GetComponentsInChildren<Camera>(true))
                {
                    if (cam.targetTexture != null || cam.clearFlags == CameraClearFlags.Depth || cam.clearFlags == CameraClearFlags.Nothing)
                        continue;
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = murk;
                }
            }
            // Una sola puntual con sombra entra en el atlas de URP
            if (key != null)
            {
                key.shadows = LightShadows.Soft;
                // La pantalla de la lámpara envuelve la luz: sin este margen tapa medio cuarto
                key.shadowNearPlane = 0.8f;
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
                // Restos quemados sobre la mancha, lejos de los expedientes desparramados
                Scraps(new Vector3(3.0f, 0.04f, 0.9f), 4, 0.35f, _paperBurn);
                // Hojas sueltas por el piso del archivo
                LooseSheet(new Vector3(2.6f, 0.04f, -0.2f), 35f);
                LooseSheet(new Vector3(4.45f, 0.04f, -0.75f), -20f);
                LooseSheet(new Vector3(3.3f, 0.04f, -0.9f), 70f);
                LooseSheet(new Vector3(3.85f, 0.04f, 0.3f), 10f);
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
            PaintGoNamed("HatchLid", Mat("Mat_HatchLid"));
            PaintGoNamed("Chair_Origen", _rust);
            PaintGoNamed("ArchiveDesk", _wood);
            PaintGoNamed("Console", _rust);
            PaintGoNamed("Decor_Stretcher", _wood);
            PaintProps();
        }

        // Tapas de mesa del greybox: una pata por esquina hasta el piso que tengan debajo
        private void SlabLegs(string name)
        {
            var slab = FindLocal(name);
            if (slab == null || !VisibleBounds(slab, out var b))
                return;
            var rend = slab.GetComponent<MeshRenderer>();
            var mat = rend != null && rend.sharedMaterial != null ? rend.sharedMaterial : _wood;
            const float leg = 0.06f;
            const float inset = 0.08f;
            Physics.SyncTransforms();
            for (int i = 0; i < 4; i++)
            {
                float x = (i & 1) == 0 ? b.min.x + inset : b.max.x - inset;
                float z = (i & 2) == 0 ? b.min.z + inset : b.max.z - inset;
                var from = new Vector3(x, b.min.y - 0.01f, z);
                float ground = float.MinValue;
                foreach (var hit in Physics.RaycastAll(from, Vector3.down, 1.5f, ~0, QueryTriggerInteraction.Ignore))
                {
                    string n = hit.collider.name;
                    if (n.StartsWith("Kit_") || n.StartsWith("Paper") || n.EndsWith("_Solid") || hit.collider.transform.IsChildOf(slab.transform))
                        continue;
                    ground = Mathf.Max(ground, hit.point.y);
                }
                float h = from.y - ground;
                if (ground == float.MinValue || h < 0.05f)
                    continue;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Kit_" + name + "_Leg";
                StripColliders(go);
                go.transform.SetParent(_root, false);
                go.transform.position = new Vector3(x, ground + h * 0.5f + 0.005f, z);
                go.transform.localScale = new Vector3(leg, h, leg);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
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

        private void LooseSheet(Vector3 pos, float yaw)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PaperScrap";
            go.transform.SetParent(_root, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            go.transform.localScale = new Vector3(0.22f, 0.008f, 0.3f);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(col);
                else
                    Object.DestroyImmediate(col);
            }
            var rend = go.GetComponent<MeshRenderer>();
            rend.sharedMaterial = _paper;
            rend.shadowCastingMode = ShadowCastingMode.Off;
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

        private void PlaceGrey(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                var box = FindLocal("Interactable_ElectricalBox");
                if (box != null)
                    HideProto(box);
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
                    if (paint != null)
                        TapeCredentialBehind(cred, paint);
                }

                PlacePuzzleMetals();
                PlaceMagnetByMetals();
            }
        }

        private void PaintProps()
        {
            DressFuses();
            PaintPrefix("Drawer", _wood);
            PaintGoNamed("Interactable_Drawer", Mat("Mat_DarkWood"));
            PaintGoNamed("DrawerHandle", Mat("Mat_Steel"));
        }

        // Fusibles de la escena con el modelo que se inspecciona, todos del largo del hueco de la caja
        private void DressFuses()
        {
            var slot = FindLocal("InsertedFuseVisual");
            float length = ItemVisuals.SizeOf("fuse");
            float girth = float.MaxValue;
            if (slot != null)
            {
                var sides = GreySize(slot);
                float[] s = { sides.x, sides.y, sides.z };
                System.Array.Sort(s);
                length = s[2];
                girth = s[1] * FuseGirthFill;
            }
            foreach (var n in new[] { "FuseDecor_A", "FuseDecor_B", "InsertedFuseVisual", "Interactable_Fuse" })
                DressFuse(FindLocal(n), length, girth);
        }

        // Cuánto del ancho del hueco ocupa el fusible: los de la caja van uno al lado del otro
        private const float FuseGirthFill = 0.8f;

        private static Vector3 GreySize(GameObject go)
        {
            var f = go.GetComponent<MeshFilter>();
            Vector3 size = Vector3.Scale(f != null && f.sharedMesh != null ? f.sharedMesh.bounds.size : Vector3.one, go.transform.lossyScale);
            return new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        }

        // El modelo sigue el lado largo del greybox sin pasarse del largo ni del ancho del hueco; su escala compensa la del padre eje por eje
        private static void DressFuse(GameObject go, float length, float girth)
        {
            if (go == null || go.transform.Find("Kit_Fuse") != null)
                return;
            var vis = ItemVisuals.Build("fuse");
            if (vis == null)
                return;
            var own = go.GetComponent<MeshRenderer>();
            if (own != null)
                own.enabled = false;

            Vector3 size = GreySize(go);
            Vector3 axis = size.x >= size.y && size.x >= size.z ? Vector3.right : (size.y >= size.z ? Vector3.up : Vector3.forward);

            var mb = Encapsulate(vis);
            float k = length / Mathf.Max(0.0001f, mb.size.x);
            k = Mathf.Min(k, girth / Mathf.Max(0.0001f, Mathf.Max(mb.size.y, mb.size.z)));
            float world = vis.transform.localScale.x * k;
            vis.name = "Kit_Fuse";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localPosition = Vector3.zero;
            var rot = Quaternion.FromToRotation(Vector3.right, axis);
            vis.transform.localRotation = rot;
            Vector3 ls = go.transform.lossyScale;
            var scale = Vector3.one;
            for (int j = 0; j < 3; j++)
            {
                Vector3 e = Vector3.zero;
                e[j] = 1f;
                Vector3 m = rot * e;
                int i = Mathf.Abs(m.x) > 0.5f ? 0 : (Mathf.Abs(m.y) > 0.5f ? 1 : 2);
                scale[j] = world / Mathf.Max(0.0001f, Mathf.Abs(ls[i]));
            }
            vis.transform.localScale = scale;
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
                var etm = existingStencil.GetComponent<TextMesh>();
                var emr = existingStencil.GetComponent<MeshRenderer>();
                if (etm != null && etm.font != null && emr != null)
                    emr.sharedMaterial = Enigma.Interaction.ItemVisuals.TextMat(etm.font);
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
                    mr.sharedMaterial = Enigma.Interaction.ItemVisuals.TextMat(font);
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
                PaintKit(bed, "HospitalBed", _rust);
            }
            else
            {
                // Por si un bake viejo la dejó de canto.
                bed.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up);
            }
            FitLongOnFloor(bed, floor.y, 2.1f, 0.72f);
            // El pivote del modelo no está en su centro: la camilla se centra sobre su gris
            var bb = Encapsulate(bed);
            bed.transform.position += new Vector3(floor.x - bb.center.x, 0f, floor.z - bb.center.z);
            HideVisual(src);
            FitColliderTo(src, bed);
            SolidShell(bed);
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

        private static readonly Vector3 LegDebrisSize = new Vector3(0.95f, 0.035f, 0.6f);

        // Chapa del techo caída sobre las piernas de Megan: el componente la acomoda mientras yace y la suelta al levantarse
        private void PlaceLegDebris()
        {
            Enigma.Player.PlayerStateController player = null;
            foreach (var p in Object.FindObjectsByType<Enigma.Player.PlayerStateController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (p.gameObject.scene == gameObject.scene)
                    player = p;
            if (player == null)
                return;
            var old = FindLocal("LegDebris");
            if (old != null)
                Object.DestroyImmediate(old);
            var prefab = Resources.Load<GameObject>("Kitbash/metal-panel");
            if (prefab == null)
                return;

            var go = new GameObject("LegDebris");
            go.transform.SetParent(_root, false);
            go.transform.position = player.transform.position - player.transform.forward * 0.6f + Vector3.up * 0.15f;
            var holder = new GameObject("Visual").transform;
            holder.SetParent(go.transform, false);
            var model = Object.Instantiate(prefab, holder);
            model.name = "Kit_metal-panel";
            StripColliders(model);
            Paint(model, _rust);
            // El lado más fino del modelo queda vertical: así apoya plano sobre las piernas
            var b = Encapsulate(model);
            if (b.size.z < b.size.y && b.size.z <= b.size.x)
                model.transform.Rotate(90f, 0f, 0f, Space.World);
            else if (b.size.x < b.size.y && b.size.x < b.size.z)
                model.transform.Rotate(0f, 0f, 90f, Space.World);
            FitToBox(holder.gameObject, 0f, LegDebrisSize.x, LegDebrisSize.y, LegDebrisSize.z);
            b = Encapsulate(holder.gameObject);
            holder.position += go.transform.position - b.center;

            var box = go.AddComponent<BoxCollider>();
            box.size = LegDebrisSize;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 4f;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            go.AddComponent<Enigma.Sequences.LegDebris>().Configure(player, player.GetComponent<CharacterController>());
        }

        private const float PcHeight = 0.36f;
        private const float PrinterSide = 0.36f;
        private const float DeskMargin = 0.06f;

        // Archivo: la PC completa en una punta del escritorio y la impresora al lado; el interactable y la pantalla siguen al monitor
        private void DressArchivePc(string sceneName)
        {
            if (sceneName != "Level03_Archives" || FindLocal("Kit_Computer") != null)
                return;
            var pc = FindLocal("Interactable_PC");
            var desk = FindLocal("ArchiveDesk");
            if (pc == null || desk == null)
                return;
            Vector3 d = desk.transform.position;
            Vector3 ds = desk.transform.lossyScale;
            float top = d.y + ds.y * 0.5f;
            var set = PutCentered("Computer", new Vector3(d.x, top, d.z), 0f, PcHeight, 0f);
            if (set == null)
                return;
            var sb = Encapsulate(set);
            set.transform.position += Vector3.right * (d.x - ds.x * 0.5f + DeskMargin - sb.min.x);
            sb = Encapsulate(set);
            var mon = FindChildNamed(set.transform, "Monitor");
            var mb = mon != null ? Encapsulate(mon.gameObject) : sb;
            pc.transform.position = new Vector3(mb.center.x, pc.transform.position.y, mb.center.z);
            HideSelf(pc);
            var screen = FindLocal("PcScreen");
            if (screen != null)
            {
                screen.transform.SetPositionAndRotation(new Vector3(mb.center.x, mb.min.y + mb.size.y * 0.56f, mb.max.z + 0.004f), Quaternion.identity);
                screen.transform.localScale = new Vector3(mb.size.x * 0.7f, mb.size.y * 0.55f, 0.008f);
                var on = Resources.Load<Material>("Mats/Mat_ScreenOn");
                var rend = screen.GetComponent<MeshRenderer>();
                if (on != null && rend != null)
                    rend.sharedMaterial = on;
            }

            var printer = FindLocal("Printer");
            var kit = PutCentered("Printer", new Vector3(sb.max.x, top, d.z), 0f, 0f, PrinterSide);
            if (printer == null || kit == null)
                return;
            var pb = Encapsulate(kit);
            kit.transform.position += Vector3.right * (sb.max.x + DeskMargin - pb.min.x);
            printer.transform.position = Encapsulate(kit).center;
            HideVisual(printer);
        }

        // Modelo apoyado en pos, centrado en planta; height fija la altura y maxSide acota el lado mayor
        private GameObject PutCentered(string kit, Vector3 pos, float yaw, float height, float maxSide)
        {
            if (!Put(kit, pos, yaw, null, height, false))
                return null;
            var go = FindLocal("Kit_" + kit);
            if (go == null)
                return null;
            var b = Encapsulate(go);
            float side = Mathf.Max(b.size.x, b.size.z);
            if (maxSide > 0.01f && side > maxSide)
                go.transform.localScale *= maxSide / side;
            b = Encapsulate(go);
            go.transform.position += new Vector3(pos.x - b.center.x, pos.y - b.min.y, pos.z - b.center.z);
            return go;
        }

        private void PlaceLeaks(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                PlaceDecal("Kit_Leak_N1a", "Docs/Leaking001_Color", "Docs/Leaking001_Opacity", new Vector3(-9.84f, 2.1f, -2.4f), new Vector3(0f, 90f, 0f), new Vector3(1.8f, 2.4f, 1f));
                PlaceDecal("Kit_Rust_N1a", "Docs/Rust001_Color", "Docs/Rust001_Opacity", new Vector3(9.84f, 1.4f, 3.2f), new Vector3(0f, -90f, 0f), new Vector3(1.5f, 1.8f, 1f));
            }
            else if (sceneName == "Level02_Lab")
            {
                PlaceDecal("Kit_Leak_N2a", "Docs/Leaking003_Color", "Docs/Leaking003_Opacity", new Vector3(7.58f, 2.2f, -4.1f), new Vector3(0f, -90f, 0f), new Vector3(1.6f, 2.2f, 1f));
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
            // El Quad se ve desde -Z: con el giro extra la cara visible queda del lado del cuarto que indica euler
            go.transform.rotation = Quaternion.Euler(euler) * Quaternion.Euler(0f, 180f, 0f);
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

        // Las manchas de una escena con la misma textura comparten material
        private readonly System.Collections.Generic.Dictionary<string, Material> _leakMats = new System.Collections.Generic.Dictionary<string, Material>();

        private Material LeakMat(string colorPath, string opacityPath)
        {
            string key = colorPath + "|" + opacityPath;
            if (_leakMats.TryGetValue(key, out var cached) && cached != null)
                return cached;
            var mat = NewLeakMat(colorPath, opacityPath);
            _leakMats[key] = mat;
            return mat;
        }

        private static Material NewLeakMat(string colorPath, string opacityPath)
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
                if (t.name.IndexOf("Cap") >= 0 || t.name.IndexOf("Jamb") >= 0 || t.name.IndexOf("Lintel") >= 0)
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
                if (!kit && !light)
                    continue;
                if (t.position.y >= target - 0.05f)
                    continue;
                if (t.position.y < 2.35f)
                    continue;
                t.position = new Vector3(t.position.x, target, t.position.z);
            }
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
                    p = new Vector3(-5.9f, y, 1.7f);
                Put("Cable_Small", p, -18f, _rust);
            }
        }

        // Chatarra que reemplaza a los escombros del greybox: parada (latas, balde), acostada (chapas, caños) o grande
        private static readonly string[] JunkSmall = { "Gas_Canister", "bucket", "piston-thin-round", "pipe-end" };
        private static readonly string[] JunkFlat = { "metal-panel-screws", "pipe-large-long", "metal-panel-narrow", "container-flat" };
        private static readonly string[] JunkBig = { "container-flat-open", "pipe-large-bend" };
        private const float JunkBigFoot = 0.9f;
        private const float JunkSmallHeight = 0.42f;

        // Cada Debris_ del greybox pasa a un modelo del mismo tamaño en planta; su collider queda como bloqueo
        private void DressDebris()
        {
            var scene = gameObject.scene;
            var list = new System.Collections.Generic.List<GameObject>();
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.gameObject.scene == scene && t.name.StartsWith("Debris_"))
                    list.Add(t.gameObject);
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            int small = 0, flat = 0, big = 0;
            foreach (var src in list)
            {
                string name = "Kit_Junk_" + src.name;
                var rend = src.GetComponent<MeshRenderer>();
                var filter = src.GetComponent<MeshFilter>();
                if (FindLocal(name) != null || rend == null)
                {
                    HideSelf(src);
                    continue;
                }
                // Algunos greybox flotan: baja junto con su collider a lo que tenga debajo
                float drop = GapBelow(src, rend.bounds);
                if (drop > 0.005f)
                    src.transform.position -= Vector3.up * drop;
                var b = rend.bounds;
                float foot = Mathf.Max(b.size.x, b.size.z);
                bool round = filter != null && filter.sharedMesh != null && filter.sharedMesh.name.StartsWith("Capsule");
                string kit;
                bool lay = false;
                if (round)
                    kit = JunkSmall[small++ % JunkSmall.Length];
                else if (foot > JunkBigFoot)
                    kit = JunkBig[big++ % JunkBig.Length];
                else
                {
                    kit = JunkFlat[flat++ % JunkFlat.Length];
                    lay = kit.StartsWith("metal-panel") || kit.StartsWith("pipe");
                }

                var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
                if (prefab == null)
                    continue;
                var go = Object.Instantiate(prefab, _root);
                go.name = name;
                go.transform.rotation = prefab.transform.rotation;
                StripColliders(go);
                if (!kit.StartsWith("container") && kit != "bucket" && kit != "Gas_Canister")
                    Paint(go, _rust);
                if (lay)
                    LayFlat(go);
                go.transform.Rotate(0f, src.transform.eulerAngles.y, 0f, Space.World);
                var mb = Encapsulate(go);
                float side = Mathf.Max(mb.size.x, mb.size.z);
                float k = side > 0.01f ? foot / side : 1f;
                if (round && mb.size.y * k > JunkSmallHeight)
                    k = JunkSmallHeight / mb.size.y;
                go.transform.localScale *= k;
                mb = Encapsulate(go);
                go.transform.position += new Vector3(b.center.x - mb.center.x, b.min.y - mb.min.y, b.center.z - mb.center.z);
                // Una chapa acostada es más baja que el bloque: el choque toma la forma de lo que se ve
                if (src.GetComponent<BoxCollider>() != null)
                    FitColliderTo(src, go);
                HideSelf(src);
            }
        }

        // Distancia entre la base y lo que hay justo abajo (sin contarse a sí mismo)
        private static float GapBelow(GameObject src, Bounds b)
        {
            Physics.SyncTransforms();
            float support = float.MinValue;
            foreach (var h in Physics.RaycastAll(new Vector3(b.center.x, b.min.y + 0.02f, b.center.z), Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore))
                if (!h.collider.transform.IsChildOf(src.transform))
                    support = Mathf.Max(support, h.point.y);
            return support == float.MinValue ? 0f : b.min.y - support;
        }

        // El lado más fino del modelo queda vertical: chapas y caños tirados en el piso
        private static void LayFlat(GameObject go)
        {
            var b = Encapsulate(go);
            if (b.size.y <= b.size.x && b.size.y <= b.size.z)
                return;
            if (b.size.z <= b.size.x)
                go.transform.Rotate(90f, 0f, 0f, Space.World);
            else
                go.transform.Rotate(0f, 0f, 90f, Space.World);
        }

        private void ScatterBoxes(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                PutNamed("cardboardBoxClosed", "Kit_Box_N1a", new Vector3(8.15f, 0.02f, -6.05f), 15f, _wood, 0.48f);
                PutNamed("cardboardBoxClosed", "Kit_Box_N1b", new Vector3(8.45f, 0.02f, -5.55f), -22f, _wood, 0.38f);
                PutNamed("radio", "Kit_radio_N1", new Vector3(-4.15f, 0.72f, -0.95f), 35f, _wood, 0.16f);
                PutNamed("books", "Kit_books_N1", new Vector3(5.2f, 0.82f, 3.03f), 12f, _wood, 0.14f);
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
                PutNamed("radio", "Kit_radio_N3", new Vector3(4.05f, 0.62f, -1.35f), -8f, _wood, 0.14f);
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

    }
}
