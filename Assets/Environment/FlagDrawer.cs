using Enigma.Core;
using UnityEngine;

namespace Enigma.Environment
{
    // Cajón que se abre solo cuando un flag pasa a true.
    public class FlagDrawer : MonoBehaviour
    {
        [SerializeField] private string flagId;
        // Recorrido en el espacio del padre
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, -0.3f);
        [SerializeField] private float duration = 0.9f;
        [SerializeField] private string sfxId = "drawer_open";

        private Vector3 _closed;
        private bool _open;

        public void Configure(string flag, Vector3 offset, float seconds)
        {
            flagId = flag;
            openOffset = offset;
            duration = seconds;
        }

        private void Awake()
        {
            _closed = transform.localPosition;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
            // Al recargar con el flag ya puesto queda abierto
            if (IsSet())
            {
                _open = true;
                transform.localPosition = _closed + openOffset;
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
            Enigma.Audio.SfxPlayer.Instance?.Play(sfxId, transform.position);
            StartCoroutine(PropAnimator.PlaySlide(transform, _closed + openOffset, duration));
        }
    }
}
