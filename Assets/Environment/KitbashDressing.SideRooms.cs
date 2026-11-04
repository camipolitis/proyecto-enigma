using Enigma.Interaction;
using UnityEngine;
using UnityEngine.Rendering;

namespace Enigma.Environment
{
    // Cuartitos con puerta propia: baño en el rincón suroeste de N1, oficina de vidrio contra la pared este de N2.
    // Sin prefijo Kit_ ni Wall_: conservan la colisión, no los estira RaiseWalls ni los toman los caños.
    public partial class KitbashDressing
    {
        // Interior del baño; la zona de uso del cuaderno baja hasta debajo del piso para incluir los pies
        public static readonly Bounds BathInside = new Bounds(new Vector3(-8.625f, 1f, -8.65f), new Vector3(2.45f, 3f, 2.4f));
        public static readonly Bounds OfficeInside = new Bounds(new Vector3(6.475f, 1.2f, -0.675f), new Vector3(2.75f, 2.4f, 3.25f));

        private const float BathWall = 0.1f;
        private const float BathTop = 2.5f;
        private const float BathDoorZ = -8.4f;
        private const float BathDoorW = 0.8f;
        private const float BathDoorH = 2.05f;
        private const float BathTileTop = 1.2f;

        private const float OfficeWall = 0.06f;
        private const float OfficeKick = 0.9f;
        private const float OfficeTop = 2.35f;
        // El techo queda debajo del caño de la pared este (la válvula baja hasta 2.49)
        private const float OfficeRoof = 0.07f;
        private const float OfficeDoorZ = -0.7f;
        private const float OfficeDoorW = 0.9f;
        private const float OfficeDoorH = 2.1f;
        private const int GlassLayer = 1;

        private void SideRooms(string sceneName)
        {
            if (Application.isPlaying)
                return;
            if (sceneName == "Level01_Room")
                BuildBath();
            else if (sceneName == "Level02_Lab")
                BuildOffice();
        }

        private void BuildBath()
        {
            var wall = Mat("Mat_PlasterWall");
            var roof = Mat("Mat_Corridor");
            var wood = Mat("Mat_DarkWood");
            var steel = Mat("Mat_Steel");
            float xw = -9.85f, xe = -7.4f, zs = -9.85f, zn = -7.45f;
            float t = BathWall, h = BathTop;
            float xm = xe + t * 0.5f;
            float d0 = BathDoorZ - BathDoorW * 0.5f, d1 = BathDoorZ + BathDoorW * 0.5f;

            SideSlab("Bath_WallN", new Vector3((xw + xe + t) * 0.5f, h * 0.5f, zn + t * 0.5f), new Vector3(xe + t - xw, h, t), wall, true);
            SideSlab("Bath_WallE_S", new Vector3(xm, h * 0.5f, (zs + d0) * 0.5f), new Vector3(t, h, d0 - zs), wall, true);
            SideSlab("Bath_WallE_N", new Vector3(xm, h * 0.5f, (d1 + zn) * 0.5f), new Vector3(t, h, zn - d1), wall, true);
            SideSlab("Bath_WallE_Lintel", new Vector3(xm, (BathDoorH + h) * 0.5f, BathDoorZ), new Vector3(t, h - BathDoorH, BathDoorW), wall, true);
            SideSlab("Bath_Roof", new Vector3((xw + xe + t) * 0.5f, h + 0.03f, (zs + zn + t) * 0.5f), new Vector3(xe + t - xw, 0.06f, zn + t - zs), roof, true);
            PlaceDecal("Kit_Leak_Bath", "Docs/Leaking001_Color", "Docs/Leaking001_Opacity", new Vector3(-9.84f, 1.3f, -8.95f), new Vector3(0f, 90f, 0f), new Vector3(1.0f, 1.7f, 1f));

            // Azulejos hasta media altura por dentro y piso de baldosas; la textura de pared abarca 1.8 m y la del piso 1.4 m
            var tile = Mat("Mat_BathTile");
            const float tileH = BathTileTop, gap = 0.004f;
            TileQuad("Bath_TilesW", new Vector3(xw + gap, 0f, zs), Vector3.forward, Vector3.right, zn - zs, tileH, tile, 1.8f);
            TileQuad("Bath_TilesS", new Vector3(xe, 0f, zs + gap), Vector3.left, Vector3.forward, xe - xw, tileH, tile, 1.8f);
            TileQuad("Bath_TilesN", new Vector3(xw, 0f, zn - gap), Vector3.right, Vector3.back, xe - xw, tileH, tile, 1.8f);
            TileQuad("Bath_TilesE_S", new Vector3(xe - gap, 0f, d0), Vector3.back, Vector3.left, d0 - zs, tileH, tile, 1.8f);
            TileQuad("Bath_TilesE_N", new Vector3(xe - gap, 0f, zn), Vector3.back, Vector3.left, zn - d1, tileH, tile, 1.8f);
            TileFloor("Bath_FloorTiles", new Vector3(xw, 0.003f, zs), xe - xw, zn - zs, Mat("Mat_BathFloor"), 1.4f);

            // Hoja de madera con rejilla abajo y chapa del lado de afuera; abre hacia el cuarto grande
            var pivot = DoorRoot("Interactable_BathDoor", new Vector3(xm, BathDoorH * 0.5f, BathDoorZ), new Vector3(0.7f, BathDoorH, BathDoorW), new Vector3(xm, 0f, d1));
            if (pivot != null)
            {
                float leafH = BathDoorH - 0.04f;
                SidePiece(pivot, "Bath_DoorLeaf", new Vector3(xm, 0.02f + leafH * 0.5f, BathDoorZ), new Vector3(0.05f, leafH, BathDoorW - 0.02f), wood, true);
                var slat = Mat("Mat_DarkPlastic");
                for (int i = 0; i < 5; i++)
                    SidePiece(pivot, "Bath_DoorSlat_" + i, new Vector3(xm + 0.03f, 0.3f + i * 0.06f, BathDoorZ), new Vector3(0.012f, 0.025f, 0.5f), slat, false);
                SidePiece(pivot, "Bath_DoorPlate", new Vector3(xm + 0.03f, 1.55f, BathDoorZ), new Vector3(0.008f, 0.1f, 0.24f), steel, false);
                SideText(pivot, "Bath_PlateText", "BAÑO", "Fonts/BonaNovaSC-Regular", new Vector3(xm + 0.036f, 1.55f, BathDoorZ),
                    Quaternion.Euler(0f, -90f, 0f), 0.011f, new Color(0.12f, 0.1f, 0.08f, 1f));
                var knob = SidePiece(pivot, "Bath_DoorKnob", new Vector3(xm + 0.06f, 1.0f, d0 + 0.1f), new Vector3(0.05f, 0.03f, 0.05f), steel, false, PrimitiveType.Cylinder);
                knob.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // Inodoro de espaldas a la pared sur; el cuaderno queda sobre la mochila
            var toilet = PlaceProp("Bathroom_Toilet", "Kit_Bathroom_Toilet", new Vector3(-9.2f, 0f, -9.45f), 0f, 0.8f, Vector3.forward);
            PlaceProp("Bathroom_Mirror1", "Kit_Bath_Mirror", new Vector3(-8.85f, 1.2f, -7.55f), 180f, 0.55f, Vector3.back);
            PlaceProp("Bathroom_ToiletPaper", "Kit_Bath_PaperA", new Vector3(-8.65f, 0f, -9.6f), 30f, 0.11f, Vector3.zero);
            PlaceProp("Bathroom_ToiletPaper", "Kit_Bath_PaperB", new Vector3(-8.4f, 0f, -9.35f), 75f, 0.11f, Vector3.zero);
            PlaceProp("bucket", "Kit_Bath_Bucket", new Vector3(-7.8f, 0f, -9.45f), 15f, 0.35f, Vector3.zero);

            SideSlab("Bath_Sign", new Vector3(-9.2f, 1.55f, zs + 0.006f), new Vector3(0.5f, 0.18f, 0.01f), Mat("Mat_PrintPaper"), false);
            SideText(_root, "Bath_SignText", "ACCIONE EL BOTÓN\nDESPUÉS DE USAR", "Fonts/BonaNovaSC-Regular", new Vector3(-9.2f, 1.55f, zs + 0.013f),
                Quaternion.Euler(0f, 180f, 0f), 0.0062f, new Color(0.1f, 0.09f, 0.08f, 1f));
            SideText(_root, "Bath_PhoneText", "(213) 555-0148", "Fonts/GiveYouGlory-Regular", new Vector3(xw + 0.006f, 1.35f, -8.45f),
                Quaternion.Euler(0f, -90f, -5f), 0.013f, new Color(0.08f, 0.09f, 0.16f, 1f));

            // Lamparita: el artefacto siempre se ve; luz y brillo viven en PoweredLights (fusible)
            var bulb = new GameObject("Bath_Bulb").transform;
            bulb.SetParent(_root, false);
            bulb.position = new Vector3(-8.6f, h - 0.02f, -8.65f);
            Part(bulb, "Bath_BulbSocket", new Vector3(0f, -0.04f, 0f), new Vector3(0.05f, 0.04f, 0.05f), steel, PrimitiveType.Cylinder);
            Part(bulb, "Bath_BulbGlass", new Vector3(0f, -0.12f, 0f), Vector3.one * 0.07f, Mat("Mat_FuseGlass"), PrimitiveType.Sphere);
            var powered = PoweredRoot("fuse_installed");
            if (powered != null)
            {
                var old = powered.transform.Find("Bath_BulbOn");
                if (old != null)
                    Object.DestroyImmediate(old.gameObject);
                var on = new GameObject("Bath_BulbOn").transform;
                on.SetParent(powered.transform, false);
                on.position = bulb.position + new Vector3(0f, -0.12f, 0f);
                var glow = Part(on, "Bath_BulbGlow", Vector3.zero, Vector3.one * 0.075f, Mat("Mat_TubeGlow"), PrimitiveType.Sphere);
                glow.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                var lightGo = new GameObject("Bath_BulbLight");
                lightGo.transform.SetParent(on, false);
                lightGo.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.8f, 0.55f);
                light.intensity = 3f;
                // Sin sombra (una sola por nivel): el rango llega a las esquinas del piso y apenas pasa el muro
                light.range = 2.8f;
                light.shadows = LightShadows.None;
            }

            var note = FindLocal("Interactable_BathNotebook");
            if (note != null)
            {
                Vector3 at = new Vector3(-9.2f, 0.85f, -9.65f);
                if (toilet != null)
                {
                    var tb = Encapsulate(toilet);
                    at = new Vector3(tb.center.x, tb.max.y + 0.01f, tb.min.z + 0.1f);
                }
                note.transform.position = at;
                note.transform.rotation = Quaternion.Euler(0f, 12f, 0f);
                note.transform.localScale = new Vector3(0.16f, 0.02f, 0.22f);
                var box = note.GetComponent<BoxCollider>();
                if (box != null)
                {
                    box.isTrigger = true;
                    box.center = Vector3.zero;
                    box.size = new Vector3(1.3f, 5f, 1.3f);
                }
                note.GetComponent<InteractableBase>()?.SetUseZone(BathInside);
            }
        }

        // Pared revestida: origin es la esquina de abajo a la izquierda vista desde el lado de normal
        private void TileQuad(string name, Vector3 origin, Vector3 right, Vector3 normal, float width, float height, Material mat, float texSize)
        {
            TiledQuad(name, origin, right, Vector3.up, normal, width, height, mat, texSize);
        }

        private void TileFloor(string name, Vector3 origin, float width, float depth, Material mat, float texSize)
        {
            TiledQuad(name, origin, Vector3.right, Vector3.forward, Vector3.up, width, depth, mat, texSize);
        }

        // UV en metros / texSize: el mismo material repite igual en paños de distinto tamaño
        private void TiledQuad(string name, Vector3 origin, Vector3 right, Vector3 up, Vector3 normal, float width, float height, Material mat, float texSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = origin;
            go.transform.rotation = Quaternion.identity;
            var mesh = new Mesh { name = name };
            mesh.vertices = new[] { Vector3.zero, right * width, right * width + up * height, up * height };
            mesh.uv = new[] { Vector2.zero, new Vector2(width / texSize, 0f), new Vector2(width / texSize, height / texSize), new Vector2(0f, height / texSize) };
            mesh.normals = new[] { normal, normal, normal, normal };
            mesh.triangles = new[] { 0, 3, 2, 0, 2, 1 };
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var rend = go.AddComponent<MeshRenderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = ShadowCastingMode.Off;
        }

        private void BuildOffice()
        {
            var steel = Mat("Mat_Steel");
            var glass = Mat("Mat_Glass");
            float xw = 5.1f, xe = 7.85f, zs = -2.3f, zn = 0.95f;
            float t = OfficeWall;
            float xm = xw - t * 0.5f;
            float d0 = OfficeDoorZ - OfficeDoorW * 0.5f, d1 = OfficeDoorZ + OfficeDoorW * 0.5f;

            OfficeSide("S", new Vector3((xw - t + xe) * 0.5f, 0f, zs - t * 0.5f), new Vector3(xe - xw + t, 0f, t), steel, glass);
            OfficeSide("N", new Vector3((xw - t + xe) * 0.5f, 0f, zn + t * 0.5f), new Vector3(xe - xw + t, 0f, t), steel, glass);
            OfficeSide("W_S", new Vector3(xm, 0f, (zs - t + d0) * 0.5f), new Vector3(t, 0f, d0 - (zs - t)), steel, glass);
            OfficeSide("W_N", new Vector3(xm, 0f, (d1 + zn + t) * 0.5f), new Vector3(t, 0f, zn + t - d1), steel, glass);
            SideSlab("Office_DoorLintel", new Vector3(xm, (OfficeDoorH + OfficeTop) * 0.5f, OfficeDoorZ), new Vector3(t, OfficeTop - OfficeDoorH, OfficeDoorW), steel, true);
            float[] postZ = { zs - t * 0.5f, d0, d1, zn + t * 0.5f };
            for (int i = 0; i < postZ.Length; i++)
                SideSlab("Office_Post_" + i, new Vector3(xm, OfficeTop * 0.5f, postZ[i]), new Vector3(0.07f, OfficeTop, 0.07f), steel, true);
            SideSlab("Office_Roof", new Vector3((xw - t + xe) * 0.5f, OfficeTop + OfficeRoof * 0.5f, (zs + zn) * 0.5f), new Vector3(xe - xw + t, OfficeRoof, zn - zs + t * 2f), steel, true);

            // Puerta de marco metálico con vidrio; abre hacia el laboratorio
            var pivot = DoorRoot("Interactable_OfficeDoor", new Vector3(xm, OfficeDoorH * 0.5f, OfficeDoorZ), new Vector3(0.7f, OfficeDoorH, OfficeDoorW), new Vector3(xm, 0f, d1));
            if (pivot != null)
            {
                float w = OfficeDoorW - 0.04f;
                SidePiece(pivot, "Office_DoorKick", new Vector3(xm, 0.02f + (OfficeKick - 0.02f) * 0.5f, OfficeDoorZ), new Vector3(0.04f, OfficeKick - 0.02f, w), steel, true);
                var pane = SidePiece(pivot, "Office_DoorGlass", new Vector3(xm, (OfficeKick + 2.0f) * 0.5f, OfficeDoorZ), new Vector3(0.02f, 2.0f - OfficeKick, w - 0.08f), glass, true);
                pane.layer = GlassLayer;
                pane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                SidePiece(pivot, "Office_DoorStileS", new Vector3(xm, (OfficeKick + 2.0f) * 0.5f, OfficeDoorZ - w * 0.5f + 0.02f), new Vector3(0.04f, 2.0f - OfficeKick, 0.04f), steel, true);
                SidePiece(pivot, "Office_DoorStileN", new Vector3(xm, (OfficeKick + 2.0f) * 0.5f, OfficeDoorZ + w * 0.5f - 0.02f), new Vector3(0.04f, 2.0f - OfficeKick, 0.04f), steel, true);
                SidePiece(pivot, "Office_DoorRail", new Vector3(xm, 2.04f, OfficeDoorZ), new Vector3(0.04f, 0.08f, w), steel, true);
                var handle = SidePiece(pivot, "Office_DoorHandle", new Vector3(xm - 0.05f, 1.0f, d0 + 0.1f), new Vector3(0.025f, 0.06f, 0.025f), steel, false, PrimitiveType.Cylinder);
                handle.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // Adentro: escritorio contra la pared este, silla y archivero
            var desk = Prop("desk", "Kit_Office_Desk", new Vector3(7.3f, 0f, -1.4f), 90f, _wood, 0.78f, true);
            Prop("chairDesk", "Kit_Office_Chair", new Vector3(6.6f, 0f, -1.35f), 90f, _rust, 0.95f, true);
            PlaceProp("FileCabinet", "Kit_Office_Cabinet", new Vector3(7.45f, 0f, 0.5f), 0f, 1.35f, Vector3.left);
            if (desk != null)
            {
                float top = Encapsulate(desk).max.y;
                Prop("books", "Kit_Office_Books", new Vector3(7.45f, top, -1.85f), 8f, null, 0.1f, false);
            }

            // Corcho con un gráfico que cae y una tira impresa: solo constata
            // Cara del corcho hacia el cuarto; mirando al este, z baja de izquierda a derecha
            float face = xe - 0.02f;
            SideSlab("Office_Board", new Vector3(xe - 0.01f, 1.6f, -1.4f), new Vector3(0.02f, 0.75f, 1.5f), Mat("Mat_Kraft"), false);
            SideSlab("Office_ChartSheet", new Vector3(face - 0.003f, 1.62f, -1.84f), new Vector3(0.004f, 0.36f, 0.5f), Mat("Mat_PrintPaper"), false);
            var ink = Mat("Mat_DarkPlastic");
            float cx = face - 0.008f;
            ChartLine("Office_ChartAxisY", new Vector3(cx, 1.5f, -1.63f), new Vector3(cx, 1.78f, -1.63f), ink, 0.005f);
            ChartLine("Office_ChartAxisX", new Vector3(cx, 1.5f, -1.63f), new Vector3(cx, 1.5f, -2.05f), ink, 0.005f);
            float[] level = { 0.26f, 0.24f, 0.25f, 0.19f, 0.16f, 0.17f, 0.1f, 0.06f, 0.03f };
            for (int i = 0; i + 1 < level.Length; i++)
            {
                float za = -1.64f - i * 0.05f, zb = -1.64f - (i + 1) * 0.05f;
                ChartLine("Office_ChartLine_" + i, new Vector3(cx, 1.5f + level[i], za), new Vector3(cx, 1.5f + level[i + 1], zb), ink, 0.004f);
            }
            var labelColor = new Color(0.12f, 0.12f, 0.14f, 1f);
            SideText(_root, "Office_ChartFrom", "74", "Fonts/KodeMono-Regular", new Vector3(cx, 1.475f, -1.67f), Quaternion.Euler(0f, 90f, 0f), 0.0055f, labelColor);
            SideText(_root, "Office_ChartTo", "78", "Fonts/KodeMono-Regular", new Vector3(cx, 1.475f, -2.02f), Quaternion.Euler(0f, 90f, 0f), 0.0055f, labelColor);
            SideSlab("Office_LogSheet", new Vector3(face - 0.003f, 1.6f, -1.1f), new Vector3(0.004f, 0.46f, 0.8f), Mat("Mat_PrintPaper"), false);
            SideText(_root, "Office_LogText",
                "09/74  INICIO DE LA SERIE\n03/75  PRIMERAS PÉRDIDAS DE MEMORIA\n11/76  EL SUJETO NO RECONOCE SU NOMBRE\n06/77  TRASLADO INTERNO\n78     LUCES A MEDIO RÉGIMEN",
                "Fonts/KodeMono-Regular", new Vector3(cx, 1.6f, -1.1f), Quaternion.Euler(0f, 90f, 0f), 0.0042f, labelColor);
        }

        // Lado de la oficina: zócalo de chapa y vidrio hasta el techo
        private void OfficeSide(string side, Vector3 center, Vector3 size, Material steel, Material glass)
        {
            SideSlab("Office_Kick" + side, new Vector3(center.x, OfficeKick * 0.5f, center.z), new Vector3(size.x, OfficeKick, size.z), steel, true);
            float gh = OfficeTop - OfficeKick;
            bool alongX = size.x > size.z;
            var pane = SideSlab("Office_Glass" + side, new Vector3(center.x, OfficeKick + gh * 0.5f, center.z),
                new Vector3(alongX ? size.x : 0.03f, gh, alongX ? 0.03f : size.z), glass, false);
            if (pane != null)
                pane.layer = GlassLayer;
        }

        private GameObject SideSlab(string name, Vector3 pos, Vector3 scale, Material mat, bool shadows)
        {
            Slab(name, pos, scale, mat);
            var go = FindLocal(name);
            if (go != null && shadows)
                go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            return go;
        }

        // Pieza de una hoja de puerta, en mundo y colgada del pivote
        private static GameObject SidePiece(Transform parent, string name, Vector3 world, Vector3 size, Material mat, bool solid, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (!solid)
                StripColliders(go);
            go.transform.SetParent(parent, false);
            go.transform.position = world;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = size;
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null && mat != null)
                rend.sharedMaterial = mat;
            return go;
        }

        // Raíz de la puerta (la arma KitbashBaker con su config): trigger en el vano y pivote en la bisagra, hoja rehecha
        private Transform DoorRoot(string name, Vector3 center, Vector3 trigger, Vector3 hinge)
        {
            var door = FindLocal(name);
            if (door == null)
                return null;
            door.transform.position = center;
            door.transform.rotation = Quaternion.identity;
            door.transform.localScale = Vector3.one;
            var box = door.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.isTrigger = true;
                box.center = Vector3.zero;
                box.size = trigger;
            }
            var pivot = door.transform.Find("DoorPivot");
            if (pivot == null)
                return null;
            pivot.position = hinge;
            pivot.localRotation = Quaternion.identity;
            pivot.localScale = Vector3.one;
            for (int i = pivot.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(pivot.GetChild(i).gameObject);
            return pivot;
        }

        // Trazo del gráfico sobre la hoja (plano de la pared este)
        private void ChartLine(string name, Vector3 a, Vector3 b, Material mat, float width)
        {
            var go = SidePiece(_root, name, (a + b) * 0.5f, new Vector3(width, Vector3.Distance(a, b), width), mat, false);
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private void SideText(Transform parent, string name, string text, string fontPath, Vector3 pos, Quaternion rot, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            // TextMesh se lee desde -Z local
            go.transform.rotation = rot;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Left;
            tm.characterSize = size;
            tm.fontSize = 64;
            tm.color = color;
            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.Off;
            var font = Resources.Load<Font>(fontPath);
            if (font != null)
            {
                tm.font = font;
                mr.sharedMaterial = font.material;
            }
        }

        // En Play los textos de los cuartitos quedan tapados por las paredes
        private void SideRoomText()
        {
            foreach (var n in new[] { "Bath_PlateText", "Bath_SignText", "Bath_PhoneText", "Office_ChartFrom", "Office_ChartTo", "Office_LogText" })
                WorldTextMat(FindLocal(n));
        }
    }
}
