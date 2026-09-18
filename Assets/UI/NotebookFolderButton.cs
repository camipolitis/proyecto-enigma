using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Botón de carpeta de la notebook.
    public class NotebookFolderButton : MonoBehaviour
    {
        [SerializeField] private NotebookFolderUI target;
        [SerializeField] private bool registro;

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
            if (registro)
                target.OnFolderRegistro();
            else
                target.OnFolderCorrupt();
        }
    }
}
