using Enigma.Audio;
using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Cuatro cables: se elige izquierda y derecha del mismo color.
    public class WirePuzzleUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private string successFlagId = "power_restored";
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string doneDialogueId = "wires_done";

        private readonly bool[] _matched = new bool[4];
        private readonly int[] _rightOrder = { 2, 0, 3, 1 };
        private int _selectedLeft = -1;
        private Button _firstButton;

        protected override ModalKind Kind => ModalKind.WirePuzzle;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        public void Open()
        {
            if (root == null)
                return;

            for (int i = 0; i < _matched.Length; i++)
                _matched[i] = false;
            _selectedLeft = -1;
            RefreshButtons();
            var left0 = root.transform.Find("Box/Left_0");
            _firstButton = left0 != null ? left0.GetComponent<Button>() : root.GetComponentInChildren<Button>(true);
            OpenModal();

            if (_firstButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_firstButton.gameObject);
        }

        private void Update()
        {
            if (!IsOpen)
                return;
            TryCloseWithBack();
        }

        public void SelectLeft(int index)
        {
            if (!IsOpen || index < 0 || index >= _matched.Length || _matched[index])
                return;
            _selectedLeft = index;
        }

        public void SelectRight(int slot)
        {
            if (!IsOpen || _selectedLeft < 0)
                return;

            int rightColor = _rightOrder[slot];
            if (_matched[rightColor])
                return;

            if (_selectedLeft == rightColor)
            {
                _matched[rightColor] = true;
                _selectedLeft = -1;
                RefreshButtons();
                if (AllMatched())
                    Complete();
            }
            else
            {
                _selectedLeft = -1;
            }
        }

        private bool AllMatched()
        {
            for (int i = 0; i < _matched.Length; i++)
            {
                if (!_matched[i])
                    return false;
            }
            return true;
        }

        private void Complete()
        {
            Close();
            if (!string.IsNullOrEmpty(successFlagId))
                GameFlagSystem.Instance?.Set(successFlagId, true);
            SfxPlayer.Instance?.Play("wires");

            var subs = SubtitleSystem.Instance;
            if (dialogue != null && dialogue.TryGet(doneDialogueId, out var line))
                subs?.Show(line.text, line.duration, line.voice);
        }

        private void RefreshButtons()
        {
            if (root == null)
                return;

            for (int i = 0; i < 4; i++)
            {
                var left = root.transform.Find("Box/Left_" + i);
                if (left != null)
                    left.GetComponent<Button>().interactable = !_matched[i];

                int rightColor = _rightOrder[i];
                var right = root.transform.Find("Box/Right_" + i);
                if (right != null)
                    right.GetComponent<Button>().interactable = !_matched[rightColor];
            }
        }
    }
}
