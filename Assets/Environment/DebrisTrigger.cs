using UnityEngine;

namespace Enigma.Environment
{
    // Un toque del jugador contra escombros (Rigidbody cinemático + trigger).
    public class DebrisTrigger : MonoBehaviour
    {
        private bool _hit;

        private void OnTriggerEnter(Collider other)
        {
            if (_hit || other == null)
                return;

            if (!other.CompareTag("Player"))
                return;

            _hit = true;
        }
    }
}
