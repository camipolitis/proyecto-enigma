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

        private void Awake()
        {
            EnsureLabel();
        }

        public void Bind(InventoryItem item, bool selected, int slotIndex)
        {
            index = slotIndex;
            EnsureLabel();

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
            {
                string n = (slotIndex + 1).ToString();
                legacyLabel.text = item != null ? n + "  " + item.displayName : n;
                legacyLabel.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
                legacyLabel.fontSize = selected ? 16 : 12;
                legacyLabel.color = selected ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            }

            if (highlightImage != null)
                highlightImage.enabled = selected;
        }

        public void ButtonClick()
        {
            owner?.OnSlotClicked(index);
        }

        private void EnsureLabel()
        {
            if (legacyLabel == null)
                legacyLabel = GetComponentInChildren<Text>(true);
            if (legacyLabel != null)
                return;

            var go = new GameObject("SlotLabel");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2f, 2f);
            rt.offsetMax = new Vector2(-2f, -2f);
            legacyLabel = go.AddComponent<Text>();
            legacyLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacyLabel.font == null)
                legacyLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            legacyLabel.alignment = TextAnchor.MiddleCenter;
            legacyLabel.fontSize = 12;
            legacyLabel.color = Color.white;
            legacyLabel.raycastTarget = false;
            legacyLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            legacyLabel.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
