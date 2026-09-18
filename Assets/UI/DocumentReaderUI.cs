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
    public class DocumentReaderUI : ModalUIBase
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

        protected override ModalKind Kind => ModalKind.Document;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private MemoryJournal LiveJournal =>
            MemoryJournal.Instance != null ? MemoryJournal.Instance : journal;

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Update()
        {
            TryCloseWithBack();
        }

        public void Open(DocumentData document, InteractContext context)
        {
            _current = document;
            _context = context;

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

            OpenModal();
        }

        public void OpenFromJournal(DocumentData document)
        {
            Open(document, null);
        }

        public override void Close()
        {
            if (!IsOpen)
                return;

            var closing = _current;
            var ctx = _context;
            base.Close();

            if (closing != null)
            {
                bool firstTime = LiveJournal != null && LiveJournal.TryAdd(closing);
                if (firstTime)
                {
                    GameFlagSystem.Instance?.Set("note_read", true);
                    journalUi?.ShowToast(closing.memoryCollectedMessage);
                }

                PlayReleaseLine(ctx, closing);
            }

            _current = null;
            _context = null;
        }

        private void PlayReleaseLine(InteractContext ctx, DocumentData doc)
        {
            if (ctx?.Subtitles == null || doc == null)
                return;

            var set = ctx.Dialogue;
            if (set != null && !string.IsNullOrEmpty(doc.releaseDialogueId) &&
                set.TryGet(doc.releaseDialogueId, out var spoken))
            {
                ctx.Subtitles.Show(spoken.text, spoken.duration, spoken.voice);
                return;
            }

            string line = string.IsNullOrEmpty(doc.releaseSubtitle) ? "M..." : doc.releaseSubtitle;
            ctx.Subtitles.Show(line, 1.5f);
        }
    }
}
