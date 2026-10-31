using Enigma.Core;
using UnityEngine;

namespace Enigma.Environment
{
    // Al cruzar el umbral, si la puerta/escotilla ya está abierta, dispara el fade.
    public class ExitSceneTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            TryExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryExit(other);
        }

        private static void TryExit(Collider other)
        {
            if (other == null)
                return;
            if (!other.CompareTag("Player"))
                return;
            GameManager.Instance?.TryCompleteFromExit();
        }
    }
}
