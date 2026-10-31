using UnityEngine;
using Enigma.Cinematics;
using Enigma.Player;

namespace Enigma.CameraSystem
{
    // Cámara tercera persona independiente (no hija del Player).
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        // CameraFollowPoint del Player.

        [SerializeField] private PlayerInputHandler input;

        [SerializeField] private float distance = 3.5f;
        [SerializeField] private float minDistance = 1.6f;
        [SerializeField] private float maxDistance = 6f;
        [SerializeField] private float zoomStep = 0.45f;
        [SerializeField] private float heightOffset = 0.15f;
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float gamepadSensitivity = 120f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private LayerMask collisionMask = ~0;
        // Colisión contra paredes para no atravesar geometría

        private const float CrouchDip = 0.6f;

        private float _yaw;
        private float _pitch = 15f;
        private float _crouchDip;
        private bool _orbitEnabled = true;
        private PlayerStateController _state;
        private MixamoPlayerVisual _mixamo;
        private ThirdPersonController _move;

        public Transform FollowTarget => followTarget;

        private void Awake()
        {
            heightOffset = 0.15f;
            // El capsule del player no puede empujar la cámara a un costado.
            int env = LayerMask.NameToLayer("Environment");
            int mask = 1 << 0;
            if (env >= 0)
                mask |= 1 << env;
            int player = LayerMask.NameToLayer("Player");
            int ui = LayerMask.NameToLayer("UI");
            int ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (player >= 0)
                mask &= ~(1 << player);
            if (ui >= 0)
                mask &= ~(1 << ui);
            if (ignore >= 0)
                mask &= ~(1 << ignore);
            collisionMask = mask;
        }

        private void Start()
        {
            BindFollow();
            if (followTarget != null)
            {
                Vector3 lp = followTarget.localPosition;
                followTarget.localPosition = new Vector3(lp.x, 1.52f, lp.z);
                _yaw = followTarget.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (followTarget == null || !_orbitEnabled)
                return;

            ReadLook();
            ReadZoom();
            PlaceCamera();
        }

        private void BindFollow()
        {
            if (followTarget == null)
                return;
            _state = followTarget.GetComponentInParent<PlayerStateController>();
            _mixamo = followTarget.GetComponentInParent<MixamoPlayerVisual>();
            _move = followTarget.GetComponentInParent<ThirdPersonController>();
        }

        private void ReadLook()
        {
            if (input == null)
                return;

            Vector2 look = input.LookInput;

            bool likelyMouse = look.magnitude > 1.5f;
            float sens = likelyMouse ? mouseSensitivity : gamepadSensitivity * Time.deltaTime;

            _yaw += look.x * sens;
            _pitch -= look.y * sens;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private void ReadZoom()
        {
            if (!CanZoom())
                return;

            float s = input.ScrollDelta;
            if (Mathf.Abs(s) < 0.01f)
                return;

            float notches = Mathf.Abs(s) > 2f ? Mathf.Clamp(s / 120f, -2f, 2f) : Mathf.Sign(s);
            distance = Mathf.Clamp(distance - notches * zoomStep, minDistance, maxDistance);
        }

        private bool CanZoom()
        {
            if (input == null || !input.GameplayInputEnabled)
                return false;
            if (_state == null || _mixamo == null)
                BindFollow();
            if (_state != null && !_state.CanMove)
                return false;
            if (_mixamo != null && _mixamo.IsGettingUp)
                return false;
            if (CinematicDirector.Instance != null && CinematicDirector.Instance.IsPlaying)
                return false;
            return true;
        }

        private void PlaceCamera()
        {
            if (_move == null || _state == null)
                BindFollow();

            bool crouch = _move != null && _move.IsCrouching
                && _state != null && _state.CanMove;
            float want = crouch ? CrouchDip : 0f;
            _crouchDip = Mathf.Lerp(_crouchDip, want, 10f * Time.deltaTime);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 targetPos = followTarget.position + Vector3.up * (heightOffset - _crouchDip);
            Vector3 desired = targetPos - rotation * Vector3.forward * distance;

            if (Physics.SphereCast(targetPos, collisionRadius, (desired - targetPos).normalized,
                    out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                desired = hit.point + hit.normal * collisionRadius;
                // Acerca la cámara si hay pared en medio
            }

            transform.position = desired;
            transform.rotation = rotation;
        }

        public void SetOrbitEnabled(bool enabled)
        {
            _orbitEnabled = enabled;
            // Zoom lo desactiva mientras mira el anchor.
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            BindFollow();
        }

        public void CaptureCurrentAngles()
        {
            Vector3 e = transform.eulerAngles;
            _yaw = e.y;
            _pitch = e.x > 180f ? e.x - 360f : e.x;
            // Al salir del zoom, mantiene la orientación actual
        }
    }
}
