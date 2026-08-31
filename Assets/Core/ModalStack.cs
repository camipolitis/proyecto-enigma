using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Core
{
        /// Pila LIFO de modales (ventanas emergentes) (zoom, inventario, documento, pause)
        public class ModalStack : MonoBehaviour
    {
        public static ModalStack Instance { get; private set; }

        private readonly List<ModalKind> _stack = new List<ModalKind>();
        
        public event Action OnStackChanged;
        // UI e input se suscriben para saber si el jugador puede moverse.

        public bool IsEmpty => _stack.Count == 0;
        public ModalKind? Top => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

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

        public bool Contains(ModalKind kind) => _stack.Contains(kind);

        public void Push(ModalKind kind)
        {
            if (_stack.Contains(kind))
                return;
            // Evita duplicar la misma ventana (ej: abrir inventario dos veces)

            _stack.Add(kind);
            OnStackChanged?.Invoke();
        }

        public bool TryPop(out ModalKind kind)
        {
            if (_stack.Count == 0)
            {
                kind = default;
                return false;
            }

            kind = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            OnStackChanged?.Invoke();
            return true;
            // Q / Back siempre cierra el modal de arriba
        }

        public bool TryPopSpecific(ModalKind kind)
        {
            if (_stack.Count == 0 || _stack[_stack.Count - 1] != kind)
                return false;

            _stack.RemoveAt(_stack.Count - 1);
            OnStackChanged?.Invoke();
            return true;
           
        }

        public void ClearAll()
        {
            _stack.Clear();
            OnStackChanged?.Invoke();
        }

        public bool BlocksGameplay()
        {
            return _stack.Count > 0;
            // Cualquier ventana bloquea Move/Look según PlayerInputHandler.
        }

        public bool AllowsInventoryOpen()
        {
            if (Contains(ModalKind.Pause) || Contains(ModalKind.Document))
                return false;
            // Pause y lectura de nota bloquean el inventario

            return true;
            // Zoom se valida aparte con allowsInventoryWhileZoom
        }
    }
}