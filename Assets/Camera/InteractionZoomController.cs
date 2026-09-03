using System.Collections;
using UnityEngine;
using Enigma.Core;
using Enigma.Player;

namespace Enigma.CameraSystem
{
    // Blend de cámara hacia un ZoomAnchor, sale con Q (Back)
    public class InteractionZoomController : MonoBehaviour
    {
        public static InteractionZoomController Instance { get; private set; }

        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private PlayerStateController playerState;
        [SerializeField] private float blendDuration = 0.35f;

        private Transform _activeAnchor;
        private bool _allowsInventory;
        private Coroutine _blendRoutine;
        private Vector3 _savedPos;
        private Quaternion _savedRot;

        public bool IsZooming => _activeAnchor != null;
        public bool AllowsInventoryWhileZoom => _allowsInventory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void EnterZoom(Transform anchor, bool allowsInventory)
        {
            if (anchor == null)
                return;

            _activeAnchor = anchor;
            _allowsInventory = allowsInventory;

            _savedPos = transform.position;
            _savedRot = transform.rotation;

            thirdPersonCamera?.SetOrbitEnabled(false);
            playerState?.SetState(PlayerGameplayState.Locked);
            ModalStack.Instance?.Push(ModalKind.Zoom);

            if (_blendRoutine != null)
                StopCoroutine(_blendRoutine);
            _blendRoutine = StartCoroutine(BlendTo(anchor.position, anchor.rotation));
        }

        public void ExitZoom()
        {
            if (!IsZooming)
                return;

            _activeAnchor = null;
            _allowsInventory = false;

            ModalStack.Instance?.TryPopSpecific(ModalKind.Zoom);

            if (_blendRoutine != null)
                StopCoroutine(_blendRoutine);
            _blendRoutine = StartCoroutine(BlendBackAndRestore());
        }

        private IEnumerator BlendTo(Vector3 pos, Quaternion rot)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, blendDuration);
                float s = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(startPos, pos, s);
                transform.rotation = Quaternion.Slerp(startRot, rot, s);
                yield return null;
            }
        }

        private IEnumerator BlendBackAndRestore()
        {
            yield return BlendTo(_savedPos, _savedRot);

            thirdPersonCamera?.CaptureCurrentAngles();
            thirdPersonCamera?.SetOrbitEnabled(true);
            playerState?.SetState(PlayerGameplayState.Exploring);
            // Vuelve el control normal de tercera persona.
        }
    }
}
