using Enigma.CameraSystem;
using Enigma.Data;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Nota/documento: se lee con E, se suelta con Q, queda en el mundo.
    public class DocumentInteractable : InteractableBase
    {
        [SerializeField] private DocumentData document;
        [SerializeField] private DocumentReaderUI readerUi;

        public DocumentData Document => document;

        // Lo usa el vestido al grabar los diarios del mundo
        public void Configure(DocumentData doc, DocumentReaderUI reader, InteractableConfig cfg)
        {
            document = doc;
            readerUi = reader;
            config = cfg;
        }

        public override bool AllowsInventoryWhileZoom => false;
        // Leer nota nunca permite inventario.

        protected override bool MeetsSuccessConditions(InteractContext context) => document != null;

        protected override void OnSuccess(InteractContext context)
        {
            HandleSuccessExtra(context);
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            if (readerUi == null)
                return;

            if (document.audioOnly)
            {
                if (config != null && !string.IsNullOrEmpty(config.successFlagId))
                    Enigma.Core.GameFlagSystem.Instance?.Set(config.successFlagId, true);
                readerUi.Listen(document);
                return;
            }

            if (RequiresZoom && InteractionZoomController.Instance != null &&
                !InteractionZoomController.Instance.IsZooming)
            {
                InteractionZoomController.Instance.EnterZoom(ZoomAnchor, false, this);
            }

            if (config != null && !string.IsNullOrEmpty(config.successFlagId))
                Enigma.Core.GameFlagSystem.Instance?.Set(config.successFlagId, true);

            Enigma.Audio.SfxPlayer.Instance?.Play("note_pickup");
            readerUi.Open(document, context);
            // Open registra memoria al cerrar con Q la primera vez.
        }

        public override string GetPrompt(InteractContext context)
        {
            string p = config != null ? config.prompt : "Leer nota";
            return $"[E] {p}";
        }
    }
}
