using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Entrada de palabra (letras). Teclado y botones en pantalla.
    public class WordEntryUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Text displayLabel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private int maxLetters = 4;
        [SerializeField] private string correctWord = "VELA";
        [SerializeField] private string successFlagId = "pc_unlocked";
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string failDialogueId = "pc_password_fail";

        private string _buffer = string.Empty;
        private Button _firstKey;

        protected override ModalKind Kind => ModalKind.WordEntry;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private bool IsTop => ModalStack.Instance == null || ModalStack.Instance.IsTop(Kind);

        public void Open()
        {
            if (root == null)
                return;
            _buffer = string.Empty;
            RefreshDisplay();
            _firstKey = root.GetComponentInChildren<Button>(true);
            OpenModal();
            if (_firstKey != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_firstKey.gameObject);
        }

        private void Update()
        {
            if (!IsOpen)
                return;
            if (TryCloseWithBack())
                return;
            if (!IsTop)
                return;
            ReadLetters();
        }

        public void PressLetter(char letter)
        {
            if (!IsOpen || !IsTop)
                return;
            Append(char.ToUpperInvariant(letter));
        }

        public void PressBackspace()
        {
            if (!IsOpen || !IsTop || _buffer.Length == 0)
                return;
            _buffer = _buffer.Substring(0, _buffer.Length - 1);
            RefreshDisplay();
        }

        public void PressSubmit()
        {
            if (!IsOpen || !IsTop)
                return;
            Submit();
        }

        private void ReadLetters()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;

            for (int i = 0; i < 26; i++)
            {
                if (kb[Key.A + i].wasPressedThisFrame)
                    Append((char)('A' + i));
            }

            if (kb.backspaceKey.wasPressedThisFrame)
                PressBackspace();
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                Submit();
        }

        private void Append(char c)
        {
            if (c < 'A' || c > 'Z' || _buffer.Length >= maxLetters)
                return;
            _buffer += c;
            RefreshDisplay();
            if (_buffer.Length >= maxLetters)
                Submit();
        }

        private void Submit()
        {
            if (_buffer == correctWord)
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
            string shown = new string('*', _buffer.Length);
            string pad = new string('_', Mathf.Max(0, maxLetters - _buffer.Length));
            displayLabel.text = shown + pad;
        }
    }
}
