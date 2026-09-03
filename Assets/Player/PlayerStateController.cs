using UnityEngine;

namespace Enigma.Player
{
    public enum PlayerGameplayState
    {
        Prone,
        Standing,
        Exploring,
        Locked
    }
    // Prone = intro tirado - Locked = zoom/UI

    public class PlayerStateController : MonoBehaviour
    {
        public PlayerGameplayState State { get; private set; } = PlayerGameplayState.Prone;

        public bool CanMove => State == PlayerGameplayState.Exploring;
        // Solo Exploring permite caminar libremente.

        public void SetState(PlayerGameplayState state)
        {
            State = state;
        }
    }
}
