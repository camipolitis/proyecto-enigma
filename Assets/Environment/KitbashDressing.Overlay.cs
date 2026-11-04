using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Enigma.Environment
{
    // Modelos Kenney encima del greybox
    public partial class KitbashDressing
    {
        private void OverlayNamed()
        {
            Overlay("Decor_Sink", "Bathroom_Sink", _rust);

            OverlayChild("Metal_A", "metal-panel", _rust);
            OverlayChild("Metal_B", "metal-panel-screws", _rust);
            OverlayChild("Metal_C", "metal-panel-narrow", _rust);
            OverlayChild("Interactable_ElectricalBox", "ElectricalBox", null, false);
            DressCredential();

            OverlayDoor("Interactable_Door");
            OverlayDoor("Interactable_DoorN3");
            OverlayDoor("Interactable_OrigenDoor");
            OverlayPapers();

            PaintPrefix("Crate_", _wood);
            PaintPrefix("Debris_", _rust);
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

        private void OverlayPapers()
        {
            var scene = gameObject.scene;
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.gameObject.scene != scene)
                    continue;
                if (!t.name.StartsWith("Doc_") && t.name != "Interactable_Note")
                    continue;
                if (t.name.IndexOf("Cassette") >= 0)
                    continue;
                // La hoja apoya plana: el modelo doblado se aplasta a la caja del interactable, medida sin el giro
                bool fresh = t.Find("Kit_UI_Paper") == null;
                var rot = t.rotation;
                t.rotation = Quaternion.identity;
                OverlayChildOn(t.gameObject, "UI_Paper", _paper, false, 0f);
                var kit = t.Find("Kit_UI_Paper");
                if (fresh && kit != null)
                {
                    Vector3 s = t.lossyScale;
                    FitToBox(kit.gameObject, t.position.y - Mathf.Abs(s.y) * 0.5f, Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
                    var b = Encapsulate(kit.gameObject);
                    kit.position += new Vector3(t.position.x - b.center.x, 0f, t.position.z - b.center.z);
                }
                t.rotation = rot;
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
            if (paint)
                PaintKit(go, kit, mat);
            // La rotación del prefab ya es la de pie: solo se ajusta la altura
            if (maxH > 0.01f)
                FitToHeight(go, pos.y, maxH);
            else
                FitToHeight(go, pos.y, Encapsulate(go).size.y);
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
            if (paint)
                PaintKit(go, kit, mat);
            if (maxH > 0.01f)
                FitToHeight(go, pos.y, maxH);
            if (ShouldBlock(kit))
                SolidShell(go);
            return true;
        }

        private void OverlayOrigenLeaf(GameObject src)
        {
            // La hoja visible ocupa la caja del interactable (la dimensiona FrameOrigenDoor)
            Vector3 size = src.transform.lossyScale;
            float floorY = src.transform.position.y - size.y * 0.5f;
            var existing = src.transform.Find("Kit_wall-window-shutters");
            if (existing != null)
            {
                FitToBox(existing.gameObject, floorY, size.x, size.y, size.z);
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
            FitToBox(go, floorY, size.x, size.y, size.z);
            HideSelf(src);
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

        // Tarjeta a tamaño real: sin ajuste a la altura del placeholder
        private void DressCredential()
        {
            var src = FindLocal("Interactable_Credential");
            if (src == null)
                return;
            const string kitName = "Kit_Pickup_CredentialCard";
            var already = src.transform.Find(kitName);
            if (already != null && already.gameObject.activeSelf)
            {
                HideSelf(src);
                return;
            }
            var prefab = Resources.Load<GameObject>("Kitbash/Pickup_CredentialCard");
            if (prefab == null)
                return;
            var go = Object.Instantiate(prefab, src.transform);
            go.name = kitName;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var s = src.transform.lossyScale;
            go.transform.localScale = new Vector3(
                s.x > 0.01f ? 1f / s.x : 1f,
                s.y > 0.01f ? 1f / s.y : 1f,
                s.z > 0.01f ? 1f / s.z : 1f);
            StripColliders(go);
            HideSelf(src);
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
            FitToHeight(go, Encapsulate(go).min.y, h);
        }
    }
}
