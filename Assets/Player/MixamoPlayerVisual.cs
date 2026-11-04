using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Enigma.Player
{
    // Megan + clips Mixamo. El capsule del player no se toca.
    public class MixamoPlayerVisual : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnScene;
            SceneManager.sceneLoaded += OnScene;
            OnScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnScene(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid() || scene.name == "MainMenu")
                return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                var low = Object.FindFirstObjectByType<LowPolyPlayerVisual>();
                if (low != null)
                    player = low.gameObject;
            }
            if (player == null)
                return;
            if (player.GetComponent<MixamoPlayerVisual>() != null)
                return;

            player.AddComponent<MixamoPlayerVisual>();
        }

        private Animator _anim;
        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private PlayerStateController _state;
        private CharacterController _cc;
        private ThirdPersonController _move;
        private bool _gettingUp;
        private bool _wasProne;
        private bool _posed;
        private bool _airborne;
        private float _upTime;
        private float _upLen = 1.2f;
        private float _jumpLen = 0.8f;
        private bool _hasJump;
        private bool _ready;
        private const int Inputs = 11;
        private const int StairsUp = 8;
        private const int StairsDown = 9;
        private const int StairsRunUp = 10;
        // Pendiente (subida/avance) desde la que el andar cuenta como escalera
        private const float StairSlope = 0.25f;
        private readonly float[] _loopLen = new float[Inputs];
        private readonly float[] _w = new float[Inputs];
        private readonly float[] _wTarget = new float[Inputs];
        // Altura que gana la cadera en una vuelta del clip de escalera
        private readonly float[] _climb = new float[Inputs];
        private const int SoleSamples = 24;
        private readonly float[] _sole = new float[Inputs];
        private float[] _standSole;
        private float _standLen = 1f;
        private Transform _head;
        // Partes del cuerpo por vértice (tronco, manos, antebrazos, pies, piernas); la cabeza no apoya
        private int[][] _vertGroup;
        private const int Groups = 9;
        private readonly float[] _groupLow = new float[Groups];
        private readonly float[] _band = new float[Groups];
        // Las partes que quedan a esta distancia del punto más bajo apoyan juntas
        private const float ContactBand = 0.08f;
        // Lo más que la cara puede quedar bajo el piso cuando el cuerpo apoya
        private const float HeadSink = 0.07f;
        private float _vy;
        private Transform _hips;
        private Vector3 _proneOffset;
        // Sombra de contacto (hijo ContactShadow del prefab): una huella por parte del cuerpo
        private struct ShadowPart
        {
            public Transform a, b;
            public float radius;
            // La mano sigue la dirección del antebrazo más allá del hueso
            public bool tip;
        }
        private Transform _shadow;
        private ShadowPart[] _shadowParts;
        private Mesh _shadowMesh;
        private Vector3[] _shadowVerts;
        private readonly RaycastHit[] _groundHits = new RaycastHit[8];
        // Altura sobre el piso a la que la huella de una parte desaparece
        private const float ShadowLift = 0.25f;
        // Cuánto se abre la huella respecto del grosor de la parte
        private const float ShadowSpread = 1.7f;
        private const float Crossfade = 0.32f;
        private const float GetUpFade = 0.5f;
        private const float MinGetUp = 0.4f;
        // Margen de altura (fracción) de cadera y cabeza para dar por terminado el levantarse
        private const float SettleTolerance = 0.02f;

        public bool IsGettingUp => _gettingUp;
        public float GetUpLength => _upLen;

        private void Start()
        {
            _state = GetComponent<PlayerStateController>();
            _cc = GetComponent<CharacterController>();
            _move = GetComponent<ThirdPersonController>();
            Attach();
        }

        private void Attach()
        {
            var prefab = Resources.Load<GameObject>("Mixamo/Megan");
            if (prefab == null)
                return;

            var existing = transform.Find("MixamoRoot");
            if (existing != null)
                Object.Destroy(existing.gameObject);

            var go = Object.Instantiate(prefab, transform);
            go.name = "MixamoRoot";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
                Object.Destroy(cols[i]);

            ApplyUrpMaterials(go);
            FitHeight(go);

            _anim = go.GetComponent<Animator>();
            if (_anim == null)
                _anim = go.AddComponent<Animator>();
            _anim.applyRootMotion = false;
            _anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var low = GetComponent<LowPolyPlayerVisual>();
            if (low != null)
            {
                low.BuildIfNeeded();
                low.SetVisible(true);
                var cubes = transform.Find("VisualRoot");
                if (cubes != null)
                    cubes.gameObject.SetActive(false);
            }

            BuildGraph();
            BindShadow();
            _ready = _graph.IsValid();
            _wasProne = _state != null && _state.State == PlayerGameplayState.Prone;
        }

        private static void ApplyUrpMaterials(GameObject go)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
                return;

            Texture body = Resources.Load<Texture2D>("Mixamo/Megan_Diffuse");
            Texture hair = Resources.Load<Texture2D>("Mixamo/Megan_Hair");
            var skins = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < skins.Length; i++)
            {
                var rend = skins[i];
                var srcs = rend.sharedMaterials;
                if (srcs == null || srcs.Length == 0)
                    continue;
                var mats = new Material[srcs.Length];
                for (int m = 0; m < srcs.Length; m++)
                {
                    var src = srcs[m];
                    var mat = new Material(lit);
                    mat.name = (src != null ? src.name : "Megan") + "_URP";
                    string n = ((src != null ? src.name : "") + " " + rend.gameObject.name).ToLowerInvariant();
                    Texture map = n.IndexOf("hair") >= 0 && hair != null ? hair : body;
                    if (map != null)
                        mat.SetTexture("_BaseMap", map);
                    mat.SetColor("_BaseColor", Color.white);
                    if (mat.HasProperty("_Smoothness"))
                        mat.SetFloat("_Smoothness", 0.22f);
                    mats[m] = mat;
                }
                rend.sharedMaterials = mats;
            }
        }

        private static void FitHeight(GameObject go)
        {
            var skins = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skins.Length == 0)
                return;
            Bounds b = skins[0].bounds;
            for (int i = 1; i < skins.Length; i++)
                b.Encapsulate(skins[i].bounds);
            if (b.size.y < 0.2f)
                return;
            go.transform.localScale *= 1.72f / b.size.y;
        }

        private void BuildGraph()
        {
            var idle = Clip("Idle");
            var walk = Clip("Walk");
            var run = Clip("Running");
            var cIdle = Clip("CrouchIdle");
            var cWalk = Clip("CrouchWalk");
            var prone = Clip("Prone");
            var stand = Clip("StandingUp");
            var jump = Clip("StandingJump");
            var up = Clip("StairsUp");
            var down = Clip("StairsDown");
            var runUp = Clip("StairsRunUp");
            if (idle == null && walk == null)
                return;

            MeasureGetUp(prone, stand);
            MeasureClimb(StairsUp, up);
            MeasureClimb(StairsDown, down);
            MeasureClimb(StairsRunUp, runUp);
            MeasureSoles(new[] { idle, walk, run, cIdle, cWalk, prone, stand, jump, up, down, runUp });
            _graph = PlayableGraph.Create("PlayerMixamo");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _mixer = AnimationMixerPlayable.Create(_graph, Inputs);
            var output = AnimationPlayableOutput.Create(_graph, "Anim", _anim);
            output.SetSourcePlayable(_mixer);

            Connect(0, idle, true);
            Connect(1, walk, true);
            Connect(2, run, true);
            Connect(3, cIdle, true);
            Connect(4, cWalk, true);
            Connect(5, prone, true);
            Connect(6, stand, false, true);
            Connect(7, jump, false, false);
            Connect(StairsUp, up, true);
            Connect(StairsDown, down, true);
            Connect(StairsRunUp, runUp, true);
            _hasJump = jump != null;
            if (_hasJump)
                _jumpLen = Mathf.Max(0.2f, jump.length);
            _graph.Play();
        }

        // Lee los huesos de los clips: desplazamiento acostada y momento en que queda de pie
        private void MeasureGetUp(AnimationClip prone, AnimationClip stand)
        {
            if (stand != null)
                _upLen = Mathf.Max(MinGetUp, stand.length);
            _hips = FindBone(_anim.transform, "Hips");
            var head = FindBone(_anim.transform, "Head");
            if (_hips == null || head == null || prone == null || stand == null)
                return;

            prone.SampleAnimation(_anim.gameObject, 0f);
            Vector3 lying = InPlayer(_hips);
            stand.SampleAnimation(_anim.gameObject, 0f);
            Vector3 rising = InPlayer(_hips);
            _proneOffset = new Vector3(rising.x - lying.x, 0f, rising.z - lying.z);

            // De pie recién cuando cadera y cabeza llegan a su altura final (agachada, la cabeza queda baja)
            stand.SampleAnimation(_anim.gameObject, stand.length);
            float hipsEnd = InPlayer(_hips).y;
            float headEnd = InPlayer(head).y;
            float step = 1f / Mathf.Max(1f, stand.frameRate);
            float settle = stand.length;
            for (float t = stand.length; t >= 0f; t -= step)
            {
                stand.SampleAnimation(_anim.gameObject, t);
                if (!Settled(InPlayer(_hips).y, hipsEnd) || !Settled(InPlayer(head).y, headEnd))
                    break;
                settle = t;
            }
            _upLen = Mathf.Max(MinGetUp, settle);
        }

        private void MeasureClimb(int i, AnimationClip clip)
        {
            if (clip == null || _hips == null)
                return;
            clip.SampleAnimation(_anim.gameObject, 0f);
            float start = InPlayer(_hips).y;
            clip.SampleAnimation(_anim.gameObject, clip.length);
            _climb[i] = InPlayer(_hips).y - start;
        }

        // Altura del punto más bajo de la malla en cada clip (índice = entrada del mixer): eso apoya en el piso
        private void MeasureSoles(AnimationClip[] clips)
        {
            var skins = _anim.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skins.Length == 0)
                return;
            _head = FindBone(_anim.transform, "Head");
            var mesh = new Mesh();
            ClassifyVertices(clips[0], skins, mesh);
            for (int i = 0; i < clips.Length && i < Inputs; i++)
            {
                var clip = clips[i];
                if (clip == null)
                    continue;
                if (i == 6)
                {
                    // Levantarse pasa de acostada a de pie: se guarda la curva
                    _standLen = Mathf.Max(0.01f, clip.length);
                    _standSole = new float[SoleSamples + 1];
                    for (int k = 0; k <= SoleSamples; k++)
                        _standSole[k] = Lowest(clip, clip.length * k / SoleSamples, skins, mesh);
                    continue;
                }
                // Los ciclos apoyan en su punto más bajo; el salto y la escalera, en su primer cuadro
                int samples = i == 7 || i >= StairsUp ? 1 : 8;
                float low = float.MaxValue;
                for (int k = 0; k < samples; k++)
                    low = Mathf.Min(low, Lowest(clip, clip.length * k / samples, skins, mesh));
                _sole[i] = low;
            }
            Object.Destroy(mesh);
        }

        // Altura de apoyo: de pie solo llegan los pies; acostada, tronco, manos, rodillas y pies apoyan juntos
        // y manda la mediana, así ninguna extremidad queda en el aire; la cabeza solo pone un límite
        private float Lowest(AnimationClip clip, float t, SkinnedMeshRenderer[] skins, Mesh mesh)
        {
            clip.SampleAnimation(_anim.gameObject, t);
            for (int g = 0; g < Groups; g++)
                _groupLow[g] = float.MaxValue;
            float headLow = float.MaxValue;
            for (int s = 0; s < skins.Length; s++)
            {
                var smr = skins[s];
                smr.BakeMesh(mesh, true);
                var verts = mesh.vertices;
                var groups = _vertGroup[s];
                Vector3 pos = smr.transform.position;
                Quaternion rot = smr.transform.rotation;
                for (int v = 0; v < verts.Length && v < groups.Length; v++)
                {
                    int g = groups[v];
                    float y = InPlayer(pos + rot * verts[v]).y;
                    if (g < 0)
                        headLow = Mathf.Min(headLow, y);
                    else if (y < _groupLow[g])
                        _groupLow[g] = y;
                }
            }
            float low = float.MaxValue;
            for (int g = 0; g < Groups; g++)
                low = Mathf.Min(low, _groupLow[g]);
            int n = 0;
            for (int g = 0; g < Groups; g++)
                if (_groupLow[g] <= low + ContactBand)
                    _band[n++] = _groupLow[g];
            if (n == 0)
                return low;
            System.Array.Sort(_band, 0, n);
            return Mathf.Min(_band[(n - 1) / 2], headLow + HeadSink);
        }

        // Cada vértice pertenece a la parte cuyo hueso tiene más cerca (la malla no es legible en el build: se usa la horneada)
        private void ClassifyVertices(AnimationClip pose, SkinnedMeshRenderer[] skins, Mesh mesh)
        {
            var root = _anim.transform;
            Transform B(string n) => FindBone(root, n);
            Vector3 Tip(Transform hand, Transform fore) =>
                hand == null ? Vector3.zero : hand.position + (fore != null ? (hand.position - fore.position).normalized * 0.15f : Vector3.zero);
            if (pose != null)
                pose.SampleAnimation(_anim.gameObject, 0f);
            Transform hips = B("Hips"), spine = B("Spine2"), neck = B("Neck"), head = B("Head"), top = B("HeadTop_End");
            Transform la = B("LeftArm"), lf = B("LeftForeArm"), lh = B("LeftHand"), ra = B("RightArm"), rf = B("RightForeArm"), rh = B("RightHand");
            Transform lu = B("LeftUpLeg"), ll = B("LeftLeg"), lft = B("LeftFoot"), lt = B("LeftToe_End"), ru = B("RightUpLeg"), rl = B("RightLeg"), rft = B("RightFoot"), rt = B("RightToe_End");
            var segs = new System.Collections.Generic.List<(Vector3 a, Vector3 b, int g)>();
            void Add(Transform a, Transform b, int g) { if (a != null && b != null) segs.Add((a.position, b.position, g)); }
            Add(hips, spine, 0); Add(spine, neck, 0); Add(neck, head, 0); Add(la, lf, 0); Add(ra, rf, 0); Add(lu, ll, 0); Add(ru, rl, 0);
            Add(head, top, -1);
            if (lh != null) segs.Add((lh.position, Tip(lh, lf), 1));
            if (rh != null) segs.Add((rh.position, Tip(rh, rf), 2));
            Add(lf, lh, 3); Add(rf, rh, 4); Add(lft, lt, 5); Add(rft, rt, 6); Add(ll, lft, 7); Add(rl, rft, 8);

            _vertGroup = new int[skins.Length][];
            for (int s = 0; s < skins.Length; s++)
            {
                skins[s].BakeMesh(mesh, true);
                var verts = mesh.vertices;
                Vector3 pos = skins[s].transform.position;
                Quaternion rot = skins[s].transform.rotation;
                var groups = new int[verts.Length];
                for (int v = 0; v < verts.Length; v++)
                {
                    Vector3 w = pos + rot * verts[v];
                    float best = float.MaxValue;
                    for (int k = 0; k < segs.Count; k++)
                    {
                        float d = SegmentDistanceSq(w, segs[k].a, segs[k].b);
                        if (d < best)
                        {
                            best = d;
                            groups[v] = segs[k].g;
                        }
                    }
                }
                _vertGroup[s] = groups;
            }
        }

        private static float SegmentDistanceSq(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float len = ab.sqrMagnitude;
            float k = len > 1e-6f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / len) : 0f;
            return (p - (a + ab * k)).sqrMagnitude;
        }

        private float Sole()
        {
            float sum = 0f, weight = 0f;
            for (int i = 0; i < Inputs; i++)
            {
                if (_w[i] <= 0.001f)
                    continue;
                float s = _sole[i];
                if (i == 6 && _standSole != null)
                {
                    var p = _mixer.GetInput(6);
                    float u = p.IsValid() ? Mathf.Clamp01((float)p.GetTime() / _standLen) * SoleSamples : SoleSamples;
                    int k = Mathf.Min(Mathf.FloorToInt(u), SoleSamples - 1);
                    s = Mathf.Lerp(_standSole[k], _standSole[k + 1], u - k);
                }
                sum += _w[i] * s;
                weight += _w[i];
            }
            return weight > 0.001f ? sum / weight : 0f;
        }

        private static bool Settled(float height, float final)
        {
            return Mathf.Abs(height - final) <= final * SettleTolerance;
        }

        // Hueso en el espacio del Player, sin contar el desplazamiento del modelo
        private Vector3 InPlayer(Transform bone)
        {
            return InPlayer(bone.position);
        }

        private Vector3 InPlayer(Vector3 world)
        {
            var model = _anim.transform;
            Vector3 local = model.InverseTransformPoint(world);
            return model.localRotation * Vector3.Scale(model.localScale, local);
        }

        private static Transform FindBone(Transform root, string bone)
        {
            var all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name.EndsWith(":" + bone) || all[i].name == bone)
                    return all[i];
            }
            return null;
        }

        private void Connect(int i, AnimationClip clip, bool loop, bool clampDuration = true)
        {
            if (clip == null)
            {
                _mixer.SetInputWeight(i, 0f);
                return;
            }
            if (loop)
            {
                clip.wrapMode = WrapMode.Loop;
                _loopLen[i] = clip.length;
            }
            else
            {
                clip.wrapMode = WrapMode.ClampForever;
            }
            var p = AnimationClipPlayable.Create(_graph, clip);
            p.SetApplyFootIK(false);
            if (loop)
                p.SetDuration(double.PositiveInfinity);
            else if (clampDuration)
                p.SetDuration(clip.length);
            _mixer.ConnectInput(i, p, 0, 0f);
        }

        private void LateUpdate()
        {
            if (!_ready)
                return;

            bool prone = _state != null && _state.State == PlayerGameplayState.Prone;
            bool standing = _state != null && _state.State == PlayerGameplayState.Standing;
            if (_wasProne && !prone)
            {
                _gettingUp = true;
                _upTime = 0f;
                var up = _mixer.GetInput(6);
                if (up.IsValid())
                    up.SetTime(0);
            }
            _wasProne = prone;

            if (prone)
            {
                SetWeights(0, 0, 0, 0, 0, 1, 0, 0);
                ApplyWeights(Crossfade);
                PinRoot(_proneOffset);
                return;
            }

            if (_gettingUp || standing)
            {
                _upTime += Time.deltaTime;
                SetWeights(0, 0, 0, 0, 0, 0, 1, 0);
                ApplyWeights(GetUpFade);
                if (_upTime >= _upLen)
                    _gettingUp = false;
                // El desplazamiento sigue el peso de Prone: la cadera no se mueve en el fundido
                float pair = _w[5] + _w[6];
                PinRoot(pair > 0.001f ? _proneOffset * (_w[5] / pair) : Vector3.zero);
                return;
            }

            bool air = _move != null ? _move.IsAirborne : (_cc != null && !_cc.isGrounded);
            if (air && !_airborne)
            {
                var jump = _mixer.GetInput(7);
                if (jump.IsValid())
                {
                    jump.SetTime(0);
                    jump.SetSpeed(1);
                }
            }
            if (!air && _airborne)
            {
                for (int i = 0; i <= 4; i++)
                {
                    var p = _mixer.GetInput(i);
                    if (p.IsValid())
                        p.SetSpeed(1);
                }
            }
            _airborne = air;
            if (air)
            {
                if (_hasJump)
                {
                    SetWeights(0, 0, 0, 0, 0, 0, 0, 1);
                    HoldEnd(7, _jumpLen);
                }
                else
                {
                    var run = _mixer.GetInput(2);
                    if (run.IsValid())
                    {
                        run.SetSpeed(0);
                        run.SetTime(_loopLen[2] * 0.22);
                    }
                    SetWeights(0, 0, 1, 0, 0, 0, 0, 0);
                }
                ApplyWeights(0.12f);
                PinRoot();
                return;
            }

            float speed = 0f;
            if (_cc != null)
            {
                var v = _cc.velocity;
                speed = new Vector3(v.x, 0f, v.z).magnitude;
            }
            bool crouch = _cc != null && _cc.height < 1.45f;
            float vy = _cc != null ? _cc.velocity.y : 0f;
            _vy = Mathf.Lerp(_vy, vy, 1f - Mathf.Exp(-12f * Time.deltaTime));
            int stairs = crouch ? -1 : StairsClip(speed);
            if (stairs >= 0)
            {
                SetWeights(0, 0, 0, 0, 0, 0, 0, 0);
                _wTarget[stairs] = 1f;
                // El clip avanza al ritmo con que el capsule sube o baja
                float rate = Mathf.Abs(_climb[stairs]) / _loopLen[stairs];
                var clip = _mixer.GetInput(stairs);
                if (clip.IsValid() && rate > 0.01f)
                    clip.SetSpeed(Mathf.Clamp(Mathf.Abs(_vy) / rate, 0.4f, 2.5f));
            }
            else if (crouch)
            {
                float w = Mathf.Clamp01(speed / 1.2f);
                SetWeights(0, 0, 0, 1f - w, w, 0, 0, 0);
            }
            else if (speed < 0.15f)
            {
                SetWeights(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
            }
            else if (speed < 3.2f)
            {
                float t = Mathf.InverseLerp(0.15f, 2.2f, speed);
                SetWeights(1f - t, t, 0, 0, 0, 0, 0, 0);
            }
            else
            {
                float t = Mathf.InverseLerp(3.2f, 4.5f, speed);
                SetWeights(0, 1f - t, t, 0, 0, 0, 0, 0);
            }
            ApplyWeights(Crossfade);
            PinRoot(new Vector3(0f, -StairsDrift(), 0f));
        }

        private int StairsClip(float speed)
        {
            if (speed < 0.15f || Mathf.Abs(_vy) < speed * StairSlope)
                return -1;
            int i = _vy < 0f ? StairsDown : (speed >= 3.2f && _loopLen[StairsRunUp] > 0f ? StairsRunUp : StairsUp);
            return _loopLen[i] > 0f && Mathf.Abs(_climb[i]) > 0.01f ? i : -1;
        }

        // Los clips de escalera suben la cadera en cada vuelta; el capsule ya sube: se descuenta la tendencia
        private float StairsDrift()
        {
            float drift = 0f;
            for (int i = StairsUp; i <= StairsRunUp; i++)
            {
                var p = _mixer.GetInput(i);
                if (_w[i] <= 0.001f || !p.IsValid() || _loopLen[i] <= 0f)
                    continue;
                float phase = (float)(p.GetTime() % _loopLen[i]) / _loopLen[i];
                drift += _w[i] * _climb[i] * phase;
            }
            return drift;
        }

        private void PinRoot(Vector3 offset = default)
        {
            if (_anim == null)
                return;
            var t = _anim.transform;
            // El capsule flota skinWidth sobre el piso; el punto más bajo de la pose va a esa altura
            float skin = _cc != null ? _cc.skinWidth : 0f;
            t.localPosition = offset + Vector3.down * (skin + Sole());
            t.localRotation = Quaternion.identity;
            PlaceShadow();
        }

        private void BindShadow()
        {
            _shadow = transform.Find("ContactShadow");
            var filter = _shadow != null ? _shadow.GetComponent<MeshFilter>() : null;
            if (filter == null || _anim == null)
            {
                _shadow = null;
                return;
            }
            var root = _anim.transform;
            var parts = new System.Collections.Generic.List<ShadowPart>();
            void Add(string a, string b, float radius, bool tip = false)
            {
                Transform ta = FindBone(root, a), tb = FindBone(root, b);
                if (ta != null && tb != null)
                    parts.Add(new ShadowPart { a = ta, b = tb, radius = radius, tip = tip });
            }
            Add("Hips", "Neck", 0.17f);
            Add("Neck", "HeadTop_End", 0.1f);
            foreach (var s in new[] { "Left", "Right" })
            {
                Add(s + "Arm", s + "ForeArm", 0.06f);
                Add(s + "ForeArm", s + "Hand", 0.05f);
                Add(s + "Hand", s + "ForeArm", 0.055f, true);
                Add(s + "UpLeg", s + "Leg", 0.09f);
                Add(s + "Leg", s + "Foot", 0.065f);
                Add(s + "Foot", s + "Toe_End", 0.055f);
            }
            _shadowParts = parts.ToArray();
            int n = _shadowParts.Length;
            _shadowVerts = new Vector3[n * 4];
            var uv = new Vector2[n * 4];
            var tris = new int[n * 6];
            for (int i = 0; i < n; i++)
            {
                uv[i * 4] = new Vector2(0f, 0f);
                uv[i * 4 + 1] = new Vector2(1f, 0f);
                uv[i * 4 + 2] = new Vector2(1f, 1f);
                uv[i * 4 + 3] = new Vector2(0f, 1f);
                tris[i * 6] = i * 4; tris[i * 6 + 1] = i * 4 + 2; tris[i * 6 + 2] = i * 4 + 1;
                tris[i * 6 + 3] = i * 4; tris[i * 6 + 4] = i * 4 + 3; tris[i * 6 + 5] = i * 4 + 2;
            }
            _shadowMesh = new Mesh { name = "ContactShadow" };
            _shadowMesh.MarkDynamic();
            _shadowMesh.vertices = _shadowVerts;
            _shadowMesh.uv = uv;
            _shadowMesh.triangles = tris;
            filter.sharedMesh = _shadowMesh;
        }

        // Una huella por parte del cuerpo sobre el piso que tiene debajo; se achica al despegarse (de pie quedan los pies, en el salto nada)
        private void PlaceShadow()
        {
            if (_shadow == null || _shadowParts == null)
                return;
            for (int i = 0; i < _shadowParts.Length; i++)
            {
                var part = _shadowParts[i];
                Vector3 a = part.a.position;
                Vector3 b = part.tip ? a + (a - part.b.position).normalized * 0.15f : part.b.position;
                int v = i * 4;
                if (!Ground((a + b) * 0.5f, out var ground))
                {
                    _shadowVerts[v] = _shadowVerts[v + 1] = _shadowVerts[v + 2] = _shadowVerts[v + 3] = Vector3.zero;
                    continue;
                }
                Vector3 up = ground.normal;
                float low = Mathf.Min(Vector3.Dot(a - ground.point, up), Vector3.Dot(b - ground.point, up)) - part.radius;
                float k = 1f - Mathf.Clamp01(low / ShadowLift);
                Vector3 axis = Vector3.ProjectOnPlane(b - a, up);
                float len = axis.magnitude;
                Vector3 dir = len > 1e-4f ? axis / len : Vector3.ProjectOnPlane(transform.forward, up).normalized;
                Vector3 side = Vector3.Cross(up, dir);
                float halfW = part.radius * ShadowSpread * k;
                float halfL = len * 0.5f * k + part.radius * ShadowSpread * k;
                Vector3 c = ground.point + up * 0.01f;
                _shadowVerts[v] = _shadow.InverseTransformPoint(c - dir * halfL - side * halfW);
                _shadowVerts[v + 1] = _shadow.InverseTransformPoint(c - dir * halfL + side * halfW);
                _shadowVerts[v + 2] = _shadow.InverseTransformPoint(c + dir * halfL + side * halfW);
                _shadowVerts[v + 3] = _shadow.InverseTransformPoint(c + dir * halfL - side * halfW);
            }
            _shadowMesh.vertices = _shadowVerts;
            _shadowMesh.RecalculateBounds();
        }

        private bool Ground(Vector3 above, out RaycastHit ground)
        {
            int n = Physics.RaycastNonAlloc(above + Vector3.up * 0.6f, Vector3.down, _groundHits, 3f, ~0, QueryTriggerInteraction.Ignore);
            int best = -1;
            for (int i = 0; i < n; i++)
            {
                // Lo que está por encima de la parte la tapa (la chapa sobre las piernas), no la sostiene
                if (_groundHits[i].collider.transform.IsChildOf(transform) || _groundHits[i].point.y > above.y)
                    continue;
                if (best < 0 || _groundHits[i].distance < _groundHits[best].distance)
                    best = i;
            }
            ground = best >= 0 ? _groundHits[best] : default;
            return best >= 0;
        }

        private void HoldEnd(int i, float len)
        {
            var p = _mixer.GetInput(i);
            if (!p.IsValid() || len <= 0.05f)
                return;
            if (p.GetTime() >= len)
            {
                p.SetTime(len - 0.02);
                p.SetSpeed(0);
            }
        }

        private void SetWeights(float idle, float walk, float run, float cIdle, float cWalk, float prone, float stand, float jump)
        {
            _wTarget[0] = idle;
            _wTarget[1] = walk;
            _wTarget[2] = run;
            _wTarget[3] = cIdle;
            _wTarget[4] = cWalk;
            _wTarget[5] = prone;
            _wTarget[6] = stand;
            _wTarget[7] = jump;
            for (int i = StairsUp; i < Inputs; i++)
                _wTarget[i] = 0f;
        }

        private void ApplyWeights(float fade)
        {
            // La primera pose entra de golpe: fundida desde cero, la malla arranca mezclada con la pose de bind
            float step = fade <= 0.01f || !_posed ? 1f : Time.deltaTime / fade;
            _posed = true;
            for (int i = 0; i < Inputs; i++)
            {
                _w[i] = Mathf.MoveTowards(_w[i], _wTarget[i], step);
                _mixer.SetInputWeight(i, _w[i]);
            }
        }

        private void OnDestroy()
        {
            if (_graph.IsValid())
                _graph.Destroy();
            if (_shadowMesh != null)
                Destroy(_shadowMesh);
        }

        private static AnimationClip Clip(string name)
        {
            var clips = Resources.LoadAll<AnimationClip>("Mixamo/" + name);
            return clips != null && clips.Length > 0 ? clips[0] : null;
        }
    }
}
