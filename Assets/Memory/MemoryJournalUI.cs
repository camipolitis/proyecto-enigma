using System.Collections.Generic;
using Enigma.Data;
using Enigma.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.Memory
{
    // Lista de memorias + toast "Memoria recolectada".
    public class MemoryJournalUI : MonoBehaviour
    {
        [SerializeField] private MemoryJournal journal;
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private Text toastLabel;
        [SerializeField] private float toastDuration = 2f;
        [SerializeField] private Transform listRoot;
        [SerializeField] private Text entryPrefabLabel;
        [SerializeField] private GameObject listPanel;
        [SerializeField] private Text emptyLabel;
        [SerializeField] private DocumentReaderUI documentReader;

        private float _toastTimer;
        private readonly List<GameObject> _spawned = new List<GameObject>();

        private MemoryJournal LiveJournal => MemoryJournal.Instance != null ? MemoryJournal.Instance : journal;

        private void OnEnable()
        {
            BindJournal();
        }

        private void Start()
        {
            BindJournal();
            if (toastRoot != null)
                toastRoot.SetActive(false);
            if (listPanel != null)
                listPanel.SetActive(false);
            if (entryPrefabLabel != null)
                entryPrefabLabel.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (journal != null)
                journal.OnMemoryAdded -= HandleAdded;
        }

        private void Update()
        {
            if (_toastTimer <= 0f)
                return;

            _toastTimer -= Time.unscaledDeltaTime;
            if (_toastTimer <= 0f && toastRoot != null)
                toastRoot.SetActive(false);
        }

        public void OpenList()
        {
            BindJournal();
            if (listPanel != null)
                listPanel.SetActive(true);
            RebuildList();
        }

        public void CloseList()
        {
            if (listPanel != null)
                listPanel.SetActive(false);
        }

        private void BindJournal()
        {
            var live = LiveJournal;
            if (journal == live)
            {
                if (journal != null)
                {
                    journal.OnMemoryAdded -= HandleAdded;
                    journal.OnMemoryAdded += HandleAdded;
                }
                return;
            }

            if (journal != null)
                journal.OnMemoryAdded -= HandleAdded;

            journal = live;
            if (journal != null)
                journal.OnMemoryAdded += HandleAdded;
        }

        private void HandleAdded(DocumentData doc)
        {
            ShowToast(string.IsNullOrEmpty(doc.memoryCollectedMessage)
                ? "Memoria recolectada"
                : doc.memoryCollectedMessage);

            if (listPanel != null && listPanel.activeSelf)
                RebuildList();
        }

        public void ShowToast(string message)
        {
            if (toastRoot != null)
                toastRoot.SetActive(true);
            if (toastLabel != null)
                toastLabel.text = message;
            _toastTimer = toastDuration;
        }

        private void RebuildList()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    Destroy(_spawned[i]);
            }
            _spawned.Clear();

            var live = LiveJournal;
            bool hasEntries = live != null && live.Entries.Count > 0;
            if (emptyLabel != null)
            {
                emptyLabel.gameObject.SetActive(!hasEntries);
                emptyLabel.text = "Todavía no hay memorias.";
            }

            if (!hasEntries || listRoot == null || entryPrefabLabel == null)
                return;

            for (int i = 0; i < live.Entries.Count; i++)
            {
                var label = Instantiate(entryPrefabLabel, listRoot);
                var doc = live.Entries[i];
                label.text = doc.title;
                label.gameObject.SetActive(true);

                var boton = label.GetComponent<Button>();
                if (boton != null && documentReader != null)
                    boton.onClick.AddListener(() => documentReader.OpenFromJournal(doc));

                _spawned.Add(label.gameObject);
            }
        }
    }
}
