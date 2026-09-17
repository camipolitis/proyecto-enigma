using Enigma.Core;
using Enigma.Data;
using Enigma.Interaction;
using Enigma.Memory;
using Enigma.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Lector de documentos. Q suelta la nota, guarda memoria la 1ra vez y dice "M..."
    public class DocumentReaderUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private RawImage viewImage;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private MemoryJournal journal;
        [SerializeField] private MemoryJournalUI journalUi;

        private DocumentData _current;
        private InteractContext _context;
        private bool _open;

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Update()
        {
            if (!_open || input == null)
                return;

            if (input.BackPressedThisFrame &&
                (ModalStack.Instance == null || ModalStack.Instance.IsTop(ModalKind.Document)))
                Close();
        }

        public void Open(DocumentData document, InteractContext context)
        {
            _current = document;
            _context = context;
            _open = true;

            if (root != null)
                root.SetActive(true);

            bool hasImage = document != null && document.viewImage != null;
            if (viewImage != null)
            {
                viewImage.gameObject.SetActive(hasImage);
                if (hasImage)
                    viewImage.texture = document.viewImage;
            }

            if (titleLabel != null)
            {
                titleLabel.gameObject.SetActive(!hasImage);
                titleLabel.text = document != null ? document.title : string.Empty;
            }

            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(!hasImage);
                bodyLabel.text = document != null ? document.body : string.Empty;
            }

            ModalStack.Instance?.Push(ModalKind.Document);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (!_open)
                return;

            _open = false;
            if (root != null)
                root.SetActive(false);

            ModalStack.Instance?.TryPopSpecific(ModalKind.Document);

            if (_current != null)
            {
                bool firstTime = journal != null && journal.TryAdd(_current);
                if (firstTime)
                {
                    GameFlagSystem.Instance?.Set("note_read", true);
                    journalUi?.ShowToast(_current.memoryCollectedMessage);
                }

                PlayReleaseLine();
            }

            _current = null;
            _context = null;
            ModalStack.Instance?.ApplyCursorForTop();
        }

        private void PlayReleaseLine()
        {
            if (_context?.Subtitles == null)
                return;

            var set = _context.Dialogue;
            if (set != null && !string.IsNullOrEmpty(_current.releaseDialogueId) &&
                set.TryGet(_current.releaseDialogueId, out var spoken))
            {
                _context.Subtitles.Show(spoken.text, spoken.duration, spoken.voice);
                return;
            }

            string line = string.IsNullOrEmpty(_current.releaseSubtitle) ? "M..." : _current.releaseSubtitle;
            _context.Subtitles.Show(line, 1.5f);
        }
    }
}
