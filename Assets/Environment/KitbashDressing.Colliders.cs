using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Enigma.Environment
{
    // Colliders de props y piso caminable
    public partial class KitbashDressing
    {
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
                || kit.IndexOf("Bathroom_Toilet") >= 0
                || kit.IndexOf("books") >= 0
                || kit.IndexOf("workbench") >= 0
                || kit.IndexOf("table") >= 0
                || kit.IndexOf("locker") >= 0
                || kit.IndexOf("Furniture") >= 0;
        }

        private void SolidShell(GameObject visual)
        {
            if (visual == null)
                return;
            if (_root == null)
                _root = transform;
            string name = visual.name + "_Solid";
            var existing = FindLocal(name);
            // Un greybox escondido no bloquea: lo cubre el sólido de su reemplazo
            if (!VisibleBounds(visual, out var b) || (b.size.x < 0.04f && b.size.z < 0.04f))
            {
                DropGo(existing);
                return;
            }

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

        private void FitGreyTo(string grey, string visual)
        {
            var src = FindLocal(grey);
            var kit = FindLocal(visual);
            if (src != null && kit != null)
                FitColliderTo(src, kit);
        }

        private static bool VisibleBounds(GameObject go, out Bounds b)
        {
            b = default;
            bool any = false;
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (!r.enabled || !r.gameObject.activeInHierarchy)
                    continue;
                if (any)
                    b.Encapsulate(r.bounds);
                else
                    b = r.bounds;
                any = true;
            }
            return any;
        }

        private static void FitColliderTo(GameObject grey, GameObject visual)
        {
            if (grey == null || visual == null)
                return;
            if (!LocalBounds(grey.transform, visual, out var b))
                return;
            var col = grey.GetComponent<BoxCollider>();
            if (col == null)
                col = grey.AddComponent<BoxCollider>();
            col.isTrigger = false;
            col.center = b.center;
            col.size = b.size;
        }

        // Igual que FitColliderTo pero contra una caja en mundo (varios modelos juntos)
        private static void FitColliderToBounds(GameObject grey, Bounds world)
        {
            if (grey == null)
                return;
            Bounds b = default;
            for (int c = 0; c < 8; c++)
            {
                var corner = world.center + Vector3.Scale(world.extents, new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f));
                var p = grey.transform.InverseTransformPoint(corner);
                if (c == 0)
                    b = new Bounds(p, Vector3.zero);
                else
                    b.Encapsulate(p);
            }
            var col = grey.GetComponent<BoxCollider>();
            if (col == null)
                col = grey.AddComponent<BoxCollider>();
            col.isTrigger = false;
            col.center = b.center;
            col.size = b.size;
        }

        // Caja de las mallas visibles medida en el espacio de 'space' (sirve aunque esté rotado)
        private static bool LocalBounds(Transform space, GameObject visual, out Bounds b)
        {
            b = default;
            bool any = false;
            var filters = visual.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                var f = filters[i];
                var r = f.GetComponent<MeshRenderer>();
                if (f.sharedMesh == null || r == null || !r.enabled || !f.gameObject.activeInHierarchy)
                    continue;
                var mb = f.sharedMesh.bounds;
                for (int c = 0; c < 8; c++)
                {
                    var corner = mb.center + Vector3.Scale(mb.extents, new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f));
                    var p = space.InverseTransformPoint(f.transform.TransformPoint(corner));
                    if (any)
                        b.Encapsulate(p);
                    else
                        b = new Bounds(p, Vector3.zero);
                    any = true;
                }
            }
            return any;
        }

        // Caja sólida con la forma de lo que se ve (el modelo puede estar en hijos con escala propia)
        private static void EnsureWalkCollider(GameObject go)
        {
            if (go == null)
                return;
            BoxCollider box = null;
            var cols = go.GetComponents<BoxCollider>();
            for (int i = 0; i < cols.Length && box == null; i++)
            {
                if (cols[i] != null && !cols[i].isTrigger)
                    box = cols[i];
            }
            // Un greybox escondido conserva la caja que le ajustó su modelo
            if (!LocalBounds(go.transform, go, out var b))
                return;
            if (box == null)
            {
                box = go.AddComponent<BoxCollider>();
                box.isTrigger = false;
            }
            box.center = b.center;
            box.size = b.size;
            // Lo muy bajo bloquea igual el paso: el sólido crece hacia abajo, no por encima de lo visible
            Vector3 ls = go.transform.lossyScale;
            const float minY = 0.45f;
            float height = b.size.y * Mathf.Abs(ls.y);
            if (Mathf.Abs(ls.y) > 0.001f && height < minY)
            {
                float h = minY / Mathf.Abs(ls.y);
                box.size = new Vector3(b.size.x, h, b.size.z);
                box.center = new Vector3(b.center.x, b.max.y - h * 0.5f, b.center.z);
            }
        }

        private void PropSolidPass()
        {
            EnsureWalkCollider(FindLocal("Interactable_Magnet"));
            EnsureWalkCollider(FindLocal("Metal_A"));
            EnsureWalkCollider(FindLocal("Metal_B"));
            EnsureWalkCollider(FindLocal("Metal_C"));
            EnsureWalkCollider(FindLocal("Lockers"));
            EnsureWalkCollider(FindLocal("Interactable_Cassette"));
            EnsureWalkCollider(FindLocal("Chair_Origen"));
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
            SolidShell(FindLocal("Kit_books_N1"));
            SolidShell(FindLocal("Kit_books_N2"));
            SolidShell(FindLocal("Kit_books_Mezz"));
            SolidShell(FindLocal("Kit_books_N4"));
            // El greybox escondido choca con la forma de lo que se ve en su lugar
            for (int i = 0; i < 4; i++)
                FitGreyTo("Crate_R" + i, "Kit_CrateBox_" + i);
            FitGreyTo("Decor_Chair", "Kit_OfficeChair_Decor_Chair");
            FitGreyTo("Chair", "Kit_OfficeChair_Chair");
            FitGreyTo("Chair_Origen", "Kit_OfficeChair_Chair_Origen");
            FitGreyTo("Decor_Sink", "Kit_Bathroom_Sink");
            FitGreyTo("Generator", "Kit_Generator");
            // Las chapas llevan el modelo adentro
            FitGreyTo("Metal_A", "Metal_A");
            FitGreyTo("Metal_B", "Metal_B");
            FitGreyTo("Metal_C", "Metal_C");
            WalkNamedProps();
        }

        // Lo que va amurado, colgado o es parte de un mecanismo no se apoya
        private static readonly string[] Mounted =
        {
            "Leak", "Rust", "Tube", "Cable", "Light", "Lamp", "pipe", "Pipe", "warning", "ElectricalBox", "Sign", "Roof",
            "Hatch", "ladder", "Door", "Lintel", "Exit", "Projector", "Stencil", "Cam", "Sink", "Lever", "Lock", "Reader",
            "Wires", "Calendar", "Painting", "Magnet", "Slot", "Rope", "Drawer", "Panel_", "Button", "Plate",
            "Metal_", "Wall", "Hall_", "Passage", "Ramp", "Pit", "Stair", "Rim", "Lid", "Shelf", "Cabinet", "Locker_Door",
            "Doc_", "Print_", "Sheet", "Generator", "Burn", "_Leg", "Ceiling", "CredentialTape", "Mirror"
        };

        private static bool IsMounted(string n)
        {
            for (int i = 0; i < Mounted.Length; i++)
                if (n.IndexOf(Mounted[i]) >= 0)
                    return true;
            return false;
        }

        private const int SettleLayer = 31;
        // Hasta cuánto flota (o se hunde) algo para considerarlo apoyado mal
        private const float SettleFloat = 0.6f;
        private const float SettleSink = 0.25f;
        // Los muebles del greybox (escritorios, consolas) son el apoyo, no se mueven
        private const float SettleMaxSize = 0.8f;

        // Props sueltos sobre lo que tienen debajo (piso, mueble u otro prop), medido contra las mallas visibles
        private void SettleProps()
        {
            if (Application.isPlaying)
                return;
            var scene = gameObject.scene;
            var cands = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < _root.childCount; i++)
            {
                var c = _root.GetChild(i).gameObject;
                if ((c.name.StartsWith("Kit_") || c.name.StartsWith("Paper")) && !IsMounted(c.name) && c.activeInHierarchy)
                    cands.Add(c);
            }
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != "Props" && root.name != "Interactables")
                    continue;
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    var c = root.transform.GetChild(i).gameObject;
                    if (c.activeInHierarchy && !IsMounted(c.name))
                        cands.Add(c);
                }
            }

            // Solo las mallas visibles cuentan como apoyo: los sólidos viejos pueden ser más altos que el mueble
            var off = new System.Collections.Generic.List<Collider>();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var col in root.GetComponentsInChildren<Collider>())
                    if (col.enabled)
                    {
                        col.enabled = false;
                        off.Add(col);
                    }
            var temps = new System.Collections.Generic.List<MeshCollider>();
            var layers = new System.Collections.Generic.Dictionary<GameObject, int>();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
                {
                    var f = r.GetComponent<MeshFilter>();
                    if (!r.enabled || f == null || f.sharedMesh == null || UnderPlayer(r.transform))
                        continue;
                    layers[r.gameObject] = r.gameObject.layer;
                    r.gameObject.layer = SettleLayer;
                    var mc = r.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = f.sharedMesh;
                    temps.Add(mc);
                }
            Physics.SyncTransforms();

            var order = new System.Collections.Generic.List<(float low, GameObject go)>();
            foreach (var c in cands)
                if (VisibleBounds(c, out var cb) && (c.name.StartsWith("Kit_") || Mathf.Max(cb.size.x, cb.size.z) <= SettleMaxSize))
                    order.Add((cb.min.y, c));
            order.Sort((a, b) => a.low.CompareTo(b.low));
            // Lo que todavía no se apoyó no sostiene a nadie (papeles apilados a la misma altura)
            var own = new System.Collections.Generic.HashSet<MeshCollider>(temps);
            foreach (var (_, go) in order)
                foreach (var mc in go.GetComponentsInChildren<MeshCollider>())
                    if (own.Contains(mc))
                        mc.enabled = false;
            var hits = new RaycastHit[16];
            foreach (var (_, go) in order)
            {
                foreach (var mc in go.GetComponentsInChildren<MeshCollider>())
                    if (own.Contains(mc))
                        mc.enabled = true;
                if (!VisibleBounds(go, out var b))
                    continue;
                float cap = b.min.y + SettleSink;
                float start = Mathf.Max(b.max.y, cap) + 0.05f;
                float support = float.MinValue;
                for (int k = 0; k < 5; k++)
                {
                    float ox = k == 0 ? 0f : ((k & 1) == 0 ? -0.35f : 0.35f) * b.size.x;
                    float oz = k == 0 ? 0f : (k < 3 ? -0.35f : 0.35f) * b.size.z;
                    var o = new Vector3(b.center.x + ox, start, b.center.z + oz);
                    int n = Physics.RaycastNonAlloc(o, Vector3.down, hits, start - b.min.y + SettleFloat, 1 << SettleLayer);
                    for (int h = 0; h < n; h++)
                    {
                        if (hits[h].collider.transform.IsChildOf(go.transform) || hits[h].point.y > cap)
                            continue;
                        support = Mathf.Max(support, hits[h].point.y);
                    }
                }
                if (support == float.MinValue)
                    continue;
                float delta = support + 0.003f - b.min.y;
                if (Mathf.Abs(delta) < 0.01f)
                    continue;
                go.transform.position += Vector3.up * delta;
                Physics.SyncTransforms();
            }

            foreach (var mc in temps)
                if (mc != null)
                    Object.DestroyImmediate(mc);
            foreach (var kv in layers)
                if (kv.Key != null)
                    kv.Key.layer = kv.Value;
            foreach (var col in off)
                if (col != null)
                    col.enabled = true;
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
                bool interact = n == "Interactable_Cassette"
                    || n == "Interactable_Magnet"
                    || n == "Interactable_ElectricalBox" || n == "Interactable_Wires"
                    || n.StartsWith("Metal_");
                if (!kit && !furniture && !interact)
                    continue;
                // Lo que tiene física choca con su propio collider: un sólido fijo lo trabaría donde se grabó
                if (kit && t.GetComponentInParent<Rigidbody>(true) != null)
                    continue;
                // Los modelos Kit_ vienen con escalas de importación enormes: un box unitario los vuelve gigantes. Solo usan el _Solid
                if (!kit)
                    EnsureWalkCollider(t.gameObject);
                if (kit || furniture)
                    SolidShell(t.gameObject);
            }
        }
    }
}
