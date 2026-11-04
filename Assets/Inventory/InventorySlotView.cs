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
        [SerializeField] private Color selectedLabelColor = Color.white;
        [SerializeField] private Color normalLabelColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        public void SetLabelColors(Color selected, Color normal)
        {
            selectedLabelColor = selected;
            normalLabelColor = normal;
        }

        private void Awake()
        {
            EnsureParts();
        }

        // Ícono y número como hijos del slot (se graban en la escena)
        public void EnsureParts()
        {
            EnsureIcon();
            EnsureLabel();
        }

        private void EnsureIcon()
        {
            if (iconImage != null)
                return;
            var existing = transform.Find("SlotIcon");
            iconImage = existing != null ? existing.GetComponent<Image>() : null;
            if (iconImage != null)
                return;

            var go = new GameObject("SlotIcon");
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6f, 6f);
            rt.offsetMax = new Vector2(-6f, -6f);
            iconImage = go.AddComponent<Image>();
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
            iconImage.enabled = false;
        }

        public void Bind(InventoryItem item, bool selected, int slotIndex)
        {
            index = slotIndex;
            EnsureLabel();

            bool hasIcon = item != null && item.icon != null && iconImage != null;
            if (iconImage != null)
            {
                iconImage.sprite = hasIcon ? item.icon : null;
                iconImage.preserveAspect = true;
                iconImage.color = Color.white;
                iconImage.enabled = hasIcon;
            }

            // Con miniatura el número sobra; el slot vacío lo conserva como guía de la tecla
            if (legacyLabel != null)
            {
                legacyLabel.enabled = !hasIcon;
                legacyLabel.text = (slotIndex + 1).ToString();
                legacyLabel.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
                legacyLabel.fontSize = selected ? 16 : 12;
                legacyLabel.color = selected ? selectedLabelColor : normalLabelColor;
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
            legacyLabel.color = normalLabelColor;
            legacyLabel.raycastTarget = false;
            legacyLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            legacyLabel.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
