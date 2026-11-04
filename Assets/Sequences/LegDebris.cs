using Enigma.Audio;
using Enigma.Player;
using UnityEngine;

namespace Enigma.Sequences
{
    // Lo que le cayó encima en la intro: tapa las piernas mientras está tirada y al levantarse cae con física
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody))]
    public class LegDebris : MonoBehaviour
    {
        [SerializeField] private PlayerStateController state;
        [SerializeField] private CharacterController body;
        [SerializeField] private float shinRadius = 0.07f;
        // Velocidades al soltarse (no dependen de la masa): sale de costado y gira para no quedar bajo las piernas
        [SerializeField] private float slideSpeed = 1.8f;
        [SerializeField] private float liftSpeed = 1.5f;
        [SerializeField] private float spinSpeed = 5f;
        [SerializeField] private string landSfx = "debris_land";
        [SerializeField] private float minImpact = 0.5f;
        [SerializeField] private float impactCooldown = 0.15f;

        private Rigidbody _rb;
        private Collider _col;
        private Transform _kneeL, _kneeR, _footL, _footR;
        private bool _released;
        private bool _launch;
        private float _lastImpact = -10f;

        public bool Released => _released;

        public void Configure(PlayerStateController player, CharacterController capsule)
        {
            state = player;
            body = capsule;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponentInChildren<Collider>();
            _rb.isKinematic = true;
            // La cápsula de Megan está parada donde ella yace: sin esto la chapa sale despedida al soltarse
            if (_col != null && body != null)
                Physics.IgnoreCollision(_col, body);
        }

        private void LateUpdate()
        {
            if (_released || state == null)
                return;
            if (state.State == PlayerGameplayState.Prone)
                RestOnShins();
            else
                Release();
        }

        private void RestOnShins()
        {
            if (_kneeL == null && !BindBones())
                return;
            Vector3 knees = (_kneeL.position + _kneeR.position) * 0.5f;
            Vector3 feet = (_footL.position + _footR.position) * 0.5f;
            Vector3 along = Vector3.ProjectOnPlane(feet - knees, Vector3.up);
            if (along.sqrMagnitude < 1e-4f)
                return;
            float half = _col is BoxCollider box ? box.size.y * 0.5f * transform.lossyScale.y : 0.02f;
            Vector3 mid = (knees + feet) * 0.5f;
            float top = Mathf.Max(knees.y, feet.y) + shinRadius;
            transform.SetPositionAndRotation(new Vector3(mid.x, top + half, mid.z), Quaternion.LookRotation(along.normalized, Vector3.up));
        }

        private bool BindBones()
        {
            if (state == null)
                return false;
            var bones = state.GetComponentsInChildren<Transform>(true);
            foreach (var t in bones)
            {
                if (Is(t, "LeftLeg")) _kneeL = t;
                else if (Is(t, "RightLeg")) _kneeR = t;
                else if (Is(t, "LeftFoot")) _footL = t;
                else if (Is(t, "RightFoot")) _footR = t;
            }
            if (_kneeL != null && _kneeR != null && _footL != null && _footR != null)
                return true;
            _kneeL = null;
            return false;
        }

        private static bool Is(Transform t, string bone)
        {
            return t.name == bone || t.name.EndsWith(":" + bone);
        }

        private void Release()
        {
            _released = true;
            _rb.isKinematic = false;
            _launch = true;
        }

        // La velocidad se da en el paso de física siguiente, ya como cuerpo dinámico
        private void FixedUpdate()
        {
            if (!_launch)
                return;
            _launch = false;
            Vector3 side = Vector3.Cross(Vector3.up, transform.forward);
            _rb.linearVelocity = side * slideSpeed + Vector3.up * liftSpeed;
            _rb.angularVelocity = -transform.forward * spinSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_released || collision.relativeVelocity.magnitude < minImpact)
                return;
            if (Time.time - _lastImpact < impactCooldown)
                return;
            _lastImpact = Time.time;
            if (SfxPlayer.Instance != null)
                SfxPlayer.Instance.Play(landSfx, collision.GetContact(0).point);
        }
    }
}
