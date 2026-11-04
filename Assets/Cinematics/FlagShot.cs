using Enigma.Core;
using UnityEngine;

namespace Enigma.Cinematics
{
    // Cuando un flag pasa a true la cámara mira lo que cambió (las luces que se prenden).
    public class FlagShot : MonoBehaviour
    {
        [SerializeField] private string flagId;
        [SerializeField] private Transform focus;
        [SerializeField] private float seconds = 2.5f;
        // Cuánto tiene que entrar en cuadro alrededor del foco
        [SerializeField] private float radius = 1f;

        private bool _played;

        public void Configure(string flag, Transform target, float duration, float reach)
        {
            flagId = flag;
            focus = target;
            seconds = duration;
            radius = reach;
        }

        private void OnEnable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void Start()
        {
            if (GameFlagSystem.Instance == null)
                return;
            GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
            GameFlagSystem.Instance.OnFlagChanged += OnFlag;
            // Si ya estaba prendido al cargar no hay nada que mostrar
            _played = GameFlagSystem.Instance.Get(flagId);
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void OnFlag(string id, bool value)
        {
            if (!value || id != flagId || _played || focus == null)
                return;
            _played = true;
            StartCoroutine(CinematicDirector.PlayFocus(focus, seconds, radius));
        }
    }
}
