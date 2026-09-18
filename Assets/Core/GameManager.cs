using System.Collections;
using Enigma.CameraSystem;
using Enigma.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Enigma.Core
{
    // Ancla del nivel: fin de nivel, fade y carga de la siguiente escena.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private string levelCompleteFlag = "door_unlocked";
        [SerializeField] private UnityEvent onLevelComplete;
        [SerializeField] private ScreenFade screenFade;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string nextSceneName = "Level02_Lab";
        [SerializeField] private GameObject endPanel;
        // Si nextSceneName está vacío, muestra endPanel (Continuará).

        private bool _levelCompleted;

        public bool IsLevelCompleted => _levelCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this &&
                Instance.gameObject.scene == gameObject.scene)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged += HandleFlagChanged;
        }

        private void Start()
        {
            if (GameFlagSystem.Instance != null)
            {
                GameFlagSystem.Instance.OnFlagChanged -= HandleFlagChanged;
                GameFlagSystem.Instance.OnFlagChanged += HandleFlagChanged;
            }

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (endPanel != null)
                endPanel.SetActive(false);

            if (screenFade != null)
                screenFade.FadeIn();
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= HandleFlagChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void HandleFlagChanged(string flagId, bool value)
        {
            if (_levelCompleted || !value)
                return;

            if (flagId == levelCompleteFlag)
                CompleteLevel();
        }

        public void CompleteLevel()
        {
            if (_levelCompleted)
                return;

            _levelCompleted = true;

            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused)
                PauseSystem.Instance.Resume();

            InteractionZoomController.Instance?.ExitZoom();
            ModalStack.Instance?.ClearAll();
            Time.timeScale = 1f;

            onLevelComplete?.Invoke();
            StartCoroutine(CompleteRoutine());
        }

        private IEnumerator CompleteRoutine()
        {
            Time.timeScale = 1f;

            if (screenFade != null)
                yield return screenFade.FadeOutRoutine();
            else
                yield return null;

            if (sceneLoader != null && !string.IsNullOrEmpty(nextSceneName))
            {
                sceneLoader.LoadSceneByName(nextSceneName);
                yield break;
            }

            if (endPanel != null)
                endPanel.SetActive(true);

            if (screenFade != null)
            {
                var fadeGroup = screenFade.GetComponent<CanvasGroup>();
                if (fadeGroup != null)
                    fadeGroup.blocksRaycasts = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
