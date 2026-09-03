using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Core
{
    
    /// Guarda flags de progreso del nivel por id string.
    
    public class GameFlagSystem : MonoBehaviour
    {
        public static GameFlagSystem Instance { get; private set; }
        // Singleton de escena para que los interactables no necesiten FindObjectOfType.

        [SerializeField] private List<string> initialFalseFlags = new List<string>();
        // Lista editable en Inspector: flags que empiezan en false al cargar el nivel.

        private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        // evita hardcodear enums de nivel en el código.

        public event Action<string, bool> OnFlagChanged;
        // Evento para UI/luces/puertas reaccionan cuando cambia un flag.

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            // Un solo GameFlagSystem por escena.

            Instance = this;

            foreach (var id in initialFalseFlags)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    _flags[id] = false;
            }
       
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool Get(string flagId)
        {
            if (string.IsNullOrEmpty(flagId))
                return false;

            return _flags.TryGetValue(flagId, out var value) && value;
            // Flags desconocidos falsee.
        }

        public void Set(string flagId, bool value)
        {
            if (string.IsNullOrEmpty(flagId))
                return;

            _flags[flagId] = value;
            OnFlagChanged?.Invoke(flagId, value);
            // Notifica a quien escuche (x ej luz de puerta cuando power_active).
        }

        public bool Has(string flagId) => Get(flagId);
        
    }
}