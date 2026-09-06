using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.STP;

namespace Enigma.Interaction
{
    // Puerta condicionada por flag: al Success rota y dispara fin de nivel vía flag.
    public class ConditionalDoorInteractable : InteractableBase
    {
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float openYaw = 90f;
        [SerializeField] private float openDuration = 0.8f;
        [SerializeField] private GameObject powerLight;
        // Luz verde opcional cuando hay energía.

        private bool _opened;

        private void OnEnable()
        {
            if (Enigma.Core.GameFlagSystem.Instance != null)
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void OnDisable()
        {
            if (Enigma.Core.GameFlagSystem.Instance != null)
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void OnFlag(string id, bool value)
        {
            if (config != null && id == config.requiredFlagId && powerLight != null)
                powerLight.SetActive(value);
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
