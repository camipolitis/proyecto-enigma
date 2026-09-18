using Enigma.Audio;
using Enigma.CameraSystem;
using Enigma.Memory;
using UnityEngine;

namespace Enigma.Core
{
    // Pause con Escape / Start. Q no cierra la pausa (eso es Continuar).
    public class PauseSystem : MonoBehaviour
    {
        public static PauseSystem Instance { get; private set; }

        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private MemoryJournalUI journalUi;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string menuSceneName = "MainMenu";

        public bool IsPaused { get; private set; }
        public bool IsDefeated { get; private set; }

        private bool BlocksPause =>
            IsDefeated || (GameManager.Instance != null && GameManager.Instance.IsLevelCompleted);

        private void Awake()
        {
            if (Instance != null && Instance != this &&
                Instance.gameObject.scene == gameObject.scene)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (defeatPanel != null)
                defeatPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Toggle()
        {
            if (BlocksPause)
                return;

            // Si hay una ventana arriba de la pausa, primero se cierra con Q.
            if (IsPaused && ModalStack.Instance != null && !ModalStack.Instance.IsTop(ModalKind.Pause))
                return;

            if (IsPaused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            if (IsPaused || BlocksPause)
                return;

            InteractionZoomController.Instance?.ExitZoom();

            IsPaused = true;
            Time.timeScale = 0f;
            ModalStack.Instance?.Push(ModalKind.Pause);
            MusicPlayer.Instance?.PlayPauseMusic();

            if (pausePanel != null)
                pausePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            if (!IsPaused || IsDefeated)
                return;

            IsPaused = false;
            Time.timeScale = 1f;
            ModalStack.Instance?.TryPopSpecific(ModalKind.Pause);

            if (pausePanel != null)
                pausePanel.SetActive(false);
            journalUi?.CloseList();

            MusicPlayer.Instance?.RestoreSceneMusic();
            ModalStack.Instance?.ApplyCursorForTop();
        }

        public void OpenMemories()
        {
            if (!IsPaused || IsDefeated)
                return;
            journalUi?.OpenList();
        }

        public void CloseMemories()
        {
            journalUi?.CloseList();
        }

        public void Surrender()
        {
            if (IsDefeated || (GameManager.Instance != null && GameManager.Instance.IsLevelCompleted))
                return;

            if (IsPaused)
            {
                IsPaused = false;
                ModalStack.Instance?.TryPopSpecific(ModalKind.Pause);
                if (pausePanel != null)
                    pausePanel.SetActive(false);
                journalUi?.CloseList();
            }

            IsDefeated = true;
            Time.timeScale = 0f;
            ModalStack.Instance?.ClearAll();
            // En derrota queda la música de pausa; no volvemos al loop del nivel.

            if (defeatPanel != null)
                defeatPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void GoToMenu()
        {
            if (sceneLoader != null)
                sceneLoader.LoadSceneByName(menuSceneName);
        }
    }
}
