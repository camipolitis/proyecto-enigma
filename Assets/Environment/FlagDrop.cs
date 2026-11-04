using Enigma.Audio;
using Enigma.Core;
using UnityEngine;

namespace Enigma.Environment
{
    // Queda quieto hasta que su flag pasa a true y ahí cae con física (el candado al abrir con la llave)
    [RequireComponent(typeof(Rigidbody))]
    public class FlagDrop : MonoBehaviour
    {
        [SerializeField] private string flagId;
        [SerializeField] private Vector3 kick = new Vector3(0f, 0.4f, -0.6f);
        [SerializeField] private string landSfx;
        [SerializeField] private float minImpact = 0.4f;

        private Rigidbody _rb;
        private bool _dropped;
        private bool _launch;

        public bool Dropped => _dropped;

        public void Configure(string flag, Vector3 impulse, string sfx)
        {
            flagId = flag;
            kick = impulse;
            landSfx = sfx;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
        }

        private void OnEnable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void Start()
        {
            if (GameFlagSystem.Instance != null)
            {
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
                if (!string.IsNullOrEmpty(flagId) && GameFlagSystem.Instance.Get(flagId))
                    Drop();
            }
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void OnFlag(string id, bool value)
        {
            if (value && id == flagId)
                Drop();
        }

        private void Drop()
        {
            if (_dropped)
                return;
            _dropped = true;
            AttemptMotion.StopAll(transform);
            _rb.isKinematic = false;
            _launch = true;
        }

        // El paso a dinámico se aplica en el próximo paso de física y descarta la velocidad puesta antes
        private void FixedUpdate()
        {
            if (!_launch)
                return;
            _launch = false;
            _rb.linearVelocity = transform.TransformDirection(kick) / Mathf.Max(0.01f, _rb.mass);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_dropped || string.IsNullOrEmpty(landSfx) || collision.relativeVelocity.magnitude < minImpact)
                return;
            if (SfxPlayer.Instance != null)
                SfxPlayer.Instance.Play(landSfx, collision.GetContact(0).point);
        }
    }
}
