using Enigma.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.Inventory
{
    // Vista de un slot del hotbar
    public class InventorySlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Text legacyLabel;
        [SerializeField] private InventoryUI owner;
        [SerializeField] private int index;

        public void Bind(InventoryItem item, bool selected, int slotIndex)
        {
            index = slotIndex;

            if (iconImage != null)
            {
                if (item != null && item.icon != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = item != null;
                    iconImage.sprite = null;
                }
            }

            if (legacyLabel != null)
                legacyLabel.text = item != null ? item.displayName : string.Empty;

            if (highlightImage != null)
                highlightImage.enabled = selected;
        }

        public void ButtonClick()
        {
            owner?.OnSlotClicked(index);
        }
    }
}
