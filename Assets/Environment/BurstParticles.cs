using System.Collections;
using UnityEngine;

namespace Enigma.Environment
{
    // Chispa / polvo / vapor. Se destruye solo cuando termina el burst.
    public class BurstParticles : MonoBehaviour
    {
        public enum Kind
        {
            Spark,
            Dust,
            Steam
        }

        [SerializeField] private Kind kind;
        [SerializeField] private float life = 2.2f;

        public static void Spawn(Kind kind, Vector3 position)
        {
            var go = new GameObject("FX_" + kind);
            go.transform.position = position;
            var burst = go.AddComponent<BurstParticles>();
            burst.kind = kind;
        }

        private IEnumerator Start()
        {
            var ps = gameObject.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 48;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            var color = ps.colorOverLifetime;
            color.enabled = true;

            if (kind == Kind.Spark)
            {
                main.startLifetime = 0.35f;
                main.startSpeed = 2.4f;
                main.startSize = 0.06f;
                main.startColor = new Color(1f, 0.72f, 0.25f);
                main.gravityModifier = 0.8f;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 25f;
                life = 1.2f;
            }
            else if (kind == Kind.Dust)
            {
                main.startLifetime = 1.4f;
                main.startSpeed = 0.4f;
                main.startSize = 0.18f;
                main.startColor = new Color(0.55f, 0.5f, 0.42f, 0.7f);
                main.gravityModifier = 0.05f;
                shape.shapeType = ParticleSystemShapeType.Hemisphere;
                shape.radius = 0.35f;
                life = 2.2f;
            }
            else
            {
                main.startLifetime = 1.6f;
                main.startSpeed = 0.55f;
                main.startSize = 0.22f;
                main.startColor = new Color(0.75f, 0.82f, 0.85f, 0.45f);
                main.gravityModifier = -0.25f;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 12f;
                life = 2.4f;
            }

            ps.Play();

            float t = 0f;
            while (t < life)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
