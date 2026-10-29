using Enigma.Core;
using UnityEngine;

namespace Enigma.Interaction
{
    // Carga otra escena con E (escotilla de bajada).
    public class LoadSceneInteractable : InteractableBase
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string carryFlagId = "hatch_unlocked";

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return !string.IsNullOrEmpty(sceneName);
        }

        protected override void OnSuccess(InteractContext context)
        {
            PlayDialogue(context, config != null ? config.successDialogueId : null);

            if (!string.IsNullOrEmpty(carryFlagId) &&
                GameFlagSystem.Instance != null &&
                GameFlagSystem.Instance.Get(carryFlagId))
                GameFlagSystem.Carry(carryFlagId);

            var loader = FindFirstObjectByType<SceneLoader>();
            if (loader == null)
                return;

            loader.LoadSceneByName(sceneName);
        }
    }
}
