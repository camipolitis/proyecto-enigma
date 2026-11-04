using UnityEngine;
using UnityEngine.Rendering;

namespace Enigma.Interaction
{
    // Modelo de cada ítem: el mismo en el mundo y en la inspección del inventario.
    public static class ItemVisuals
    {
        static Font _marker;
        static Font _note;
        static readonly System.Collections.Generic.Dictionary<Font, Material> _textMats = new System.Collections.Generic.Dictionary<Font, Material>();

        // Medida mayor en el mundo, cuando difiere de la de inspección (la hoja impresa es A4)
        public static float WorldSizeOf(string id)
        {
            if (id != null && id.StartsWith("print_"))
                return 0.297f;
            return SizeOf(id);
        }

        // Medida mayor del objeto en metros
        public static float SizeOf(string id)
        {
            if (string.IsNullOrEmpty(id))
                return 0.3f;
            if (id == "override_key")
                return 0.13f;
            if (id == "credential")
                return 0.0856f;
            if (id == "battery")
                return 0.34f;
            if (id == "fuse")
                return 0.11f;
            if (id == "tape")
                return 0.2f;
            if (id.StartsWith("print_"))
                return 0.5f;
            if (id == "painting")
                return 0.6f;
            return 0.3f;
        }

        public static bool HasCustom(string id)
        {
            return id == "override_key" || id == "battery" || id == "tape" || id == "fuse"
                || id == "credential" || id == "painting" || (id != null && id.StartsWith("print_"));
        }

        // Devuelve el modelo centrado en el origen, sin colliders, con la medida de SizeOf
        public static GameObject Build(string id, string label = null, float size = 0f)
        {
            GameObject go;
            if (id == "override_key")
                go = Kit("Key4");
            else if (id == "credential")
                go = Kit("Pickup_CredentialCard");
            else if (id == "battery")
                go = Kit("Battery");
            else if (id == "tape")
                go = Reel();
            else if (id == "fuse")
                go = Fuse();
            else if (id != null && id.StartsWith("print_"))
                go = Print(label ?? id.Substring(6));
            else if (id == "painting")
                go = Painting();
            else
                go = Kit("UI_Paper");
            if (go == null)
                return null;
            go.name = "Visual_" + id;
            Strip(go);
            Fit(go, size > 0.0001f ? size : SizeOf(id));
            return go;
        }

        // En Play el texto de los modelos grabados pasa al material que respeta la profundidad
        public static void WorldText(GameObject root)
        {
            if (root == null || !Application.isPlaying)
                return;
            foreach (var tm in root.GetComponentsInChildren<TextMesh>(true))
            {
                var mr = tm.GetComponent<MeshRenderer>();
                if (tm.font != null && mr != null)
                    mr.sharedMaterial = TextMat(tm.font);
            }
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
                    if (Application.isPlaying)
                        Object.Destroy(c.gameObject);
                    else
                        Object.DestroyImmediate(c.gameObject);
                }
            }
            var own = pickup.GetComponent<MeshRenderer>();
            if (own != null)
                own.enabled = false;

            var vis = Build(id, null, WorldSizeOf(id));
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
            var metal = Mat("Mat_Steel");
            var tape = Mat("Mat_ReelTape");
            var hub = Mat("Mat_Rubber");
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
            var glass = Resources.Load<Material>("Mats/Mat_FuseGlass");
            var metal = Resources.Load<Material>("Mats/Mat_FuseCap");
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
            var paper = Mat("Mat_PrintPaper");
            Part(root, PrimitiveType.Cube, Vector3.zero, new Vector3(0.21f, 0.002f, 0.29f), paper);

            var txt = new GameObject("Print_Number");
            txt.transform.SetParent(root.transform, false);
            txt.transform.localPosition = new Vector3(0f, 0.0015f, 0f);
            txt.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var tm = txt.AddComponent<TextMesh>();
            tm.text = number;
            tm.font = MarkerFont;
            tm.fontSize = 96;
            tm.characterSize = 0.009f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.72f, 0.08f, 0.06f, 1f);
            var mr = txt.GetComponent<MeshRenderer>();
            mr.sharedMaterial = FontMat(tm.font);
            mr.shadowCastingMode = ShadowCastingMode.Off;
            return root;
        }

        // Marcador rojo de las impresiones
        public static Font MarkerFont => _marker != null ? _marker : (_marker = LoadFont("Fonts/GiveYouGlory-Regular"));

        // Letra de la nota del dorso del cuadro
        public static Font NoteFont => _note != null ? _note : (_note = LoadFont("Fonts/CedarvilleCursive-Regular"));

        static Font LoadFont(string path)
        {
            var f = Resources.Load<Font>(path);
            return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // Los modelos ya grabados conservan su TextMesh: se les pone la fuente vigente
        public static void Refont(GameObject root)
        {
            if (root == null)
                return;
            foreach (var tm in root.GetComponentsInChildren<TextMesh>(true))
            {
                Font f = tm.name == "Print_Number" ? MarkerFont : tm.name == "Painting_Note" ? NoteFont : null;
                if (f == null || tm.font == f)
                    continue;
                tm.font = f;
                var mr = tm.GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.sharedMaterial = FontMat(f);
            }
        }

        // Frente: la pintura. Dorso: papel madera con la nota de Panizza, se lee al girar
        static GameObject Painting()
        {
            var root = new GameObject();
            // Proporción del óleo original (244 x 183 cm): la textura se importa cuadrada
            float w = 1f;
            float h = 0.75f;
            const float t = 0.04f;
            const float bar = 0.06f;

            var wood = Mat("Mat_FrameWood");
            Part(root, PrimitiveType.Cube, new Vector3(0f, h * 0.5f + bar * 0.5f, 0f), new Vector3(w + bar * 2f, bar, t), wood);
            Part(root, PrimitiveType.Cube, new Vector3(0f, -h * 0.5f - bar * 0.5f, 0f), new Vector3(w + bar * 2f, bar, t), wood);
            Part(root, PrimitiveType.Cube, new Vector3(w * 0.5f + bar * 0.5f, 0f, 0f), new Vector3(bar, h, t), wood);
            Part(root, PrimitiveType.Cube, new Vector3(-w * 0.5f - bar * 0.5f, 0f, 0f), new Vector3(bar, h, t), wood);

            Part(root, PrimitiveType.Quad, new Vector3(0f, 0f, -t * 0.25f), new Vector3(w, h, 1f), Mat("Mat_Canvas"));

            var back = Part(root, PrimitiveType.Quad, new Vector3(0f, 0f, t * 0.25f), new Vector3(w, h, 1f), Mat("Mat_Kraft"));
            back.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var txt = new GameObject("Painting_Note");
            txt.transform.SetParent(root.transform, false);
            txt.transform.localPosition = new Vector3(0f, 0.04f, t * 0.25f + 0.002f);
            txt.transform.localRotation = Quaternion.Euler(0f, 180f, -4f);
            var tm = txt.AddComponent<TextMesh>();
            tm.text = "Lo que hoy se susurra en este cuarto\nmañana lo oirán todas las banderas.\n"
                + "Querrán torcer el tiempo a su favor\ny el cielo, sempiterno, guardará la ceniza;\n"
                + "cada ciudad será un cadáver que todavía respira.\n"
                + "No es pigricia lo que me detiene:\nes saber que ya empezó.\n\n— Panizza";
            tm.font = NoteFont;
            tm.fontSize = 64;
            tm.characterSize = 0.0052f;
            tm.lineSpacing = 0.95f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.97f, 0.93f, 0.82f, 1f);
            var mr = txt.GetComponent<MeshRenderer>();
            mr.sharedMaterial = FontMat(tm.font);
            mr.shadowCastingMode = ShadowCastingMode.Off;
            return root;
        }

        // Fuera de Play queda el material de la fuente (no se embebe nada en la escena)
        static Material FontMat(Font font)
        {
            return Application.isPlaying ? TextMat(font) : font.material;
        }

        // Material de TextMesh que queda tapado por lo que tenga delante
        public static Material TextMat(Font font)
        {
            if (_textMats.TryGetValue(font, out var cached) && cached != null)
                return cached;
            var shader = Shader.Find("Enigma/WorldText");
            if (shader == null)
                return font.material;
            var mat = new Material(shader) { mainTexture = font.material.mainTexture };
            _textMats[font] = mat;
            return mat;
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

        static Material Mat(string name) => Resources.Load<Material>("Mats/" + name);

        static void Strip(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
                if (Application.isPlaying)
                    Object.Destroy(c);
                else
                    Object.DestroyImmediate(c);
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
