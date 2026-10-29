using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Tecla de letra del panel de palabra (A-Z, borrar, enter).
    public class WordPadKey : MonoBehaviour
    {
        [SerializeField] private WordEntryUI target;
        [SerializeField] private char letter = 'A';
        [SerializeField] private bool backspace;
        [SerializeField] private bool submit;

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
            if (backspace)
                target.PressBackspace();
            else if (submit)
                target.PressSubmit();
            else
                target.PressLetter(letter);
        }
    }
}
