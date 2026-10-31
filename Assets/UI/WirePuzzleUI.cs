using Enigma.Audio;
using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Cables negros. Adivinás el color. Un error reinicia la racha.
    public class WirePuzzleUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private string successFlagId = "power_restored";
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string doneDialogueId = "wires_done";

        private readonly bool[] _matchedColor = new bool[4];
        private readonly int[] _leftId = { 0, 1, 2, 3 };
        private readonly int[] _rightId = { 0, 1, 2, 3 };
        private int _selectedLeft = -1;
        private Button _firstButton;
        private bool _dressed;

        private static readonly Color Cable = new Color(0.12f, 0.12f, 0.13f, 1f);
        private static readonly Color CablePick = new Color(0.32f, 0.32f, 0.34f, 1f);
        private static readonly Color NeonWhite = new Color(0.92f, 1f, 1f, 1f);
        private static readonly Color[] Terminals =
        {
            new Color(0.82f, 0.16f, 0.14f, 1f),
            new Color(0.18f, 0.62f, 0.28f, 1f),
            new Color(0.18f, 0.42f, 0.86f, 1f),
            new Color(0.86f, 0.72f, 0.12f, 1f)
        };

        protected override ModalKind Kind => ModalKind.WirePuzzle;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        public void Open()
        {
            if (root == null)
                return;

            ShuffleIds(_leftId);
            ShuffleIds(_rightId);

            DressOnce();
            for (int i = 0; i < _matchedColor.Length; i++)
                _matchedColor[i] = false;
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
            if (!IsOpen || index < 0 || index >= _leftId.Length)
                return;
            if (_matchedColor[_leftId[index]])
                return;
            _selectedLeft = index;
            RefreshButtons();
        }

        public void SelectRight(int slot)
        {
            if (!IsOpen || _selectedLeft < 0 || slot < 0 || slot >= _rightId.Length)
                return;

            int color = _rightId[slot];
            if (_matchedColor[color])
                return;

            if (_leftId[_selectedLeft] == color)
            {
                _matchedColor[color] = true;
                _selectedLeft = -1;
                RefreshButtons();
                if (AllMatched())
                    Complete();
            }
            else
            {
                for (int i = 0; i < _matchedColor.Length; i++)
                    _matchedColor[i] = false;
                _selectedLeft = -1;
                RefreshButtons();
            }
        }

        private static void ShuffleIds(int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
                ids[i] = i;
            var rng = new System.Random(unchecked((int)System.DateTime.UtcNow.Ticks) ^ ids.GetHashCode());
            for (int i = ids.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = ids[i];
                ids[i] = ids[j];
                ids[j] = tmp;
            }
            if (ids[0] == 0 && ids[1] == 1 && ids[2] == 2 && ids[3] == 3)
            {
                ids[0] = 1;
                ids[1] = 0;
            }
        }

        private bool AllMatched()
        {
            for (int i = 0; i < _matchedColor.Length; i++)
            {
                if (!_matchedColor[i])
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

        private void DressOnce()
        {
            if (_dressed || root == null)
                return;
            _dressed = true;

            for (int i = 0; i < 4; i++)
            {
                var left = root.transform.Find("Box/Left_" + i);
                DressPad(left, true);
                var right = root.transform.Find("Box/Right_" + i);
                DressPad(right, false);
            }
        }

        private void DressPad(Transform t, bool left)
        {
            if (t == null)
                return;

            var btn = t.GetComponent<Button>();
            if (btn != null)
                btn.transition = Selectable.Transition.None;

            var texts = t.GetComponentsInChildren<Text>(true);
            for (int n = 0; n < texts.Length; n++)
            {
                texts[n].text = string.Empty;
                texts[n].enabled = false;
            }

            var rt = t as RectTransform;
            if (rt != null)
            {
                if (left)
                    rt.sizeDelta = new Vector2(168f, 22f);
                else
                    rt.sizeDelta = new Vector2(48f, 48f);
            }
        }

        private void RefreshButtons()
        {
            if (root == null)
                return;

            for (int i = 0; i < 4; i++)
            {
                int leftColor = _leftId[i];
                var left = root.transform.Find("Box/Left_" + i);
                if (left != null)
                {
                    var b = left.GetComponent<Button>();
                    if (b != null)
                        b.interactable = !_matchedColor[leftColor];
                    PaintPad(left, true, i, _selectedLeft == i, _matchedColor[leftColor]);
                }

                int rightColor = _rightId[i];
                var right = root.transform.Find("Box/Right_" + i);
                if (right != null)
                {
                    var b = right.GetComponent<Button>();
                    if (b != null)
                        b.interactable = !_matchedColor[rightColor];
                    PaintPad(right, false, i, false, _matchedColor[rightColor]);
                }
            }
        }

        private void PaintPad(Transform t, bool left, int i, bool selected, bool matched)
        {
            if (t == null)
                return;

            Color c;
            if (matched)
                c = NeonWhite;
            else if (left)
                c = selected ? CablePick : Cable;
            else
                c = Terminals[_rightId[i]];

            var imgs = t.GetComponentsInChildren<Image>(true);
            for (int n = 0; n < imgs.Length; n++)
            {
                if (imgs[n] == null)
                    continue;
                imgs[n].color = c;
            }
        }
    }
}
