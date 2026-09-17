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
        [SerializeField] private float interactRange;
        // 0 = usa el range del detector.

        private bool _localCompleted;

        public float InteractRange => interactRange;

        public virtual bool RequiresZoom => config != null && config.requiresZoom;
        public virtual bool AllowsInventoryWhileZoom => config != null && config.allowsInventoryWhileZoom;
        public virtual Transform ZoomAnchor => zoomAnchor != null ? zoomAnchor : transform;

        public virtual string GetPrompt(InteractContext context)
        {
            if (IsCompleted())
                return string.Empty;

            string prompt = config != null ? config.prompt : "Interactuar";
            return $"[E] {prompt}";
        }

        public virtual bool CanAttemptInteract(InteractContext context)
        {
            if (!enabled || !gameObject.activeInHierarchy)
                return false;
            if (IsCompleted())
                return false;
            return true;
        }

        public void Interact(InteractContext context)
        {
            if (IsCompleted())
                return;

            if (RequiresZoom && InteractionZoomController.Instance != null &&
                !InteractionZoomController.Instance.IsZooming)
            {
                InteractionZoomController.Instance.EnterZoom(ZoomAnchor, AllowsInventoryWhileZoom, this);
                // Primer E solo acerca; el siguiente resuelve Success/Fail.
                return;
            }

            if (MeetsSuccessConditions(context))
                OnSuccess(context);
            else
                OnFail(context);
        }

        protected virtual bool IsCompleted()
        {
            if (_localCompleted)
                return true;

            if (config != null && !string.IsNullOrEmpty(config.successFlagId) &&
                GameFlagSystem.Instance != null &&
                GameFlagSystem.Instance.Get(config.successFlagId))
                return true;

            return false;
        }

        protected void MarkCompleted()
        {
            _localCompleted = true;
        }

        protected virtual bool MeetsSuccessConditions(InteractContext context)
        {
            if (config == null)
                return false;
            // Sin config no hay Success: la puerta no se abre sola.

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
            MarkCompleted();
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
                context.Subtitles.Show(line.text, line.duration, line.voice);
            else
                context.Subtitles.Show(dialogueId, 2.5f);
        }

        protected virtual void HandleSuccessExtra(InteractContext context) { }
        protected virtual void HandleFailExtra(InteractContext context) { }
    }
}
