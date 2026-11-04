using System;
using System.Collections.Generic;
using Enigma.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Paleta y fuentes de la UI. Se aplica al grabar la escena, nunca en Play.
    [CreateAssetMenu(fileName = "SO_UiTheme", menuName = "Enigma/UI Theme")]
    public class UiTheme : ScriptableObject
    {
        [Serializable]
        public struct SizeOverride
        {
            public string path;
            public int size;
        }

        [Header("Fuentes")]
        public Font titleFont;
        public Font bodyFont;

        [Header("Colores")]
        public Color textColor = new Color32(0xDC, 0xD3, 0xC2, 0xFF);
        public Color dimTextColor = new Color32(0x9A, 0x91, 0x84, 0xFF);
        public Color accentColor = new Color32(0xC2, 0x7A, 0x2E, 0xFF);
        public Color panelColor = new Color32(0x0E, 0x0C, 0x0A, 0xEB);
        public Color buttonColor = new Color32(0x24, 0x20, 0x1C, 0xFF);
        public Color buttonHighlight = new Color32(0x5C, 0x3A, 0x18, 0xFF);
        public Color buttonPressed = new Color32(0x8A, 0x55, 0x22, 0xFF);
        public float disabledAlpha = 0.5f;
        public Color slotColor = new Color32(0x1A, 0x17, 0x14, 0xCC);
        public Color frameColor = new Color32(0x6E, 0x4A, 0x24, 0xFF);
        public float frameWidth = 2f;

        [Header("Reglas")]
        // Pantallas con estética propia: no se tocan
        public List<string> skipRoots = new List<string>
        {
            "DocumentPanel", "DocumentReaderHost", "ComputerDesktopPanel", "NotebookFolders",
            "StandPromptRoot", "FadePanel", "InspectBlur"
        };
        // Solo cambia la fuente: los colores son parte del puzzle o del fundido
        public List<string> fontOnlyRoots = new List<string> { "WirePuzzlePanel", "EndPanel" };
        // Barras del HUD: color sin marco y con su propia transparencia
        public List<string> barRoots = new List<string> { "MemoryToastRoot", "Prompt", "Hotbar", "Subtitles" };
        public List<string> titlePaths = new List<string> { "PausePanel/Label", "PausePanel/PauseLabel" };
        public List<SizeOverride> sizeOverrides = new List<SizeOverride>
        {
            new SizeOverride { path = "Title", size = 120 },
            new SizeOverride { path = "PausePanel/PauseLabel", size = 28 }
        };

        public void Apply(Canvas canvas)
        {
            if (canvas == null)
                return;

            var subtitles = new HashSet<Text>();
            foreach (var subs in canvas.GetComponentsInChildren<SubtitleSystem>(true))
                if (subs.Label != null)
                    subtitles.Add(subs.Label);

            var root = canvas.transform;
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (skipRoots.Contains(child.name))
                    continue;
                bool fontOnly = fontOnlyRoots.Contains(child.name);

                foreach (var text in child.GetComponentsInChildren<Text>(true))
                    StyleText(text, PathFrom(root, text.transform), fontOnly || subtitles.Contains(text));

                if (fontOnly)
                    continue;

                foreach (var sel in child.GetComponentsInChildren<Selectable>(true))
                    StyleButton(sel);

                var img = child.GetComponent<Image>();
                if (img != null)
                {
                    if (barRoots.Contains(child.name))
                        img.color = new Color(panelColor.r, panelColor.g, panelColor.b, img.color.a);
                    else if (child.name.EndsWith("Panel"))
                        StylePanel(img);
                }

                foreach (var slot in child.GetComponentsInChildren<InventorySlotView>(true))
                {
                    var bg = slot.GetComponent<Image>();
                    if (bg != null)
                        bg.color = slotColor;
                    slot.SetLabelColors(accentColor, dimTextColor);
                }
            }
        }

        public bool IsTitle(Text text, string path)
        {
            return text.name == "Title" || text.name.EndsWith("Title") || titlePaths.Contains(path);
        }

        private void StyleText(Text text, string path, bool fontOnly)
        {
            bool title = IsTitle(text, path);
            var font = title ? titleFont : bodyFont;
            if (font != null)
                text.font = font;
            // El stencil es más alto que Arial: un título de una línea no se recorta por el alto del rect
            if (title)
                text.verticalOverflow = VerticalWrapMode.Overflow;
            for (int i = 0; i < sizeOverrides.Count; i++)
                if (sizeOverrides[i].path == path)
                    text.fontSize = sizeOverrides[i].size;
            if (!fontOnly)
                text.color = textColor;
        }

        private void StyleButton(Selectable sel)
        {
            var img = sel.targetGraphic as Image;
            if (img == null)
                return;
            img.color = Color.white;
            var colors = sel.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonHighlight;
            colors.selectedColor = buttonHighlight;
            colors.pressedColor = buttonPressed;
            colors.disabledColor = new Color(buttonColor.r, buttonColor.g, buttonColor.b, disabledAlpha);
            colors.colorMultiplier = 1f;
            sel.colors = colors;
        }

        private void StylePanel(Image img)
        {
            img.color = panelColor;
            var frame = img.GetComponent<Outline>();
            if (frame == null)
                frame = img.gameObject.AddComponent<Outline>();
            frame.effectColor = frameColor;
            frame.effectDistance = new Vector2(frameWidth, -frameWidth);
            frame.useGraphicAlpha = false;
        }

        public static string PathFrom(Transform root, Transform t)
        {
            var path = t.name;
            for (var p = t.parent; p != null && p != root; p = p.parent)
                path = p.name + "/" + path;
            return path;
        }
    }
}
