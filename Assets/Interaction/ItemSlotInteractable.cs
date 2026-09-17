using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Receptáculo que consume el ítem seleccionado (caja eléctrica, etc.)
    public class ItemSlotInteractable : InteractableBase
    {
        [SerializeField] private GameObject insertedVisual;
        [SerializeField] private bool consumeItem = true;
        [SerializeField] private string placePrompt = "Colocar fusible";
        // Prompt cuando el ítem requerido está seleccionado.

        public override bool AllowsInventoryWhileZoom =>
            config != null ? config.allowsInventoryWhileZoom : true;
        // Default true: en zoom de receptáculo sí se puede abrir el inventario.

        public override string GetPrompt(InteractContext context)
        {
            if (config != null && config.requiredItem != null &&
                context != null && context.SelectedItem != null &&
                context.SelectedItem.id == config.requiredItem.id)
            {
                string p = string.IsNullOrEmpty(placePrompt) ? "Colocar ítem" : placePrompt;
                return $"[E] {p}";
            }

            return base.GetPrompt(context);
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            if (consumeItem && config != null && config.requiredItem != null)
                context.Inventory?.Remove(config.requiredItem);

            if (insertedVisual != null)
                insertedVisual.SetActive(true);
        }
    }
}
