using Enigma.Interaction;
using UnityEngine;

namespace Enigma.Environment
{
    // Muebles y objetos armados donde el greybox era un cubo; cada pieza se mide contra lo que tiene alrededor
    public partial class KitbashDressing
    {
        private readonly System.Collections.Generic.List<GameObject> _built = new System.Collections.Generic.List<GameObject>();

        private void DressGreyProps(string sceneName)
        {
            if (Application.isPlaying)
                return;
            _built.Clear();
            BuildGreyProps(sceneName);
            foreach (var go in _built)
                ClearDecor(go);
            _built.Clear();
        }

        // Lo decorativo suelto que quedó metido en un modelo nuevo se corre hacia afuera por el lado más corto
        private void ClearDecor(GameObject model)
        {
            if (model == null || !VisibleBounds(model, out var mb))
                return;
            for (int i = 0; i < _root.childCount; i++)
            {
                var c = _root.GetChild(i).gameObject;
                if (c == model || !(c.name.StartsWith("Kit_Junk") || c.name.StartsWith("Kit_Table")) || !VisibleBounds(c, out var cb) || !mb.Intersects(cb))
                    continue;
                float right = mb.max.x - cb.min.x, left = cb.max.x - mb.min.x;
                float fwd = mb.max.z - cb.min.z, back = cb.max.z - mb.min.z;
                float best = Mathf.Min(Mathf.Min(right, left), Mathf.Min(fwd, back));
                Vector3 push = best == right ? Vector3.right * right : best == left ? Vector3.left * left : best == fwd ? Vector3.forward * fwd : Vector3.back * back;
                push += push.normalized * 0.03f;
                c.transform.position += push;
                var solid = FindLocal(c.name + "_Solid");
                if (solid != null)
                    solid.transform.position += push;
            }
        }

        private void BuildGreyProps(string sceneName)
        {
            if (sceneName == "Level01_Room")
            {
                DressRoomDesk();
                DressRoomShelves();
                DressFallenShelf();
                DressLever();
            }
            else if (sceneName == "Level02_Lab")
            {
                DressLabDesk();
                DressBottle("Bottle_A");
                DressBottle("Bottle_B");
                DressCalendar();
                DressNotebook();
                DressCrate("Crate_A");
                for (int i = 0; i < 4; i++)
                    DressCrate("Crate_L" + i);
                DressMachine();
            }
            else if (sceneName == "Level03_Archives")
            {
                var left = DressCabinetRow("CabinetRow_L", "N3L", true);
                var right = DressCabinetRow("CabinetRow_R", "N3R", false);
                // Dos letras de cada lado, entreveradas con registros del personal
                if (left.HasValue)
                {
                    SpillDocs(new[] { "Doc_084", "Doc_StaffA", "Doc_117", "Doc_Staff204" }, left.Value, LeftSpill, 0, 0f);
                    DropFlat("Binder", "Kit_Binder_N3a", left.Value + new Vector3(-1.3f, 0f, -0.5f), 30f, 0.32f);
                    DropFlat("Binder", "Kit_Binder_N3b", left.Value + new Vector3(1.25f, 0f, -0.85f), -65f, 0.32f);
                }
                if (right.HasValue)
                {
                    SpillDocs(new[] { "Doc_StaffB", "Doc_129", "Doc_Staff731", "Doc_206" }, right.Value, RightSpill, 4, 15f);
                    DropFlat("Binder", "Kit_Binder_N3c", right.Value + new Vector3(1.35f, 0f, -0.8f), 110f, 0.32f);
                }
                DressPrinterTray();
            }
            else if (sceneName == "Level03_Mezzanine")
            {
                DressPaintingFrame();
                DressCredentialTape();
                DressCardReader();
                DressMagnetHousing();
            }
            else if (sceneName == "Level04_Origen")
            {
                DressTapeLocker();
                DressCassette();
            }
        }

        // Caja en mundo de un cubo del greybox, aunque tenga el renderer apagado
        private static Bounds GreyBox(GameObject go)
        {
            var mf = go.GetComponent<MeshFilter>();
            var lb = mf != null && mf.sharedMesh != null ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
            var t = go.transform;
            var b = new Bounds(t.TransformPoint(lb.center), Vector3.zero);
            for (int c = 0; c < 8; c++)
                b.Encapsulate(t.TransformPoint(lb.center + Vector3.Scale(lb.extents, new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f))));
            return b;
        }

        private GameObject NewKit(string kit, string name)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return null;
            var go = Object.Instantiate(prefab, _root);
            go.name = name;
            go.transform.rotation = prefab.transform.rotation;
            StripColliders(go);
            _built.Add(go);
            return go;
        }

        private GameObject NewRoot(string name, Vector3 pos, Quaternion rot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.SetPositionAndRotation(pos, rot);
            _built.Add(go);
            return go;
        }

        // Pieza con medidas de mundo bajo un padre escalado; las medidas siguen los ejes del padre
        private static GameObject PartIn(Transform parent, string name, Vector3 world, Vector3 size, Material mat, PrimitiveType type = PrimitiveType.Cube)
        {
            var s = parent.lossyScale;
            var local = new Vector3(size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x)), size.y / Mathf.Max(0.0001f, Mathf.Abs(s.y)), size.z / Mathf.Max(0.0001f, Mathf.Abs(s.z)));
            return Part(parent, name, parent.InverseTransformPoint(world), local, mat, type);
        }

        // Altura del apoyo debajo de p (piso, mueble o mancha), sin triggers ni sólidos armados
        private static float FloorBelow(Vector3 p, Transform skip = null)
        {
            Physics.SyncTransforms();
            float best = float.MinValue;
            foreach (var hit in Physics.RaycastAll(p + Vector3.up * 0.5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (skip != null && hit.collider.transform.IsChildOf(skip))
                    continue;
                if (hit.collider.name.EndsWith("_Solid") || hit.point.y > p.y + 0.05f)
                    continue;
                best = Mathf.Max(best, hit.point.y);
            }
            return best == float.MinValue ? p.y : best;
        }

        private static void DestroyNow(GameObject go)
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }

        // ---------- N1 ----------

        // Escritorio del cuarto: el cajón del fusible ocupa el hueco del cajón del modelo, con el frente a la silla
        private void DressRoomDesk()
        {
            var grey = FindLocal("Decor_Desk");
            var drawer = FindLocal("Interactable_Drawer");
            if (grey == null || drawer == null)
                return;
            var g = GreyBox(grey);
            var desk = NewKit("desk", "Kit_Desk_N1");
            if (desk == null)
                return;
            // El largo del modelo va en X: se mide derecho y después se gira para que el frente mire a +X
            FitToBox(desk, g.min.y, g.size.z, g.size.y, g.size.x);
            desk.transform.rotation = Quaternion.AngleAxis(-90f, Vector3.up) * desk.transform.rotation;
            var b = Encapsulate(desk);
            desk.transform.position += new Vector3(g.center.x - b.center.x, g.min.y - b.min.y, g.center.z - b.center.z);
            PaintSlots(desk, new[] { Mat("Mat_DarkWood"), Mat("Mat_Steel") });

            var kitDrawer = FindChildNamed(desk.transform, "drawer");
            var kitBody = FindChildNamed(desk.transform, "desk");
            if (kitDrawer == null || kitBody == null)
                return;
            var db = Encapsulate(kitDrawer.gameObject);
            HideSelf(kitDrawer.gameObject);
            float front = FrontAlongX(kitBody.gameObject, db, db.max.x);
            float depth = Mathf.Max(0.12f, front - db.min.x);
            float h = db.size.y;
            float w = db.size.z;

            var t = drawer.transform;
            t.SetPositionAndRotation(new Vector3(front - depth * 0.5f, db.center.y, db.center.z), Quaternion.identity);
            t.localScale = Vector3.one;
            foreach (var box in drawer.GetComponents<BoxCollider>())
            {
                box.center = new Vector3(0.02f, 0f, 0f);
                box.size = new Vector3(depth + 0.04f, h + 0.03f, w + 0.03f);
            }
            var slide = FindChildNamed(t, "DrawerSlide");
            if (slide == null)
                return;
            slide.localPosition = Vector3.zero;
            slide.localRotation = Quaternion.identity;
            slide.localScale = Vector3.one;

            // Frente y manija viajan con el cajón; el frente tapa el hueco con un margen
            var face = FindChildNamed(t, "DrawerFace");
            if (face != null)
            {
                face.SetParent(slide, false);
                face.localRotation = Quaternion.identity;
                face.localPosition = new Vector3(depth * 0.5f + 0.01f, 0f, 0f);
                face.localScale = new Vector3(0.02f, h + 0.03f, w + 0.03f);
            }
            var handle = FindChildNamed(t, "DrawerHandle");
            if (handle != null)
            {
                handle.SetParent(slide, false);
                handle.localRotation = Quaternion.identity;
                handle.localPosition = new Vector3(depth * 0.5f + 0.032f, 0f, 0f);
                handle.localScale = new Vector3(0.024f, 0.014f, 0.1f);
            }
            float inner = h - 0.012f;
            var body = FindChildNamed(t, "DrawerBody");
            if (body != null)
            {
                var mf = body.GetComponent<MeshFilter>();
                body.localRotation = Quaternion.identity;
                if (mf != null && mf.sharedMesh != null)
                {
                    var mb = mf.sharedMesh.bounds;
                    var size = new Vector3(depth - 0.01f, inner, w - 0.012f);
                    body.localScale = new Vector3(size.x / Mathf.Max(0.001f, mb.size.x), size.y / Mathf.Max(0.001f, mb.size.y), size.z / Mathf.Max(0.001f, mb.size.z));
                    body.localPosition = -Vector3.Scale(mb.center, body.localScale) - new Vector3(0.005f, 0f, 0f);
                }
            }
            var fuse = FindChildNamed(t, "Interactable_Fuse");
            if (fuse != null)
            {
                fuse.localRotation = Quaternion.Euler(0f, 180f, 90f);
                fuse.localScale = new Vector3(0.03f, 0.15f, 0.1f);
                fuse.localPosition = new Vector3(-0.02f, -inner * 0.5f + 0.026f, 0f);
            }
            var cont = drawer.GetComponent<ContainerInteractable>();
            if (cont != null)
                cont.SetOpenPosition(new Vector3(depth * 0.8f, 0f, 0f));

            var pcGrey = FindChildNamed(grey.transform, "DecorBrokenPC");
            if (pcGrey != null)
                DressDeskTower(GreyBox(pcGrey.gameObject), Encapsulate(desk).max.y);
            FitColliderTo(grey, desk);
            HideVisual(grey);
        }

        // Frente del mueble a los costados del hueco del cajón (el primer impacto viniendo desde +X)
        private static float FrontAlongX(GameObject mesh, Bounds hole, float fallback)
        {
            var mf = mesh.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                return fallback;
            var mc = mesh.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            Physics.SyncTransforms();
            float x = float.MinValue;
            foreach (float z in new[] { hole.min.z - 0.03f, hole.max.z + 0.03f })
            {
                if (mc.Raycast(new Ray(new Vector3(hole.max.x + 0.3f, hole.center.y, z), Vector3.left), out var hit, 2f))
                    x = Mathf.Max(x, hit.point.x);
            }
            Object.DestroyImmediate(mc);
            return x == float.MinValue ? fallback : x;
        }

        // Gabinete de PC sobre el escritorio, del alto del cubo que había
        private void DressDeskTower(Bounds box, float top)
        {
            var pc = NewKit("Computer", "Kit_DeskPC_N1");
            if (pc == null)
                return;
            var unit = FindChildNamed(pc.transform, "System_unit");
            if (unit == null)
            {
                DestroyNow(pc);
                return;
            }
            foreach (var r in pc.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r == null || r.transform.IsChildOf(unit) || unit.IsChildOf(r.transform))
                    continue;
                DestroyNow(r.gameObject);
            }
            FitToHeight(pc, top, box.size.y);
            var b = Encapsulate(pc);
            if ((b.size.x > b.size.z) != (box.size.x > box.size.z))
                pc.transform.Rotate(0f, 90f, 0f, Space.World);
            b = Encapsulate(pc);
            pc.transform.position += new Vector3(box.center.x - b.center.x, top - b.min.y, box.center.z - b.center.z);
        }

        // Dos bibliotecas angostas lado a lado en la huella del cubo del estante
        private void DressRoomShelves()
        {
            var grey = FindLocal("Decor_Shelf");
            if (grey == null)
                return;
            var g = GreyBox(grey);
            Bounds all = default;
            for (int i = 0; i < 2; i++)
            {
                // La segunda no queda gemela: un poco girada y corrida
                float z = g.center.z + (i == 0 ? -0.27f : 0.30f);
                var go = PlaceProp("Shelf_2", "Kit_Shelf_N1" + (char)('a' + i), new Vector3(g.center.x, g.min.y, z), i == 0 ? 90f : 93f, g.size.y, Vector3.zero, Mat("Mat_DarkWood"));
                if (go == null)
                    return;
                var b = Encapsulate(go);
                if (i == 0)
                    all = b;
                else
                    all.Encapsulate(b);
            }
            FitColliderToBounds(grey, all);
            HideVisual(grey);
        }

        // Biblioteca caída de espaldas en el piso, girada en planta
        private void DressFallenShelf()
        {
            var grey = FindLocal("Decor_FallenFurniture");
            if (grey == null)
                return;
            var g = GreyBox(grey);
            var go = NewKit("Shelf_2", "Kit_FallenShelf_N1");
            if (go == null)
                return;
            Paint(go, Mat("Mat_DarkWood"));
            FitToHeight(go, 0f, 1.6f);
            go.transform.Rotate(90f, 0f, 0f, Space.World);
            go.transform.Rotate(0f, 25f, 0f, Space.World);
            float floor = FloorBelow(new Vector3(g.center.x, 0.3f, g.center.z), grey.transform);
            var b = Encapsulate(go);
            go.transform.position += new Vector3(g.center.x - b.center.x, floor - b.min.y, g.center.z - b.center.z);
            grey.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            FitColliderTo(grey, go);
            HideVisual(grey);
        }

        // Palanca de acero con mango de goma en la punta
        private void DressLever()
        {
            var steel = Mat("Mat_Steel");
            var plate = FindLocal("LeverPlate");
            if (plate != null)
                Paint(plate, steel);
            var handle = FindLocal("LeverHandle");
            if (handle == null)
                return;
            Paint(handle, steel);
            var s = handle.transform.lossyScale;
            int axis = s.x >= s.y && s.x >= s.z ? 0 : (s.y >= s.z ? 1 : 2);
            var pos = Vector3.zero;
            pos[axis] = 0.35f;
            var size = Vector3.one * 1.3f;
            size[axis] = 0.3f;
            Part(handle.transform, "Kit_LeverGrip", pos, size, Mat("Mat_Rubber"));
        }

        // ---------- N2 ----------

        private void DressLabDesk()
        {
            var grey = FindLocal("Desk");
            if (grey == null)
                return;
            var g = GreyBox(grey);
            var desk = NewKit("desk", "Kit_Desk_N2");
            if (desk == null)
                return;
            FitToBox(desk, g.min.y, g.size.x, g.size.y, g.size.z);
            var b = Encapsulate(desk);
            desk.transform.position += new Vector3(g.center.x - b.center.x, g.min.y - b.min.y, g.center.z - b.center.z);
            PaintSlots(desk, new[] { Mat("Mat_DarkWood"), Mat("Mat_Steel") });
            FitColliderTo(grey, desk);
            HideVisual(grey);
        }

        // Botella de vidrio ámbar del alto del cubo: cuerpo, hombro, cuello y tapa
        private void DressBottle(string name)
        {
            var grey = FindLocal(name);
            if (grey == null)
                return;
            var g = GreyBox(grey);
            var t = NewRoot("Kit_" + name, new Vector3(g.center.x, g.min.y, g.center.z), Quaternion.identity).transform;
            var glass = Mat("Mat_AmberGlass");
            float d = Mathf.Min(g.size.x, g.size.z) * 0.92f;
            float h = g.size.y;
            float body = h * 0.6f;
            float neck = h * 0.28f;
            Part(t, "Kit_BottleBody", new Vector3(0f, body * 0.5f, 0f), new Vector3(d, body * 0.5f, d), glass, PrimitiveType.Cylinder);
            Part(t, "Kit_BottleShoulder", new Vector3(0f, body, 0f), new Vector3(d, d * 0.6f, d), glass, PrimitiveType.Sphere);
            Part(t, "Kit_BottleNeck", new Vector3(0f, body + d * 0.1f + neck * 0.5f, 0f), new Vector3(d * 0.34f, neck * 0.5f, d * 0.34f), glass, PrimitiveType.Cylinder);
            Part(t, "Kit_BottleCap", new Vector3(0f, h - 0.01f, 0f), new Vector3(d * 0.4f, 0.01f, d * 0.4f), Mat("Mat_Steel"), PrimitiveType.Cylinder);
            HideVisual(grey);
        }

        // Calendario de escritorio con atril: hoja con la textura, cartón detrás, espiral arriba y pata
        private void DressCalendar()
        {
            var cal = FindLocal("Interactable_Calendar");
            if (cal == null)
                return;
            var rend = cal.GetComponent<MeshRenderer>();
            var sheet = rend != null ? rend.sharedMaterial : null;
            var desk = FindLocal("Kit_Desk_N2");
            var g = GreyBox(cal);
            float top = desk != null ? Encapsulate(desk).max.y : g.min.y;
            float z = desk != null ? Mathf.Min(cal.transform.position.z, Encapsulate(desk).max.z - 0.3f) : cal.transform.position.z;
            const float w = 0.36f;
            const float h = 0.47f;
            const float tilt = 14f;
            var root = NewRoot("Kit_DeskCalendar", new Vector3(cal.transform.position.x, top, z), Quaternion.identity).transform;
            var face = new GameObject("Kit_CalendarFace").transform;
            face.SetParent(root, false);
            face.localRotation = Quaternion.Euler(tilt, 0f, 0f);
            Part(face, "Kit_CalendarSheet", new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, 0.003f), sheet);
            Part(face, "Kit_CalendarBoard", new Vector3(0f, h * 0.5f, 0.0035f), new Vector3(w, h, 0.004f), Mat("Mat_Kraft"));
            var steel = Mat("Mat_Steel");
            for (int i = 0; i < 9; i++)
            {
                var ring = Part(face, "Kit_CalendarRing", new Vector3(-w * 0.4f + i * w * 0.1f, h, 0.001f), new Vector3(0.012f, 0.002f, 0.012f), steel, PrimitiveType.Cylinder);
                ring.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            // Pata de cartón desde arriba de la hoja hasta el escritorio
            Vector3 a = new Vector3(0f, h * Mathf.Cos(tilt * Mathf.Deg2Rad) - 0.02f, h * Mathf.Sin(tilt * Mathf.Deg2Rad) + 0.006f);
            Vector3 foot = new Vector3(0f, 0f, a.z + 0.15f);
            Vector3 dir = a - foot;
            var leg = Part(root, "Kit_CalendarLeg", (a + foot) * 0.5f, new Vector3(w * 0.8f, dir.magnitude, 0.004f), Mat("Mat_Kraft"));
            leg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
            HideSelf(cal);
        }

        // Cuaderno cerrado con espiral, a escala real
        private void DressNotebook()
        {
            var nb = FindLocal("Interactable_Notebook");
            if (nb == null)
                return;
            var g = GreyBox(nb);
            var t = NewRoot("Kit_Notebook_N2", new Vector3(g.center.x, g.min.y, g.center.z), Quaternion.Euler(0f, 12f, 0f)).transform;
            var cover = Mat("Mat_Leather");
            Part(t, "Kit_NotebookBack", new Vector3(0f, 0.0015f, 0f), new Vector3(0.3f, 0.003f, 0.22f), cover);
            Part(t, "Kit_NotebookPages", new Vector3(0.006f, 0.011f, 0f), new Vector3(0.286f, 0.016f, 0.21f), Mat("Mat_PrintPaper"));
            Part(t, "Kit_NotebookCover", new Vector3(0f, 0.0205f, 0f), new Vector3(0.3f, 0.003f, 0.22f), cover);
            var steel = Mat("Mat_Steel");
            for (int i = 0; i < 10; i++)
            {
                var ring = Part(t, "Kit_NotebookRing", new Vector3(-0.152f, 0.011f, -0.095f + i * 0.021f), new Vector3(0.026f, 0.002f, 0.026f), steel, PrimitiveType.Cylinder);
                ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            HideSelf(nb);
        }

        // Cajón de madera de listones: postes en las aristas, tablas con luz entre ellas y tapa
        private void DressCrate(string name)
        {
            var grey = FindLocal(name);
            if (grey == null)
                return;
            var gt = grey.transform;
            Vector3 size = gt.lossyScale;
            var t = NewRoot("Kit_WoodCrate_" + name, gt.position - gt.up * size.y * 0.5f, Quaternion.Euler(0f, gt.eulerAngles.y, 0f)).transform;
            float X = size.x, Y = size.y, Z = size.z;
            const float post = 0.05f;
            const float plank = 0.02f;
            var frame = Mat("Mat_DarkWood");
            for (int i = 0; i < 4; i++)
            {
                float x = ((i & 1) == 0 ? -1f : 1f) * (X - post) * 0.5f;
                float z = ((i & 2) == 0 ? -1f : 1f) * (Z - post) * 0.5f;
                Part(t, "Kit_CratePost", new Vector3(x, Y * 0.5f, z), new Vector3(post, Y, post), frame);
            }
            for (int k = 0; k < 2; k++)
            {
                float y = k == 0 ? post * 0.5f : Y - post * 0.5f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Part(t, "Kit_CrateRail", new Vector3(0f, y, s * (Z - post) * 0.5f), new Vector3(X - post * 2f, post, post), frame);
                    Part(t, "Kit_CrateRail", new Vector3(s * (X - post) * 0.5f, y, 0f), new Vector3(post, post, Z - post * 2f), frame);
                }
            }
            const int boards = 3;
            float span = Y - post * 2f;
            float bh = span / boards - 0.012f;
            for (int k = 0; k < boards; k++)
            {
                float y = post + (k + 0.5f) * span / boards;
                for (int s = -1; s <= 1; s += 2)
                {
                    Part(t, "Kit_CrateBoard", new Vector3(0f, y, s * (Z * 0.5f - 0.01f - plank * 0.5f)), new Vector3(X - post * 2f, bh, plank), _wood);
                    Part(t, "Kit_CrateBoard", new Vector3(s * (X * 0.5f - 0.01f - plank * 0.5f), y, 0f), new Vector3(plank, bh, Z - post * 2f), _wood);
                }
            }
            const int lids = 4;
            float lw = (X - post * 2f) / lids;
            for (int k = 0; k < lids; k++)
                Part(t, "Kit_CrateLid", new Vector3(-X * 0.5f + post + (k + 0.5f) * lw, Y - plank * 0.5f, 0f), new Vector3(lw - 0.01f, plank, Z - post * 2f), _wood);
            HideVisual(grey);
        }

        // La máquina rota pasa a ser un tablero eléctrico del alto del cubo, con el lado largo como el greybox
        private void DressMachine()
        {
            var grey = FindLocal("MachineBroken");
            if (grey == null)
                return;
            var g = GreyBox(grey);
            var go = NewKit("ElectricalBox02", "Kit_Machine_N2");
            if (go == null)
                return;
            FitToHeight(go, g.min.y, g.size.y);
            var b = Encapsulate(go);
            if ((b.size.x > b.size.z) != (g.size.x > g.size.z))
                go.transform.Rotate(0f, 90f, 0f, Space.World);
            b = Encapsulate(go);
            go.transform.position += new Vector3(g.center.x - b.center.x, g.min.y - b.min.y, g.center.z - b.center.z);
            FitColliderTo(grey, go);
            HideVisual(grey);
        }

        // ---------- N3 ----------

        // Expedientes delante del cajón: lado a lo ancho y distancia hacia la sala (-Z)
        private static readonly Vector2[] LeftSpill = { new Vector2(0f, 0.35f), new Vector2(-0.55f, 0.6f), new Vector2(0.55f, 0.65f), new Vector2(0.05f, 1.0f) };
        private static readonly Vector2[] RightSpill = { new Vector2(-0.7f, 0.4f), new Vector2(0.7f, 0.35f), new Vector2(-0.25f, 0.9f), new Vector2(0.45f, 1.0f) };
        private static readonly float[] SpillYaws = { -18f, 34f, -52f, 12f };

        // Grosor de una carpeta en el piso
        private const float PileSheet = 0.006f;
        private const float PileReach = 0.04f;

        // Tres archiveros por fila; devuelve el punto del piso desde donde se desparraman los expedientes
        private Vector3? DressCabinetRow(string rowName, string tag, bool openDrawer)
        {
            var row = FindLocal(rowName);
            if (row == null)
                return null;
            var g = GreyBox(row);
            float floor = g.min.y;
            float backZ = g.max.z;
            var cabinets = new GameObject[3];
            Bounds bank = default;
            for (int i = 0; i < 3; i++)
            {
                var go = PlaceProp("FileCabinet", "Kit_Furniture_FileCabinet_" + tag + (char)('a' + i), new Vector3(g.center.x, floor, backZ), 0f, CabinetHeight, Vector3.zero);
                if (go == null)
                    return null;
                var body = FindChildNamed(go.transform, "Main_");
                var top = FindChildNamed(go.transform, "Draw");
                if (body == null || top == null)
                    return null;
                Vector3 outward = Encapsulate(top.gameObject).center - Encapsulate(body.gameObject).center;
                outward.y = 0f;
                go.transform.Rotate(0f, Vector3.SignedAngle(outward, Vector3.back, Vector3.up), 0f, Space.World);
                var bb = Encapsulate(body.gameObject);
                go.transform.position += new Vector3(g.center.x + (i - 1) * (bb.size.x + 0.01f) - bb.center.x, 0f, backZ - bb.max.z);
                bb = Encapsulate(body.gameObject);
                if (i == 0)
                    bank = bb;
                else
                    bank.Encapsulate(bb);
                cabinets[i] = go;
            }

            var mid = cabinets[1].transform;
            Vector3 origin;
            if (openDrawer)
            {
                // El cajón del medio quedó abierto
                var d = FindChildNamed(mid, "Draw.001");
                if (d == null)
                    return null;
                d.position += Vector3.back * 0.35f;
                var db = Encapsulate(d.gameObject);
                origin = new Vector3(db.center.x, floor, db.min.z);
            }
            else
            {
                // Un cajón salió entero y quedó boca abajo delante del archivero
                var d = FindChildNamed(mid, "Draw.002");
                if (d == null)
                    return null;
                var db = Encapsulate(d.gameObject);
                SlotLiner(db, tag);
                float cabFront = Encapsulate(mid.gameObject).min.z;
                d.SetParent(_root, true);
                d.name = "Kit_FallenDrawer_" + tag;
                d.Rotate(0f, 0f, 180f, Space.World);
                d.Rotate(0f, 20f, 0f, Space.World);
                var fb = Encapsulate(d.gameObject);
                Vector3 at = new Vector3(db.center.x, 0f, cabFront - 0.12f - fb.extents.z);
                float support = float.MinValue;
                for (int c = 0; c < 4; c++)
                {
                    var p = new Vector3(at.x + ((c & 1) == 0 ? -1f : 1f) * fb.extents.x * 0.8f, 0.4f, at.z + ((c & 2) == 0 ? -1f : 1f) * fb.extents.z * 0.8f);
                    support = Mathf.Max(support, FloorBelow(p, d));
                }
                d.position += new Vector3(at.x - fb.center.x, support - fb.min.y, at.z - fb.center.z);
                SolidShell(d.gameObject);
                origin = Encapsulate(d.gameObject).center;
                origin.y = floor;
            }
            for (int i = 0; i < 3; i++)
                SolidShell(cabinets[i]);
            HideVisual(row);
            FitColliderToBounds(row, bank);
            return origin;
        }

        // Fondo oscuro del hueco que dejó el cajón
        private void SlotLiner(Bounds slot, string tag)
        {
            var t = NewRoot("Kit_DrawerSlot_" + tag, slot.center, Quaternion.identity).transform;
            var dark = Mat("Mat_Rubber");
            Vector3 s = slot.size - Vector3.one * 0.02f;
            const float th = 0.008f;
            Part(t, "Kit_SlotBack", new Vector3(0f, 0f, s.z * 0.5f), new Vector3(s.x, s.y, th), dark);
            Part(t, "Kit_SlotFloor", new Vector3(0f, -s.y * 0.5f, 0f), new Vector3(s.x, th, s.z), dark);
            Part(t, "Kit_SlotTop", new Vector3(0f, s.y * 0.5f, 0f), new Vector3(s.x, th, s.z), dark);
            Part(t, "Kit_SlotSideL", new Vector3(-s.x * 0.5f, 0f, 0f), new Vector3(th, s.y, s.z), dark);
            Part(t, "Kit_SlotSideR", new Vector3(s.x * 0.5f, 0f, 0f), new Vector3(th, s.y, s.z), dark);
        }

        // Expedientes desparramados: separados para que se cuenten de un vistazo y cada uno con su tinte
        private void SpillDocs(string[] names, Vector3 origin, Vector2[] spots, int tint, float yaw)
        {
            for (int i = 0; i < names.Length && i < spots.Length; i++)
            {
                var go = FindLocal(names[i]);
                if (go == null)
                    continue;
                var t = go.transform;
                Vector3 at = origin + new Vector3(spots[i].x, 0f, -spots[i].y);
                float floor = FloorBelow(new Vector3(at.x, 0.3f, at.z), t);
                Vector3 s = t.localScale;
                t.localScale = new Vector3(s.x, PileSheet, s.z);
                // El trigger queda más alto que la hoja para que se pueda agarrar del piso
                var box = go.GetComponent<BoxCollider>();
                if (box != null && box.isTrigger)
                {
                    box.size = new Vector3(1f, PileReach / PileSheet, 1f);
                    box.center = Vector3.zero;
                }
                t.SetPositionAndRotation(new Vector3(at.x, floor + PileSheet * 0.5f + 0.001f, at.z), Quaternion.Euler(0f, SpillYaws[i] + yaw, 0f));
                var kit = t.Find("Kit_UI_Paper");
                if (kit != null)
                    Paint(kit.gameObject, Mat("Mat_DocTint_" + (tint + i)));
            }
        }

        // Impresora con la bandeja hacia la sala; las hojas se graban en la bandeja y salen de adentro al prenderse
        private void DressPrinterTray()
        {
            var kit = FindLocal("Kit_Printer");
            var desk = FindLocal("ArchiveDesk");
            if (kit == null || desk == null)
                return;
            if (InputSide(kit) > 0f)
                kit.transform.RotateAround(Encapsulate(kit).center, Vector3.up, 180f);
            var d = GreyBox(desk);
            var pb = Encapsulate(kit);
            kit.transform.position += Vector3.forward * (d.min.z + 0.03f - pb.min.z);
            pb = Encapsulate(kit);
            var printer = FindLocal("Printer");
            if (printer != null)
                printer.transform.position = pb.center;
            float top = d.max.y;
            Vector3 feed = Vector3.forward;
            var tray = NewRoot("Kit_PrinterTray", new Vector3(pb.center.x, top, pb.max.z), Quaternion.identity).transform;
            Part(tray, "Kit_PrinterTrayBed", new Vector3(0f, 0.002f, 0.1f), new Vector3(0.23f, 0.004f, 0.2f), Mat("Mat_DarkPlastic"));

            Vector3 slot = new Vector3(pb.center.x, pb.min.y + pb.size.y * 0.55f, pb.center.z);
            Vector3 rest = new Vector3(pb.center.x, top + 0.004f, pb.max.z + 0.005f + ItemVisuals.WorldSizeOf("print_") * 0.5f);
            int layer = LayerMask.NameToLayer("Interactable");
            foreach (var name in new[] { "Print_10", "Print_109", "Print_3" })
            {
                var go = FindLocal(name);
                if (go == null)
                    continue;
                bool was = go.activeSelf;
                go.SetActive(true);
                string id = "print_" + name.Substring(6);
                ItemVisuals.Dress(go, id);
                // Al regrabar el modelo ya existe y Dress no apaga el cubo que RestoreHidden volvió a prender
                HideSelf(go);
                // El número se lee parado delante de la bandeja
                go.transform.rotation = Quaternion.LookRotation(-feed);
                var vis = go.transform.Find("Visual_" + id);
                if (vis != null)
                {
                    var vb = ItemVisuals.Bounds(vis.gameObject);
                    go.transform.position += new Vector3(rest.x - vb.center.x, rest.y - vb.min.y, rest.z - vb.center.z);
                    var box = go.GetComponent<BoxCollider>();
                    if (box == null)
                        box = go.AddComponent<BoxCollider>();
                    if (LocalBounds(go.transform, vis.gameObject, out var lb))
                    {
                        box.isTrigger = true;
                        box.center = lb.center + new Vector3(0f, 0.03f, 0f);
                        box.size = lb.size + new Vector3(0.03f, 0.06f, 0.03f);
                    }
                }
                if (layer >= 0)
                    go.layer = layer;
                var feeder = go.GetComponent<PrintFeed>();
                if (feeder == null)
                    feeder = go.AddComponent<PrintFeed>();
                feeder.Configure(slot, feed);
                go.SetActive(was);
            }
        }

        // Lado de la impresora donde está la tapa alta de entrada de papel (+1 = +Z)
        private static float InputSide(GameObject kit)
        {
            var b = Encapsulate(kit);
            float sum = 0f;
            int n = 0;
            foreach (var mf in kit.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null)
                    continue;
                foreach (var v in mf.sharedMesh.vertices)
                {
                    var w = mf.transform.TransformPoint(v);
                    if (w.y < b.min.y + b.size.y * 0.75f)
                        continue;
                    sum += w.z - b.center.z;
                    n++;
                }
            }
            return n == 0 ? -1f : sum / n;
        }

        // ---------- Mezzanine ----------

        // El cuadro es el mismo modelo que se ve en el inventario, colgado a 1 cm de la pared
        private void DressPaintingFrame()
        {
            var paint = FindLocal("Interactable_Painting");
            if (paint == null)
                return;
            var vis = ItemVisuals.Build("painting");
            if (vis == null)
                return;
            vis.name = "Kit_PaintingFrame";
            var b = ItemVisuals.Bounds(vis);
            vis.transform.localScale *= 1.6f / Mathf.Max(0.01f, b.size.x);
            Vector3 into = IntoRoomFrom(paint.transform);
            Vector3 back = -into;
            vis.transform.rotation = Quaternion.LookRotation(back);
            b = ItemVisuals.Bounds(vis);
            Vector3 p = paint.transform.position;
            vis.transform.position += new Vector3(p.x - b.center.x, p.y - b.center.y, p.z - b.center.z);
            b = ItemVisuals.Bounds(vis);
            float wall = WallFace(p, back) * (back.x + back.y + back.z);
            float backNow = Vector3.Dot(b.center, back) + Mathf.Abs(Vector3.Dot(b.extents, new Vector3(Mathf.Abs(back.x), Mathf.Abs(back.y), Mathf.Abs(back.z))));
            vis.transform.position += back * (wall - 0.01f - backNow);
            vis.transform.SetParent(paint.transform, true);
            var rend = paint.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.sharedMaterial = Mat("Mat_Canvas");
                rend.enabled = false;
            }
        }

        // Credencial pegada con cinta a la pared detrás del cuadro; se puede agarrar recién cuando el cuadro se abre
        private void TapeCredentialBehind(GameObject cred, GameObject paint)
        {
            Vector3 into = IntoRoomFrom(paint.transform);
            Vector3 back = -into;
            Vector3 p = paint.transform.position;
            float wall = WallFace(p, back) * (back.x + back.y + back.z);
            Vector3 cp = p + back * (wall - Vector3.Dot(p, back) - 0.002f);
            cp.y = p.y - 0.42f;
            // El frente de la tarjeta (su -z) mira a la sala
            cred.transform.SetPositionAndRotation(cp, Quaternion.LookRotation(back));
            cred.transform.localScale = new Vector3(0.42f, 0.24f, 0.06f);
            cred.SetActive(true);
            foreach (var box in cred.GetComponents<BoxCollider>())
                Object.DestroyImmediate(box);

            var zone = cred.transform.Find("PickupZone");
            if (zone == null)
            {
                zone = new GameObject("PickupZone").transform;
                zone.SetParent(cred.transform, false);
            }
            zone.gameObject.layer = cred.layer;
            zone.localPosition = Vector3.zero;
            zone.localRotation = Quaternion.identity;
            zone.localScale = Vector3.one;
            var zb = zone.GetComponent<BoxCollider>();
            if (zb == null)
                zb = zone.gameObject.AddComponent<BoxCollider>();
            zb.isTrigger = true;
            var s = cred.transform.lossyScale;
            zb.size = new Vector3(0.16f / s.x, 0.12f / s.y, 0.12f / s.z);
            zb.center = new Vector3(0f, 0f, -0.05f / s.z);
            zone.gameObject.SetActive(false);

            var cont = paint.GetComponent<ContainerInteractable>();
            if (cont != null)
                cont.SetReveal(null, new[] { zone.gameObject });
        }

        // Dos tiras de cinta de papel cruzando las esquinas de arriba de la tarjeta
        private void DressCredentialTape()
        {
            var cred = FindLocal("Interactable_Credential");
            if (cred == null)
                return;
            var card = cred.transform.Find("Kit_Pickup_CredentialCard");
            var ct = cred.transform;
            Vector3 c = card != null ? Encapsulate(card.gameObject).center : ct.position;
            float hw = ItemVisuals.SizeOf("credential") * 0.5f;
            float hh = hw * 0.63f;
            var tape = Mat("Mat_MaskingTape");
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Kit_CredentialTape_" + (i == 0 ? "A" : "B");
                StripColliders(go);
                go.transform.SetParent(_root, false);
                go.transform.SetPositionAndRotation(c + ct.right * side * hw + ct.up * hh - ct.forward * 0.001f, ct.rotation * Quaternion.Euler(0f, 0f, -side * 40f));
                go.transform.localScale = new Vector3(0.035f, 0.012f, 0.0006f);
                go.GetComponent<MeshRenderer>().sharedMaterial = tape;
            }
        }

        // Lector de tarjetas armado dentro de la caja del interactable, con la ranura a la altura de la tarjeta insertada
        private void DressCardReader()
        {
            var reader = FindLocal("Interactable_CardReader");
            if (reader == null)
                return;
            var t = reader.transform;
            var g = GreyBox(reader);
            Vector3 into = IntoRoomFrom(t);
            Vector3 c = g.center;
            float half = Mathf.Abs(Vector3.Dot(g.extents, new Vector3(Mathf.Abs(into.x), 0f, Mathf.Abs(into.z))));
            Vector3 fr = c + into * half;
            float slotY = c.y + g.extents.y * 0.55f;
            for (int i = 0; i < t.childCount; i++)
                if (t.GetChild(i).name.StartsWith("Inserted"))
                    slotY = t.GetChild(i).position.y;
            Vector3 side = Vector3.Cross(Vector3.up, into).normalized;
            var steel = Mat("Mat_Steel");
            var plastic = Mat("Mat_DarkPlastic");
            PartIn(t, "Kit_ReaderHousing", c - into * 0.01f, new Vector3(g.size.x, g.size.y, g.size.z - 0.02f), steel);
            PartIn(t, "Kit_ReaderFace", fr - into * 0.012f, new Vector3(g.size.x - 0.03f, g.size.y - 0.03f, 0.012f), plastic);
            PartIn(t, "Kit_ReaderSlot", new Vector3(fr.x, slotY, fr.z) + into * 0.001f, new Vector3(0.11f, 0.008f, 0.012f), Mat("Mat_Rubber"));
            PartIn(t, "Kit_ReaderScreen", new Vector3(fr.x, c.y + 0.02f, fr.z) + into * 0.001f, new Vector3(0.12f, 0.045f, 0.004f), Mat("Mat_ScreenOn"));
            PartIn(t, "Kit_ReaderLed", new Vector3(fr.x, c.y + 0.1f, fr.z) + side * 0.065f + into * 0.002f, new Vector3(0.012f, 0.012f, 0.006f), Mat("Mat_Neon"));
            for (int r = 0; r < 4; r++)
                for (int k = 0; k < 3; k++)
                    PartIn(t, "Kit_ReaderKey", new Vector3(fr.x, c.y - 0.035f - r * 0.025f, fr.z) + side * ((k - 1) * 0.04f) + into * 0.002f, new Vector3(0.03f, 0.02f, 0.006f), steel);
            // Si la caja no llega a la pared, una placa la sostiene
            float wall = WallFace(c, -into) * (-(into.x + into.z));
            float backFace = Vector3.Dot(c, -into) + half;
            float gap = wall - backFace;
            if (gap > 0.01f && gap < 0.6f)
                PartIn(t, "Kit_ReaderMount", c - into * (half + gap * 0.5f), new Vector3(g.size.x * 0.8f, g.size.y * 0.8f, gap), plastic);
            HideSelf(reader);
        }

        // Carcasa del imán del tamaño exacto de su caja; las piezas no son Kit_ porque el arranque apaga esos hijos
        private void DressMagnetHousing()
        {
            var mag = FindLocal("Interactable_Magnet");
            if (mag == null)
                return;
            var t = mag.transform;
            var housing = t.Find("Housing");
            if (housing == null)
            {
                housing = new GameObject("Housing").transform;
                housing.SetParent(t, false);
            }
            for (int i = housing.childCount - 1; i >= 0; i--)
                DestroyNow(housing.GetChild(i).gameObject);
            housing.localPosition = Vector3.zero;
            housing.localRotation = Quaternion.identity;
            housing.localScale = Vector3.one;
            var g = GreyBox(mag);
            Vector3 c = g.center;
            Vector3 s = g.size;
            var steel = Mat("Mat_Steel");
            var dark = Mat("Mat_Rubber");
            PartIn(housing, "Housing_Plinth", c + Vector3.down * (s.y * 0.5f - 0.04f), new Vector3(s.x, 0.08f, s.z), dark);
            PartIn(housing, "Housing_Body", c + Vector3.down * 0.005f, new Vector3(s.x - 0.05f, s.y - 0.17f, s.z - 0.05f), steel);
            PartIn(housing, "Housing_Cap", c + Vector3.up * (s.y * 0.5f - 0.045f), new Vector3(s.x, 0.09f, s.z), Mat("Mat_DarkPlastic"));
            for (int side = -1; side <= 1; side += 2)
                for (int k = 0; k < 4; k++)
                    PartIn(housing, "Housing_Vent", c + new Vector3(side * (s.x * 0.5f - 0.022f), 0.25f - k * 0.1f, 0f), new Vector3(0.006f, 0.03f, s.z * 0.6f), dark);
            HideSelf(mag);
        }

        // ---------- N4 ----------

        private const float LockerW = 0.6f;
        private const float LockerD = 0.45f;
        private const float LockerH = 1.8f;
        private const float LockerShelf = 1.05f;

        // Casillero de chapa con la espalda hacia la pared más cercana (arrimado si está cerca); la puerta se abre sola con el código y la cinta está en el estante
        private void DressTapeLocker()
        {
            var lockGo = FindLocal("Interactable_Locker");
            var tape = FindLocal("Interactable_Tape");
            if (lockGo == null)
                return;
            var lt = lockGo.transform;
            Vector3 c = lt.position;
            float floor = FloorBelow(new Vector3(c.x, c.y, c.z), lt);

            Vector3 back = Vector3.forward;
            float best = float.MaxValue;
            foreach (var dir in new[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left })
            {
                float face = WallFace(new Vector3(c.x, floor + 1f, c.z), dir) * (dir.x + dir.z);
                float dist = face - Vector3.Dot(c, dir);
                if (dist > 0f && dist < best)
                {
                    best = dist;
                    back = dir;
                }
            }
            Vector3 front = -back;
            float along = best < 3f ? best - 0.02f - LockerD * 0.5f : 0f;
            Vector3 basePos = new Vector3(c.x, floor, c.z) + back * along;
            var rot = Quaternion.LookRotation(back);
            var root = NewRoot("Kit_TapeLocker", basePos, rot).transform;

            var steel = Mat("Mat_Steel");
            var dark = Mat("Mat_Rubber");
            const float wall = 0.02f;
            const float plinth = 0.08f;
            float hd = LockerD * 0.5f, hw = LockerW * 0.5f;
            Part(root, "Kit_LockerBack", new Vector3(0f, LockerH * 0.5f, hd - wall * 0.5f), new Vector3(LockerW, LockerH, wall), steel);
            Part(root, "Kit_LockerSideL", new Vector3(-hw + wall * 0.5f, LockerH * 0.5f, 0f), new Vector3(wall, LockerH, LockerD), steel);
            Part(root, "Kit_LockerSideR", new Vector3(hw - wall * 0.5f, LockerH * 0.5f, 0f), new Vector3(wall, LockerH, LockerD), steel);
            Part(root, "Kit_LockerTop", new Vector3(0f, LockerH - wall * 0.5f, 0f), new Vector3(LockerW, wall, LockerD), steel);
            Part(root, "Kit_LockerPlinth", new Vector3(0f, plinth * 0.5f, 0f), new Vector3(LockerW, plinth, LockerD), dark);
            Part(root, "Kit_LockerShelf", new Vector3(0f, LockerShelf, wall * 0.5f), new Vector3(LockerW - wall * 2f, wall, LockerD - wall * 2f), steel);
            Part(root, "Kit_LockerHatShelf", new Vector3(0f, 1.55f, wall * 0.5f), new Vector3(LockerW - wall * 2f, wall, LockerD - wall * 2f), steel);

            // Bisagra del lado contrario a la consola para que la puerta abierta no la toque
            float hingeSide = -1f;
            var console = FindLocal("Console");
            if (console != null && Vector3.Dot(console.transform.position - basePos, root.right) < 0f)
                hingeSide = 1f;
            var pivot = new GameObject("Kit_TapeLockerDoor").transform;
            pivot.SetParent(root, false);
            pivot.localPosition = new Vector3(hingeSide * hw, plinth, -hd);
            pivot.localRotation = Quaternion.identity;
            float doorW = LockerW - 0.004f;
            float doorH = LockerH - plinth - 0.004f;
            float dx = -hingeSide * doorW * 0.5f;
            Part(pivot, "Kit_LockerDoorLeaf", new Vector3(dx, doorH * 0.5f, -0.009f), new Vector3(doorW, doorH, 0.018f), steel);
            for (int i = 0; i < 4; i++)
                Part(pivot, "Kit_LockerDoorVent", new Vector3(dx, doorH - 0.12f - i * 0.03f, -0.0185f), new Vector3(doorW * 0.55f, 0.012f, 0.002f), dark);
            float freeX = -hingeSide * (doorW - 0.06f);
            Part(pivot, "Kit_LockerHandle", new Vector3(freeX, doorH * 0.5f, -0.03f), new Vector3(0.02f, 0.12f, 0.024f), dark);
            Part(pivot, "Kit_LockerKeypad", new Vector3(freeX, doorH * 0.5f + 0.13f, -0.023f), new Vector3(0.07f, 0.09f, 0.01f), Mat("Mat_DarkPlastic"));
            for (int r = 0; r < 3; r++)
                for (int k = 0; k < 3; k++)
                    Part(pivot, "Kit_LockerKey", new Vector3(freeX + (k - 1) * 0.02f, doorH * 0.5f + 0.155f - r * 0.022f, -0.029f), new Vector3(0.014f, 0.014f, 0.004f), steel);
            var code = lockGo.GetComponent<CodeEntryInteractable>();
            var hinge = pivot.gameObject.AddComponent<FlagHinge>();
            hinge.Configure(code != null ? code.UnlockFlagId : "", hingeSide < 0f ? 100f : -100f, 0.9f);

            // El interactable ocupa el casillero; el sólido es aparte para no tapar la detección
            lt.SetPositionAndRotation(basePos + Vector3.up * LockerH * 0.5f, rot);
            lt.localScale = new Vector3(LockerW, LockerH, LockerD);
            foreach (var box in lockGo.GetComponents<BoxCollider>())
            {
                if (box.isTrigger)
                {
                    box.center = Vector3.zero;
                    box.size = Vector3.one;
                }
                else
                    Object.DestroyImmediate(box);
            }
            HideSelf(lockGo);
            var solid = NewRoot("Kit_TapeLocker_Solid", basePos, rot);
            var sb = solid.AddComponent<BoxCollider>();
            sb.center = new Vector3(0f, LockerH * 0.5f, 0f);
            sb.size = new Vector3(LockerW, LockerH, LockerD);

            if (tape != null)
                SeatTapeOnShelf(tape, root.TransformPoint(new Vector3(0f, LockerShelf + wall * 0.5f, wall * 0.5f)));
        }

        // La cinta acostada en el estante, con su modelo grabado; sigue apagada hasta abrir el casillero
        private static void SeatTapeOnShelf(GameObject tape, Vector3 shelf)
        {
            bool was = tape.activeSelf;
            tape.SetActive(true);
            ItemVisuals.Dress(tape, "tape");
            HideSelf(tape);
            var t = tape.transform;
            t.rotation = Quaternion.identity;
            var vis = t.Find("Visual_tape");
            if (vis != null)
            {
                var b = ItemVisuals.Bounds(vis.gameObject);
                t.position += new Vector3(shelf.x - b.center.x, shelf.y + 0.002f - b.min.y, shelf.z - b.center.z);
                var box = tape.GetComponent<BoxCollider>();
                if (box != null && LocalBounds(t, vis.gameObject, out var lb))
                {
                    box.isTrigger = true;
                    box.center = lb.center;
                    box.size = lb.size + Vector3.one * 0.03f;
                }
            }
            tape.SetActive(was);
        }

        // Casete compacto acostado en el piso, a 1,3 veces su tamaño real
        private void DressCassette()
        {
            var cass = FindLocal("Interactable_Cassette");
            if (cass == null)
                return;
            Vector3 c = cass.transform.position;
            float floor = FloorBelow(c, cass.transform);
            var t = NewRoot("Kit_Cassette_N4", new Vector3(c.x, floor, c.z), Quaternion.Euler(0f, 20f, 0f)).transform;
            var plastic = Mat("Mat_DarkPlastic");
            var paper = Mat("Mat_PrintPaper");
            Part(t, "Kit_CassetteShell", new Vector3(0f, 0.0078f, 0f), new Vector3(0.13f, 0.0156f, 0.083f), plastic);
            Part(t, "Kit_CassetteLabel", new Vector3(0f, 0.0161f, 0.008f), new Vector3(0.112f, 0.001f, 0.05f), paper);
            Part(t, "Kit_CassetteWindow", new Vector3(0f, 0.0168f, 0.004f), new Vector3(0.05f, 0.0012f, 0.016f), Mat("Mat_Rubber"));
            for (int i = -1; i <= 1; i += 2)
                Part(t, "Kit_CassetteHub", new Vector3(i * 0.021f, 0.0172f, 0.004f), new Vector3(0.014f, 0.0007f, 0.014f), paper, PrimitiveType.Cylinder);
            Part(t, "Kit_CassetteEdge", new Vector3(0f, 0.0161f, -0.033f), new Vector3(0.09f, 0.001f, 0.016f), plastic);
            HideSelf(cass);
        }
    }
}
