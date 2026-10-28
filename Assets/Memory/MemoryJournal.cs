using System;
using System.Collections.Generic;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Memory
{
    // Registro de memorias (notas leídas). Persiste entre escenas.
    public class MemoryJournal : MonoBehaviour
    {
        public static MemoryJournal Instance { get; private set; }

        private readonly List<DocumentData> _entries = new List<DocumentData>();
        private readonly HashSet<string> _ids = new HashSet<string>();

        public event Action<DocumentData> OnMemoryAdded;
        public IReadOnlyList<DocumentData> Entries => _entries;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // El journal de esta escena toma Instance y hereda las notas.
                var previo = Instance;
                for (int i = 0; i < previo._entries.Count; i++)
                {
                    var doc = previo._entries[i];
                    if (doc == null || string.IsNullOrEmpty(doc.id))
                        continue;
                    if (_ids.Add(doc.id))
                        _entries.Add(doc);
                }

                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                Destroy(previo.gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool Has(DocumentData doc)
        {
            return doc != null && _ids.Contains(doc.id);
        }

        public bool TryAdd(DocumentData doc)
        {
            if (doc == null || string.IsNullOrEmpty(doc.id))
                return false;

            if (!_ids.Add(doc.id))
                return false;
            // false = ya estaba, no repetimos el toast.

            _entries.Add(doc);
            OnMemoryAdded?.Invoke(doc);
            return true;
        }
    }
}
