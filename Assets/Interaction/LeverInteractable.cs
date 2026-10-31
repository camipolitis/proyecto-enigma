using System.Collections;
using Enigma.Environment;
using UnityEngine;

namespace Enigma.Interaction
{
    // Palanca: anima en Fail y Success, setea flag solo en Success.
    public class LeverInteractable : InteractableBase
    {
        [SerializeField] private Transform leverPivot;
        [SerializeField] private float failedAngle = 25f;
        [SerializeField] private float successAngle = 45f;
        [SerializeField] private float animDuration = 0.35f;
        [SerializeField] private GameObject sparkPrefab;

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
            Enigma.Audio.SfxPlayer.Instance?.Play("lever", transform.position);
            PlayAngle(successAngle);
            SpawnSpark();
            Vector3 steamPos = leverPivot != null ? leverPivot.position : transform.position;
            BurstParticles.Spawn(BurstParticles.Kind.Steam, steamPos);
        }

        protected override void HandleFailExtra(InteractContext context)
        {
            PlayAngle(failedAngle, returnBack: true);
        }

        private void SpawnSpark()
        {
            Vector3 pos = leverPivot != null ? leverPivot.position : transform.position;
            BurstParticles.Spawn(BurstParticles.Kind.Spark, pos);
            if (sparkPrefab != null)
                Instantiate(sparkPrefab, pos, Quaternion.identity);
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
            yield return PropAnimator.Play(leverPivot, target, animDuration);
            if (returnBack)
                yield return PropAnimator.Play(leverPivot, start, animDuration);
        }
    }
}
