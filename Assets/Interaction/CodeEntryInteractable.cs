using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Abre un panel de código (notebook o candado).
    public class CodeEntryInteractable : InteractableBase
    {
        [SerializeField] private CodeEntryUI codeUi;
        [SerializeField] private string unlockFlagId;
        [SerializeField] private GameObject[] enableOnUnlock;
        [SerializeField] private NotebookFolderUI notebookFolders;

        private bool _attemptQueued;

        public string UnlockFlagId => unlockFlagId;

        protected override bool IsCompleted()
        {
            if (notebookFolders != null)
                return false;
            if (string.IsNullOrEmpty(unlockFlagId) || Enigma.Core.GameFlagSystem.Instance == null)
                return false;
            return Enigma.Core.GameFlagSystem.Instance.Get(unlockFlagId);
        }

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            return codeUi != null;
        }

        protected override void OnSuccess(InteractContext context)
        {
            bool alreadyOpen = !string.IsNullOrEmpty(unlockFlagId) &&
                               Enigma.Core.GameFlagSystem.Instance != null &&
                               Enigma.Core.GameFlagSystem.Instance.Get(unlockFlagId);
            if (notebookFolders != null && !alreadyOpen)
                PlayDialogue(context, config != null ? config.successDialogueId : null);
            HandleSuccessExtra(context);
        }

        protected override void HandleSuccessExtra(InteractContext context)
        {
            if (!string.IsNullOrEmpty(unlockFlagId) &&
                Enigma.Core.GameFlagSystem.Instance != null &&
                Enigma.Core.GameFlagSystem.Instance.Get(unlockFlagId))
            {
                if (notebookFolders != null)
                    notebookFolders.Open();
                return;
            }

            codeUi.Open(QueueAttempt);
        }

        // El panel tapa el candado: se sacude al cerrarlo, si no se acertó antes
        private void QueueAttempt()
        {
            _attemptQueued = true;
        }

        private void Update()
        {
            if (!_attemptQueued || codeUi == null || codeUi.Showing)
                return;
            _attemptQueued = false;
            PlayAttempt();
        }

        private void OnEnable()
        {
            if (Enigma.Core.GameFlagSystem.Instance != null)
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void Start()
        {
            if (Enigma.Core.GameFlagSystem.Instance != null)
            {
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged += OnFlag;
            }
        }

        private void OnDisable()
        {
            if (Enigma.Core.GameFlagSystem.Instance != null)
                Enigma.Core.GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void OnFlag(string id, bool value)
        {
            if (!value || string.IsNullOrEmpty(unlockFlagId) || id != unlockFlagId)
                return;
            _attemptQueued = false;

            if (enableOnUnlock != null)
            {
                foreach (var go in enableOnUnlock)
                {
                    if (go != null)
                        go.SetActive(true);
                }
            }

            if (notebookFolders != null)
                notebookFolders.Open();
        }
    }
}
