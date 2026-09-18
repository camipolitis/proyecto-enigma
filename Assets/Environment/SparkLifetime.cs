using System.Collections;
using UnityEngine;

namespace Enigma.Environment
{
    // Destruye la chispa con tiempo no escalado (la pausa no la deja colgada).
    public class SparkLifetime : MonoBehaviour
    {
        [SerializeField] private float duration = 1.1f;

        private IEnumerator Start()
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
