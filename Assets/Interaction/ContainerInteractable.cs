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
        [SerializeField] private Vector3 openLocalPosition;
        // Desplazamiento local al abrir (ej. cajón en X).

        [SerializeField] private bool opened;
        [SerializeField] private GameObject[] revealOnOpen;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return !opened;
            // Una sola apertura; después el fusible se recoge aparte si quedó en escena.
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            opened = true;

            if (animatedPart != null)
            {
                animatedPart.localRotation = Quaternion.Euler(openLocalEuler);
                animatedPart.localPosition = openLocalPosition;
            }

            if (contentObject != null)
                contentObject.SetActive(true);

            if (revealOnOpen != null)
            {
                foreach (var go in revealOnOpen)
                {
                    if (go != null)
                        go.SetActive(true);
                }
            }

            var box = GetComponent<Collider>();
            if (box != null)
                box.enabled = false;

            if (autoAddItem != null && context.Inventory != null && context.Inventory.CanAdd(autoAddItem))
            {
                context.Inventory.Add(autoAddItem);
                if (contentObject != null)
                    contentObject.SetActive(false);
                // El fusible del cajón se recoge aparte.
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
