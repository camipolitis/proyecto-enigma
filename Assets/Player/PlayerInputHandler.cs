using UnityEngine;
using UnityEngine.InputSystem;
using Enigma.Core;

namespace Enigma.Player
{
    // Lee InputSystem_Actions por código y expone estado + eventos
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        // InputSystem_Actions va acá en el Inspector

        private InputActionMap _playerMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _interact;
        private InputAction _back;
        private InputAction _pause;
        private InputAction _inventory;
        private InputAction _sprint;
        private InputAction _jump;
        private InputAction _crouch;
        private InputAction _previous;
        private InputAction _next;
        // Referencias cacheadas: más barato que buscar por string cada frame...

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchPressedThisFrame { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }
        public bool BackPressedThisFrame { get; private set; }
        public bool PausePressedThisFrame { get; private set; }
        public bool InventoryPressedThisFrame { get; private set; }
        public bool PreviousPressedThisFrame { get; private set; }
        public bool NextPressedThisFrame { get; private set; }

        public bool GameplayInputEnabled { get; private set; } = true;
        // Intro y modales apagan Move/Look/Jump.

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("Falta InputActionAsset");
                enabled = false;
                return;
            }

            _playerMap = inputActions.FindActionMap("Player", true);
            _move = _playerMap.FindAction("Move", true);
            _look = _playerMap.FindAction("Look", true);
            _interact = _playerMap.FindAction("Interact", true);
            _back = _playerMap.FindAction("Back", true);
            _pause = _playerMap.FindAction("Pause", true);
            _inventory = _playerMap.FindAction("Inventory", true);
            _sprint = _playerMap.FindAction("Sprint", true);
            _jump = _playerMap.FindAction("Jump", true);
            _crouch = _playerMap.FindAction("Crouch", true);
            _previous = _playerMap.FindAction("Previous", true);
            _next = _playerMap.FindAction("Next", true);
        }

        private void OnEnable()
        {
            _playerMap?.Enable();
        }

        private void OnDisable()
        {
            _playerMap?.Disable();
        }

        private void Update()
        {
            JumpPressed = false;
            CrouchPressedThisFrame = false;
            InteractPressedThisFrame = false;
            BackPressedThisFrame = false;
            PausePressedThisFrame = false;
            InventoryPressedThisFrame = false;
            PreviousPressedThisFrame = false;
            NextPressedThisFrame = false;
            // Los "pressed this frame" se consumen una vez por Update

            PausePressedThisFrame = _pause.WasPressedThisFrame();
            BackPressedThisFrame = _back.WasPressedThisFrame();
            InventoryPressedThisFrame = _inventory.WasPressedThisFrame();
            InteractPressedThisFrame = _interact.WasPressedThisFrame();
            // Interact también fuera de gameplay: te levantás con la E

            bool blocked = ModalStack.Instance != null && ModalStack.Instance.BlocksGameplay();
            bool paused = PauseSystem.Instance != null && PauseSystem.Instance.IsPaused;

            if (!GameplayInputEnabled || blocked || paused)
            {
                MoveInput = Vector2.zero;
                LookInput = Vector2.zero;
                SprintHeld = false;
                return;
                // Congela movimiento mientras hay modal o intro
            }

            MoveInput = _move.ReadValue<Vector2>();
            LookInput = _look.ReadValue<Vector2>();
            SprintHeld = _sprint.IsPressed();
            JumpPressed = _jump.WasPressedThisFrame();
            CrouchPressedThisFrame = _crouch.WasPressedThisFrame();
            PreviousPressedThisFrame = _previous.WasPressedThisFrame();
            NextPressedThisFrame = _next.WasPressedThisFrame();
        }

        public void SetGameplayInputEnabled(bool enabled)
        {
            GameplayInputEnabled = enabled;
            // IntroSequence lo pone en false hasta levantarse.
        }
    }
}
