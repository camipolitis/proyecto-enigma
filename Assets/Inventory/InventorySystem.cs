using System;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Inventory
{
    // Inventario de 8 slots con selección para usar ítems en receptáculos.
    public class InventorySystem : MonoBehaviour
    {
        public const int SlotCount = 8;
        // Fijo para todos los niveles

        public static InventorySystem Instance { get; private set; }

        private readonly InventoryItem[] _slots = new InventoryItem[SlotCount];
        private int _selectedIndex;

        public event Action OnChanged;
        public int SelectedIndex => _selectedIndex;
        public InventoryItem SelectedItem =>
            _selectedIndex >= 0 && _selectedIndex < SlotCount ? _slots[_selectedIndex] : null;

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

        public InventoryItem GetSlot(int index)
        {
            if (index < 0 || index >= SlotCount)
                return null;
            return _slots[index];
        }

        public bool CanAdd(InventoryItem item)
        {
            if (item == null)
                return false;
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] == null)
                    return true;
            }
            return false;
        }

        public bool Add(InventoryItem item)
        {
            if (!CanAdd(item))
                return false;

            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = item;
                    _selectedIndex = i;
                    OnChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool Remove(InventoryItem item)
        {
            if (item == null)
                return false;

            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] != null && _slots[i].id == item.id)
                {
                    _slots[i] = null;
                    OnChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool Has(InventoryItem item)
        {
            if (item == null)
                return false;
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] != null && _slots[i].id == item.id)
                    return true;
            }
            return false;
        }

        public void SelectNext()
        {
            _selectedIndex = (_selectedIndex + 1) % SlotCount;
            OnChanged?.Invoke();
        }

        public void SelectPrevious()
        {
            _selectedIndex = (_selectedIndex - 1 + SlotCount) % SlotCount;
            OnChanged?.Invoke();
        }

        public void SelectIndex(int index)
        {
            if (index < 0 || index >= SlotCount)
                return;
            _selectedIndex = index;
            OnChanged?.Invoke();
        }
    }
}
