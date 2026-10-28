using Enigma.CameraSystem;
using Enigma.Core;
using Enigma.Player;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Inventory
{
    // Hotbar de 8 slots.
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private InventorySlotView[] slotViews = new InventorySlotView[InventorySystem.SlotCount];
        // 8 vistas hijas; se pueden crear en el prefab UI

        [SerializeField] private bool hotbarAlwaysVisible = true;
        // Hotbar visible en exploración

        [SerializeField] private DocumentReaderUI readerUi;

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
            if (readerUi == null)
                readerUi = FindFirstObjectByType<DocumentReaderUI>();

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
            if (input.HotbarSlotPressedThisFrame >= 0)
                inventory.SelectIndex(input.HotbarSlotPressedThisFrame);

            if (input.InventoryPressedThisFrame)
                TryInspectSelected();
        }

        private void TryInspectSelected()
        {
            if (ModalStack.Instance != null && !ModalStack.Instance.AllowsInventoryOpen())
                return;
            // Documento, pad, PC y pause se quedan con el teclado.

            if (InteractionZoomController.Instance != null &&
                InteractionZoomController.Instance.IsZooming &&
                !InteractionZoomController.Instance.AllowsInventoryWhileZoom)
                return;
            // Zoom de nota no permite inventario.

            InspectSelected();
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            enabled = false;
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
            if (inventory == null)
                return;

            inventory.SelectIndex(index);
        }

        private void InspectSelected()
        {
            if (readerUi == null)
                readerUi = FindFirstObjectByType<DocumentReaderUI>();
            if (readerUi == null || inventory == null)
                return;
            readerUi.Inspect(inventory.SelectedItem);
        }
    }
}
