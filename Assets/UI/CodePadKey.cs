using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Tecla del pad numérico (0-9, borrar, enter).
    public class CodePadKey : MonoBehaviour
    {
        [SerializeField] private CodeEntryUI target;
        [SerializeField] private int digit = -1;
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
                target.PressDigit(digit);
        }
    }
}
