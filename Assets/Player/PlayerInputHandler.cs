using UnityEngine;
using UnityEngine.InputSystem;
using Enigma.Core;

namespace Enigma.Player
{
    // Lee InputSystem_Actions por código y expone estado + eventos
    [DefaultExecutionOrder(-100)]
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
        private InputAction _hotbarScroll;
        private bool _backConsumed;

        private static readonly Key[] HotbarDigitKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
            Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8
        };

        private static readonly Key[] HotbarNumpadKeys =
        {
            Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4,
            Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8
        };

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchPressedThisFrame { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }
        public bool BackPressedThisFrame { get; private set; }
        public bool PausePressedThisFrame { get; private set; }
        public bool InventoryPressedThisFrame { get; private set; }
        public bool PreviousPressedThisFrame { get; private set; }
        public bool NextPressedThisFrame { get; private set; }
        public int HotbarSlotPressedThisFrame { get; private set; } = -1;
        // 0-7 = tecla 1-8; -1 = ninguna.

        public bool GameplayInputEnabled { get; private set; } = true;

        private void Awake()
        {
            BindActions();
        }

        public void BindActions()
        {
            if (inputActions == null)
                return;

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
            _hotbarScroll = _playerMap.FindAction("HotbarScroll");

            if (isActiveAndEnabled)
                _playerMap.Enable();
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
            CrouchHeld = false;
            InteractPressedThisFrame = false;
            BackPressedThisFrame = false;
            PausePressedThisFrame = false;
            InventoryPressedThisFrame = false;
            PreviousPressedThisFrame = false;
            NextPressedThisFrame = false;
            HotbarSlotPressedThisFrame = -1;
            _backConsumed = false;

            if (_pause == null)
                return;

            PausePressedThisFrame = _pause.WasPressedThisFrame();
            BackPressedThisFrame = _back.WasPressedThisFrame();
            InventoryPressedThisFrame = _inventory.WasPressedThisFrame();
            InteractPressedThisFrame = _interact.WasPressedThisFrame();

            bool paused = PauseSystem.Instance != null && PauseSystem.Instance.IsPaused;
            bool blockHotbar = paused ||
                (ModalStack.Instance != null && ModalStack.Instance.BlocksWorldInteract());

            if (GameplayInputEnabled && !blockHotbar)
                ReadHotbar();

            bool blocked = ModalStack.Instance != null && ModalStack.Instance.BlocksGameplay();
            if (!GameplayInputEnabled || blocked || paused)
            {
                MoveInput = Vector2.zero;
                LookInput = Vector2.zero;
                SprintHeld = false;
                return;
            }

            MoveInput = _move.ReadValue<Vector2>();
            LookInput = _look.ReadValue<Vector2>();
            SprintHeld = _sprint.IsPressed();
            JumpPressed = _jump.WasPressedThisFrame();
            CrouchPressedThisFrame = _crouch.WasPressedThisFrame();
            CrouchHeld = _crouch.IsPressed();
        }

        // Rueda cicla el slot; 1-8 saltan directo.
        private void ReadHotbar()
        {
            if (_previous != null && _previous.WasPressedThisFrame())
                PreviousPressedThisFrame = true;
            if (_next != null && _next.WasPressedThisFrame())
                NextPressedThisFrame = true;

            float scroll = ReadScrollDelta();
            if (scroll > 0.0001f)
                PreviousPressedThisFrame = true;
            else if (scroll < -0.0001f)
                NextPressedThisFrame = true;

            var kb = Keyboard.current;
            if (kb == null)
                return;

            for (int i = 0; i < 8; i++)
            {
                if (kb[HotbarDigitKeys[i]].wasPressedThisFrame ||
                    kb[HotbarNumpadKeys[i]].wasPressedThisFrame)
                {
                    HotbarSlotPressedThisFrame = i;
                    return;
                }
            }
        }

        private float ReadScrollDelta()
        {
            Vector2 delta = Vector2.zero;
            if (_hotbarScroll != null)
                delta = _hotbarScroll.ReadValue<Vector2>();

            if (delta.sqrMagnitude < 0.0000001f && Mouse.current != null)
                delta = Mouse.current.scroll.ReadValue();

            return Mathf.Abs(delta.y) >= Mathf.Abs(delta.x) ? delta.y : delta.x;
        }

        // Un solo modal por frame se queda con Q.
        public bool TryConsumeBack()
        {
            if (!BackPressedThisFrame || _backConsumed)
                return false;

            _backConsumed = true;
            return true;
        }

        public void SetGameplayInputEnabled(bool enabled)
        {
            GameplayInputEnabled = enabled;
        }
    }
}
