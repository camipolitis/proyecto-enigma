using Enigma.Core;
using Enigma.Player;
using UnityEngine;

namespace Enigma.UI
{
    // Base de paneles modales: Push/Pop y cursor. Los campos siguen en cada hijo.
    public abstract class ModalUIBase : MonoBehaviour
    {
        protected abstract ModalKind Kind { get; }
        protected abstract GameObject ModalRoot { get; }
        protected abstract PlayerInputHandler Input { get; }

        protected bool IsOpen { get; private set; }

        protected void OpenModal()
        {
            if (IsOpen)
                return;

            IsOpen = true;
            if (ModalRoot != null)
                ModalRoot.SetActive(true);

            ModalStack.Instance?.Push(Kind);
            ModalStack.Instance?.ApplyCursorForTop();
        }

        public virtual void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            if (ModalRoot != null)
                ModalRoot.SetActive(false);

            ModalStack.Instance?.TryPopSpecific(Kind);
            ModalStack.Instance?.ApplyCursorForTop();
        }

        // Q solo si este panel es el tope y nadie más consumió el Back.
        protected bool TryCloseWithBack()
        {
            if (!IsOpen || Input == null)
                return false;

            if (ModalStack.Instance != null && !ModalStack.Instance.IsTop(Kind))
                return false;

            if (!Input.TryConsumeBack())
                return false;

            Close();
            return true;
        }
    }
}
