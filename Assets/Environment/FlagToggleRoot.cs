using Enigma.Core;
using UnityEngine;

namespace Enigma.Environment
{
    // Prende o apaga un objeto cuando un flag pasa a true.
    public class FlagToggleRoot : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private string flagId;

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
            }

            Apply();
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void OnFlag(string id, bool value)
        {
            if (id == flagId)
                Apply();
        }

        private void Apply()
        {
            if (target == null || string.IsNullOrEmpty(flagId))
                return;

            bool on = GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(flagId);
            target.SetActive(on);
        }
    }
}
