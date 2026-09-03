using UnityEngine;
using Enigma.Player;

namespace Enigma.CameraSystem
{
    /// Cámara tercera persona independiente (no hija del Player).
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        // CameraFollowPoint del Player.

        [SerializeField] private PlayerInputHandler input;

        [SerializeField] private float distance = 3.5f;
        [SerializeField] private float heightOffset = 0.4f;
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float gamepadSensitivity = 120f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private LayerMask collisionMask = ~0;
        // Colisión contra paredes para no atravesar geometry

        private float _yaw;
        private float _pitch = 15f;
        private bool _orbitEnabled = true;

        public Transform FollowTarget => followTarget;

        private void Start()
        {
            if (followTarget != null)
                _yaw = followTarget.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (followTarget == null || !_orbitEnabled)
                return;

            ReadLook();
            PlaceCamera();
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

        private void PlaceCamera()
        {
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 targetPos = followTarget.position + Vector3.up * heightOffset;
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
