using System.Collections;
using Enigma.CameraSystem;
using Enigma.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Enigma.Cinematics
{
    // Tomas con Cinemachine: intro y lo que se prende. El gameplay sigue en ThirdPersonCamera; el zoom E no se toca.
    [RequireComponent(typeof(Camera))]
    public class CinematicDirector : MonoBehaviour
    {
        public static CinematicDirector Instance { get; private set; }

        [SerializeField] private CinemachineBrain brain;
        [SerializeField] private CinemachineCamera shotCamera;
        // Copia la pose del gameplay: la toma entra y sale con blend en vez de corte
        [SerializeField] private CinemachineCamera returnCamera;
        [SerializeField] private PlayerStateController state;
        [SerializeField] private float blendTime = 0.9f;
        [SerializeField] private float shotFov = 48f;
        [SerializeField] private float maxFov = 78f;
        [SerializeField] private float fitMargin = 1.15f;
        [SerializeField] private float introHeight = 2.1f;
        [SerializeField] private float introBack = 2.6f;
        [SerializeField] private float introSeconds = 2.2f;

        private const int Idle = 0;
        private const int Live = 20;
        private const int Over = 30;

        private Camera _cam;
        private ThirdPersonCamera _gameplay;
        private bool _playing;

        public bool IsPlaying => _playing;

        private void Awake()
        {
            Instance = this;
            _cam = GetComponent<Camera>();
            _gameplay = GetComponent<ThirdPersonCamera>();
            if (brain != null)
            {
                brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendTime);
                brain.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // La grabación deja el rig en la escena: brain apagado y las dos cámaras virtuales inactivas
        public void EnsureRig()
        {
            brain = GetComponent<CinemachineBrain>();
            if (brain == null)
                brain = gameObject.AddComponent<CinemachineBrain>();
            brain.IgnoreTimeScale = true;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendTime);
            brain.enabled = false;

            Transform rig = null;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                if (root.name == "CinemachineRig")
                    rig = root.transform;
            if (rig == null)
                rig = new GameObject("CinemachineRig").transform;

            shotCamera = RigCamera(rig, "CM_Shot");
            if (shotCamera.GetComponent<CinemachineRotationComposer>() == null)
                shotCamera.gameObject.AddComponent<CinemachineRotationComposer>();
            returnCamera = RigCamera(rig, "CM_Return");

            // La grabación abre escenas aditivas: el jugador tiene que ser el de esta escena
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                if (state != null)
                    break;
                state = root.GetComponentInChildren<PlayerStateController>(true);
            }
        }

        private static CinemachineCamera RigCamera(Transform rig, string name)
        {
            var t = rig.Find(name);
            if (t == null)
            {
                t = new GameObject(name).transform;
                t.SetParent(rig, false);
            }
            var vcam = t.GetComponent<CinemachineCamera>();
            if (vcam == null)
                vcam = t.gameObject.AddComponent<CinemachineCamera>();
            vcam.Priority = Idle;
            t.gameObject.SetActive(false);
            return vcam;
        }

        public static IEnumerator PlayIntro(Transform player)
        {
            var dir = Instance;
            if (dir == null || player == null)
                yield break;

            Vector3 pos = player.position + Vector3.up * dir.introHeight - player.forward * dir.introBack;
            yield return dir.StartCoroutine(dir.PlayShot(pos, player, dir.introSeconds, false, dir.shotFov));
        }

        // Desde donde está la cámara, mira algo que acaba de cambiar; espera a que no haya zoom ni UI
        public static IEnumerator PlayFocus(Transform focus, float seconds, float radius)
        {
            var dir = Instance;
            if (dir == null || focus == null)
                yield break;
            while (dir.Busy())
                yield return null;
            yield return dir.StartCoroutine(dir.PlayShot(dir.transform.position, focus, seconds, true, dir.FovToFit(focus.position, radius)));
        }

        // Lente que abre lo justo para que entre el radio pedido a lo ancho
        private float FovToFit(Vector3 center, float radius)
        {
            float dist = Mathf.Max(0.5f, Vector3.Distance(transform.position, center));
            float halfWide = Mathf.Atan(radius * fitMargin / dist);
            float halfTall = Mathf.Atan(Mathf.Tan(halfWide) / Mathf.Max(0.1f, _cam.aspect));
            return Mathf.Clamp(halfTall * 2f * Mathf.Rad2Deg, shotFov, maxFov);
        }

        private bool Busy()
        {
            if (_playing)
                return true;
            if (InteractionZoomController.Instance != null && InteractionZoomController.Instance.IsZooming)
                return true;
            if (_gameplay != null && (!_gameplay.enabled || !_gameplay.OrbitEnabled))
                return true;
            return state != null && !state.CanMove;
        }

        // Corre en el director: si se apaga quien la pidió, la cámara igual vuelve al gameplay
        private IEnumerator PlayShot(Vector3 worldPos, Transform lookAt, float seconds, bool blendIn, float fov)
        {
            if (_playing || brain == null || shotCamera == null || returnCamera == null || lookAt == null)
                yield break;
            if (InteractionZoomController.Instance != null && InteractionZoomController.Instance.IsZooming)
                yield break;

            _playing = true;
            if (blendIn && state != null)
                state.SetState(PlayerGameplayState.Locked);
            if (_gameplay != null)
                _gameplay.enabled = false;

            try
            {
                Place(returnCamera, transform.position, transform.rotation, _cam.fieldOfView, Live);
                shotCamera.LookAt = lookAt;
                Place(shotCamera, worldPos, Quaternion.LookRotation(lookAt.position - worldPos), fov, blendIn ? Idle : Over);
                brain.enabled = true;
                if (blendIn)
                {
                    yield return null;
                    shotCamera.Priority = Over;
                }
                yield return Hold(seconds + (blendIn ? blendTime : 0f));

                Pose back = _gameplay != null ? _gameplay.DesiredPose() : new Pose(transform.position, transform.rotation);
                returnCamera.transform.SetPositionAndRotation(back.position, back.rotation);
                returnCamera.Priority = Over + 1;
                yield return Hold(blendTime);
            }
            finally
            {
                brain.enabled = false;
                shotCamera.Priority = Idle;
                returnCamera.Priority = Idle;
                shotCamera.gameObject.SetActive(false);
                returnCamera.gameObject.SetActive(false);
                if (_gameplay != null)
                {
                    _gameplay.CaptureCurrentAngles();
                    _gameplay.enabled = true;
                }
                if (blendIn && state != null && state.State == PlayerGameplayState.Locked)
                    state.SetState(PlayerGameplayState.Exploring);
                _playing = false;
            }
        }

        private void Place(CinemachineCamera vcam, Vector3 pos, Quaternion rot, float fov, int priority)
        {
            vcam.transform.SetPositionAndRotation(pos, rot);
            var lens = LensSettings.FromCamera(_cam);
            lens.FieldOfView = fov;
            vcam.Lens = lens;
            vcam.Priority = priority;
            vcam.gameObject.SetActive(true);
            vcam.PreviousStateIsValid = false;
        }

        private static IEnumerator Hold(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
