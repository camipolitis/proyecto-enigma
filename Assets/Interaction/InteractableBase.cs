using Enigma.CameraSystem;
using Enigma.Core;
using Enigma.Data;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Base configurable: Attempt siempre posible, Success/Fail según ciertas condiciones.
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected InteractableConfig config;
        [SerializeField] protected DialogueLineSet dialogue;
        [SerializeField] protected Transform zoomAnchor;

        public virtual bool RequiresZoom => config != null && config.requiresZoom;
        public virtual bool AllowsInventoryWhileZoom => config != null && config.allowsInventoryWhileZoom;
        public virtual Transform ZoomAnchor => zoomAnchor != null ? zoomAnchor : transform;

        public virtual string GetPrompt(InteractContext context)
        {
            string prompt = config != null ? config.prompt : "Interactuar";
            return $"[E] {prompt}";
        }

        public virtual bool CanAttemptInteract(InteractContext context)
        {
            return enabled && gameObject.activeInHierarchy;
            // Siempre intentable si está activo: el Fail da el diálogo.
        }

        public void Interact(InteractContext context)
        {
            if (RequiresZoom && InteractionZoomController.Instance != null &&
                !InteractionZoomController.Instance.IsZooming)
            {
                InteractionZoomController.Instance.EnterZoom(ZoomAnchor, AllowsInventoryWhileZoom);
                // Primer E acerca la cámara; el segundo resuelve.
            }

            if (MeetsSuccessConditions(context))
                OnSuccess(context);
            else
                OnFail(context);
        }

        protected virtual bool MeetsSuccessConditions(InteractContext context)
        {
            if (config == null)
                return true;

            if (!string.IsNullOrEmpty(config.requiredFlagId))
            {
                if (GameFlagSystem.Instance == null || !GameFlagSystem.Instance.Get(config.requiredFlagId))
                    return false;
            }

            if (config.requiredItem != null)
            {
                if (context.SelectedItem == null || context.SelectedItem.id != config.requiredItem.id)
                    return false;
                // Hay que tener el ítem seleccionado en el hotbar.
            }

            return true;
        }

        protected virtual void OnSuccess(InteractContext context)
        {
            if (config != null && !string.IsNullOrEmpty(config.successFlagId))
                GameFlagSystem.Instance?.Set(config.successFlagId, true);

            PlayDialogue(context, config != null ? config.successDialogueId : null);
            HandleSuccessExtra(context);
        }

        protected virtual void OnFail(InteractContext context)
        {
            PlayDialogue(context, config != null ? config.failDialogueId : null);
            HandleFailExtra(context);
        }

        protected void PlayDialogue(InteractContext context, string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId) || context?.Subtitles == null)
                return;

            DialogueLineSet set = dialogue != null ? dialogue : context.Dialogue;
            if (set != null && set.TryGet(dialogueId, out var line))
                context.Subtitles.Show(line.text, line.duration);
            else
                context.Subtitles.Show(dialogueId, 2.5f);
        }

        protected virtual void HandleSuccessExtra(InteractContext context) { }
        protected virtual void HandleFailExtra(InteractContext context) { }
    }
}
