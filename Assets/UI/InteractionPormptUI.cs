using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text label;

        public void Show(string text)
        {
            if (root != null)
                root.SetActive(true);
            if (label != null)
                label.text = text;
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
