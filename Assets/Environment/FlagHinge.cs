using Enigma.Core;
using UnityEngine;

namespace Enigma.Environment
{
    // Puerta con bisagra que se abre sola cuando un flag pasa a true.
    public class FlagHinge : MonoBehaviour
    {
        [SerializeField] private string flagId;
        // Giro sobre el eje Y local del pivote
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float duration = 0.9f;
        [SerializeField] private string sfxId = "door_open";

        private Quaternion _closed;
        private bool _open;

        public float OpenAngle => openAngle;

        public void Configure(string flag, float angle, float seconds)
        {
            flagId = flag;
            openAngle = angle;
            duration = seconds;
        }

        private Quaternion OpenRotation => _closed * Quaternion.Euler(0f, openAngle, 0f);

        private void Awake()
        {
            _closed = transform.localRotation;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
            // Al recargar con el flag ya puesto queda abierta
            if (IsSet())
            {
                _open = true;
                transform.localRotation = OpenRotation;
            }
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void Subscribe()
        {
            if (GameFlagSystem.Instance == null)
                return;
            GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
            GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private bool IsSet()
        {
            return !string.IsNullOrEmpty(flagId) && GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(flagId);
        }

        private void OnFlag(string id, bool value)
        {
            if (id != flagId || !value || _open)
                return;
            _open = true;
            AttemptMotion.StopAll(transform);
            Enigma.Audio.SfxPlayer.Instance?.Play(sfxId, transform.position);
            StartCoroutine(PropAnimator.Play(transform, OpenRotation, duration));
        }
    }
}
