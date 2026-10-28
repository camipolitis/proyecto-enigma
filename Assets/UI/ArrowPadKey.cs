using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Botón izquierda o derecha del candado de flechas.
    public class ArrowPadKey : MonoBehaviour
    {
        [SerializeField] private ArrowLockUI target;
        [SerializeField] private bool isLeft = true;

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
                target.PressLeft();
            else
                target.PressRight();
        }
    }
}
