using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Candado de flechas izquierda/derecha. Teclado, pad y botones.
    public class ArrowLockUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Text displayLabel;
        [SerializeField] private string correctCode = "LLLRRRLLR";
        [SerializeField] private string successFlagId = "hatch_unlocked";
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string failDialogueId = "hatch_wrong";

        private string _buffer = string.Empty;
        private Button _leftButton;

        protected override ModalKind Kind => ModalKind.ArrowLock;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private bool IsTop => ModalStack.Instance == null || ModalStack.Instance.IsTop(Kind);

        public void Open()
        {
            if (root == null)
                return;
            _buffer = string.Empty;
            RefreshDisplay();
            _leftButton = root.GetComponentInChildren<Button>(true);
            OpenModal();
            if (_leftButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_leftButton.gameObject);
        }

        private void Update()
        {
            if (!IsOpen)
                return;
            if (TryCloseWithBack())
                return;
            if (!IsTop)
                return;
            ReadArrows();
        }

        public void PressLeft()
        {
            if (!IsOpen || !IsTop)
                return;
            Append('L');
        }

        public void PressRight()
        {
            if (!IsOpen || !IsTop)
                return;
            Append('R');
        }

        private void ReadArrows()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                    Append('L');
                if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                    Append('R');
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                if (pad.dpad.left.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame)
                    Append('L');
                if (pad.dpad.right.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame)
                    Append('R');
            }
        }

        private void Append(char side)
        {
            if (_buffer.Length >= correctCode.Length)
                return;

            _buffer += side;
            RefreshDisplay();
            if (_buffer.Length >= correctCode.Length)
                Submit();
        }

        private void Submit()
        {
            if (_buffer == correctCode)
            {
                Close();
                if (!string.IsNullOrEmpty(successFlagId))
                    GameFlagSystem.Instance?.Set(successFlagId, true);
            }
            else
            {
                _buffer = string.Empty;
                RefreshDisplay();
                var subs = SubtitleSystem.Instance;
                if (dialogue != null && !string.IsNullOrEmpty(failDialogueId) &&
                    dialogue.TryGet(failDialogueId, out var line))
                    subs?.Show(line.text, line.duration, line.voice);
            }
        }

        private void RefreshDisplay()
        {
            if (displayLabel == null)
                return;

            var shown = new System.Text.StringBuilder();
            for (int i = 0; i < correctCode.Length; i++)
            {
                if (i < _buffer.Length)
                    shown.Append(_buffer[i] == 'L' ? "← " : "→ ");
                else
                    shown.Append("_ ");
            }
            displayLabel.text = shown.ToString();
        }
    }
}
