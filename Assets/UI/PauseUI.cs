using Enigma.Core;
using Enigma.Player;
using UnityEngine;

namespace Enigma.UI
{
    // Escucha Pause del input y delega en PauseSystem. Q no reanuda.
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private PauseSystem pauseSystem;

        private void Update()
        {
            if (input == null || pauseSystem == null)
                return;

            if (input.PausePressedThisFrame)
                pauseSystem.Toggle();
        }
    }
}
