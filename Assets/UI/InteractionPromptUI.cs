using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text label;
        // Color de la tecla entre corchetes del aviso
        [SerializeField] private Color keyColor = Color.white;

        public void SetKeyColor(Color color)
        {
            keyColor = color;
        }

        public void Show(string text)
        {
            if (root != null)
                root.SetActive(true);
            if (label != null)
                label.text = Format(text);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private string Format(string text)
        {
            if (string.IsNullOrEmpty(text) || text[0] != '[')
                return text;
            int close = text.IndexOf(']');
            if (close < 0)
                return text;
            string hex = ColorUtility.ToHtmlStringRGB(keyColor);
            return "<color=#" + hex + ">" + text.Substring(0, close + 1) + "</color>" + text.Substring(close + 1);
        }
    }
}
