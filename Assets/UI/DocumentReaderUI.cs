using Enigma.Core;
using Enigma.Data;
using Enigma.Interaction;
using Enigma.Memory;
using Enigma.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Lector de documentos. Q suelta la nota y guarda memoria la 1ra vez.
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
        private bool _journalOnClose = true;

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
            _journalOnClose = true;

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

            if (document != null && document.openVoice != null && SubtitleSystem.Instance != null)
                SubtitleSystem.Instance.PlayClip(document.openVoice);
        }

        public void OpenFromJournal(DocumentData document)
        {
            Open(document, null);
        }

        // Ver ítem del inventario: Q cierra, no gasta el objeto ni suma memoria.
        public void Inspect(InventoryItem item)
        {
            if (item == null)
                return;

            if (item.inspectDoc != null)
            {
                _journalOnClose = false;
                Open(item.inspectDoc, null);
                _journalOnClose = false;
                return;
            }

            OpenInspect(item.displayName, item.description);
        }

        public void OpenInspect(string title, string body)
        {
            _current = null;
            _context = null;
            _journalOnClose = false;

            if (viewImage != null)
                viewImage.gameObject.SetActive(false);

            if (titleLabel != null)
            {
                titleLabel.gameObject.SetActive(true);
                titleLabel.text = title ?? string.Empty;
            }

            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(true);
                bodyLabel.text = body ?? string.Empty;
            }

            OpenModal();
        }

        public override void Close()
        {
            if (!IsOpen)
                return;

            var closing = _current;
            var ctx = _context;
            base.Close();

            if (SubtitleSystem.Instance != null)
                SubtitleSystem.Instance.StopVoice();

            if (_journalOnClose && closing != null)
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
            _journalOnClose = true;
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

            if (string.IsNullOrEmpty(doc.releaseSubtitle))
                return;

            ctx.Subtitles.Show(doc.releaseSubtitle, 1.5f);
        }
    }
}
