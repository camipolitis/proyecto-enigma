using Enigma.CameraSystem;
using Enigma.Core;
using Enigma.Data;
using Enigma.Inventory;
using Enigma.Memory;
using Enigma.Player;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Raycast desde la cámara hacia layer Interactable.
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Camera rayCamera;
        [SerializeField] private float range = 3f;
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private MemoryJournal memory;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private DialogueLineSet defaultDialogue;
        [SerializeField] private InteractionPromptUI promptUi;
        [SerializeField] private string interactableLayerName = "Interactable";

        private IInteractable _current;
        private int _layerMask;

        private void Awake()
        {
            int layer = LayerMask.NameToLayer(interactableLayerName);
            _layerMask = layer >= 0 ? (1 << layer) : ~0;
        }

        private void Update()
        {
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused)
            {
                ClearPrompt();
                return;
            }

            if (ModalStack.Instance != null &&
                (ModalStack.Instance.Contains(ModalKind.Document) ||
                 ModalStack.Instance.Contains(ModalKind.Pause)))
            {
                ClearPrompt();
                HandleBackOnly();
                return;
            }

            UpdateFocus();
            HandleInteract();
            HandleBackOnly();
        }

        private void UpdateFocus()
        {
            _current = null;
            if (rayCamera == null)
            {
                ClearPrompt();
                return;
            }

            Ray ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, _layerMask, QueryTriggerInteraction.Collide))
            {
                // QueryTriggerInteraction.Collide: los triggers sí reciben el ray.
                _current = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (_current != null && _current.CanAttemptInteract(BuildContext()))
                promptUi?.Show(_current.GetPrompt(BuildContext()));
            else
                ClearPrompt();
        }

        private void HandleInteract()
        {
            if (input == null || !input.InteractPressedThisFrame || _current == null)
                return;

            if (ModalStack.Instance != null && ModalStack.Instance.Contains(ModalKind.Inventory))
                return;
            // Con inventario abierto, E no interactúa con el mundo, evita misclickear

            var ctx = BuildContext();
            if (_current.CanAttemptInteract(ctx))
                _current.Interact(ctx);
        }

        private void HandleBackOnly()
        {
            if (input == null || !input.BackPressedThisFrame)
                return;

            if (ModalStack.Instance == null || ModalStack.Instance.IsEmpty)
                return;

            var top = ModalStack.Instance.Top;
            if (top == ModalKind.Zoom)
                InteractionZoomController.Instance?.ExitZoom();
            // Document/Inventory se cierran en sus propios scripts al escuchar Back.
        }

        private InteractContext BuildContext()
        {
            return new InteractContext
            {
                Actor = gameObject,
                Inventory = inventory,
                Memory = memory,
                Subtitles = subtitles,
                Dialogue = defaultDialogue,
                SelectedItem = inventory != null ? inventory.SelectedItem : null
            };
        }

        private void ClearPrompt()
        {
            promptUi?.Hide();
        }
    }
}
