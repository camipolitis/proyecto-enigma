using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Recoge un ítem al inventario (fusible, batería, credencial...).
    public class PickupInteractable : InteractableBase
    {
        [SerializeField] private InventoryItem item;
        [SerializeField] private bool disableOnPickup = true;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return item != null && context.Inventory != null && context.Inventory.CanAdd(item);
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            context.Inventory.Add(item);
            if (disableOnPickup)
                gameObject.SetActive(false);
        }

        protected override void OnFail(InteractContext context)
        {
            context.Subtitles?.Show("No puedo llevar más objetos...", 2f);
        }
    }
}
