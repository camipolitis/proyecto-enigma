using UnityEngine;

namespace Enigma.Core
{
    /// Pause con Escape / Start. No cierra zoom ni documentos (eso es Q).
   
    public class PauseSystem : MonoBehaviour
    {
        public static PauseSystem Instance { get; private set; }

        [SerializeField] private GameObject pausePanel;
        // Panel UI opcional; puede quedar vacío hasta que armemos el prefab.

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Toggle()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
            Time.timeScale = 0f;
            // Congela animaciones y física del nivel.

            ModalStack.Instance?.Push(ModalKind.Pause);

            if (pausePanel != null)
                pausePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            Time.timeScale = 1f;

            ModalStack.Instance?.TryPopSpecific(ModalKind.Pause);

            if (pausePanel != null)
                pausePanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Vuelve al control de cámara en tercera persona
        }
    }
}
