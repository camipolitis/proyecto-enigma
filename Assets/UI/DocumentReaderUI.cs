using Enigma.Core;
using Enigma.Data;
using Enigma.Interaction;
using Enigma.Memory;
using Enigma.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Lector: imagen o ficha en papel; pantalla sólo si viene de la PC.
    public class DocumentReaderUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private RawImage viewImage;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private MemoryJournal journal;
        [SerializeField] private MemoryJournalUI journalUi;

        private DocumentData _current;
        private InteractContext _context;
        private bool _journalOnClose = true;
        private Transform _host;
        private Font _zen;
        private Font _antiqua;
        private Font _script;
        private Font _legacy;

        protected override ModalKind Kind => ModalKind.Document;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private MemoryJournal LiveJournal =>
            MemoryJournal.Instance != null ? MemoryJournal.Instance : journal;

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
            LoadFonts();
        }

        private void Update()
        {
            TryCloseWithBack();
        }

        public void Open(DocumentData document, InteractContext context)
        {
            Open(document, context, false);
        }

        public void Open(DocumentData document, InteractContext context, bool fromComputer)
        {
            _current = document;
            _context = context;
            _journalOnClose = true;
            LoadFonts();

            bool cassette = document != null && document.id == "cassette_leak";
            bool burned = document != null && document.id != null && document.id.IndexOf("burn") >= 0;
            bool calendar = IsCalendar(document);
            bool print = IsPrint(document);
            Texture photo = null;
            if (!fromComputer && !cassette && !burned && !calendar && !print)
                photo = PickPhoto(document);

            if (titleLabel != null)
            {
                titleLabel.supportRichText = true;
                titleLabel.text = document != null ? document.title : string.Empty;
            }

            string mark = null;
            string markBefore = null;
            if (bodyLabel != null)
            {
                bodyLabel.supportRichText = true;
                string text = cassette ? CassetteBody() : (document != null ? document.body : string.Empty);
                bodyLabel.text = ExtractMark(text ?? string.Empty, out mark, out markBefore);
            }

            if (calendar)
                LayoutCalendar(document);
            else if (print)
                LayoutPrint(document);
            else if (fromComputer)
                LayoutComputer(document);
            else
                LayoutPhysical(photo, cassette);

            ApplyMarks(document, burned, mark, markBefore);
            OpenModal();

            if (document != null && document.tensionOnRead)
                Enigma.Audio.MusicPlayer.Instance?.EnterTension();

            if (cassette && SubtitleSystem.Instance != null)
                SubtitleSystem.Instance.Show(CassetteSpanish(), 8f, document.openVoice);
            else if (document != null && document.openVoice != null && SubtitleSystem.Instance != null)
                SubtitleSystem.Instance.PlayClip(document.openVoice);
        }

        // ((texto)) en el cuerpo: se muestra el texto y se le dibuja un círculo de tinta
        private static string ExtractMark(string body, out string mark, out string before)
        {
            mark = null;
            before = null;
            int a = body.IndexOf("((", System.StringComparison.Ordinal);
            int b = a >= 0 ? body.IndexOf("))", a + 2, System.StringComparison.Ordinal) : -1;
            if (a < 0 || b < 0)
                return body;
            mark = body.Substring(a + 2, b - a - 2);
            before = body.Substring(0, a);
            return before + mark + body.Substring(b + 2);
        }

        private void ApplyMarks(DocumentData document, bool burned, string mark, string markBefore)
        {
            var host = Host();
            var paper = host != null ? host.Find("PaperSheet") : null;
            if (paper == null)
                return;
            HideMark(paper, "BurnOverlay");
            HideMark(paper, "ClipMark");
            HideMark(paper, "SmokeMark");
            if (bodyLabel != null)
                HideMark(bodyLabel.transform, "InkRing");

            string id = document != null ? document.id : null;
            if (burned)
                ShowOverlay(paper, "BurnOverlay", DocArt.Burn(id == "staff_burn_b" ? 2 : 1),
                    Vector2.zero, Vector2.one, 0f);
            if (id == "staff_rest")
            {
                ShowOverlay(paper, "SmokeMark", DocArt.Smoke(), new Vector2(0.02f, 0.62f), new Vector2(0.34f, 0.99f), 0f);
                ShowOverlay(paper, "ClipMark", DocArt.Clip(), new Vector2(0.09f, 0.84f), new Vector2(0.15f, 1.02f), -8f);
            }
            if (!string.IsNullOrEmpty(mark) && bodyLabel != null && isActiveAndEnabled)
                StartCoroutine(PlaceRing(mark, markBefore));
        }

        private static void HideMark(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null)
                t.gameObject.SetActive(false);
        }

        private static RawImage ShowOverlay(Transform parent, string name, Texture tex, Vector2 min, Vector2 max, float angle)
        {
            var t = parent.Find(name);
            RawImage img = t != null ? t.GetComponent<RawImage>() : null;
            if (img == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<RectTransform>();
                img = go.AddComponent<RawImage>();
                img.raycastTarget = false;
            }
            img.texture = tex;
            img.color = Color.white;
            var rt = img.rectTransform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
            img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
            return img;
        }

        private System.Collections.IEnumerator PlaceRing(string mark, string before)
        {
            yield return null;
            if (bodyLabel == null || !IsOpen)
                yield break;
            Canvas.ForceUpdateCanvases();
            var rt = bodyLabel.rectTransform;
            var rect = rt.rect;
            var settings = bodyLabel.GetGenerationSettings(new Vector2(rect.width, 0f));
            var gen = bodyLabel.cachedTextGeneratorForLayout;
            float ppu = Mathf.Max(0.0001f, bodyLabel.pixelsPerUnit);

            int nl = before.LastIndexOf('\n');
            string prefix = nl >= 0 ? before.Substring(nl + 1) : before;
            float through = gen.GetPreferredHeight(before + mark, settings) / ppu;
            float lineH = gen.GetPreferredHeight(mark, settings) / ppu;
            float bar = gen.GetPreferredWidth("|", settings) / ppu;
            float prefixW = gen.GetPreferredWidth(prefix + "|", settings) / ppu - bar;
            float markW = gen.GetPreferredWidth(mark, settings) / ppu;

            var ring = ShowOverlay(rt, "InkRing", DocArt.Ring(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), -4f);
            var r = ring.rectTransform;
            r.sizeDelta = new Vector2(markW * 1.45f + lineH * 0.9f, lineH * 2f);
            r.anchoredPosition = new Vector2(rect.xMin + prefixW + markW * 0.5f, rect.yMax - through + lineH * 0.5f);
        }

        private static string CassetteBody()
        {
            return "<i>Wir verfügen über die Mittel, ORIGEN zu erreichen.\n"
                + "Wer sich einmischt, weiß, womit er zu rechnen hat.\n"
                + "Von diesem Tag an…</i>\n\n"
                + CassetteSpanish();
        }

        private static string CassetteSpanish()
        {
            return "Tenemos los medios para llegar a ORIGEN.\n"
                + "Quien interfiera sabe a lo que se enfrenta.\n"
                + "A partir de este día…";
        }

        private static bool IsCalendar(DocumentData document)
        {
            if (document == null)
                return false;
            if (!string.IsNullOrEmpty(document.id) && document.id.IndexOf("calendar") >= 0)
                return true;
            return document.viewImage != null && document.id == "calendar_dec1978";
        }

        private static bool IsPrint(DocumentData document)
        {
            return document != null && !string.IsNullOrEmpty(document.id) && document.id.StartsWith("print_");
        }

        private static Texture PickPhoto(DocumentData document)
        {
            if (document == null || string.IsNullOrEmpty(document.id))
                return null;
            if (document.id == "staff_rest")
                return null;
            string path = null;
            if (document.id == "staff_204")
                path = "Docs/id_1";
            else if (document.id == "staff_731")
                path = "Docs/id_2";
            else if (document.id == "subject_084")
                path = "Docs/id_3";
            else if (document.id == "subject_117")
                path = "Docs/id_4";
            else if (document.id == "subject_129")
                path = "Docs/id_8";
            else if (document.id == "subject_206")
                path = "Docs/id_9";
            if (path == null)
                return null;
            return Resources.Load<Texture2D>(path);
        }

        public void OpenFromJournal(DocumentData document)
        {
            Open(document, null, false);
        }

        public void Inspect(InventoryItem item)
        {
            if (item == null)
                return;
            if (item.inspectDoc != null)
            {
                _journalOnClose = false;
                Open(item.inspectDoc, null, false);
                _journalOnClose = false;
                return;
            }
            OpenInspect(item.displayName, item.description);
        }

        public void OpenInspect(string title, string body)
        {
            _current = null;
            _context = null;
            _journalOnClose = false;
            if (viewImage != null)
                viewImage.gameObject.SetActive(false);
            if (titleLabel != null)
            {
                titleLabel.gameObject.SetActive(false);
                titleLabel.text = title ?? string.Empty;
            }
            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(false);
                bodyLabel.text = body ?? string.Empty;
            }
            LayoutPhysical(null, false);
            OpenModal();
        }

        public override void Close()
        {
            if (!IsOpen)
                return;

            var closing = _current;
            var ctx = _context;
            base.Close();

            Enigma.Audio.MusicPlayer.Instance?.ExitTension();

            if (SubtitleSystem.Instance != null)
                SubtitleSystem.Instance.StopVoice();

            if (_journalOnClose && closing != null)
            {
                bool firstTime = LiveJournal != null && LiveJournal.TryAdd(closing);
                if (firstTime)
                {
                    GameFlagSystem.Instance?.Set("note_read", true);
                    journalUi?.ShowToast(closing.memoryCollectedMessage);
                }

                PlayReleaseLine(ctx, closing);
            }

            _current = null;
            _context = null;
            _journalOnClose = true;
        }

        private Transform Host()
        {
            if (_host != null)
                return _host;
            Transform t = null;
            if (bodyLabel != null)
                t = bodyLabel.transform.parent;
            else if (titleLabel != null)
                t = titleLabel.transform.parent;
            // Las etiquetas viven dentro de PaperSheet tras la primera apertura: el host es el contenedor original
            while (t != null && t.name == "PaperSheet")
                t = t.parent;
            _host = t != null ? t : (root != null ? root.transform : transform);
            return _host;
        }

        private void SetHostBackdrop(Transform host, bool on)
        {
            Transform t = host;
            while (t != null)
            {
                if (t.name == "DocumentPanel" || t.name == "Dimmer")
                {
                    var img = t.GetComponent<Image>();
                    if (img != null && t.name == "DocumentPanel")
                        img.enabled = on;
                }
                var named = t.Find("DocumentPanel");
                if (named != null)
                {
                    var img = named.GetComponent<Image>();
                    if (img != null)
                        img.enabled = on;
                }
                t = t.parent;
            }
            if (root != null)
            {
                var img = root.GetComponent<Image>();
                if (img != null)
                    img.enabled = on;
            }
        }

        private void LayoutCalendar(DocumentData document)
        {
            var host = Host();
            EnsureDimmer(host).gameObject.SetActive(true);
            SetHostBackdrop(host, false);
            SetPaper(host, false, Color.clear);
            ClearPrintLines(host);
            KillBurnHoles(host);
            PlaceIdPhoto(host, null);

            if (titleLabel != null)
                titleLabel.gameObject.SetActive(false);
            if (bodyLabel != null)
                bodyLabel.gameObject.SetActive(false);

            if (viewImage == null || document == null || document.viewImage == null)
                return;

            viewImage.texture = document.viewImage;
            viewImage.color = Color.white;
            viewImage.gameObject.SetActive(true);
            viewImage.raycastTarget = false;
            viewImage.transform.SetParent(host, false);
            var fitter = viewImage.GetComponent<AspectRatioFitter>();
            if (fitter == null)
                fitter = viewImage.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            float w = Mathf.Max(1f, document.viewImage.width);
            float h = Mathf.Max(1f, document.viewImage.height);
            fitter.aspectRatio = w / h;
            fitter.enabled = true;
            var rt = viewImage.rectTransform;
            rt.anchorMin = new Vector2(0.04f, 0.04f);
            rt.anchorMax = new Vector2(0.96f, 0.96f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            viewImage.transform.SetAsLastSibling();
        }

        private void LayoutPrint(DocumentData document)
        {
            var host = Host();
            EnsureDimmer(host).gameObject.SetActive(true);
            SetHostBackdrop(host, false);
            SetPaper(host, true, new Color(0.95f, 0.92f, 0.82f, 1f));
            KillBurnHoles(host);
            var paper = EnsurePaper(host);
            PlaceIdPhoto(paper.transform, null);
            DrawPrintLines(paper.transform);

            if (titleLabel != null)
                titleLabel.gameObject.SetActive(false);

            if (viewImage != null)
                viewImage.gameObject.SetActive(false);

            if (bodyLabel == null)
                return;
            bodyLabel.gameObject.SetActive(true);
            ApplyFont(bodyLabel, _script != null ? _script : _legacy, 86, new Color(0.72f, 0.08f, 0.06f, 1f));
            bodyLabel.alignment = TextAnchor.MiddleCenter;
            bodyLabel.fontStyle = FontStyle.Normal;
            string n = PrintNumber(document);
            bodyLabel.text = n;
            SitIn(bodyLabel.rectTransform, paper.transform, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f));
            bodyLabel.transform.SetAsLastSibling();
        }

        private static string PrintNumber(DocumentData document)
        {
            if (document == null)
                return string.Empty;
            if (!string.IsNullOrEmpty(document.body))
            {
                string t = document.body.Trim();
                if (t.Length <= 5)
                    return t;
            }
            if (document.id == "print_10")
                return "10";
            if (document.id == "print_109")
                return "109";
            if (document.id == "print_3")
                return "3";
            return document.body ?? string.Empty;
        }

        private void LayoutComputer(DocumentData document)
        {
            var host = Host();
            EnsureDimmer(host).gameObject.SetActive(true);
            SetHostBackdrop(host, true);
            var paper = EnsurePaper(host);
            SetPaper(host, true, new Color(0.04f, 0.07f, 0.05f, 0.96f));
            ClearPrintLines(host);
            PlaceIdPhoto(paper.transform, null);
            KillBurnHoles(host);
            if (viewImage != null)
                viewImage.gameObject.SetActive(false);

            Color phosphor = new Color(0.55f, 1f, 0.62f, 1f);
            Font zen = _zen != null ? _zen : _legacy;

            if (titleLabel != null)
            {
                titleLabel.gameObject.SetActive(true);
                ApplyFont(titleLabel, zen, 22, phosphor);
                titleLabel.alignment = TextAnchor.UpperLeft;
                titleLabel.text = document != null ? document.title : string.Empty;
                SitIn(titleLabel.rectTransform, paper.transform, new Vector2(0.07f, 0.84f), new Vector2(0.93f, 0.96f));
            }

            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(true);
                ApplyFont(bodyLabel, zen, 16, phosphor);
                bodyLabel.alignment = TextAnchor.UpperLeft;
                SitIn(bodyLabel.rectTransform, paper.transform, new Vector2(0.07f, 0.06f), new Vector2(0.93f, 0.82f));
            }
        }

        private void LayoutPhysical(Texture photo, bool cassette)
        {
            var host = Host();
            EnsureDimmer(host).gameObject.SetActive(true);
            SetHostBackdrop(host, false);
            SetPaper(host, true, new Color(0.96f, 0.93f, 0.85f, 1f));
            ClearPrintLines(host);
            KillBurnHoles(host);
            var paper = EnsurePaper(host);

            Font face = _antiqua != null ? _antiqua : _legacy;
            Color ink = new Color(0.14f, 0.11f, 0.09f, 1f);
            bool hasPhoto = photo != null;

            if (titleLabel != null)
            {
                titleLabel.gameObject.SetActive(true);
                ApplyFont(titleLabel, face, 22, ink);
                titleLabel.alignment = TextAnchor.UpperLeft;
                SitIn(titleLabel.rectTransform, paper.transform,
                    new Vector2(0.07f, 0.84f),
                    new Vector2(hasPhoto ? 0.66f : 0.93f, 0.96f));
            }

            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(true);
                ApplyFont(bodyLabel, face, cassette ? 18 : 17, ink);
                bodyLabel.alignment = TextAnchor.UpperLeft;
                SitIn(bodyLabel.rectTransform, paper.transform,
                    new Vector2(0.07f, 0.06f),
                    new Vector2(hasPhoto ? 0.66f : 0.93f, 0.82f));
            }

            PlaceIdPhoto(paper.transform, photo);
        }

        private static void SitIn(RectTransform rt, Transform parent, Vector2 min, Vector2 max)
        {
            if (rt == null || parent == null)
                return;
            rt.SetParent(parent, false);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        private void SetPaper(Transform host, bool on, Color color)
        {
            var paper = EnsurePaper(host);
            paper.gameObject.SetActive(on);
            if (on)
                paper.color = color;
            paper.transform.SetSiblingIndex(1);
        }

        private static Image EnsurePaper(Transform host)
        {
            var t = host.Find("PaperSheet");
            Image img = t != null ? t.GetComponent<Image>() : null;
            if (img != null)
                return img;
            var go = new GameObject("PaperSheet");
            go.transform.SetParent(host, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.18f, 0.08f);
            rt.anchorMax = new Vector2(0.82f, 0.92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(0.93f, 0.88f, 0.76f, 1f);
            return img;
        }

        private static Image EnsureDimmer(Transform host)
        {
            var t = host.Find("Dimmer");
            Image img = t != null ? t.GetComponent<Image>() : null;
            if (img != null)
                return img;
            var go = new GameObject("Dimmer");
            go.transform.SetParent(host, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            img = go.AddComponent<Image>();
            img.raycastTarget = true;
            img.color = new Color(0f, 0f, 0f, 0.62f);
            go.transform.SetAsFirstSibling();
            return img;
        }

        private void PlaceIdPhoto(Transform host, Texture photo)
        {
            if (viewImage == null)
                return;
            var fitter = viewImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.enabled = false;
            viewImage.raycastTarget = false;
            if (photo == null)
            {
                viewImage.gameObject.SetActive(false);
                return;
            }
            viewImage.texture = photo;
            viewImage.color = Color.white;
            viewImage.gameObject.SetActive(true);
            viewImage.transform.SetParent(host, false);
            var rt = viewImage.rectTransform;
            rt.anchorMin = new Vector2(0.70f, 0.62f);
            rt.anchorMax = new Vector2(0.94f, 0.96f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            viewImage.transform.SetAsLastSibling();
        }

        private static void KillBurnHoles(Transform host)
        {
            if (host == null)
                return;
            for (int i = 0; i < 4; i++)
            {
                var t = host.Find("BurnHole_" + i);
                if (t != null)
                    t.gameObject.SetActive(false);
            }
            var paper = host.Find("PaperSheet");
            if (paper != null && paper != host)
                KillBurnHoles(paper);
        }

        private static void DrawPrintLines(Transform host)
        {
            for (int i = 0; i < 16; i++)
            {
                string n = "PrintLine_" + i;
                var t = host.Find(n);
                Image img = t != null ? t.GetComponent<Image>() : null;
                if (img == null)
                {
                    var go = new GameObject(n);
                    go.transform.SetParent(host, false);
                    go.AddComponent<RectTransform>();
                    img = go.AddComponent<Image>();
                    img.raycastTarget = false;
                }
                img.gameObject.SetActive(true);
                img.color = new Color(0.55f, 0.62f, 0.78f, 0.45f);
                var rt = img.rectTransform;
                float y = 0.88f - i * 0.05f;
                rt.anchorMin = new Vector2(0.08f, y);
                rt.anchorMax = new Vector2(0.92f, y);
                rt.offsetMin = new Vector2(0f, -1f);
                rt.offsetMax = new Vector2(0f, 1f);
                img.transform.SetSiblingIndex(0);
            }
        }

        private static void ClearPrintLines(Transform host)
        {
            if (host == null)
                return;
            for (int i = 0; i < 16; i++)
            {
                var t = host.Find("PrintLine_" + i);
                if (t != null)
                    t.gameObject.SetActive(false);
            }
            var paper = host.Find("PaperSheet");
            if (paper != null && paper != host)
                ClearPrintLines(paper);
        }

        private void LoadFonts()
        {
            if (_zen == null)
                _zen = Resources.Load<Font>("Fonts/ZenDots-Regular");
            if (_antiqua == null)
                _antiqua = Resources.Load<Font>("Fonts/GlassAntiqua-Regular");
            if (_script == null)
                _script = Resources.Load<Font>("Fonts/SegoeScript");
            if (_legacy == null)
            {
                _legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_legacy == null)
                    _legacy = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private static void ApplyFont(Text label, Font font, int size, Color color)
        {
            if (label == null)
                return;
            if (font != null)
                label.font = font;
            label.fontSize = size;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void PlayReleaseLine(InteractContext ctx, DocumentData doc)
        {
            if (ctx?.Subtitles == null || doc == null)
                return;

            var set = ctx.Dialogue;
            if (set != null && !string.IsNullOrEmpty(doc.releaseDialogueId) &&
                set.TryGet(doc.releaseDialogueId, out var spoken))
            {
                ctx.Subtitles.Show(spoken.text, spoken.duration, spoken.voice);
                return;
            }

            if (string.IsNullOrEmpty(doc.releaseSubtitle))
                return;

            ctx.Subtitles.Show(doc.releaseSubtitle, 1.5f);
        }
    }
}
