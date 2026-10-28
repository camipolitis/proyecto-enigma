using Enigma.Core;
using UnityEngine;

namespace Enigma.Interaction
{
    // Panel de la fosa: avisa al controlador su índice.
    public class SequencePanelInteractable : InteractableBase
    {
        [SerializeField] private SequencePuzzleController controller;
        [SerializeField] private int panelIndex;
        [SerializeField] private string completedFlagId = "sequence_ok";

        protected override bool IsCompleted()
        {
            if (string.IsNullOrEmpty(completedFlagId) || GameFlagSystem.Instance == null)
                return false;
            return GameFlagSystem.Instance.Get(completedFlagId);
        }

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (controller == null)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        protected override void OnSuccess(InteractContext context)
        {
            controller.RegisterPress(panelIndex, context);
        }
    }
}
