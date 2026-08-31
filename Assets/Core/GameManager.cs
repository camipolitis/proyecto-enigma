using UnityEngine;
using UnityEngine.Events;

namespace Enigma.Core
{
    
    /// Punto de anclaje del nivel: referencias y evento de fin de nivel.
    
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private string levelCompleteFlag = "door_unlocked";
        // Flag configurable: cuando se pone true, dispara el fin de nivel.

        [SerializeField] private UnityEvent onLevelComplete;
        

        private bool _levelCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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
            // Por si GameFlagSystem despierta después en el mismo frame.
            if (GameFlagSystem.Instance != null)
            {
                GameFlagSystem.Instance.OnFlagChanged -= HandleFlagChanged;
                GameFlagSystem.Instance.OnFlagChanged += HandleFlagChanged;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
            

            onLevelComplete?.Invoke();
        }
    }
}
