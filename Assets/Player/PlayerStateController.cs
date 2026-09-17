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
        [SerializeField] private PlayerGameplayState initialState = PlayerGameplayState.Exploring;
        // Level01 IntroSequence lo pasa a Prone al arrancar.

        public PlayerGameplayState State { get; private set; } = PlayerGameplayState.Exploring;

        public bool CanMove => State == PlayerGameplayState.Exploring;
        // Solo Exploring permite caminar libremente.

        private void Awake()
        {
            State = initialState;
        }

        public void SetState(PlayerGameplayState state)
        {
            State = state;
        }
    }
}
