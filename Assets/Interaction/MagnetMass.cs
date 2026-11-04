using UnityEngine;

namespace Enigma.Interaction
{
    // Peso del metal frente al imán. Cuanto más pesa, más fuerza necesita para llegar.
    public class MagnetMass : MonoBehaviour
    {
        [SerializeField] private float kg = 1f;

        public float Kg => kg;

        public void Set(float value)
        {
            kg = Mathf.Max(0.01f, value);
        }
    }
}
