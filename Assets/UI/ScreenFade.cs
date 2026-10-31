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
