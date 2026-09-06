using Enigma.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.Memory
{
    // Lista simple de memorias + "Memoria recolectada".
    public class MemoryJournalUI : MonoBehaviour
    {
        [SerializeField] private MemoryJournal journal;
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private Text toastLabel;
        [SerializeField] private float toastDuration = 2f;
        [SerializeField] private Transform listRoot;
        [SerializeField] private Text entryPrefabLabel;

        private float _toastTimer;

        private void OnEnable()
        {
            if (journal != null)
                journal.OnMemoryAdded += HandleAdded;
        }

        private void OnDisable()
        {
            if (journal != null)
                journal.OnMemoryAdded -= HandleAdded;
        }

        private void Start()
        {
            if (toastRoot != null)
                toastRoot.SetActive(false);
        }

        private void Update()
        {
            if (_toastTimer <= 0f)
                return;

            _toastTimer -= Time.unscaledDeltaTime;
            if (_toastTimer <= 0f && toastRoot != null)
                toastRoot.SetActive(false);
        }

        private void HandleAdded(DocumentData doc)
        {
            ShowToast(string.IsNullOrEmpty(doc.memoryCollectedMessage)
                ? "Memoria recolectada"
                : doc.memoryCollectedMessage);

            if (listRoot != null && entryPrefabLabel != null)
            {
                var label = Instantiate(entryPrefabLabel, listRoot);
                label.text = doc.title;
                label.gameObject.SetActive(true);
            }
        }

        public void ShowToast(string message)
        {
            if (toastRoot != null)
                toastRoot.SetActive(true);
            if (toastLabel != null)
                toastLabel.text = message;
            _toastTimer = toastDuration;
        }
    }
}
