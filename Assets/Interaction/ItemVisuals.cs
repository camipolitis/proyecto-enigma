using UnityEngine;
using UnityEngine.Rendering;

namespace Enigma.Interaction
{
    // Modelo de cada ítem: el mismo en el mundo y en la inspección del inventario.
    public static class ItemVisuals
    {
        static Shader _lit;
        static Font _font;

        // Medida mayor del objeto en metros
        public static float SizeOf(string id)
        {
            if (string.IsNullOrEmpty(id))
                return 0.3f;
            if (id == "override_key")
                return 0.13f;
            if (id == "credential")
                return 0.12f;
            if (id == "battery")
                return 0.34f;
            if (id == "fuse")
                return 0.11f;
            if (id == "tape")
                return 0.2f;
            if (id.StartsWith("print_"))
                return 0.5f;
            return 0.3f;
        }

        public static bool HasCustom(string id)
        {
            return id == "override_key" || id == "battery" || id == "tape" || id == "fuse"
                || id == "credential" || (id != null && id.StartsWith("print_"));
        }

        // Devuelve el modelo centrado en el origen, sin colliders, con la medida de SizeOf
        public static GameObject Build(string id, string label = null)
        {
            GameObject go;
            if (id == "override_key")
                go = Kit("Key4");
            else if (id == "credential")
                go = Kit("Pickup_KeyCard");
            else if (id == "battery")
                go = Kit("Battery");
            else if (id == "tape")
                go = Reel();
            else if (id == "fuse")
                go = Fuse();
            else if (id != null && id.StartsWith("print_"))
                go = Print(label ?? id.Substring(6));
            else
                go = Kit("UI_Paper");
            if (go == null)
                return null;
            go.name = "Visual_" + id;
            Strip(go);
            Fit(go, SizeOf(id));
            return go;
        }

        // Reemplaza el placeholder de un pickup por su modelo, manteniendo el área de interacción
        public static void Dress(GameObject pickup, string id)
        {
            if (pickup == null || !HasCustom(id) || id == "credential" || id == "fuse")
                return;
            if (pickup.transform.Find("Visual_" + id) != null)
                return;

            Vector3 s = pickup.transform.localScale;
            if (s != Vector3.one)
            {
                foreach (var box in pickup.GetComponents<BoxCollider>())
                {
                    box.size = Vector3.Scale(box.size, s);
                    box.center = Vector3.Scale(box.center, s);
                }
                pickup.transform.localScale = Vector3.one;
            }

            for (int i = pickup.transform.childCount - 1; i >= 0; i--)
            {
                var c = pickup.transform.GetChild(i);
                if (c.name.StartsWith("Kit_"))
                {
                    c.gameObject.SetActive(false);
                    Object.Destroy(c.gameObject);
                }
            }
            var own = pickup.GetComponent<MeshRenderer>();
            if (own != null)
                own.enabled = false;

            var vis = Build(id);
            if (vis == null)
            {
                if (own != null)
                    own.enabled = true;
                return;
            }
            vis.transform.SetParent(pickup.transform, false);
            vis.transform.localPosition = Vector3.zero;
            // Apoyado: la base del modelo a la altura de la base del placeholder
            var b = Bounds(vis);
            float baseY = pickup.transform.position.y - s.y * 0.5f;
            vis.transform.position += Vector3.up * (baseY - b.min.y + 0.002f);
        }

        static GameObject Kit(string kit)
        {
            var prefab = Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return null;
            var inst = Object.Instantiate(prefab);
            var root = new GameObject();
            inst.transform.SetParent(root.transform, false);
            inst.transform.localRotation = prefab.transform.rotation;
            var b = Bounds(root);
            inst.transform.localPosition -= b.center;
            return root;
        }

        static GameObject Reel()
        {
            var root = new GameObject();
            var metal = Mat(new Color(0.55f, 0.56f, 0.58f), 0.85f, 0.55f);
            var tape = Mat(new Color(0.16f, 0.1f, 0.07f), 0.1f, 0.75f);
            var hub = Mat(new Color(0.12f, 0.12f, 0.12f), 0.3f, 0.4f);
            Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 0f), new Vector3(0.2f, 0.0015f, 0.2f), metal);
            Part(root, PrimitiveType.Cylinder, new Vector3(0f, -0.012f, 0f), new Vector3(0.2f, 0.0015f, 0.2f), metal);
            Part(root, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.15f, 0.011f, 0.15f), tape);
            Part(root, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.05f, 0.0135f, 0.05f), hub);
            // Ventanas de la bobina para que se lea como carrete
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Cos(a) * 0.062f, 0.0128f, Mathf.Sin(a) * 0.062f);
                Part(root, PrimitiveType.Cylinder, p, new Vector3(0.045f, 0.0004f, 0.045f), hub);
            }
            var tail = Part(root, PrimitiveType.Cube, new Vector3(0.09f, 0f, 0.03f), new Vector3(0.05f, 0.018f, 0.0015f), tape);
            tail.transform.localRotation = Quaternion.Euler(0f, -25f, 0f);
            return root;
        }

        static GameObject Fuse()
        {
            var root = new GameObject();
            var glass = Mat(new Color(0.18f, 1f, 0.22f, 1f), 0f, 0.9f);
            glass.EnableKeyword("_EMISSION");
            glass.SetColor("_EmissionColor", new Color(0.1f, 0.9f, 0.15f) * 1.5f);
            var metal = Mat(new Color(0.75f, 0.75f, 0.72f), 0.9f, 0.6f);
            var body = Part(root, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.022f, 0.035f, 0.022f), glass);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var capA = Part(root, PrimitiveType.Cylinder, new Vector3(0.042f, 0f, 0f), new Vector3(0.025f, 0.012f, 0.025f), metal);
            capA.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var capB = Part(root, PrimitiveType.Cylinder, new Vector3(-0.042f, 0f, 0f), new Vector3(0.025f, 0.012f, 0.025f), metal);
            capB.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            return root;
        }

        static GameObject Print(string number)
        {
            var root = new GameObject();
            var paper = Mat(new Color(0.95f, 0.92f, 0.82f), 0f, 0.15f);
            Part(root, PrimitiveType.Cube, Vector3.zero, new Vector3(0.21f, 0.002f, 0.29f), paper);

            if (_font == null)
                _font = Resources.Load<Font>("Fonts/SegoeScript");
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var txt = new GameObject("Print_Number");
            txt.transform.SetParent(root.transform, false);
            txt.transform.localPosition = new Vector3(0f, 0.0015f, 0f);
            txt.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var tm = txt.AddComponent<TextMesh>();
            tm.text = number;
            tm.font = _font;
            tm.fontSize = 96;
            tm.characterSize = 0.012f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.72f, 0.08f, 0.06f, 1f);
            var mr = txt.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _font.material;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            return root;
        }

        static GameObject Part(GameObject root, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
        {
            var p = GameObject.CreatePrimitive(type);
            p.transform.SetParent(root.transform, false);
            p.transform.localPosition = pos;
            p.transform.localScale = scale;
            p.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return p;
        }

        static Material Mat(Color c, float metallic, float smooth)
        {
            if (_lit == null)
                _lit = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(_lit != null ? _lit : Shader.Find("Standard"));
            m.color = c;
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Metallic"))
                m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness"))
                m.SetFloat("_Smoothness", smooth);
            return m;
        }

        static void Strip(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
                Object.Destroy(c);
            }
        }

        static void Fit(GameObject go, float size)
        {
            var b = Bounds(go);
            float m = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            if (m > 0.0001f)
                go.transform.localScale *= size / m;
        }

        public static Bounds Bounds(GameObject go)
        {
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            if (rends.Length == 0)
                return new Bounds(go.transform.position, Vector3.zero);
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            return b;
        }
    }
}
