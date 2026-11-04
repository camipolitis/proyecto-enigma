using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Fade a negro reutilizable (fin de nivel / transiciones).
    public class ScreenFade : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 1.2f;

        public float FadeDuration => fadeDuration;

        private const float MaxFadeStep = 1f / 30f;

        private void Awake()
        {
            var img = GetComponent<Image>();
            if (img == null)
                img = GetComponentInChildren<Image>(true);
            if (img != null)
            {
                img.color = Color.black;
                img.enabled = true;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public void FadeOut()
        {
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
        }

        public void FadeIn()
        {
            StopAllCoroutines();
            StartCoroutine(FadeTo(0f));
        }

        public System.Collections.IEnumerator FadeOutRoutine(float duration = -1f)
        {
            yield return FadeTo(1f, duration);
        }

        // Arranque de nivel: sale de negro de a poco
        public System.Collections.IEnumerator FadeFromBlack(float duration)
        {
            if (canvasGroup == null)
                yield break;
            canvasGroup.blocksRaycasts = false;
            float t = 0f;
            while (t < duration)
            {
                canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, t / duration);
                // Los primeros cuadros de la escena tardan en cargar: sin tope el fundido se consume en un salto
                t += Mathf.Min(Time.unscaledDeltaTime, MaxFadeStep);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        public void SetBlack()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator FadeTo(float target, float duration = -1f)
        {
            if (canvasGroup == null)
                yield break;

            float dur = duration > 0.01f ? duration : fadeDuration;
            canvasGroup.blocksRaycasts = target > 0.5f;
            canvasGroup.alpha = target;
            if (target < 0.5f)
                yield break;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
