using System.Collections;
using Enigma.CameraSystem;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Enigma.Cinematics
{
    // Tomas de intro / cierre. El gameplay sigue en ThirdPersonCamera; el zoom E no se toca.
    public class CinematicDirector : MonoBehaviour
    {
        public static CinematicDirector Instance { get; private set; }

        private Camera _cam;
        private ThirdPersonCamera _gameplay;
        private CinemachineBrain _brain;
        private bool _playing;

        public bool IsPlaying => _playing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnScene;
            SceneManager.sceneLoaded += OnScene;
            Bind(SceneManager.GetActiveScene());
        }

        private static void OnScene(Scene scene, LoadSceneMode mode)
        {
            Bind(scene);
        }

        private static void Bind(Scene scene)
        {
            if (!scene.IsValid() || scene.name == "MainMenu")
                return;
            if (FindFirstObjectByType<CinematicDirector>() != null)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            var director = cam.gameObject.AddComponent<CinematicDirector>();
            director._cam = cam;
            director._gameplay = cam.GetComponent<ThirdPersonCamera>();
        }

        private void Awake()
        {
            Instance = this;
            if (_cam == null)
                _cam = GetComponent<Camera>();
            if (_gameplay == null)
                _gameplay = GetComponent<ThirdPersonCamera>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static IEnumerator PlayIntro(Transform player)
        {
            var dir = Instance;
            if (dir == null || player == null)
                yield break;

            Vector3 pos = player.position + Vector3.up * 2.1f + player.forward * -2.6f;
            yield return dir.PlayShot(pos, player, 2.2f);
        }

        public static IEnumerator PlayClose()
        {
            var dir = Instance;
            if (dir == null)
                yield break;

            Transform look = FindCloseTarget();
            if (look == null)
                yield break;

            Vector3 pos = look.position + Vector3.up * 1.5f + look.forward * -2.4f;
            yield return dir.PlayShot(pos, look, 1.5f);
        }

        private static Transform FindCloseTarget()
        {
            var hatch = GameObject.Find("HatchPivot");
            if (hatch != null)
                return hatch.transform;

            var door = GameObject.Find("DoorPivot");
            if (door != null)
                return door.transform;

            var n3 = GameObject.Find("DoorPivotN3");
            if (n3 != null)
                return n3.transform;

            var origen = GameObject.Find("Interactable_OrigenDoor");
            if (origen != null)
                return origen.transform;

            var player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform : null;
        }

        private IEnumerator PlayShot(Vector3 worldPos, Transform lookAt, float seconds)
        {
            if (_playing || _cam == null || lookAt == null)
                yield break;
            if (InteractionZoomController.Instance != null && InteractionZoomController.Instance.IsZooming)
                yield break;

            _playing = true;
            float savedFov = _cam.fieldOfView;
            float savedNear = _cam.nearClipPlane;
            float savedFar = _cam.farClipPlane;
            GameObject shot = null;
            if (_gameplay != null)
                _gameplay.enabled = false;

            try
            {
                _brain = _cam.GetComponent<CinemachineBrain>();
                if (_brain == null)
                    _brain = _cam.gameObject.AddComponent<CinemachineBrain>();
                _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
                _brain.enabled = true;

                shot = new GameObject("CineShot");
                shot.transform.position = worldPos;
                shot.transform.LookAt(lookAt);
                var vcam = shot.AddComponent<CinemachineCamera>();
                vcam.Priority = 100;
                var lens = vcam.Lens;
                lens.FieldOfView = 48f;
                vcam.Lens = lens;

                float t = 0f;
                while (t < seconds)
                {
                    t += Time.unscaledDeltaTime;
                    if (shot != null && lookAt != null)
                        shot.transform.LookAt(lookAt);
                    yield return null;
                }
            }
            finally
            {
                if (shot != null)
                    Destroy(shot);
                if (_brain != null)
                    _brain.enabled = false;
                _cam.fieldOfView = savedFov;
                _cam.nearClipPlane = savedNear;
                _cam.farClipPlane = savedFar;
                if (_gameplay != null)
                {
                    _gameplay.CaptureCurrentAngles();
                    _gameplay.enabled = true;
                }

                _playing = false;
            }
        }
    }
}
