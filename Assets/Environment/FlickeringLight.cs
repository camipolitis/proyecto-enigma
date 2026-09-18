using UnityEngine;

namespace Enigma.Environment
{
    // Luz parpadeante configurable 
    [RequireComponent(typeof(Light))]
    public class FlickeringLight : MonoBehaviour
    {
        [SerializeField] private float baseIntensity = 1.5f;
        [SerializeField] private float flickerAmplitude = 0.6f;
        [SerializeField] private float flickerSpeed = 8f;
        [SerializeField] private float chanceOfHardFlicker = 0.02f;

        private Light _light;
        private float _seed;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _seed = Random.value * 100f;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(_seed, Time.time * flickerSpeed);
            float intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmplitude;

            if (Random.value < chanceOfHardFlicker)
                intensity *= 0.15f;
            // Apagón corto ocasional

            _light.intensity = Mathf.Max(0f, intensity);
        }
    }
}
