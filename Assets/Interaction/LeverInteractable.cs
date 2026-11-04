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
        // Arranque a soga: la manija sale hacia el frente del pivote y vuelve (0 = palanca que gira)
        [SerializeField] private float pullDistance;
        // Cuerda que va del anclaje a la manija; se estira sobre su eje Z
        [SerializeField] private Transform pullCord;

        private bool _activated;
        private Coroutine _anim;
        private Vector3 _pivotRest;
        private Quaternion _rotationRest;
        private int _leverBusy;

        public void ConfigurePull(Transform pivot, float distance, Transform cord)
        {
            leverPivot = pivot;
            pullDistance = distance;
            pullCord = cord;
            // El arranque sale de la tapa del motor, no de la manija que se mueve
            AnchorCompleteFx(transform);
            FitCord();
        }

        private void Awake()
        {
            if (leverPivot != null)
            {
                _pivotRest = leverPivot.localPosition;
                _rotationRest = leverPivot.localRotation;
            }
        }

        private void FitCord()
        {
            if (pullCord == null || leverPivot == null)
                return;
            Vector3 to = leverPivot.position - pullCord.position;
            float scale = pullCord.parent != null ? pullCord.parent.lossyScale.z : 1f;
            pullCord.localScale = new Vector3(1f, 1f, Mathf.Max(0.001f, to.magnitude / Mathf.Max(0.0001f, scale)));
        }

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

        // Un E durante el rebote no lo reinicia: siempre vuelve a la pose grabada
        protected override void HandleFailExtra(InteractContext context)
        {
            if (_leverBusy > 0 || (pullDistance > 0f && _anim != null))
                return;
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

            if (pullDistance > 0f)
            {
                if (_anim != null)
                    StopCoroutine(_anim);
                _anim = StartCoroutine(AnimatePull(Mathf.Abs(angle) >= Mathf.Abs(successAngle) ? 1f : 0.55f));
                return;
            }
            // Cortar el clip a la mitad deja su grafo vivo: el éxito espera que termine el rebote
            _anim = StartCoroutine(AfterCurrent(_anim, angle, returnBack));
        }

        private IEnumerator AfterCurrent(Coroutine current, float angle, bool returnBack)
        {
            _leverBusy++;
            if (current != null)
                yield return current;
            yield return AnimateLever(angle, returnBack);
            _leverBusy--;
        }

        // Tirón rápido y la soga se recoge más lento, como el resorte del arranque
        private IEnumerator AnimatePull(float amount)
        {
            Vector3 rest = _pivotRest;
            Vector3 dir = leverPivot.parent != null
                ? leverPivot.parent.InverseTransformDirection(leverPivot.forward)
                : leverPivot.forward;
            Vector3 outPos = rest + dir.normalized * (pullDistance * amount);
            yield return Slide(leverPivot.localPosition, outPos, animDuration);
            yield return Slide(outPos, rest, animDuration * 1.8f);
            FitCord();
            _anim = null;
        }

        private IEnumerator Slide(Vector3 from, Vector3 to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                leverPivot.localPosition = Vector3.Lerp(from, to, k);
                FitCord();
                yield return null;
            }
            leverPivot.localPosition = to;
            FitCord();
        }

        private IEnumerator AnimateLever(float targetAngle, bool returnBack)
        {
            Quaternion target = Quaternion.Euler(targetAngle, 0f, 0f);
            yield return PropAnimator.Play(leverPivot, target, animDuration);
            if (returnBack)
                yield return PropAnimator.Play(leverPivot, _rotationRest, animDuration);
        }
    }
}
