using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Contenedor: al éxito revela/entrega un ítem.
    public class ContainerInteractable : InteractableBase
    {
        [SerializeField] private GameObject contentObject;
        // Fusible hijo desactivado hasta abrir.

        [SerializeField] private InventoryItem autoAddItem;
        [SerializeField] private Transform animatedPart;
        [SerializeField] private Vector3 openLocalEuler = new Vector3(25f, 0f, 0f);
        [SerializeField] private bool opened;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return !opened;
            // Una sola apertura; después el fusible se recoge aparte si quedó en escena.
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            opened = true;

            if (animatedPart != null)
                animatedPart.localRotation = Quaternion.Euler(openLocalEuler);

            if (contentObject != null)
                contentObject.SetActive(true);

            if (autoAddItem != null && context.Inventory != null && context.Inventory.CanAdd(autoAddItem))
            {
                context.Inventory.Add(autoAddItem);
                if (contentObject != null)
                    contentObject.SetActive(false);
                // Acá este AutoAddItem lo dejamos vacío (Inspector), pregunten.
            }
        }

        protected override void OnFail(InteractContext context)
        {
            if (opened)
                context.Subtitles?.Show("Ya está abierto.", 1.5f);
            else
                base.OnFail(context);
        }
    }
}
