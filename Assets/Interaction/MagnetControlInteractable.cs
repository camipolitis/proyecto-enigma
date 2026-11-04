using System.Collections;
using System.Collections.Generic;
using Enigma.Audio;
using Enigma.Core;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Cada E sube el magnetismo y arrastra el metal hacia el imán.
    public class MagnetControlInteractable : InteractableBase
    {
        [SerializeField] private Transform[] metalParts;
        [SerializeField] private GameObject paintingObject;
        [SerializeField] private Transform[] flyBits;
        [SerializeField] private Transform[] junk;
        [SerializeField] private string usedFlagId = "magnet_used";
        [SerializeField] private float step = 0.33f;
        [SerializeField] private float pullTime = 0.55f;
        [SerializeField] private float layerGap = 0.03f;
        // Lo que todavía no vuela se arrastra por el piso hasta esta fracción del camino
        [SerializeField, Range(0f, 1f)] private float dragReach = 0.6f;
        [SerializeField] private float dragGap = 0.1f;
        [SerializeField] private float flyArc = 0.35f;
        // Paso con el que se acorta un arrastre hasta que no choca
        private const float DragStep = 0.05f;
        // Alto común de las cajas planas: solo se comparan en el piso
        private const float FlatHeight = 1f;
        // Límites en kg entre livianos, medianos y pesados. Sin MagnetMass cuenta como pesado.
        [SerializeField] private float[] massLimits = { 1f, 2.5f };

        private Vector3[] _starts;
        private Vector3[] _ends;
        private Quaternion[] _startRots;
        private Quaternion[] _endRots;
        private Vector3[] _flyStarts;
        private Vector3[] _flyEnds;
        private Quaternion[] _flyRots;
        private Quaternion[] _flyEndRots;
        private Vector3[] _junkStarts;
        private Vector3[] _junkEnds;
        private Quaternion[] _junkRots;
        private Quaternion[] _junkEndRots;
        private Vector3[] _grounds;
        private Vector3[] _flyGrounds;
        private Vector3[] _junkGrounds;
        private float _pull;
        private float _shown;
        private bool _done;
        private Transform _needle;
        private Renderer _lamp;
        private Material _gaugeMat;
        private Coroutine _anim;
        private Vector3 _metalNormal;

        private void Awake()
        {
            CacheStarts();
            EnsureGauge();
            SetPaintingReady(false);
        }

        public void BakerSetup(GameObject painting, Transform[] metals, Transform[] flies, Transform[] smalls = null)
        {
            paintingObject = painting;
            if (metals != null && metals.Length > 0)
                metalParts = metals;
            flyBits = flies;
            if (smalls != null)
                junk = smalls;
            step = 0.33f;
            CacheStarts();
            EnsureGauge();
            SetPaintingReady(false);
            ApplyPull();
        }

        protected override bool IsCompleted() => _done;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (_done || metalParts == null || metalParts.Length == 0)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        protected override void OnSuccess(InteractContext context)
        {
            _pull = Mathf.Clamp01(_pull + step);
            if (_pull >= 0.99f)
                _pull = 1f;
            if (_anim != null)
                StopCoroutine(_anim);
            _anim = StartCoroutine(Animate(_shown, _pull));
            SfxPlayer.Instance?.Play("magnet", transform.position);
            PlayDialogue(context, config != null ? config.successDialogueId : null);

            if (_pull < 1f)
                return;

            _done = true;
            SetPaintingReady(true);
            if (!string.IsNullOrEmpty(usedFlagId))
                GameFlagSystem.Instance?.Set(usedFlagId, true);
            MarkCompleted();
        }

        private IEnumerator Animate(float from, float to)
        {
            float t = 0f;
            while (t < pullTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / pullTime);
                Show(Mathf.Lerp(from, to, k * k));
                yield return null;
            }
            _anim = null;
            Show(to);
        }

        private void OnValidate()
        {
            step = Mathf.Max(0.01f, step);
            if (massLimits != null)
                System.Array.Sort(massLimits);
        }

        private void CacheStarts()
        {
            _starts = Positions(metalParts);
            _startRots = Rotations(metalParts);
            _flyStarts = Positions(flyBits);
            _flyRots = Rotations(flyBits);
            _junkStarts = Positions(junk);
            _junkRots = Rotations(junk);
            ComputeEnds();
        }

        private static Vector3[] Positions(Transform[] arr)
        {
            if (arr == null)
                return null;
            var p = new Vector3[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                p[i] = arr[i] != null ? arr[i].position : Vector3.zero;
            return p;
        }

        private static Quaternion[] Rotations(Transform[] arr)
        {
            if (arr == null)
                return null;
            var r = new Quaternion[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                r[i] = arr[i] != null ? arr[i].rotation : Quaternion.identity;
            return r;
        }

        private Bounds Body()
        {
            var r = GetComponent<Renderer>();
            if (r != null)
                return r.bounds;
            return new Bounds(transform.position, transform.lossyScale);
        }

        // Bounds del renderer: el collider no se sincroniza hasta el próximo paso de física
        private static Bounds PartBounds(Transform t)
        {
            var rends = t.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0)
                return new Bounds(t.position, Vector3.one * 0.1f);
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            return b;
        }

        // Las chapas terminan apiladas en capas contra la cara del imán que mira hacia ellas
        private void ComputeEnds()
        {
            Bounds body = Body();
            if (metalParts != null && _starts != null)
            {
                _ends = new Vector3[metalParts.Length];
                _endRots = new Quaternion[metalParts.Length];
                Vector3 avg = Vector3.zero;
                int n = 0;
                for (int i = 0; i < metalParts.Length; i++)
                {
                    if (metalParts[i] == null)
                        continue;
                    avg += _starts[i];
                    n++;
                }
                Vector3 normal = n > 0 ? avg / n - body.center : Vector3.forward;
                normal.y = 0f;
                normal = Mathf.Abs(normal.x) > Mathf.Abs(normal.z)
                    ? new Vector3(Mathf.Sign(normal.x), 0f, 0f)
                    : new Vector3(0f, 0f, Mathf.Sign(normal.z));
                _metalNormal = normal;
                Vector3 side = Vector3.Cross(Vector3.up, normal);
                float faceDist = Mathf.Abs(Vector3.Dot(body.extents, normal));

                var order = new int[metalParts.Length];
                for (int i = 0; i < order.Length; i++)
                    order[i] = i;
                System.Array.Sort(order, (a, b) =>
                    Vector3.Dot(_starts[a] - body.center, normal).CompareTo(Vector3.Dot(_starts[b] - body.center, normal)));

                float sideDist = Mathf.Abs(Vector3.Dot(body.extents, side));
                // La más cercana cubre la cara de atrás; las demás giran y se pegan a los costados,
                // así el frente del cuadro queda libre
                float backDepth = 0f;
                float rightDepth = 0f;
                float leftDepth = 0f;
                bool first = true;
                foreach (int i in order)
                {
                    if (metalParts[i] == null)
                        continue;
                    var pb = PartBounds(metalParts[i]);
                    float thick = Mathf.Abs(Vector3.Dot(pb.extents, normal));
                    // La malla puede no estar centrada en el pivote: se ubica la caja visible
                    Vector3 offset = pb.center - _starts[i];
                    float lateral = Vector3.Dot(pb.center - body.center, side);
                    Vector3 center;
                    Quaternion turn = Quaternion.identity;
                    if (first)
                    {
                        float l = Mathf.Clamp(lateral * 0.3f, -sideDist * 0.5f, sideDist * 0.5f);
                        center = body.center + normal * (faceDist + layerGap + backDepth + thick) + side * l;
                        backDepth += thick * 2f + layerGap;
                        first = false;
                    }
                    else
                    {
                        float sign = lateral >= 0f ? 1f : -1f;
                        if (sign > 0f ? rightDepth > leftDepth + 0.01f : leftDepth > rightDepth + 0.01f)
                            sign = -sign;
                        float acc = sign > 0f ? rightDepth : leftDepth;
                        // El borde trasero queda al ras de la cara de atrás, sin cruzarse con esa chapa
                        float width = Mathf.Abs(Vector3.Dot(pb.extents, side));
                        center = body.center + side * sign * (sideDist + layerGap + acc + thick) + normal * (faceDist - width);
                        turn = Quaternion.AngleAxis(90f * sign, Vector3.up);
                        if (sign > 0f)
                            rightDepth += thick * 2f + layerGap;
                        else
                            leftDepth += thick * 2f + layerGap;
                    }
                    Vector3 end = center - turn * offset;
                    end.y = _starts[i].y;
                    _ends[i] = end;
                    _endRots[i] = turn * _startRots[i];
                }
            }

            ComputeStick(flyBits, _flyStarts, _flyRots, out _flyEnds, out _flyEndRots, 11);
            ComputeStick(junk, _junkStarts, _junkRots, out _junkEnds, out _junkEndRots, 23);

            ComputeGrounds(body);
        }

        private class DragPiece
        {
            public Vector3[] grounds;
            public int index;
            public int tier;
            public Vector3 start;
            public Vector3 dir;
            public float reach;
            public Bounds box;
        }

        // Lo que va por el piso frena antes del imán, del cuadro y de las otras piezas, en cada E
        private void ComputeGrounds(Bounds body)
        {
            _grounds = _starts != null ? (Vector3[])_starts.Clone() : null;
            _flyGrounds = _flyStarts != null ? (Vector3[])_flyStarts.Clone() : null;
            _junkGrounds = _junkStarts != null ? (Vector3[])_junkStarts.Clone() : null;
            var pieces = new List<DragPiece>();
            AddPieces(pieces, metalParts, _starts, _ends, _grounds, body);
            AddPieces(pieces, flyBits, _flyStarts, _flyEnds, _flyGrounds, body);
            AddPieces(pieces, junk, _junkStarts, _junkEnds, _junkGrounds, body);
            pieces.Sort((a, b) => a.tier != b.tier ? a.tier.CompareTo(b.tier) : a.reach.CompareTo(b.reach));

            Bounds? painting = null;
            if (paintingObject != null)
                painting = Flat(PartBounds(paintingObject.transform));
            for (int i = 0; i < pieces.Count; i++)
            {
                var p = pieces[i];
                float r = p.reach;
                while (r > 0f && Blocked(pieces, i, r, painting))
                    r -= DragStep;
                p.reach = Mathf.Max(0f, r);
                p.grounds[p.index] = p.start + p.dir * p.reach;
            }
        }

        private void AddPieces(List<DragPiece> pieces, Transform[] arr, Vector3[] starts, Vector3[] ends, Vector3[] grounds, Bounds body)
        {
            if (arr == null || starts == null || ends == null)
                return;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null)
                    continue;
                int tier = Tier(arr[i]);
                if (tier == 0)
                    continue;
                Vector3 to = new Vector3(ends[i].x - starts[i].x, 0f, ends[i].z - starts[i].z);
                pieces.Add(new DragPiece
                {
                    grounds = grounds,
                    index = i,
                    tier = tier,
                    start = starts[i],
                    dir = to.sqrMagnitude > 0f ? to.normalized : Vector3.zero,
                    reach = GroundReach(arr[i], starts[i], ends[i], body),
                    box = Flat(PartBounds(arr[i])),
                });
            }
        }

        // Caja en el plano del piso, agrandada a la mitad del espacio entre capas
        private Bounds Flat(Bounds b)
        {
            return new Bounds(new Vector3(b.center.x, 0f, b.center.z), new Vector3(b.size.x + layerGap, FlatHeight, b.size.z + layerGap));
        }

        // Las piezas ya ubicadas se comparan en la posición de cada E; las que faltan, donde arrancan
        private bool Blocked(List<DragPiece> pieces, int i, float reach, Bounds? painting)
        {
            var p = pieces[i];
            for (int k = 1; k <= p.tier; k++)
            {
                Bounds b = p.box;
                b.center += p.dir * (reach * k / p.tier);
                if (painting.HasValue && !p.box.Intersects(painting.Value) && b.Intersects(painting.Value))
                    return true;
                for (int j = 0; j < pieces.Count; j++)
                {
                    var o = pieces[j];
                    // Si ya arrancan encimadas (chapas apiladas), no se frenan entre sí
                    if (j == i || o.box.Intersects(p.box))
                        continue;
                    Bounds ob = o.box;
                    if (j < i)
                    {
                        if (o.tier < k)
                            continue;
                        ob.center += o.dir * (o.reach * k / o.tier);
                    }
                    if (b.Intersects(ob))
                        return true;
                }
            }
            return false;
        }

        // Hasta dónde se arrastra por el piso: una parte del camino, sin tocar el imán
        private float GroundReach(Transform t, Vector3 start, Vector3 end, Bounds body)
        {
            Vector3 to = new Vector3(end.x - start.x, 0f, end.z - start.z);
            float dist = to.magnitude;
            if (dist < 0.001f)
                return 0f;
            Vector3 dir = to / dist;
            float reach = dist * dragReach;
            // La caja del imán se agranda con la de la pieza: el rayo sale del centro visible de la pieza
            var pb = PartBounds(t);
            Vector3 from = new Vector3(pb.center.x, body.center.y, pb.center.z);
            var wall = new Bounds(body.center, new Vector3(
                body.size.x + pb.size.x + dragGap * 2f,
                body.size.y,
                body.size.z + pb.size.z + dragGap * 2f));
            if (wall.Contains(from))
                return 0f;
            if (wall.IntersectRay(new Ray(from, dir), out float hit))
                reach = Mathf.Min(reach, hit);
            return reach;
        }

        // Cada objeto chico queda apoyado sobre la cara del imán más cercana, en un punto propio
        private void ComputeStick(Transform[] arr, Vector3[] starts, Quaternion[] rots, out Vector3[] ends, out Quaternion[] endRots, int seed)
        {
            ends = null;
            endRots = null;
            if (arr == null || starts == null)
                return;
            Bounds body = Body();
            var rng = new System.Random(seed);
            ends = new Vector3[arr.Length];
            endRots = new Quaternion[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null)
                    continue;
                // Atrás y a los costados van las chapas: lo chico queda al frente o arriba,
                // arriba del lado del frente para no tapar el indicador
                Vector3 front = _metalNormal == Vector3.zero ? Vector3.back : -_metalNormal;
                bool top = rng.NextDouble() < 0.3;
                Vector3 n = top ? Vector3.up : front;
                Vector3 a = Vector3.Cross(Vector3.up, front);
                Vector3 b = top ? front : Vector3.up;
                float ea = Mathf.Abs(Vector3.Dot(body.extents, a)) * 0.75f;
                float eb = Mathf.Abs(Vector3.Dot(body.extents, b)) * 0.7f;
                float u = (float)(rng.NextDouble() * 2.0 - 1.0) * ea;
                float v = top
                    ? (float)rng.NextDouble() * eb
                    : (float)(rng.NextDouble() * 2.0 - 1.0) * eb;
                float half = Mathf.Abs(Vector3.Dot(PartBounds(arr[i]).extents, n));
                Vector3 face = body.center + Vector3.Scale(body.extents, n).magnitude * n;
                ends[i] = face + a * u + b * v + n * (half * 0.8f);
                endRots[i] = Quaternion.AngleAxis((float)rng.NextDouble() * 360f, n) * rots[i];
            }
        }

        private void ApplyPull()
        {
            if (_anim != null)
            {
                StopCoroutine(_anim);
                _anim = null;
            }
            Show(_pull);
        }

        private void Show(float p)
        {
            _shown = p;
            if (metalParts != null && _starts != null && _ends != null)
            {
                int n = Mathf.Min(metalParts.Length, _starts.Length);
                for (int i = 0; i < n; i++)
                {
                    if (metalParts[i] == null)
                        continue;
                    Phases(metalParts[i], p, out float drag, out float fly);
                    // Solo se atraviesan mientras se mueven; quietas a mitad de camino son sólidas
                    bool moving = _anim != null && drag > 0.05f && fly < 0.999f;
                    foreach (var col in metalParts[i].GetComponentsInChildren<Collider>())
                        col.isTrigger = moving;
                }
            }

            Move(metalParts, _starts, _grounds, _ends, _startRots, _endRots, p);
            Move(junk, _junkStarts, _junkGrounds, _junkEnds, _junkRots, _junkEndRots, p);
            Move(flyBits, _flyStarts, _flyGrounds, _flyEnds, _flyRots, _flyEndRots, p);

            UpdateGauge();
        }

        // 0 liviano, 1 mediano, 2 pesado
        public int Tier(Transform t)
        {
            var m = t != null ? t.GetComponent<MagnetMass>() : null;
            int last = massLimits != null ? massLimits.Length : 0;
            if (m == null)
                return last;
            for (int i = 0; i < last; i++)
            {
                if (m.Kg < massLimits[i])
                    return i;
            }
            return last;
        }

        // Las E anteriores a la suya lo arrastran por el piso; con la suya despega y vuela al imán
        private void Phases(Transform t, float p, out float drag, out float fly)
        {
            int tier = Tier(t);
            float to = Mathf.Min(1f, (tier + 1) * step);
            if (tier >= (massLimits != null ? massLimits.Length : 0))
                to = 1f;
            float from = Mathf.Min(tier * step, to - 0.01f);
            drag = tier == 0 ? 1f : Mathf.Clamp01(p / from);
            fly = Mathf.InverseLerp(from, to, p);
        }

        private void Move(Transform[] arr, Vector3[] starts, Vector3[] grounds, Vector3[] ends, Quaternion[] rots, Quaternion[] endRots, float p)
        {
            if (arr == null || starts == null || grounds == null || ends == null || rots == null || endRots == null)
                return;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null)
                    continue;
                Phases(arr[i], p, out float drag, out float fly);
                float arc = flyArc / (Tier(arr[i]) + 1);
                Vector3 ground = Vector3.Lerp(starts[i], grounds[i], drag);
                arr[i].position = Vector3.Lerp(ground, ends[i], fly) + Vector3.up * (Mathf.Sin(fly * Mathf.PI) * arc);
                arr[i].rotation = Quaternion.Slerp(rots[i], endRots[i], fly);
            }
        }

        private void SetPaintingReady(bool ready)
        {
            if (paintingObject == null)
                return;
            paintingObject.SetActive(true);
            var col = paintingObject.GetComponent<Collider>();
            if (col != null)
                col.enabled = ready;
        }

        private void EnsureGauge()
        {
            var face = transform.Find("MagnetFace");
            if (face == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "MagnetFace";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.62f, 0.42f);
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                go.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
                DropCollider(go);
                face = go.transform;
            }

            _lamp = face.GetComponent<Renderer>();
            EnsureGaugeMat();

            var needle = face.Find("MagnetNeedle");
            // Queda una sola aguja
            for (int i = face.childCount - 1; i >= 0; i--)
            {
                var c = face.GetChild(i);
                if (c != needle && c.name == "MagnetNeedle")
                {
                    c.gameObject.SetActive(false);
                    if (Application.isPlaying)
                        Object.Destroy(c.gameObject);
                    else
                        Object.DestroyImmediate(c.gameObject);
                }
            }
            if (needle == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "MagnetNeedle";
                go.transform.SetParent(face, false);
                go.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = new Vector3(0.08f, 0.08f, 0.85f);
                DropCollider(go);
                needle = go.transform;
            }

            _needle = needle;
            UpdateGauge();
        }

        private static void DropCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(col);
            else
                Object.DestroyImmediate(col);
        }

        private void EnsureGaugeMat()
        {
            if (_lamp == null)
                return;
            // Instancia propia una vez. renderer.material clona en el editor.
            if (_gaugeMat == null)
            {
                var src = _lamp.sharedMaterial;
                _gaugeMat = src != null ? new Material(src) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                _gaugeMat.name = "MagnetGaugeMat";
            }
            _lamp.sharedMaterial = _gaugeMat;
        }

        private void UpdateGauge()
        {
            Color c;
            float angle;
            if (_shown < 0.3f)
            {
                c = new Color(0.18f, 0.72f, 0.22f);
                angle = -48f;
            }
            else if (_shown < 0.85f)
            {
                c = new Color(0.92f, 0.78f, 0.12f);
                angle = 0f;
            }
            else
            {
                c = new Color(0.82f, 0.12f, 0.1f);
                angle = 48f;
            }

            if (_gaugeMat == null)
                EnsureGaugeMat();
            if (_gaugeMat != null)
                _gaugeMat.color = c;

            if (_needle != null)
                _needle.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
