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
        private bool _airborne;
        private float _upTime;
        private float _upLen = 1.2f;
        private float _jumpLen = 0.8f;
        private bool _hasJump;
        private bool _ready;
        private readonly float[] _loopLen = new float[8];
        private readonly float[] _w = new float[8];
        private readonly float[] _wTarget = new float[8];
        private const float Crossfade = 0.32f;
        private const float ProneForward = -0.4f;

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
            AlignFeet(go);

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

        private static void AlignFeet(GameObject go)
        {
            var skins = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skins.Length == 0)
                return;
            Bounds b = skins[0].bounds;
            for (int i = 1; i < skins.Length; i++)
                b.Encapsulate(skins[i].bounds);
            var parent = go.transform.parent;
            float groundY = parent != null ? parent.position.y : go.transform.position.y;
            float dy = Mathf.Clamp(groundY - b.min.y, -0.04f, 0.06f);
            if (Mathf.Abs(dy) > 0.002f)
                go.transform.position += new Vector3(0f, dy, 0f);
            var lp = go.transform.localPosition;
            go.transform.localPosition = new Vector3(lp.x, 0f, lp.z);
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
            if (idle == null && walk == null)
                return;

            _graph = PlayableGraph.Create("PlayerMixamo");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _mixer = AnimationMixerPlayable.Create(_graph, 8);
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
            if (stand != null)
                _upLen = Mathf.Max(0.4f, stand.length);
            _hasJump = jump != null;
            if (_hasJump)
                _jumpLen = Mathf.Max(0.2f, jump.length);
            _graph.Play();
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
                PinRoot(ProneForward);
                return;
            }

            if (_gettingUp || standing)
            {
                _upTime += Time.deltaTime;
                SetWeights(0, 0, 0, 0, 0, 0, 1, 0);
                ApplyWeights(0.12f);
                if (_upTime >= _upLen)
                    _gettingUp = false;
                float standT = Mathf.Clamp01(_upTime / Mathf.Max(0.2f, _upLen));
                PinRoot(Mathf.Lerp(ProneForward, 0f, standT));
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
            if (crouch)
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
            PinRoot();
        }

        private void PinRoot(float forward = 0f)
        {
            if (_anim == null)
                return;
            var t = _anim.transform;
            t.localPosition = new Vector3(0f, 0f, forward);
            t.localRotation = Quaternion.identity;
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
        }

        private void ApplyWeights(float fade)
        {
            float step = fade <= 0.01f ? 1f : Time.deltaTime / fade;
            for (int i = 0; i < 8; i++)
            {
                _w[i] = Mathf.MoveTowards(_w[i], _wTarget[i], step);
                _mixer.SetInputWeight(i, _w[i]);
            }
        }

        private void OnDestroy()
        {
            if (_graph.IsValid())
                _graph.Destroy();
        }

        private static AnimationClip Clip(string name)
        {
            var clips = Resources.LoadAll<AnimationClip>("Mixamo/" + name);
            return clips != null && clips.Length > 0 ? clips[0] : null;
        }
    }
}
