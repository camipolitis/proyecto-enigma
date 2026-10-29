using System.Collections;
using Enigma.Core;
using Enigma.Data;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Escotilla: abre el candado de flechas y gira al acertar.
    public class ArrowLockInteractable : InteractableBase
    {
        [SerializeField] private ArrowLockUI lockUi;
        [SerializeField] private Transform hatchPivot;
        [SerializeField] private float openPitch = -85f;
        [SerializeField] private float openDuration = 0.8f;
        [SerializeField] private string unlockFlagId = "hatch_unlocked";

        private bool _opened;

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
                if (GameFlagSystem.Instance.Get(unlockFlagId))
                    SnapOpen();
            }
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        protected override bool IsCompleted() => _opened;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (lockUi == null || _opened)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        protected override void OnSuccess(InteractContext context)
        {
            lockUi.Open();
        }

        private void OnFlag(string id, bool value)
        {
            if (!value || id != unlockFlagId || _opened)
                return;

            _opened = true;
            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
            var subs = SubtitleSystem.Instance;
            string lineId = config != null ? config.successDialogueId : null;
            if (subs != null && dialogue != null && !string.IsNullOrEmpty(lineId) &&
                dialogue.TryGet(lineId, out var line))
                subs.Show(line.text, line.duration, line.voice);
            if (hatchPivot != null)
                StartCoroutine(OpenHatch());
        }

        private void SnapOpen()
        {
            _opened = true;
            if (hatchPivot != null)
                hatchPivot.localRotation = Quaternion.Euler(openPitch, 0f, 0f);
        }

        private IEnumerator OpenHatch()
        {
            Quaternion start = hatchPivot.localRotation;
            Quaternion end = Quaternion.Euler(openPitch, 0f, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / openDuration;
                hatchPivot.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }
    }
}
