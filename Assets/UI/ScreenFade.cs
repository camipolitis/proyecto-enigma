using UnityEngine;

namespace Enigma.UI
{
    // Fade a negro reutilizable (fin de nivel / transiciones).
    public class ScreenFade : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 1.2f;

        private void Awake()
        {
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

        private System.Collections.IEnumerator FadeTo(float target)
        {
            if (canvasGroup == null)
                yield break;

            canvasGroup.blocksRaycasts = target > 0.5f;
            float start = canvasGroup.alpha;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }
            canvasGroup.alpha = target;
        }
    }
}
