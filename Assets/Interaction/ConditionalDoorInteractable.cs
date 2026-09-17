using System.Collections;
using UnityEngine;

namespace Enigma.Interaction
{
    // Puerta condicionada por flag: al Success rota y dispara fin de nivel vía flag.
    public class ConditionalDoorInteractable : InteractableBase
    {
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float openYaw = 90f;
        [SerializeField] private float openDuration = 0.8f;
        [SerializeField] private GameObject powerLight;
        // Luz verde opcional cuando hay energía / fusible.

        [SerializeField] private string fuseInstalledFlagId = "fuse_installed";
        // Si se setea, la luz también prende al colocar el fusible.

        private bool _opened;

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
            // Por si el flag ya estaba (reload) o el sistema nació después.
            TryApplyLightFromFlags();
        }

        private void OnDisable()
        {
            if (Enigma.Core.GameFlagSystem.Instance != null)
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void Subscribe()
        {
            if (Enigma.Core.GameFlagSystem.Instance == null)
                return;
            Enigma.Core.GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
            Enigma.Core.GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void TryApplyLightFromFlags()
        {
            if (powerLight == null || Enigma.Core.GameFlagSystem.Instance == null)
                return;

            if (!string.IsNullOrEmpty(fuseInstalledFlagId) &&
                Enigma.Core.GameFlagSystem.Instance.Get(fuseInstalledFlagId))
                SetLightsOn();

            if (config != null && !string.IsNullOrEmpty(config.requiredFlagId) &&
                Enigma.Core.GameFlagSystem.Instance.Get(config.requiredFlagId))
                SetLightsOn();
        }

        private void OnFlag(string id, bool value)
        {
            if (powerLight == null || !value)
                return;

            if (!string.IsNullOrEmpty(fuseInstalledFlagId) && id == fuseInstalledFlagId)
                SetLightsOn();

            if (config != null && id == config.requiredFlagId)
                SetLightsOn();
        }

        private void SetLightsOn()
        {
            if (powerLight == null)
                return;

            powerLight.SetActive(true);
            foreach (var l in powerLight.GetComponentsInChildren<Light>(true))
                l.enabled = true;
        }

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (_opened)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            _opened = true;
            MarkCompleted();
            if (doorPivot != null)
                StartCoroutine(OpenDoor());
        }

        private IEnumerator OpenDoor()
        {
            Quaternion start = doorPivot.localRotation;
            Quaternion end = Quaternion.Euler(0f, openYaw, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / openDuration;
                doorPivot.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }
    }
}
