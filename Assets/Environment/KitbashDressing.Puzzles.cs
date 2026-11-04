using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Enigma.Environment
{
    // Ubicación y cableado de piezas de puzzle (imán, impresiones, llave, salidas)
    public partial class KitbashDressing
    {
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

        private void PlaceMetal(string name, Vector3 pos, Vector3 scale)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            go.transform.position = pos;
            go.transform.localScale = scale;
            // El tamaño lo ajusta PropSolidPass a la chapa visible
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                box = go.AddComponent<BoxCollider>();
            box.isTrigger = false;
            EnsureWalkCollider(go);
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
            // Los recortes se van volando: si la caja de colisión compartida quedó sobre uno, se apaga
            var bitSolid = FindLocal("Kit_metal-panel-narrow_Solid");
            if (bitSolid != null)
            {
                Vector3 sp = bitSolid.transform.position;
                foreach (var bit in bits)
                {
                    Vector3 d2 = bit.position - sp;
                    d2.y = 0f;
                    if (d2.sqrMagnitude < 0.36f)
                    {
                        bitSolid.SetActive(false);
                        break;
                    }
                }
            }

            for (int i = 0; i < metals.Length; i++)
                Mass(metals[i], 12f);
            for (int i = 0; i < bits.Count; i++)
                Mass(bits[i], 1.5f);
            control.BakerSetup(paint, metals, bits.ToArray(), PlaceMagnetJunk(mag.transform.position));
        }

        private static void Mass(Transform t, float kg)
        {
            if (t == null)
                return;
            var m = t.GetComponent<MagnetMass>();
            if (m == null)
                m = t.gameObject.AddComponent<MagnetMass>();
            m.Set(kg);
        }

        // Chatarra alrededor del imán: cada pieza llega según lo que pesa
        private Transform[] PlaceMagnetJunk(Vector3 mag)
        {
            if (_root == null)
                _root = transform;
            EnsureMats();
            var steel = Resources.Load<Material>("Mats/Mat_Steel");

            var list = new System.Collections.Generic.List<Transform>();
            Junk(list, "Spoon", mag + new Vector3(0.9f, 0f, -2.0f), -40f, 0.26f, steel, 0.05f);
            Junk(list, "Knife", mag + new Vector3(-1.3f, 0f, -0.9f), 75f, 0.32f, steel, 0.2f);
            Junk(list, "pipe-end", mag + new Vector3(0.4f, 0f, -1.5f), 15f, 0.34f, steel, 0.6f);
            Junk(list, "tool-hammer", mag + new Vector3(1.6f, 0f, -1.2f), 30f, 0.42f, steel, 1.2f);
            Junk(list, "bucket", mag + new Vector3(2.3f, 0f, 0.2f), 0f, 0.4f, steel, 1.6f);
            Junk(list, "Gas_Canister", mag + new Vector3(-2.1f, 0f, -0.5f), 10f, 0.42f, _rust, 2f);
            Junk(list, "tool-shovel", mag + new Vector3(-0.4f, 0f, -2.9f), 95f, 0.9f, steel, 2.8f);
            Junk(list, "tool-axe", mag + new Vector3(2.0f, 0f, -2.6f), -65f, 0.66f, steel, 3f);
            Junk(list, "tool-pickaxe", mag + new Vector3(-1.9f, 0f, -2.2f), 120f, 0.75f, steel, 3.5f);
            return list.ToArray();
        }

        private void Junk(System.Collections.Generic.List<Transform> list, string kit, Vector3 pos, float yaw, float size, Material mat, float kg)
        {
            string name = "Kit_Junk_" + kit;
            var go = FindLocal(name);
            if (go == null)
            {
                var prefab = Application.isPlaying ? null : Resources.Load<GameObject>("Kitbash/" + kit);
                if (prefab == null)
                    return;
                go = Object.Instantiate(prefab, _root);
                go.name = name;
                go.transform.rotation = prefab.transform.rotation;
                StripColliders(go);
                Paint(go, mat);
                // Acostado: el lado más largo sobre el piso
                var b = Encapsulate(go);
                if (b.size.y > Mathf.Max(b.size.x, b.size.z))
                    go.transform.Rotate(90f, 0f, 0f, Space.World);
                go.transform.Rotate(0f, yaw, 0f, Space.World);
                go.transform.position = new Vector3(pos.x, 0f, pos.z);
                FitLongOnFloor(go, 0f, size, size);
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                    r.shadowCastingMode = ShadowCastingMode.On;
            }
            Mass(go.transform, kg);
            list.Add(go.transform);
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

        // La llave espera en el cajón del gabinete y sale con él
        private void PlaceOverrideKey()
        {
            var key = FindLocal("Interactable_OverrideKey");
            var drawer = FindLocal("Drawer_Override");
            var tray = drawer != null ? FindChildNamed(drawer.transform, "Kit_DrawerTray") : null;
            if (key == null || tray == null)
                return;
            var b = Encapsulate(tray.gameObject);
            key.transform.SetParent(drawer.transform, true);
            key.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            key.transform.position = new Vector3(b.center.x, b.max.y, b.center.z);
            // El modelo de la llave lo pone PickupInteractable (ItemVisuals) a escala real.
            // La llave está apagada (sin bounds): se mide la malla para apoyarla sobre la bandeja
            float low = float.MaxValue;
            foreach (var mf in key.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.gameObject == key || mf.sharedMesh == null)
                    continue;
                var mb = mf.sharedMesh.bounds;
                for (int c = 0; c < 8; c++)
                {
                    var corner = mb.center + Vector3.Scale(mb.extents, new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f));
                    low = Mathf.Min(low, mf.transform.TransformPoint(corner).y);
                }
            }
            if (low < float.MaxValue)
                key.transform.position += Vector3.up * (b.max.y - low + 0.003f);

            // Cada malla de la llave lleva el material de su modelo, no el del cajón
            var model = Resources.Load<GameObject>("Kitbash/Key4");
            if (model == null)
                return;
            var originals = model.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in key.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = mf.GetComponent<MeshRenderer>();
                if (mf.gameObject == key || r == null)
                    continue;
                foreach (var o in originals)
                {
                    var or = o.GetComponent<MeshRenderer>();
                    if (o.sharedMesh == mf.sharedMesh && or != null)
                        r.sharedMaterials = or.sharedMaterials;
                }
            }
        }

        // Nivel 4: botones de piso con neón, cajón de la llave y proyector
        private void DressOrigen()
        {
            var cabinet = PlaceOverrideCabinet();
            if (cabinet != null)
                WireFloorButtons(cabinet.Value);
            PlaceProjector();
        }

        // El vano sale de las paredes: la hoja lo cubre con solape y detrás hay un pasillo, no el vacío
        private void FrameOrigenDoor()
        {
            var door = FindLocal("Interactable_OrigenDoor");
            var left = FindLocal("Wall_N_L");
            var right = FindLocal("Wall_N_R");
            var lintel = FindLocal("Kit_DoorLintel");
            var floor = FindLocal("Gallery_N");
            if (door == null || left == null || right == null || lintel == null || floor == null)
                return;
            Bounds lb = Encapsulate(left), rb = Encapsulate(right), tb = Encapsulate(lintel);
            float x0 = lb.max.x, x1 = rb.min.x, top = tb.min.y, face = lb.min.z, back = lb.max.z;
            float floorY = Encapsulate(floor).max.y;
            const float overlap = 0.1f, thick = 0.14f, gap = 0.02f, depth = 1.9f;

            Vector3 size = new Vector3(x1 - x0 + overlap * 2f, top - floorY + overlap, thick);
            Vector3 center = new Vector3((x0 + x1) * 0.5f, floorY + size.y * 0.5f, face - gap - thick * 0.5f);
            var pivot = door.transform.parent;
            if (pivot != null)
                pivot.position += center - door.transform.position;
            else
                door.transform.position = center;
            Vector3 ps = pivot != null ? pivot.lossyScale : Vector3.one;
            door.transform.localScale = new Vector3(size.x / ps.x, size.y / ps.y, size.z / ps.z);

            var mat = Resources.Load<Material>("Mats/Mat_Corridor");
            float w = x1 - x0, h = top - floorY, len = back - face + depth;
            float cx = (x0 + x1) * 0.5f, cz = face + len * 0.5f;
            PassageSlab("OrigenPassage_Floor", new Vector3(cx, floorY - 0.05f, cz), new Vector3(w, 0.1f, len), mat);
            PassageSlab("OrigenPassage_Ceiling", new Vector3(cx, top + 0.05f, cz), new Vector3(w, 0.1f, len), mat);
            PassageSlab("OrigenPassage_L", new Vector3(x0 - 0.05f, floorY + h * 0.5f, cz), new Vector3(0.1f, h, len), mat);
            PassageSlab("OrigenPassage_R", new Vector3(x1 + 0.05f, floorY + h * 0.5f, cz), new Vector3(0.1f, h, len), mat);
            PassageSlab("OrigenPassage_Back", new Vector3(cx, floorY + h * 0.5f, face + len + 0.05f), new Vector3(w + 0.2f, h + 0.2f, 0.1f), mat);
        }

        private const float HaspHeight = 1.05f;
        private const float PadlockHeight = 0.15f;
        private static readonly Vector3 StapleSize = new Vector3(0.07f, 0.12f, 0f);
        private static readonly Vector3 HaspSize = new Vector3(0.26f, 0.07f, 0.016f);

        // Cerrojo de ORIGEN: armella amurada, pasador en la hoja (corre con ella) y el candado que cae al abrir con la llave
        private void LockOrigenDoor()
        {
            if (FindLocal("OrigenPadlock") != null)
                return;
            var door = FindLocal("Interactable_OrigenDoor");
            var left = FindLocal("Wall_N_L");
            var prefab = Resources.Load<GameObject>("Kitbash/Padlock");
            if (door == null || left == null || door.transform.parent == null || prefab == null)
                return;
            Vector3 c = door.transform.position;
            Vector3 s = door.transform.lossyScale;
            float edge = c.x - s.x * 0.5f;
            float front = c.z - s.z * 0.5f;
            float wallFace = Encapsulate(left).min.z;
            float y = c.y - s.y * 0.5f + HaspHeight;
            float sx = edge - StapleSize.x * 0.9f;
            var steel = Resources.Load<Material>("Mats/Mat_Steel");

            Slab("Kit_OrigenLockStaple", new Vector3(sx, y, (front + wallFace) * 0.5f), new Vector3(StapleSize.x, StapleSize.y, wallFace - front), steel);
            Slab("Kit_OrigenLockHasp", new Vector3(edge + HaspSize.x * 0.5f - StapleSize.x, y, front - HaspSize.z * 0.5f), HaspSize, steel);
            FindLocal("Kit_OrigenLockHasp").transform.SetParent(door.transform.parent, true);
            HangPadlock("OrigenPadlock", sx, y, front, "origen_door_unlocked");
        }

        // Candado colgado de la armella en (x, y), con la cara hacia la sala (-Z); cae cuando se pone flag
        private GameObject HangPadlock(string name, float x, float y, float front, string flag)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/Padlock");
            if (prefab == null)
                return null;
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.SetPositionAndRotation(new Vector3(x, y, front), Quaternion.identity);
            var model = Object.Instantiate(prefab, go.transform);
            model.name = "Kit_Padlock";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = prefab.transform.rotation;
            StripColliders(model);
            // La cara del candado mira a la sala: el lado fino queda en profundidad
            var b = Encapsulate(model);
            if (b.size.x < b.size.z)
                model.transform.Rotate(0f, 90f, 0f, Space.World);
            FitToHeight(model, 0f, PadlockHeight);
            b = Encapsulate(model);
            model.transform.position += new Vector3(x - b.center.x, y + StapleSize.y * 0.2f - b.max.y, front - HaspSize.z - b.extents.z - b.center.z);
            b = Encapsulate(model);

            var box = go.AddComponent<BoxCollider>();
            box.center = go.transform.InverseTransformPoint(b.center);
            box.size = b.size;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.6f;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            go.AddComponent<FlagDrop>().Configure(flag, new Vector3(0f, 0.3f, -0.5f), "padlock_drop");
            return go;
        }

        private const string LockerFlag = "locker_opened";
        private const float CabinetHeight = 1.55f;
        private const float LockerDrawerTravel = 0.4f;
        // Piso interior del cajón medido sobre el modelo: 0.073 de 0.365 de alto
        private const float DrawerFloorRatio = 0.073f / 0.365f;

        // N2: el bloque de lockers pasa a tres archiveros; la batería sale del cajón con candado del medio
        private void DressLockerBank()
        {
            var lockers = FindLocal("Lockers");
            var holder = FindLocal("LockerDrawer_1");
            var battery = FindLocal("Interactable_Battery");
            var lockGo = FindLocal("Interactable_Lock");
            if (lockers == null || holder == null || battery == null || lockGo == null || FindLocal("LockerPadlock") != null)
                return;

            var lt = lockers.transform;
            float floorY = lt.position.y - lt.lossyScale.y * 0.5f;
            float backZ = lt.position.z + lt.lossyScale.z * 0.5f;
            float cx = lt.position.x;

            var cabinets = new GameObject[3];
            Bounds bank = default;
            for (int i = 0; i < 3; i++)
            {
                var go = PlaceProp("FileCabinet", "Kit_Furniture_FileCabinet_N2" + (char)('a' + i), new Vector3(cx, floorY, backZ), 0f, CabinetHeight, Vector3.zero);
                if (go == null)
                    return;
                var body = FindChildNamed(go.transform, "Main_");
                var top = FindChildNamed(go.transform, "Draw");
                if (body == null || top == null)
                    return;
                // Los cajones salen hacia la sala
                Vector3 outward = Encapsulate(top.gameObject).center - Encapsulate(body.gameObject).center;
                outward.y = 0f;
                go.transform.Rotate(0f, Vector3.SignedAngle(outward, Vector3.back, Vector3.up), 0f, Space.World);
                var bb = Encapsulate(body.gameObject);
                float width = bb.size.x;
                // Los de los costados quedan apenas torcidos; el del medio sostiene el cajón y el candado
                if (i != 1)
                {
                    go.transform.Rotate(0f, i == 0 ? -2f : 2.5f, 0f, Space.World);
                    bb = Encapsulate(body.gameObject);
                }
                go.transform.position += new Vector3(cx + (i - 1) * (width + 0.03f) - bb.center.x, 0f, backZ - bb.max.z);
                SolidShell(go);
                bb = Encapsulate(body.gameObject);
                if (i == 0)
                    bank = bb;
                else
                    bank.Encapsulate(bb);
                cabinets[i] = go;
            }

            var mid = cabinets[1].transform;
            var drawerKit = FindChildNamed(mid, "Draw.001");
            var midBody = FindChildNamed(mid, "Main_");
            if (drawerKit == null || midBody == null)
                return;

            HideVisual(lockers);
            for (int i = 0; i < lt.childCount; i++)
                HideSelf(lt.GetChild(i).gameObject);
            FitColliderToBounds(lockers, bank);

            // El cajón es el objeto de escena: lo que no es Kit_ (la batería) sobrevive al re-grabado
            Bounds db = Encapsulate(drawerKit.gameObject);
            var t = holder.transform;
            if (t.parent != lt.parent)
                t.SetParent(lt.parent, true);
            t.rotation = mid.rotation;
            t.localScale = Vector3.one;
            t.position = db.center;
            HideSelf(holder);
            drawerKit.SetParent(t, true);
            drawerKit.name = "Kit_LockerDrawer";
            float innerFloor = db.min.y + db.size.y * DrawerFloorRatio;
            // Solo el fondo del cajón es sólido: frena al jugador y deja el hueco libre para ver la batería
            var dbox = holder.GetComponent<BoxCollider>();
            if (dbox == null)
                dbox = holder.AddComponent<BoxCollider>();
            if (LocalBounds(t, drawerKit.gameObject, out var dl))
            {
                float low = dl.min.y, high = innerFloor - t.position.y;
                dbox.isTrigger = false;
                dbox.center = new Vector3(dl.center.x, (low + high) * 0.5f, dl.center.z);
                dbox.size = new Vector3(dl.size.x, high - low, dl.size.z);
            }
            var slider = holder.GetComponent<FlagDrawer>();
            if (slider == null)
                slider = holder.AddComponent<FlagDrawer>();
            Vector3 open = Vector3.back * LockerDrawerTravel;
            if (t.parent != null)
                open = t.parent.InverseTransformVector(open);
            slider.Configure(LockerFlag, open, 0.9f);

            Vector3 inside = new Vector3(db.center.x, innerFloor, db.center.z + 0.05f);
            SeatBatteryIn(battery, t, inside);

            Bounds body2 = Encapsulate(midBody.gameObject);
            float y = db.max.y - 0.07f;
            float sx = (db.max.x + body2.max.x) * 0.5f;
            float face = DrawerFaceZ(drawerKit.gameObject, new Vector3(db.max.x - 0.03f, y, db.min.z - 0.5f), db.min.z);
            float tip = face - 0.02f;
            var steel = Resources.Load<Material>("Mats/Mat_Steel");
            float stapleW = Mathf.Max(0.012f, body2.max.x - db.max.x - 0.006f);
            Slab("Kit_LockerStaple", new Vector3(sx, y, (body2.min.z + tip) * 0.5f), new Vector3(stapleW, 0.1f, body2.min.z - tip), steel);
            StripColliders(FindLocal("Kit_LockerStaple"));
            const float haspLen = 0.16f;
            Slab("Kit_LockerHasp", new Vector3(sx + stapleW * 0.5f - haspLen * 0.5f, y, face - 0.006f), new Vector3(haspLen, 0.05f, 0.012f), steel);
            var hasp = FindLocal("Kit_LockerHasp");
            StripColliders(hasp);
            hasp.transform.SetParent(t, true);
            var padlock = HangPadlock("LockerPadlock", sx, y, tip, LockerFlag);
            if (padlock == null)
                return;

            // El código se pide apuntando al candado
            Bounds lb = Encapsulate(padlock);
            lb.Encapsulate(Encapsulate(FindLocal("Kit_LockerStaple")));
            lockGo.transform.SetPositionAndRotation(lb.center, Quaternion.identity);
            Vector3 ps = lockGo.transform.parent != null ? lockGo.transform.parent.lossyScale : Vector3.one;
            lockGo.transform.localScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z);
            HideSelf(lockGo);
            var lbox = lockGo.GetComponent<BoxCollider>();
            if (lbox != null)
            {
                lbox.isTrigger = true;
                lbox.center = Vector3.zero;
                lbox.size = lb.size + Vector3.one * 0.04f;
            }
        }

        // Frente del cajón a la altura del candado (la manija sobresale más que la chapa)
        private static float DrawerFaceZ(GameObject drawer, Vector3 from, float fallback)
        {
            var mf = drawer.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                return fallback;
            var mc = drawer.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            Physics.SyncTransforms();
            float z = fallback;
            foreach (var hit in Physics.RaycastAll(from, Vector3.forward, 1.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == mc)
                    z = hit.point.z;
            }
            Object.DestroyImmediate(mc);
            return z;
        }

        // Batería acostada a lo largo en el piso del cajón, con su modelo grabado y el área de interacción encima
        private static void SeatBatteryIn(GameObject battery, Transform drawer, Vector3 floor)
        {
            var bt = battery.transform;
            if (bt.parent != drawer)
                bt.SetParent(drawer, true);
            bt.rotation = drawer.rotation;
            bt.position = floor + Vector3.up * 0.06f;
            bool wasActive = battery.activeSelf;
            // Las cajas de los renderers solo se miden con el objeto activo
            battery.SetActive(true);
            ItemVisuals.Dress(battery, "battery");
            var vis = bt.Find("Visual_battery");
            if (vis != null)
            {
                vis.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.back) * drawer.rotation;
                var b = ItemVisuals.Bounds(vis.gameObject);
                vis.position += new Vector3(floor.x - b.center.x, floor.y + 0.002f - b.min.y, floor.z - b.center.z);
                var box = battery.GetComponent<BoxCollider>();
                if (box != null && LocalBounds(bt, vis.gameObject, out var lb))
                {
                    box.isTrigger = true;
                    box.center = lb.center;
                    box.size = lb.size + Vector3.one * 0.03f;
                }
            }
            battery.SetActive(wasActive);
        }

        private void PassageSlab(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            if (FindLocal(name) == null)
                Slab(name, pos, scale, mat);
        }

        // Gabinete bajo la pantalla con un cajón (Drawer_Override) que abre la secuencia
        private Vector3? PlaceOverrideCabinet()
        {
            var drawer = FindLocal("Drawer_Override");
            var slide = FindLocal("SlideWall");
            var plate = slide != null ? FindChildNamed(slide.transform, "Plate") : null;
            var pit = FindLocal("PitFloor");
            SequencePuzzleController ctrl = null;
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                ctrl = root.GetComponentInChildren<SequencePuzzleController>(true);
                if (ctrl != null)
                    break;
            }
            if (drawer == null || plate == null || pit == null || ctrl == null)
                return null;

            Bounds pb = Encapsulate(plate.gameObject);
            float floorY = Encapsulate(pit).max.y;
            const float w = 0.8f, h = 0.78f, d = 0.45f, wall = 0.03f, slot = 0.2f;
            Vector3 basePos = new Vector3(pb.center.x, floorY, pb.max.z - d * 0.5f);

            var cabinet = FindLocal("OverrideCabinet");
            if (cabinet == null)
            {
                cabinet = new GameObject("OverrideCabinet");
                cabinet.transform.SetParent(_root, false);
                cabinet.transform.position = basePos;
                var col = cabinet.AddComponent<BoxCollider>();
                col.center = new Vector3(0f, h * 0.5f, 0f);
                col.size = new Vector3(w, h, d);
                var c = cabinet.transform;
                Part(c, "Kit_CabinetSideL", new Vector3(-(w - wall) * 0.5f, h * 0.5f, 0f), new Vector3(wall, h, d), _rust);
                Part(c, "Kit_CabinetSideR", new Vector3((w - wall) * 0.5f, h * 0.5f, 0f), new Vector3(wall, h, d), _rust);
                Part(c, "Kit_CabinetTop", new Vector3(0f, h - wall * 0.5f, 0f), new Vector3(w, wall, d), _rust);
                Part(c, "Kit_CabinetBottom", new Vector3(0f, wall * 0.5f, 0f), new Vector3(w, wall, d), _rust);
                Part(c, "Kit_CabinetBack", new Vector3(0f, h * 0.5f, (d - wall) * 0.5f), new Vector3(w, h, wall), _rust);
                float lower = h - wall - slot;
                Part(c, "Kit_CabinetFront", new Vector3(0f, lower * 0.5f, -(d - wall) * 0.5f), new Vector3(w, lower, wall), _rust);
                Part(c, "Kit_CabinetShelf", new Vector3(0f, lower - wall * 0.5f, 0f), new Vector3(w - wall * 2f, wall, d - wall), _rust);
            }

            // El cajón es el objeto de escena: lo que no es Kit_ (la llave) sobrevive al re-grabado
            drawer.transform.rotation = Quaternion.identity;
            drawer.transform.localScale = Vector3.one;
            drawer.transform.position = basePos + Vector3.up * (h - wall - slot);
            HideSelf(drawer);
            var box = drawer.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.isTrigger = false;
                box.center = new Vector3(0f, slot * 0.5f, -(d - wall) * 0.5f);
                box.size = new Vector3(w, slot, wall);
            }
            var t = drawer.transform;
            if (FindChildNamed(t, "Kit_DrawerTray") == null)
            {
                float inner = w - wall * 2f - 0.01f, deep = d - wall - 0.02f;
                // Fondo oscuro: la llave chica se distingue al abrir
                Part(t, "Kit_DrawerTray", new Vector3(0f, 0.0075f, 0f), new Vector3(inner, 0.015f, deep), Resources.Load<Material>("Mats/Mat_Corridor"));
                Part(t, "Kit_DrawerFront", new Vector3(0f, slot * 0.5f, -(d - wall) * 0.5f), new Vector3(w - 0.01f, slot - 0.01f, wall), _rust);
                Part(t, "Kit_DrawerSideL", new Vector3(-inner * 0.5f, slot * 0.35f, 0f), new Vector3(0.012f, slot * 0.7f, deep), _wood);
                Part(t, "Kit_DrawerSideR", new Vector3(inner * 0.5f, slot * 0.35f, 0f), new Vector3(0.012f, slot * 0.7f, deep), _wood);
                Part(t, "Kit_DrawerBack", new Vector3(0f, slot * 0.35f, deep * 0.5f), new Vector3(inner, slot * 0.7f, 0.012f), _wood);
                Part(t, "Kit_DrawerHandle", new Vector3(0f, slot * 0.5f, -d * 0.5f - 0.012f), new Vector3(0.16f, 0.025f, 0.03f), Resources.Load<Material>("Mats/Mat_Steel"));
            }
            var slider = drawer.GetComponent<FlagDrawer>();
            if (slider == null)
                slider = drawer.AddComponent<FlagDrawer>();
            Vector3 open = Vector3.back * (d - wall - 0.08f);
            if (t.parent != null)
                open = t.parent.InverseTransformVector(open);
            slider.Configure(ctrl.SuccessFlagId, open, 0.9f);

            return new Vector3(basePos.x, floorY, basePos.z - d * 0.5f);
        }

        // Pieza de caja sin collider, en coordenadas locales de parent
        private static GameObject Part(Transform parent, string name, Vector3 local, Vector3 size, Material mat, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            StripColliders(go);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = size;
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null && mat != null)
                rend.sharedMaterial = mat;
            return go;
        }

        private const float GeneratorLength = 1.4f;
        private const float RopePull = 0.45f;

        // Generador portátil: arranque a soga en la tapa del motor y compartimento a la medida de la batería que se levanta
        private void DressGenerator()
        {
            var grey = FindLocal("Generator");
            var prefab = Resources.Load<GameObject>("Kitbash/Generator");
            if (grey == null || prefab == null || FindLocal("Kit_Generator") != null)
                return;
            Vector3 g = grey.transform.position;
            float floor = g.y - grey.transform.lossyScale.y * 0.5f;
            var go = Object.Instantiate(prefab, _root);
            go.name = "Kit_Generator";
            go.transform.rotation = prefab.transform.rotation;
            StripColliders(go);
            var b = Encapsulate(go);
            go.transform.localScale *= GeneratorLength / Mathf.Max(b.size.x, b.size.z);
            b = Encapsulate(go);
            go.transform.position += new Vector3(g.x - b.center.x, floor - b.min.y, g.z - b.center.z);
            b = Encapsulate(go);
            HideSelf(grey);

            // La tapa con el arranque es la punta del motor, en el extremo +X y abajo del tanque
            var rope = FindLocal("Interactable_Rope");
            if (rope != null)
                DressPullStart(rope, new Vector3(b.max.x, b.min.y + b.size.y * 0.36f, b.center.z));
            var slot = FindLocal("Interactable_BatterySlot");
            if (slot != null)
                DressBatteryBay(slot, b);
        }

        private void DressPullStart(GameObject rope, Vector3 cap)
        {
            var t = rope.transform;
            var lever = rope.GetComponent<LeverInteractable>();
            var pivot = FindChildNamed(t, "RopePivot");
            if (lever == null || pivot == null)
                return;
            var rubber = Resources.Load<Material>("Mats/Mat_Rubber");
            var cordMat = Resources.Load<Material>("Mats/Mat_Rope");
            HideSelf(rope);
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
            t.position = cap;
            var box = rope.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = new Vector3(0.04f, 0f, 0f);
                box.size = new Vector3(0.22f, 0.22f, 0.26f);
            }

            // El pivote mira hacia afuera de la tapa: la manija sale por ahí al tirar
            pivot.localRotation = Quaternion.LookRotation(Vector3.right);
            pivot.localPosition = new Vector3(0.05f, 0f, 0f);
            pivot.localScale = Vector3.one;
            var grip = Part(pivot, "Kit_PullGrip", new Vector3(0f, 0f, 0.022f), new Vector3(0.032f, 0.065f, 0.032f), rubber, PrimitiveType.Cylinder);
            grip.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var stem = Part(pivot, "Kit_PullStem", new Vector3(0f, 0f, 0.006f), new Vector3(0.02f, 0.012f, 0.02f), rubber, PrimitiveType.Cylinder);
            stem.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Part(t, "Kit_PullBoss", new Vector3(-0.045f, 0f, 0f), new Vector3(0.012f, 0.05f, 0.05f), rubber, PrimitiveType.Cube);

            var cord = new GameObject("Kit_PullCord").transform;
            cord.SetParent(t, false);
            cord.localPosition = new Vector3(-0.06f, 0f, 0f);
            cord.localRotation = Quaternion.LookRotation(Vector3.right);
            var strand = Part(cord, "Kit_PullStrand", new Vector3(0f, 0f, 0.5f), new Vector3(0.007f, 0.5f, 0.007f), cordMat, PrimitiveType.Cylinder);
            strand.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lever.ConfigurePull(pivot, RopePull, cord);
        }

        private void DressBatteryBay(GameObject slot, Bounds gen)
        {
            var vis = ItemVisuals.Build("battery");
            if (vis == null)
                return;
            var cell = ItemVisuals.Bounds(vis);
            const float wall = 0.015f;
            const float gap = 0.02f;
            float w = cell.size.x + gap * 2f, h = cell.size.y + gap * 2f, d = cell.size.z + gap * 2f;
            var t = slot.transform;
            HideSelf(slot);
            t.localScale = Vector3.one;
            // Apoyado en el piso contra el bastidor del frente, del lado opuesto a la soga
            t.position = new Vector3(gen.center.x - gen.size.x * 0.2f, gen.min.y + h * 0.5f + wall + 0.003f, gen.min.z - d * 0.5f - wall);
            var box = slot.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = Vector3.zero;
                box.size = new Vector3(w + wall * 2f, h + wall * 2f, d + wall);
            }

            // El frente del slot (+Z local) da al jugador; el fondo apoya en el generador
            var steel = Resources.Load<Material>("Mats/Mat_Steel");
            var rubber = Resources.Load<Material>("Mats/Mat_Rubber");
            BayPanel(t, "Panel_Left", new Vector3(-(w + wall) * 0.5f, 0f, 0f), new Vector3(wall, h + wall * 2f, d), steel);
            BayPanel(t, "Panel_Right", new Vector3((w + wall) * 0.5f, 0f, 0f), new Vector3(wall, h + wall * 2f, d), steel);
            BayPanel(t, "Panel_Top", new Vector3(0f, (h + wall) * 0.5f, 0f), new Vector3(w, wall, d), steel);
            BayPanel(t, "Panel_Bottom", new Vector3(0f, -(h + wall) * 0.5f, 0f), new Vector3(w, wall, d), steel);
            BayPanel(t, "Panel_Back", new Vector3(0f, 0f, -(d + wall) * 0.5f), new Vector3(w + wall * 2f, h + wall * 2f, wall), rubber);

            var inserted = FindChildNamed(t, "InsertedBatteryVisual");
            if (inserted == null)
            {
                Object.DestroyImmediate(vis);
                return;
            }
            HideSelf(inserted.gameObject);
            inserted.localPosition = Vector3.zero;
            inserted.localRotation = Quaternion.identity;
            inserted.localScale = Vector3.one;
            vis.name = "Kit_BatteryInserted";
            vis.transform.SetParent(inserted, false);
            vis.transform.localPosition = Vector3.zero;
        }

        private static void BayPanel(Transform bay, string name, Vector3 local, Vector3 size, Material mat)
        {
            var p = FindChildNamed(bay, name);
            if (p == null)
                return;
            p.localPosition = local;
            p.localRotation = Quaternion.identity;
            p.localScale = size;
            Paint(p.gameObject, mat);
        }

        // Los paneles pasan a ser botones de piso; cada cable llega al frente del gabinete sin cruzarse
        private void WireFloorButtons(Vector3 cabinetFront)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/button-floor-round");
            // Un solo material; el color de cada panel llega por bloque de propiedades
            var neonMat = Resources.Load<Material>("Mats/Mat_Neon");
            var panels = new System.Collections.Generic.List<SequencePanelInteractable>();
            foreach (var root in gameObject.scene.GetRootGameObjects())
                panels.AddRange(root.GetComponentsInChildren<SequencePanelInteractable>(true));
            if (prefab == null || panels.Count == 0)
                return;
            float floorY = cabinetFront.y;
            const float pad = 0.55f, cable = 0.03f, lift = 0.012f;

            // Por lado del gabinete: el más cercano entra por afuera, el lejano rodea por adentro
            panels.Sort((a, b) => (a.transform.position - cabinetFront).sqrMagnitude.CompareTo((b.transform.position - cabinetFront).sqrMagnitude));
            var seen = new System.Collections.Generic.Dictionary<int, int>();
            for (int i = 0; i < panels.Count; i++)
            {
                var p = panels[i];
                var go = p.gameObject;
                var grey = go.GetComponent<MeshRenderer>();
                Color color = grey != null && grey.sharedMaterial != null && grey.sharedMaterial.HasProperty("_BaseColor")
                    ? grey.sharedMaterial.GetColor("_BaseColor") : Color.white;

                Vector3 pos = go.transform.position;
                go.transform.position = new Vector3(pos.x, floorY, pos.z);
                go.transform.rotation = Quaternion.identity;
                Vector3 ps = go.transform.parent != null ? go.transform.parent.lossyScale : Vector3.one;
                go.transform.localScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z);
                HideSelf(go);
                var col = go.GetComponent<BoxCollider>();
                if (col != null)
                {
                    col.center = new Vector3(0f, 0.2f, 0f);
                    col.size = new Vector3(pad + 0.05f, 0.4f, pad + 0.05f);
                }

                var button = Object.Instantiate(prefab, go.transform);
                button.name = "Kit_FloorButton";
                StripColliders(button);
                button.transform.localPosition = Vector3.zero;
                button.transform.localRotation = prefab.transform.rotation;
                var bb = Encapsulate(button);
                if (bb.size.x > 0.01f)
                    button.transform.localScale *= pad / bb.size.x;
                FitToHeight(button, floorY + 0.002f, Encapsulate(button).size.y);
                Paint(button, _rust);
                var neon = new System.Collections.Generic.List<Renderer>();
                var cap = FindChildNamed(button.transform, "button");
                if (cap != null && cap.GetComponent<MeshRenderer>() != null)
                {
                    cap.GetComponent<MeshRenderer>().sharedMaterial = neonMat;
                    neon.Add(cap.GetComponent<MeshRenderer>());
                }

                int side = go.transform.position.x < cabinetFront.x ? -1 : 1;
                seen.TryGetValue(side, out int rank);
                seen[side] = rank + 1;
                float portX = cabinetFront.x + side * (rank == 0 ? 0.27f : 0.09f);
                float lane = cabinetFront.z - (rank == 0 ? 0.18f : 0.32f);
                Vector3 start = go.transform.position;
                var path = new System.Collections.Generic.List<Vector3> { start };
                if (rank > 0)
                    path.Add(new Vector3(start.x - side * 0.45f, floorY, start.z));
                path.Add(new Vector3(path[path.Count - 1].x, floorY, lane));
                path.Add(new Vector3(portX, floorY, lane));
                path.Add(new Vector3(portX, floorY, cabinetFront.z));
                for (int k = 0; k + 1 < path.Count; k++)
                {
                    Vector3 a = path[k], b = path[k + 1];
                    // El tramo que sale del botón arranca en su borde, no debajo
                    if (k == 0)
                        a = Vector3.MoveTowards(a, b, pad * 0.45f);
                    float length = Vector3.Distance(a, b);
                    if (length < 0.01f)
                        continue;
                    var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    seg.name = "Kit_Cable_" + k;
                    StripColliders(seg);
                    seg.transform.SetParent(go.transform, true);
                    seg.transform.position = (a + b) * 0.5f + Vector3.up * lift;
                    seg.transform.rotation = Quaternion.LookRotation(b - a);
                    seg.transform.localScale = Vector3.one;
                    Vector3 ls = seg.transform.lossyScale;
                    seg.transform.localScale = new Vector3(cable / ls.x, (cable * 0.6f) / ls.y, (length + cable) / ls.z);
                    var r = seg.GetComponent<MeshRenderer>();
                    r.sharedMaterial = neonMat;
                    r.shadowCastingMode = ShadowCastingMode.Off;
                    neon.Add(r);
                }

                var glowGo = new GameObject("Kit_NeonGlow");
                glowGo.transform.SetParent(go.transform, false);
                glowGo.transform.position = go.transform.position + Vector3.up * 0.3f;
                var glow = glowGo.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.color = color;
                glow.intensity = 1.6f;
                glow.range = 1.4f;
                glow.shadows = LightShadows.None;
                glow.enabled = false;
                p.Configure(neon.ToArray(), glow, color);
            }
        }

        // Proyector colgado del techo frente a la pantalla, apuntando a su centro; la cinta va en la bandeja de la consola
        private void PlaceProjector()
        {
            var slot = FindLocal("Interactable_ReelSlot");
            var slide = FindLocal("SlideWall");
            var plate = slide != null ? FindChildNamed(slide.transform, "Plate") : null;
            var console = FindLocal("Console");
            var prefab = Resources.Load<GameObject>("Kitbash/Projector");
            if (slot == null || plate == null || prefab == null || FindLocal("Kit_Projector") != null)
                return;
            var item = slot.GetComponent<InteractableBase>();
            string flag = item != null && item.Config != null ? item.Config.successFlagId : null;
            if (string.IsNullOrEmpty(flag))
                return;

            // Tela gris mate: con la luz roja del pozo queda apagada y el proyector la levanta
            Paint(plate.gameObject, Resources.Load<Material>("Mats/Mat_ProjectionScreen"));

            Bounds screen = Encapsulate(plate.gameObject);
            float deskY = console != null ? Encapsulate(console).max.y : Encapsulate(slot).min.y;
            HideSelf(slot);
            ReelDeck(slot, deskY, flag);

            // Del lado de la tela que da a la sala, a la misma distancia que la consola
            Vector3 normal = -slide.transform.forward;
            float throwDist = Vector3.Dot(slot.transform.position - screen.center, normal);
            if (throwDist < 0f)
            {
                normal = -normal;
                throwDist = -throwDist;
            }
            Vector3 hang = screen.center + normal * throwDist;
            float ceiling = RoofYOf(gameObject.scene.name) - RoofThick * 0.5f;
            Physics.SyncTransforms();
            float floorY = Physics.Raycast(new Vector3(hang.x, ceiling - 0.05f, hang.z), Vector3.down, out var down, ceiling + 10f, ~0, QueryTriggerInteraction.Ignore)
                ? down.point.y : 0f;

            var go = Object.Instantiate(prefab, _root);
            go.name = "Kit_Projector";
            StripColliders(go);
            // El modelo trae un mástil: colgado del techo no va
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var rb = r.bounds;
                if (rb.size.y > 2.5f * Mathf.Max(rb.size.x, rb.size.z))
                    DropGo(r.gameObject);
            }
            // La lente mira a +Z del modelo
            Vector3 fwd = -normal;
            fwd.y = 0f;
            fwd.Normalize();
            go.transform.rotation = Quaternion.LookRotation(fwd) * prefab.transform.rotation;
            var b = Encapsulate(go);
            float horiz = Mathf.Max(b.size.x, b.size.z);
            if (horiz > 0.01f)
                go.transform.localScale *= 0.42f / horiz;
            b = Encapsulate(go);
            go.transform.position += new Vector3(hang.x, floorY + ProjectorHeadroom + b.extents.y, hang.z) - b.center;

            b = Encapsulate(go);
            float halfLen = Mathf.Abs(fwd.x) * b.extents.x + Mathf.Abs(fwd.z) * b.extents.z;
            Vector3 lens = b.center + fwd * halfLen;

            float face = Vector3.Dot(normal, Vector3.forward) < 0f ? screen.min.z : screen.max.z;
            var corners = new[]
            {
                new Vector3(screen.min.x, screen.min.y, face), new Vector3(screen.max.x, screen.min.y, face),
                new Vector3(screen.max.x, screen.max.y, face), new Vector3(screen.min.x, screen.max.y, face)
            };

            // Lo justo más arriba para que el borde del pozo no corte la imagen, sin tocar el techo
            float raise = Mathf.Min(ProjectorRise(lens, corners, slide.transform), ceiling - ProjectorMountGap - b.max.y);
            if (raise > 0f)
            {
                go.transform.position += Vector3.up * raise;
                lens += Vector3.up * raise;
                b = Encapsulate(go);
            }

            // Inclinado hacia el centro de la tela
            Vector3 dir = (screen.center - lens).normalized;
            var tilt = Quaternion.FromToRotation(fwd, dir);
            Vector3 pivot = b.center;
            go.transform.rotation = tilt * go.transform.rotation;
            go.transform.position = pivot + tilt * (go.transform.position - pivot);
            lens = pivot + dir * halfLen;
            ProjectorMount(go.transform, pivot, b.extents.y, ceiling);

            var on = new GameObject("ProjectorOn");
            on.transform.SetParent(go.transform, true);
            on.transform.position = lens;
            on.transform.rotation = Quaternion.LookRotation(dir);

            // Cono que alcanza las cuatro esquinas de la pantalla
            float half = 0f;
            foreach (var c in corners)
                half = Mathf.Max(half, Vector3.Angle(dir, c - lens));
            var spot = on.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1f, 0.78f, 0.5f);
            spot.spotAngle = half * 2f + 2f;
            spot.innerSpotAngle = half * 1.6f;
            spot.range = Vector3.Distance(lens, screen.center) + 2f;
            spot.intensity = 320f;
            spot.shadows = LightShadows.None;

            var beam = new GameObject("ProjectorBeam");
            beam.transform.SetParent(on.transform, false);
            beam.transform.position = lens;
            beam.transform.rotation = Quaternion.identity;
            beam.AddComponent<MeshFilter>().sharedMesh = BeamMesh(lens, dir, corners);
            var br = beam.AddComponent<MeshRenderer>();
            br.sharedMaterial = Resources.Load<Material>("Mats/Mat_ProjectorBeam");
            br.shadowCastingMode = ShadowCastingMode.Off;
            br.receiveShadows = false;

            on.SetActive(false);
            go.AddComponent<FlagToggleRoot>().Configure(on, flag);
        }

        private const float ProjectorMaxRise = 2.2f;
        private const float ProjectorRiseStep = 0.05f;
        private const float ProjectorRiseMargin = 0.12f;
        // Debajo del proyector pasa Megan de pie sin tocarlo
        private const float ProjectorHeadroom = 2.3f;
        // Lugar para la placa y la horquilla entre el techo y el proyector
        private const float ProjectorMountGap = 0.3f;

        // Altura extra de la lente hasta ver libres las cuatro esquinas de la pantalla
        private static float ProjectorRise(Vector3 lens, Vector3[] corners, Transform screen)
        {
            Physics.SyncTransforms();
            for (float r = 0f; r <= ProjectorMaxRise; r += ProjectorRiseStep)
            {
                Vector3 from = lens + Vector3.up * r;
                bool clear = true;
                for (int i = 0; i < corners.Length && clear; i++)
                {
                    Vector3 to = corners[i] + (from - corners[i]).normalized * 0.06f;
                    if (Physics.Linecast(from, to, out var hit, ~0, QueryTriggerInteraction.Ignore) && !hit.collider.transform.IsChildOf(screen))
                        clear = false;
                }
                if (clear)
                    return r > 0f ? r + ProjectorRiseMargin : 0f;
            }
            return ProjectorMaxRise;
        }

        // Placa en el techo, barra y horquilla que abraza al proyector
        private void ProjectorMount(Transform projector, Vector3 center, float halfHeight, float ceiling)
        {
            var steel = Resources.Load<Material>("Mats/Mat_Steel");
            var mount = new GameObject("Kit_ProjectorMount");
            mount.transform.SetParent(_root, false);
            mount.transform.position = new Vector3(center.x, ceiling, center.z);
            mount.transform.rotation = Quaternion.Euler(0f, projector.eulerAngles.y, 0f);
            float top = center.y + halfHeight;
            float rod = Mathf.Max(0.05f, ceiling - top);
            Part(mount.transform, "Kit_MountPlate", new Vector3(0f, -0.01f, 0f), new Vector3(0.22f, 0.01f, 0.22f), steel, PrimitiveType.Cylinder);
            Part(mount.transform, "Kit_MountRod", new Vector3(0f, -rod * 0.5f, 0f), new Vector3(0.035f, rod * 0.5f, 0.035f), steel, PrimitiveType.Cylinder);
            Part(mount.transform, "Kit_MountYoke", new Vector3(0f, -rod, 0f), new Vector3(0.3f, 0.02f, 0.06f), steel);
        }

        // Bandeja sobre la consola donde se apoya la cinta; aparece con la misma flag que prende el proyector
        private void ReelDeck(GameObject slot, float deskY, string flag)
        {
            var steel = Resources.Load<Material>("Mats/Mat_Steel");
            var rubber = Resources.Load<Material>("Mats/Mat_Rubber");
            var deck = new GameObject("Kit_ReelDeck");
            deck.transform.SetParent(_root, false);
            deck.transform.position = new Vector3(slot.transform.position.x, deskY, slot.transform.position.z);
            deck.transform.rotation = Quaternion.Euler(0f, slot.transform.eulerAngles.y, 0f);
            Vector3 s = slot.transform.lossyScale;
            float plat = ItemVisuals.SizeOf("tape") * 1.1f;
            Part(deck.transform, "Kit_DeckBase", new Vector3(0f, 0.02f, 0f), new Vector3(Mathf.Max(s.x, plat + 0.04f), 0.04f, Mathf.Max(s.z, plat + 0.04f)), steel);
            Part(deck.transform, "Kit_DeckPlatter", new Vector3(0f, 0.043f, 0f), new Vector3(plat, 0.003f, plat), rubber, PrimitiveType.Cylinder);
            Part(deck.transform, "Kit_DeckSpindle", new Vector3(0f, 0.06f, 0f), new Vector3(0.012f, 0.02f, 0.012f), steel, PrimitiveType.Cylinder);

            var loaded = new GameObject("ReelLoaded");
            loaded.transform.SetParent(deck.transform, false);
            var reel = ItemVisuals.Build("tape");
            if (reel != null)
            {
                reel.name = "ProjectorReel";
                StripColliders(reel);
                reel.transform.SetParent(loaded.transform, false);
                var rb = Encapsulate(reel);
                reel.transform.position += new Vector3(deck.transform.position.x - rb.center.x, deskY + 0.046f - rb.min.y, deck.transform.position.z - rb.center.z);
            }
            loaded.SetActive(false);
            deck.AddComponent<FlagToggleRoot>().Configure(loaded, flag);
        }

        // Haz tenue de la lente a la pantalla: vértices con alfa que se apaga lejos
        private static Mesh BeamMesh(Vector3 lens, Vector3 dir, Vector3[] far)
        {
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
            Vector3 up = Vector3.Cross(dir, right);
            var verts = new Vector3[8];
            var cols = new Color[8];
            Vector2[] near = { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f) };
            for (int i = 0; i < 4; i++)
            {
                verts[i] = right * (near[i].x * 0.035f) + up * (near[i].y * 0.025f);
                verts[i + 4] = far[i] - lens;
                cols[i] = new Color(1f, 0.8f, 0.55f, 0.06f);
                cols[i + 4] = new Color(1f, 0.8f, 0.55f, 0f);
            }
            var tris = new int[24];
            for (int i = 0, t = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                tris[t++] = i; tris[t++] = i + 4; tris[t++] = j + 4;
                tris[t++] = i; tris[t++] = j + 4; tris[t++] = j;
            }
            var mesh = new Mesh { name = "ProjectorBeam" };
            mesh.vertices = verts;
            mesh.colors = cols;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
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

            if (FindLocal("Kit_oliveladderpoly") == null)
                Put("oliveladderpoly", new Vector3(0f, 0f, 2.4f), 0f, _rust, y);
            // La malla no es legible en el build: se apoya al grabar y queda en la escena
            var ladder = FindLocal("Kit_oliveladderpoly");
            if (ladder != null && !Application.isPlaying)
                LeanInto(ladder, new Vector3(0f, RoofYMezz - RoofThick * 0.5f + 0.1f, 2.2f), Vector3.back);
        }

        // Apoya la escalera por su geometría: la punta entra al hueco y la base se abre hacia lean
        private static void LeanInto(GameObject go, Vector3 top, Vector3 lean)
        {
            FitToHeight(go, 0f, top.y);
            LadderEnds(go, out Vector3 tip, out Vector3 foot);
            Vector3 open = Vector3.ProjectOnPlane(foot - tip, Vector3.up);
            if (open.sqrMagnitude > 0.0001f)
                go.transform.RotateAround(tip, Vector3.up, Vector3.SignedAngle(open, lean, Vector3.up));
            LadderEnds(go, out tip, out foot);
            go.transform.position += new Vector3(top.x - tip.x, 0f, top.z - tip.z);
            FitToHeight(go, 0f, top.y);
        }

        private static void LadderEnds(GameObject go, out Vector3 tip, out Vector3 foot)
        {
            var b = Encapsulate(go);
            float band = b.size.y * 0.05f;
            Vector3 hi = Vector3.zero, lo = Vector3.zero;
            int nHi = 0, nLo = 0;
            foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null)
                    continue;
                var verts = mf.sharedMesh.vertices;
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 w = mf.transform.TransformPoint(verts[i]);
                    if (w.y > b.max.y - band) { hi += w; nHi++; }
                    else if (w.y < b.min.y + band) { lo += w; nLo++; }
                }
            }
            tip = nHi > 0 ? hi / nHi : b.center;
            foot = nLo > 0 ? lo / nLo : b.center;
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
            // Tapa y dintel del mismo material que la pared: no se recortan como un parche claro
            var wall = FindLocal("Wall_N_L");
            var wallRend = wall != null ? wall.GetComponent<MeshRenderer>() : null;
            var mat = wallRend != null && wallRend.sharedMaterial != null ? wallRend.sharedMaterial : _plaster;
            if (mat != null)
                Paint(cap, mat);

            var lintel = FindLocal("Kit_DoorLintel");
            if (lintel == null)
                Slab("Kit_DoorLintel", new Vector3(0f, 2.85f, 6.1f), new Vector3(2.2f, 0.5f, 0.4f), mat);
            else if (mat != null)
                Paint(lintel, mat);
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
    }
}
