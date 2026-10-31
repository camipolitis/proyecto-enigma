using System.Collections;
using Enigma.Audio;
using Enigma.CameraSystem;
using Enigma.Inventory;
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

        [SerializeField] private float completeFadeDuration = 3.2f;
        // Más largo que el fade de hatch para que entre el sting.

        private bool _levelCompleted;
        private float _exitArmedAt = -1f;

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
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void Start()
        {
            if (GameFlagSystem.Instance != null)
            {
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
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
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void OnFlag(string flagId, bool value)
        {
            if (!value || flagId != levelCompleteFlag)
                return;
            _exitArmedAt = Time.unscaledTime + 0.9f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void TryCompleteFromExit()
        {
            if (_levelCompleted)
                return;
            if (GameFlagSystem.Instance == null || string.IsNullOrEmpty(levelCompleteFlag))
                return;
            if (!GameFlagSystem.Instance.Get(levelCompleteFlag))
                return;
            if (_exitArmedAt > 0f && Time.unscaledTime < _exitArmedAt)
                return;
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
            var playerState = FindFirstObjectByType<Enigma.Player.PlayerStateController>();
            if (playerState != null)
                playerState.SetState(Enigma.Player.PlayerGameplayState.Locked);
            // Durante el fade el jugador no camina más allá del umbral

            onLevelComplete?.Invoke();
            StartCoroutine(CompleteRoutine());
        }

        private IEnumerator CompleteRoutine()
        {
            Time.timeScale = 1f;
            MusicPlayer.Instance?.PlayResolve();
            if (screenFade != null)
                yield return screenFade.FadeOutRoutine(completeFadeDuration);
            else
                yield return null;

            if (sceneLoader != null && !string.IsNullOrEmpty(nextSceneName))
            {
                if (GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get("hatch_unlocked"))
                    GameFlagSystem.Carry("hatch_unlocked");
                sceneLoader.LoadSceneByName(nextSceneName);
                yield break;
            }

            if (endPanel != null)
                endPanel.SetActive(true);

            var hotbar = FindFirstObjectByType<InventoryUI>();
            hotbar?.Hide();

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
