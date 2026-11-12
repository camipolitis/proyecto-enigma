using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Core
{
    // Pila LIFO de modales (zoom, inventario, documento, pause, código)
    public class ModalStack : MonoBehaviour
    {
        public static ModalStack Instance { get; private set; }

        private readonly List<ModalKind> _stack = new List<ModalKind>();
        private readonly List<GameObject> _openRoots = new List<GameObject>();
        
        public event Action OnStackChanged;
        // UI e input se suscriben para saber si el jugador puede moverse.

        public bool IsEmpty => _stack.Count == 0;
        public ModalKind? Top => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        public bool IsTop(ModalKind kind) => Top == kind;

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

        public bool TryRemove(ModalKind kind)
        {
            int index = _stack.LastIndexOf(kind);
            if (index < 0)
                return false;

            _stack.RemoveAt(index);
            OnStackChanged?.Invoke();
            return true;
        }

        public void ClearAll()
        {
            _stack.Clear();
            _openRoots.Clear();
            OnStackChanged?.Invoke();
        }

        // El panel que se abre se dibuja encima de los abiertos que comparten su padre.
        // No sube padres ni mueve paneles con Canvas propio: fundido, fin y hotbar quedan donde están.
        public void Open(GameObject root)
        {
            if (root == null)
                return;
            _openRoots.Remove(root);
            if (root.GetComponent<Canvas>() == null)
            {
                var parent = root.transform.parent;
                int top = -1;
                for (int i = 0; i < _openRoots.Count; i++)
                {
                    var other = _openRoots[i];
                    if (other != null && other.activeInHierarchy && other.transform.parent == parent)
                        top = Mathf.Max(top, other.transform.GetSiblingIndex());
                }
                if (top > root.transform.GetSiblingIndex())
                    root.transform.SetSiblingIndex(top);
            }
            _openRoots.Add(root);
        }

        public void Close(GameObject root)
        {
            _openRoots.Remove(root);
        }

        public bool BlocksGameplay()
        {
            return _stack.Count > 0;
            // Cualquier ventana bloquea Move/Look según PlayerInputHandler.
        }

        public bool AllowsInventoryOpen()
        {
            if (Contains(ModalKind.Pause) || Contains(ModalKind.Document) ||
                Contains(ModalKind.CodeEntry) || Contains(ModalKind.WirePuzzle) ||
                Contains(ModalKind.WordEntry) || Contains(ModalKind.ArrowLock) ||
                Contains(ModalKind.Computer) || Contains(ModalKind.ItemInspect) ||
                Contains(ModalKind.Memories) || Contains(ModalKind.Notebook))
                return false;
            // Pause, lectura y puzzles de UI bloquean el inventario

            return true;
            // Zoom se valida aparte con allowsInventoryWhileZoom
        }

        public bool BlocksWorldInteract()
        {
            return Contains(ModalKind.Pause) || Contains(ModalKind.Document) ||
                   Contains(ModalKind.CodeEntry) || Contains(ModalKind.WirePuzzle) ||
                   Contains(ModalKind.WordEntry) || Contains(ModalKind.ArrowLock) ||
                   Contains(ModalKind.Computer) || Contains(ModalKind.Inventory) ||
                   Contains(ModalKind.ItemInspect) || Contains(ModalKind.Memories) ||
                   Contains(ModalKind.Notebook);
        }

        public void ApplyCursorForTop()
        {
            var top = Top;
            bool uiOpen = top == ModalKind.Pause ||
                          top == ModalKind.Document ||
                          top == ModalKind.CodeEntry ||
                          top == ModalKind.Inventory ||
                          top == ModalKind.WirePuzzle ||
                          top == ModalKind.WordEntry ||
                          top == ModalKind.ArrowLock ||
                          top == ModalKind.Computer ||
                          top == ModalKind.ItemInspect ||
                          top == ModalKind.Memories ||
                          top == ModalKind.Notebook;
            Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = uiOpen;
        }
    }
}