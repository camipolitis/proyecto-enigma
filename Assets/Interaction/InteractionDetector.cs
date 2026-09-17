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
    // SphereCast desde la cámara hacia layer Interactable.
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Camera rayCamera;
        [SerializeField] private float range = 8f;
        [SerializeField] private float aimRadius = 0.35f;
        // Grosor del apuntado: un rayo fino no pega notebooks ni candados chicos.
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private MemoryJournal memory;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private DialogueLineSet defaultDialogue;
        [SerializeField] private InteractionPromptUI promptUi;
        [SerializeField] private string interactableLayerName = "Interactable";

        private IInteractable _current;
        private int _layerMask;
        private readonly RaycastHit[] _hits = new RaycastHit[16];

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
                 ModalStack.Instance.Contains(ModalKind.Pause) ||
                 ModalStack.Instance.Contains(ModalKind.CodeEntry)))
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

            var zoom = InteractionZoomController.Instance;
            if (zoom != null && zoom.IsZooming && zoom.ZoomTarget != null)
            {
                if (zoom.ZoomTarget.CanAttemptInteract(BuildContext()))
                    _current = zoom.ZoomTarget;
            }
            else
            {
                Ray ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
                int count = Physics.SphereCastNonAlloc(ray, aimRadius, _hits, range, _layerMask, QueryTriggerInteraction.Collide);
                if (count > 1)
                    System.Array.Sort(_hits, 0, count, HitDistanceComparer.Instance);

                for (int i = 0; i < count; i++)
                {
                    var interactable = _hits[i].collider.GetComponentInParent<IInteractable>();
                    if (interactable == null || !interactable.CanAttemptInteract(BuildContext()))
                        continue;

                    float allowed = range;
                    if (interactable is InteractableBase body && body.InteractRange > 0.01f)
                        allowed = body.InteractRange;

                    if (_hits[i].distance <= allowed)
                    {
                        _current = interactable;
                        break;
                    }
                }
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

        private sealed class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitDistanceComparer Instance = new HitDistanceComparer();

            public int Compare(RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            }
        }
    }
}
