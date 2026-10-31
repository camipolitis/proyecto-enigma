using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Extremo: izquierda cable gris, derecha terminal de color.
    public class WirePadKey : MonoBehaviour
    {
        [SerializeField] private WirePuzzleUI target;
        [SerializeField] private bool isLeft = true;
        [SerializeField] private int index;

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
            if (isLeft)
                target.SelectLeft(index);
            else
                target.SelectRight(index);
        }
    }
}
