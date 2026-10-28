using Enigma.Core;
using Enigma.Data;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // PC del escritorio: pide VELA y después abre el escritorio.
    public class PcInteractable : InteractableBase
    {
        [SerializeField] private WordEntryUI wordUi;
        [SerializeField] private ComputerDesktopUI desktopUi;
        [SerializeField] private string powerFlagId = "power_restored";
        [SerializeField] private string unlockFlagId = "pc_unlocked";

        protected override bool IsCompleted() => false;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(powerFlagId);
        }

        protected override void OnSuccess(InteractContext context)
        {
            if (GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(unlockFlagId))
            {
                desktopUi?.Open();
                return;
            }

            wordUi?.Open();
        }
    }
}
