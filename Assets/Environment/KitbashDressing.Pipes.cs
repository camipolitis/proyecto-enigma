using UnityEngine;

namespace Enigma.Environment
{
    // Cañerías de pared: tramos continuos con uniones, abrazaderas, codos y bridas donde entran a la pared o al techo
    public partial class KitbashDressing
    {
        private const float PipeGap = 0.05f;
        private const float PipeSection = 1f;
        private const float MainPipeR = 0.075f;
        private const float ThinPipeR = 0.05f;

        private void WallPipes(string sceneName)
        {
            if (Application.isPlaying)
                return;
            var west = FindLocal("Wall_W");
            var east = FindLocal("Wall_E");
            var south = FindLocal("Wall_S");
            var north = FindLocal("Wall_N_L");
            if (west == null || east == null || south == null || north == null)
                return;
            float cx = (west.transform.position.x + east.transform.position.x) * 0.5f;
            float cz = (south.transform.position.z + north.transform.position.z) * 0.5f;
            float fallback = RoofYOf(sceneName) - RoofThick * 0.5f;

            // A: sale de la pared sur, sube con dos codos y entra en la pared norte
            const float lowA = 2.9f, highA = 3.5f;
            float wx = WallFace(new Vector3(cx, lowA, cz), Vector3.left) + MainPipeR + PipeGap;
            float sz = WallFace(new Vector3(cx, lowA, cz), Vector3.back);
            float nz = WallFace(new Vector3(cx, lowA, cz), Vector3.forward);
            float stepZ = Mathf.Lerp(sz, nz, 0.4f);
            PipeRun("Kit_PipeRunA", new[]
            {
                new Vector3(wx, lowA, sz), new Vector3(wx, lowA, stepZ), new Vector3(wx, highA, stepZ), new Vector3(wx, highA, nz)
            }, MainPipeR, Vector3.left, -1);

            // B: baja del techo, corre por la pared este con una válvula y vuelve a subir
            const float lowB = 2.6f;
            float ex = WallFace(new Vector3(cx, lowB, cz), Vector3.right) - ThinPipeR - PipeGap;
            float z0 = Mathf.Lerp(sz, nz, 0.2f), z1 = Mathf.Lerp(sz, nz, 0.75f);
            PipeRun("Kit_PipeRunB", new[]
            {
                new Vector3(ex, CeilingAt(ex, z0, fallback), z0), new Vector3(ex, lowB, z0), new Vector3(ex, lowB, z1), new Vector3(ex, CeilingAt(ex, z1, fallback), z1)
            }, ThinPipeR, Vector3.right, 1);
        }

        // Cara inferior del techo visible sobre (x, z): se mide la malla para que el caño entre justo donde se ve el techo
        private float CeilingAt(float x, float z, float fallback)
        {
            foreach (var n in new[] { "Kit_Ceiling", "Kit_Ceiling_N", "Kit_Ceiling_S", "Kit_Ceiling_W", "Kit_Ceiling_E" })
            {
                var go = FindLocal(n);
                var r = go != null ? go.GetComponent<MeshRenderer>() : null;
                if (r == null || !r.enabled)
                    continue;
                var b = r.bounds;
                if (x >= b.min.x && x <= b.max.x && z >= b.min.z && z <= b.max.z)
                    return b.min.y;
            }
            return fallback;
        }

        // Cara de la pared más cercana en esa dirección (primer collider Wall_*)
        private static float WallFace(Vector3 from, Vector3 dir)
        {
            Physics.SyncTransforms();
            float best = float.MaxValue;
            float face = Vector3.Dot(from, dir) + 6f;
            foreach (var hit in Physics.RaycastAll(from, dir, 30f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.name.StartsWith("Wall_") || hit.distance >= best)
                    continue;
                best = hit.distance;
                face = Vector3.Dot(hit.point, dir);
            }
            return face * (dir.x + dir.y + dir.z);
        }

        // Tramo por los puntos dados; valveSeg es el segmento que lleva la válvula (-1 sin válvula)
        private void PipeRun(string name, Vector3[] pts, float r, Vector3 toWall, int valveSeg)
        {
            if (Application.isPlaying || FindLocal(name) != null || pts.Length < 2)
                return;
            var run = new GameObject(name).transform;
            run.SetParent(_root, false);
            var steel = Resources.Load<Material>("Mats/Mat_Steel");

            for (int i = 0; i + 1 < pts.Length; i++)
            {
                Vector3 a = pts[i], b = pts[i + 1];
                Vector3 dir = (b - a).normalized;
                float len = Vector3.Distance(a, b);
                int n = Mathf.Max(1, Mathf.CeilToInt(len / PipeSection - 0.001f));
                float step = len / n;
                for (int k = 0; k < n; k++)
                {
                    Vector3 c = a + dir * (step * (k + 0.5f));
                    PipePiece(run, "Pipe_" + i + "_" + k, c, dir, r, step, _rust);
                    if (k > 0)
                        Clamp(run, "Clamp_" + i + "_" + k, a + dir * (step * k), dir, r, toWall, steel);
                }
                // Tramos de una sola sección también se sujetan a la pared
                if (n == 1 && len > 0.6f)
                    Clamp(run, "Clamp_" + i + "_mid", (a + b) * 0.5f, dir, r, toWall, steel);
                if (i == valveSeg)
                    Valve(run, (a + b) * 0.5f, dir, r, toWall, steel);
            }

            for (int i = 1; i + 1 < pts.Length; i++)
            {
                Vector3 inDir = (pts[i] - pts[i - 1]).normalized;
                Vector3 outDir = (pts[i + 1] - pts[i]).normalized;
                Ball(run, "Elbow_" + i, pts[i], r * 1.08f, _rust);
                PipePiece(run, "Collar_" + i + "_in", pts[i] - inDir * r * 1.4f, inDir, r * 1.22f, 0.06f, steel);
                PipePiece(run, "Collar_" + i + "_out", pts[i] + outDir * r * 1.4f, outDir, r * 1.22f, 0.06f, steel);
            }

            Flange(run, "Flange_Start", pts[0], (pts[0] - pts[1]).normalized, r, steel);
            Flange(run, "Flange_End", pts[pts.Length - 1], (pts[pts.Length - 1] - pts[pts.Length - 2]).normalized, r, steel);
        }

        private static GameObject PipePiece(Transform parent, string name, Vector3 center, Vector3 dir, float r, float length, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            StripColliders(go);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(center, Quaternion.FromToRotation(Vector3.up, dir));
            go.transform.localScale = new Vector3(r * 2f, length * 0.5f, r * 2f);
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null && mat != null)
                rend.sharedMaterial = mat;
            return go;
        }

        private static void Ball(Transform parent, string name, Vector3 center, float r, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            StripColliders(go);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.localScale = Vector3.one * r * 2f;
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null && mat != null)
                rend.sharedMaterial = mat;
        }

        // Virola en la unión y planchuela hasta la pared
        private static void Clamp(Transform parent, string name, Vector3 at, Vector3 dir, float r, Vector3 toWall, Material mat)
        {
            PipePiece(parent, name, at, dir, r * 1.22f, 0.06f, mat);
            var strap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strap.name = name + "_Strap";
            StripColliders(strap);
            strap.transform.SetParent(parent, false);
            float depth = r + PipeGap;
            strap.transform.SetPositionAndRotation(at + toWall * (depth * 0.5f + r * 0.1f), Quaternion.LookRotation(toWall, dir));
            strap.transform.localScale = new Vector3(r * 1.1f, 0.04f, depth - r * 0.2f);
            var rend = strap.GetComponent<MeshRenderer>();
            if (rend != null && mat != null)
                rend.sharedMaterial = mat;
        }

        // Brida y placa donde el caño entra a la pared o al techo; into apunta hacia la superficie
        private static void Flange(Transform parent, string name, Vector3 at, Vector3 into, float r, Material mat)
        {
            PipePiece(parent, name, at - into * 0.0125f, into, r * 1.7f, 0.025f, mat);
            PipePiece(parent, name + "_Plate", at - into * 0.005f, into, r * 2.1f, 0.01f, mat);
        }

        private static void Valve(Transform parent, Vector3 at, Vector3 dir, float r, Vector3 toWall, Material mat)
        {
            Ball(parent, "Valve_Body", at, r * 1.35f, mat);
            Vector3 room = -toWall;
            const float stem = 0.12f, wheel = 0.11f;
            PipePiece(parent, "Valve_Stem", at + room * (r + stem * 0.5f), room, 0.012f, stem, mat);
            Vector3 hub = at + room * (r + stem);
            PipePiece(parent, "Valve_Wheel", hub, room, wheel, 0.02f, mat);
            for (int i = 0; i < 2; i++)
            {
                var spoke = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spoke.name = "Valve_Spoke_" + i;
                StripColliders(spoke);
                spoke.transform.SetParent(parent, false);
                Vector3 across = i == 0 ? dir : Vector3.Cross(dir, room).normalized;
                spoke.transform.SetPositionAndRotation(hub + room * 0.012f, Quaternion.LookRotation(room, across));
                spoke.transform.localScale = new Vector3(0.016f, wheel * 2f, 0.01f);
                var rend = spoke.GetComponent<MeshRenderer>();
                if (rend != null && mat != null)
                    rend.sharedMaterial = mat;
            }
        }
    }
}
