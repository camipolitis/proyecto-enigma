using Enigma.CameraSystem;
using Enigma.Core;
using Enigma.Player;
using UnityEngine;

namespace Enigma.Inventory
{
    // UI del hotbar de 8 slots + apertura con Tab 
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private InventorySlotView[] slotViews = new InventorySlotView[InventorySystem.SlotCount];
        // 8 vistas hijas; se pueden crear en el prefab UI

        [SerializeField] private bool hotbarAlwaysVisible = true;
        // Hotbar visible en exploración

        private bool _panelOpen;

        private void OnEnable()
        {
            if (inventory != null)
                inventory.OnChanged += Refresh;
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.OnChanged -= Refresh;
        }

        private void Start()
        {
            if (panelRoot != null && !hotbarAlwaysVisible)
                panelRoot.SetActive(false);
            Refresh();
        }

        private void Update()
        {
            if (input == null || inventory == null)
                return;

            if (input.PreviousPressedThisFrame)
                inventory.SelectPrevious();
            if (input.NextPressedThisFrame)
                inventory.SelectNext();

            if (input.InventoryPressedThisFrame)
                TogglePanel();

            if (input.BackPressedThisFrame && _panelOpen)
                ClosePanel();
        }

        private void TogglePanel()
        {
            if (_panelOpen)
            {
                ClosePanel();
                return;
            }

            if (ModalStack.Instance != null && !ModalStack.Instance.AllowsInventoryOpen())
                return;
            // Documento/pause bloquean.

            if (InteractionZoomController.Instance != null &&
                InteractionZoomController.Instance.IsZooming &&
                !InteractionZoomController.Instance.AllowsInventoryWhileZoom)
                return;
            // Zoom de nota no permite inventario.

            OpenPanel();
        }

        private void OpenPanel()
        {
            _panelOpen = true;
            if (panelRoot != null)
                panelRoot.SetActive(true);
            ModalStack.Instance?.Push(ModalKind.Inventory);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ClosePanel()
        {
            if (!_panelOpen)
                return;

            _panelOpen = false;
            if (panelRoot != null && !hotbarAlwaysVisible)
                panelRoot.SetActive(false);
            ModalStack.Instance?.TryPopSpecific(ModalKind.Inventory);

            if (ModalStack.Instance == null || ModalStack.Instance.IsEmpty)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Refresh()
        {
            if (inventory == null || slotViews == null)
                return;

            for (int i = 0; i < slotViews.Length && i < InventorySystem.SlotCount; i++)
            {
                if (slotViews[i] == null)
                    continue;
                slotViews[i].Bind(inventory.GetSlot(i), i == inventory.SelectedIndex, i);
            }
        }

        public void OnSlotClicked(int index)
        {
            inventory?.SelectIndex(index);
        }
    }
}
