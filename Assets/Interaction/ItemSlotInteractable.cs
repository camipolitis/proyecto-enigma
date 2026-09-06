using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Receptáculo que consume el ítem seleccionado (caja eléctrica, etc.)
    public class ItemSlotInteractable : InteractableBase
    {
        [SerializeField] private GameObject insertedVisual;
        [SerializeField] private bool consumeItem = true;

        public override bool AllowsInventoryWhileZoom =>
            config != null ? config.allowsInventoryWhileZoom : true;
        // Default true: en zoom de receptáculo sí se puede abrir el inventario.

        protected override void HandleSuccessExtra(InteractContext context)
        {
            if (consumeItem && config != null && config.requiredItem != null)
                context.Inventory?.Remove(config.requiredItem);

            if (insertedVisual != null)
                insertedVisual.SetActive(true);
        }
    }
}
