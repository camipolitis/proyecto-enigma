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

            if (input.BackPressedThisFrame)
                Close();
        }

        public void Open(DocumentData document, InteractContext context)
        {
            _current = document;
            _context = context;
            _open = true;

            if (root != null)
                root.SetActive(true);
            if (titleLabel != null)
                titleLabel.text = document != null ? document.title : string.Empty;
            if (bodyLabel != null)
                bodyLabel.text = document != null ? document.body : string.Empty;

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

                string line = string.IsNullOrEmpty(_current.releaseSubtitle) ? "M..." : _current.releaseSubtitle;
                _context?.Subtitles?.Show(line, 1.5f);
                // Voz al soltar con Q.
            }

            _current = null;
            _context = null;

            if (ModalStack.Instance == null || ModalStack.Instance.IsEmpty)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
