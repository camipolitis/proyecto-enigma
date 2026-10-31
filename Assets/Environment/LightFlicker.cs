using UnityEngine;

namespace Enigma.Environment
{
    // Luz intermitente de N1. Contraste + apagones cortos.
    public class LightFlicker : MonoBehaviour
    {
        public float baseIntensity = 6.5f;
        private Light _light;
        private int _phase;
        private float _wait;
        private float _off;
        private float _gap;
        private int _left;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _wait = Random.Range(4f, 12f);
        }

        private void Update()
        {
            if (_light == null)
                return;

            if (_phase == 1)
            {
                _off -= Time.deltaTime;
                _light.intensity = baseIntensity * 0.04f;
                if (_off > 0f)
                    return;
                if (_left > 0)
                {
                    _phase = 2;
                    _gap = Random.Range(0.04f, 0.08f);
                }
                else
                {
                    _phase = 0;
                    _wait = Random.Range(4f, 12f);
                }
                return;
            }

            if (_phase == 2)
            {
                FlickerNow();
                _gap -= Time.deltaTime;
                if (_gap > 0f)
                    return;
                _left--;
                _phase = 1;
                _off = Random.Range(0.06f, 0.09f);
                return;
            }

            FlickerNow();
            _wait -= Time.deltaTime;
            if (_wait > 0f)
                return;

            _left = Random.Range(1, 3);
            _left--;
            _phase = 1;
            _off = Random.Range(0.06f, 0.09f);
        }

        private void FlickerNow()
        {
            float t = Time.time;
            float n = Mathf.PerlinNoise(t * 8.4f, 0.37f);
            _light.intensity = baseIntensity * (0.28f + n * 1.12f) + Mathf.Sin(t * 22f) * 1.15f;
        }
    }
}
