using System;
using System.Collections.Generic;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Memory
{
    // Registro de memorias (notas leídas). 
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
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
