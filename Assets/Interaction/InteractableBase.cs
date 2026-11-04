using Enigma.Audio;
using Enigma.CameraSystem;
using Enigma.Core;
using Enigma.Data;
using Enigma.Environment;
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
        [SerializeField] private GameObject completeFx;
        [SerializeField] private Transform completeFxAnchor;
        [SerializeField] private float completeFxLife = 6f;
        // Efecto cuando la E lo resuelve. Sin anchor sale del piso, bajo el objeto.
        [SerializeField] private Bounds useZone;
        // En mundo. Con tamaño, solo se usa con el jugador adentro (cuarto cerrado).

        private bool _localCompleted;

        public float InteractRange => interactRange;
        public InteractableConfig Config => config;

        public virtual bool RequiresZoom => config != null && config.requiresZoom;
        public virtual bool AllowsInventoryWhileZoom => config != null && config.allowsInventoryWhileZoom;
        public virtual Transform ZoomAnchor => zoomAnchor != null ? zoomAnchor : transform;

        protected void AnchorCompleteFx(Transform anchor)
        {
            completeFxAnchor = anchor;
        }

        public void SetUseZone(Bounds zone)
        {
            useZone = zone;
        }

        public bool HasUseZone => useZone.size.sqrMagnitude > 0.0001f;

        private bool InUseZone(InteractContext context)
        {
            // Sin actor no hay posición que medir: la zona no bloquea
            if (!HasUseZone || context == null || context.Actor == null)
                return true;
            return useZone.Contains(context.Actor.transform.position);
        }

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
            return InUseZone(context);
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
            if (IsCompleted())
                SpawnCompleteFx();
        }

        private void SpawnCompleteFx()
        {
            if (completeFx == null)
                return;
            Vector3 at;
            if (completeFxAnchor != null)
                at = completeFxAnchor.position;
            else
            {
                var b = new Bounds(transform.position, Vector3.zero);
                foreach (var r in GetComponentsInChildren<Renderer>())
                    b.Encapsulate(r.bounds);
                at = new Vector3(b.center.x, b.min.y, b.center.z);
            }
            var fx = Instantiate(completeFx, at, Quaternion.identity);
            if (completeFxLife > 0f)
                Destroy(fx, completeFxLife);
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
                if (context.Inventory == null || !context.Inventory.Has(config.requiredItem))
                    return false;
            }

            return true;
        }

        protected virtual void OnSuccess(InteractContext context)
        {
            StopAttempt();
            if (config != null && !string.IsNullOrEmpty(config.successFlagId))
                GameFlagSystem.Instance?.Set(config.successFlagId, true);

            PlayDialogue(context, config != null ? config.successDialogueId : null);
            HandleSuccessExtra(context);
            if (config != null && config.requiredItem != null)
                context.Inventory?.Remove(config.requiredItem);
            MarkCompleted();
        }

        protected virtual void OnFail(InteractContext context)
        {
            if (config != null && config.failDialogueId == "door_locked")
                SfxPlayer.Instance?.Play("door_locked", transform.position);
            PlayDialogue(context, config != null ? config.failDialogueId : null);
            HandleFailExtra(context);
            PlayAttempt();
        }

        protected void PlayAttempt()
        {
            foreach (var m in GetComponents<AttemptMotion>())
                m.Play();
        }

        protected void StopAttempt()
        {
            foreach (var m in GetComponents<AttemptMotion>())
                m.Stop(true);
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
