using Enigma.CameraSystem;
using Enigma.Data;
using Enigma.UI;
using UnityEngine;
using static UnityEngine.Rendering.STP;

namespace Enigma.Interaction
{
    // Nota/documento: se lee con E, se suelta con Q, queda en el mundo.
    public class DocumentInteractable : InteractableBase
    {
        [SerializeField] private DocumentData document;
        [SerializeField] private DocumentReaderUI readerUi;

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
            {
                Debug.LogWarning("DocumentInteractable sin DocumentReaderUI.");
                return;
            }

            if (RequiresZoom && InteractionZoomController.Instance != null &&
                !InteractionZoomController.Instance.IsZooming)
            {
                InteractionZoomController.Instance.EnterZoom(ZoomAnchor, false);
            }

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
