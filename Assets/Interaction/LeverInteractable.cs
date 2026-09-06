using System.Collections;
using UnityEngine;

namespace Enigma.Interaction
{
    // Palanca: anima en Fail y Success, setea flag solo en Success.
    public class LeverInteractable : InteractableBase
    {
        [SerializeField] private Transform leverPivot;
        [SerializeField] private float failedAngle = -25f;
        [SerializeField] private float successAngle = -45f;
        [SerializeField] private float animDuration = 0.35f;

        private bool _activated;
        private Coroutine _anim;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (_activated)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            _activated = true;
            PlayAngle(successAngle);
        }

        protected override void HandleFailExtra(InteractContext context)
        {
            PlayAngle(failedAngle, returnBack: true);
        }

        private void PlayAngle(float angle, bool returnBack = false)
        {
            if (leverPivot == null)
                return;

            if (_anim != null)
                StopCoroutine(_anim);
            _anim = StartCoroutine(AnimateLever(angle, returnBack));
        }

        private IEnumerator AnimateLever(float targetAngle, bool returnBack)
        {
            Quaternion start = leverPivot.localRotation;
            Quaternion target = Quaternion.Euler(targetAngle, 0f, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / animDuration;
                leverPivot.localRotation = Quaternion.Slerp(start, target, t);
                yield return null;
            }

            if (returnBack)
            {
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / animDuration;
                    leverPivot.localRotation = Quaternion.Slerp(target, start, t);
                    yield return null;
                }
            }
        }
    }
}
