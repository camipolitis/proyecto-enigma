using System;
using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Panel numérico: teclado + pad en pantalla.
    public class CodeEntryUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text displayLabel;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private int maxDigits = 6;
        [SerializeField] private string correctCode = "131278";
        [SerializeField] private string successFlagId = "notebook_unlocked";
        [SerializeField] private string failSubtitle = "Código incorrecto...";
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string failDialogueId;

        private string _buffer = string.Empty;
        private Action _onSuccess;

        protected override ModalKind Kind => ModalKind.CodeEntry;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private bool IsTop => ModalStack.Instance == null || ModalStack.Instance.IsTop(ModalKind.CodeEntry);

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (TryCloseWithBack())
                return;

            if (!IsTop)
                return;

            ReadDigits();
        }

        public void Configure(string title, string code, int digits, string flagId, string failLine, Action onSuccess = null)
        {
            if (titleLabel != null)
                titleLabel.text = title;
            correctCode = code;
            maxDigits = digits;
            successFlagId = flagId;
            failSubtitle = failLine;
            _onSuccess = onSuccess;
        }

        public void Open()
        {
            _buffer = string.Empty;
            RefreshDisplay();
            OpenModal();

            var firstKey = root != null ? root.GetComponentInChildren<CodePadKey>(true) : null;
            if (firstKey != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstKey.gameObject);
        }

        public void Close(bool success)
        {
            if (!IsOpen)
                return;

            base.Close();
            if (success)
                _onSuccess?.Invoke();
        }

        public void PressDigit(int digit)
        {
            if (!IsOpen || !IsTop)
                return;
            AppendDigit((char)('0' + Mathf.Clamp(digit, 0, 9)));
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

        private void ReadDigits()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;

            for (int i = 0; i <= 9; i++)
            {
                Key key = Key.Digit0 + i;
                Key numpad = Key.Numpad0 + i;
                if (kb[key].wasPressedThisFrame || kb[numpad].wasPressedThisFrame)
                    AppendDigit((char)('0' + i));
            }

            if (kb.backspaceKey.wasPressedThisFrame)
                PressBackspace();

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                Submit();
        }

        private void AppendDigit(char c)
        {
            if (_buffer.Length >= maxDigits)
                return;
            _buffer += c;
            RefreshDisplay();
            if (_buffer.Length >= maxDigits)
                Submit();
        }

        private void Submit()
        {
            if (_buffer == correctCode)
            {
                Close(true);
                if (!string.IsNullOrEmpty(successFlagId))
                    GameFlagSystem.Instance?.Set(successFlagId, true);
            }
            else
            {
                _buffer = string.Empty;
                RefreshDisplay();
                PlayFailLine();
            }
        }

        private void RefreshDisplay()
        {
            if (displayLabel == null)
                return;

            string shown = new string('*', _buffer.Length);
            string pad = new string('_', Mathf.Max(0, maxDigits - _buffer.Length));
            displayLabel.text = shown + pad;
        }

        private void PlayFailLine()
        {
            var subs = FindFirstObjectByType<SubtitleSystem>();
            if (subs == null)
                return;

            if (dialogue != null && !string.IsNullOrEmpty(failDialogueId) &&
                dialogue.TryGet(failDialogueId, out var line))
            {
                subs.Show(line.text, line.duration, line.voice);
                return;
            }

            subs.Show(failSubtitle, 2f);
        }
    }
}
