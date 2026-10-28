using Enigma.Core;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Caja de cables: abre el panel de emparejar colores.
    public class WirePuzzleInteractable : InteractableBase
    {
        [SerializeField] private WirePuzzleUI wireUi;

        protected override bool IsCompleted()
        {
            if (config == null || string.IsNullOrEmpty(config.successFlagId) || GameFlagSystem.Instance == null)
                return false;
            return GameFlagSystem.Instance.Get(config.successFlagId);
        }

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return wireUi != null;
        }

        protected override void OnSuccess(InteractContext context)
        {
            PlayDialogue(context, config != null ? config.failDialogueId : null);
            wireUi.Open();
        }
    }
}
