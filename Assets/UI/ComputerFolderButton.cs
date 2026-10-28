using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Botón del escritorio: carpeta corrupta, restauración o documento.
    public class ComputerFolderButton : MonoBehaviour
    {
        public enum ActionKind
        {
            Corrupt,
            Restore,
            RestoreYes,
            RestoreNo,
            DocOrigen,
            DocSubjects,
            DocStaff,
            DocParaM
        }

        [SerializeField] private ComputerDesktopUI target;
        [SerializeField] private ActionKind action;

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(Press);
        }

        public void Press()
        {
            if (target == null)
                return;

            switch (action)
            {
                case ActionKind.Corrupt:
                    target.OnFolderCorrupt();
                    break;
                case ActionKind.Restore:
                    target.OnFolderRestore();
                    break;
                case ActionKind.RestoreYes:
                    target.OnRestoreYes();
                    break;
                case ActionKind.RestoreNo:
                    target.OnRestoreNo();
                    break;
                case ActionKind.DocOrigen:
                    target.OpenOrigenDoc();
                    break;
                case ActionKind.DocSubjects:
                    target.OpenSubjectsDoc();
                    break;
                case ActionKind.DocStaff:
                    target.OpenStaffDoc();
                    break;
                case ActionKind.DocParaM:
                    target.OpenParaMDoc();
                    break;
            }
        }
    }
}
