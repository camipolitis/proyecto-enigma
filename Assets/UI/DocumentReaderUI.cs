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
        // Cuánto queda el subtítulo de una grabación sin clip
        [SerializeField] private float listenSeconds = 8f;
        // Un texto que no entra en la hoja se parte en páginas
        [SerializeField] private Button pagePrev;
        [SerializeField] private Button pageNext;
        [SerializeField] private Text pageLabel;
        // En papel la letra se achica hasta esta fracción antes de partir en páginas
        [SerializeField] private float minBodyScale = 0.6f;

        private const int PagerFontSize = 16;
        private const int PagerArrowSize = 26;

        private struct Page
        {
            public int start;
            public int end;
            public string prefix;
            public string text;
        }

        private readonly System.Collections.Generic.List<Page> _pages = new System.Collections.Generic.List<Page>();
        private int _page;
        private string _fullBody = string.Empty;
        private string _mark;
        private int _markStart = -1;
        private bool _paged;
        private bool _shrinkBody;
        private Coroutine _ring;

        private DocumentData _current;
        private InteractContext _context;
        private bool _journalOnClose = true;
        private float _listenUntil;
        private bool _fromJournal;
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

        // Abierto desde la lista de memorias: al cerrar la lista se cierra con ella
        public bool FromJournal => IsOpen && _fromJournal;

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
            LoadFonts();
            if (pagePrev != null)
                pagePrev.onClick.AddListener(PrevPage);
            if (pageNext != null)
                pageNext.onClick.AddListener(NextPage);
            RefreshPager();
        }

        // Lo llama el baker: la hoja, el fondo y los controles de página quedan grabados en la escena
        public void EnsureParts()
        {
            var host = Host();
            if (host == null)
                return;
            EnsureDimmer(host);
            var paper = EnsurePaper(host).transform;
            if (pagePrev == null)
                pagePrev = PagerButton(paper, "PagePrev", "<", new Vector2(0.02f, 0.015f), new Vector2(0.14f, 0.10f));
            if (pageNext == null)
                pageNext = PagerButton(paper, "PageNext", ">", new Vector2(0.86f, 0.015f), new Vector2(0.98f, 0.10f));
            if (pageLabel == null)
                pageLabel = PagerText(paper, "PageLabel", "1/1", new Vector2(0.38f, 0.015f), new Vector2(0.62f, 0.10f));
            pagePrev.gameObject.SetActive(false);
            pageNext.gameObject.SetActive(false);
            pageLabel.gameObject.SetActive(false);
        }

        private Button PagerButton(Transform parent, string name, string glyph, Vector2 min, Vector2 max)
        {
            var label = PagerText(parent, name, glyph, min, max);
            label.raycastTarget = true;
            var button = label.gameObject.AddComponent<Button>();
            button.targetGraphic = label;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        private Text PagerText(Transform parent, string name, string value, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var text = go.AddComponent<Text>();
            text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = bodyLabel != null ? bodyLabel.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            return text;
        }

        private void Update()
        {
            if (TryCloseWithBack())
                return;
            ReadPageKeys();
        }

        private void ReadPageKeys()
        {
            if (!IsOpen || _pages.Count < 2 || (ModalStack.Instance != null && !ModalStack.Instance.IsTop(Kind)))
                return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if ((kb != null && kb.rightArrowKey.wasPressedThisFrame) || (pad != null && pad.dpad.right.wasPressedThisFrame))
                NextPage();
            else if ((kb != null && kb.leftArrowKey.wasPressedThisFrame) || (pad != null && pad.dpad.left.wasPressedThisFrame))
                PrevPage();
        }

        public void NextPage()
        {
            if (_page + 1 < _pages.Count)
                ShowPage(_page + 1);
        }

        public void PrevPage()
        {
            if (_page > 0)
                ShowPage(_page - 1);
        }

        public int PageCount => _pages.Count;
        public int PageIndex => _page;

        public void Open(DocumentData document, InteractContext context)
        {
            Open(document, context, false);
        }

        public void Open(DocumentData document, InteractContext context, bool fromComputer)
        {
            _fromJournal = false;
            _current = document;
            _context = context;
            _journalOnClose = true;
            LoadFonts();

            bool burned = document != null && document.id != null && document.id.IndexOf("burn") >= 0;
            bool imageOnly = IsImageOnly(document);
            bool print = IsPrint(document);
            Texture photo = null;
            if (document != null && !fromComputer && !burned && !imageOnly && !print)
                photo = document.photo;

            if (titleLabel != null)
            {
                titleLabel.supportRichText = true;
                titleLabel.text = document != null ? document.title : string.Empty;
            }

            string mark = null;
            string markBefore = null;
            string text = document != null ? document.body : string.Empty;
            _fullBody = ExtractMark(text ?? string.Empty, out mark, out markBefore);
            _mark = mark;
            _markStart = mark != null ? markBefore.Length : -1;
            if (bodyLabel != null)
            {
                bodyLabel.supportRichText = true;
                bodyLabel.text = _fullBody;
            }

            _paged = false;
            if (imageOnly)
                LayoutImage(document);
            else if (print)
                LayoutPrint(document);
            else if (fromComputer)
                LayoutComputer(document);
            else
                LayoutPhysical(photo, document);

            ApplyMarks(document, burned);
            OpenModal();
            Paginate();

            if (document != null && document.tensionOnRead)
                Enigma.Audio.MusicPlayer.Instance?.EnterTension();

            if (document != null && document.openVoice != null && SubtitleSystem.Instance != null)
                SubtitleSystem.Instance.PlayClip(document.openVoice);
        }

        // Grabación: se escucha sin hoja y se guarda en el diario al empezar
        public void Listen(DocumentData document)
        {
            _fromJournal = false;
            if (document == null || Time.unscaledTime < _listenUntil)
                return;
            float duration = document.openVoice != null ? document.openVoice.length : listenSeconds;
            _listenUntil = Time.unscaledTime + duration;
            SubtitleSystem.Instance?.Show(document.body, duration, document.openVoice);
            Remember(document);
        }

        // El toast lo muestra MemoryJournalUI al recibir OnMemoryAdded
        private void Remember(DocumentData document)
        {
            if (document == null || !document.isMemory || LiveJournal == null)
                return;
            if (LiveJournal.TryAdd(document))
                GameFlagSystem.Instance?.Set("note_read", true);
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

        private void ApplyMarks(DocumentData document, bool burned)
        {
            var host = Host();
            var paper = host != null ? host.Find("PaperSheet") : null;
            if (paper == null)
                return;
            HideMark(paper, "BurnOverlay");
            HideMark(paper, "SmokeMark");
            HideMark(paper, "CornerLetter");
            if (bodyLabel != null)
                HideMark(bodyLabel.transform, "InkRing");

            string id = document != null ? document.id : null;
            if (burned)
                ShowOverlay(paper, "BurnOverlay", DocArt.Burn(id == "staff_burn_b" ? 2 : 1),
                    Vector2.zero, Vector2.one, 0f);
            if (id == "staff_rest")
                ShowOverlay(paper, "SmokeMark", DocArt.Smoke(), new Vector2(0.02f, 0.62f), new Vector2(0.34f, 0.99f), 0f);
            if (document != null && !string.IsNullOrEmpty(document.cornerLetter))
                ShowCornerLetter(paper, document.cornerLetter);
        }

        // Parte el cuerpo en páginas que entran en la hoja; sin páginas no hay controles
        private void Paginate()
        {
            _pages.Clear();
            _page = 0;
            if (!_paged || bodyLabel == null || !bodyLabel.gameObject.activeInHierarchy)
            {
                RefreshPager();
                return;
            }
            Canvas.ForceUpdateCanvases();
            float height = bodyLabel.rectTransform.rect.height;
            bodyLabel.resizeTextForBestFit = false;
            bodyLabel.verticalOverflow = VerticalWrapMode.Overflow;
            if (_shrinkBody)
            {
                int min = Mathf.Max(1, Mathf.RoundToInt(bodyLabel.fontSize * minBodyScale));
                while (bodyLabel.fontSize > min && TextHeight(_fullBody) > height)
                    bodyLabel.fontSize--;
            }
            SplitPages(_fullBody, height);
            ShowPage(0);
        }

        private float TextHeight(string text)
        {
            var settings = bodyLabel.GetGenerationSettings(new Vector2(bodyLabel.rectTransform.rect.width, 0f));
            float ppu = Mathf.Max(0.0001f, bodyLabel.pixelsPerUnit);
            return bodyLabel.cachedTextGeneratorForLayout.GetPreferredHeight(text, settings) / ppu;
        }

        private void SplitPages(string full, float height)
        {
            var breaks = BreakPoints(full);
            int start = SkipBlank(full, 0);
            while (start < full.Length)
            {
                string prefix = string.Concat(OpenTags(full, start));
                // La página llega hasta el último corte que todavía entra
                int first = breaks.FindIndex(b => b > start);
                if (first < 0)
                    first = breaks.Count - 1;
                int lo = first, hi = breaks.Count - 1, best = -1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) / 2;
                    if (TextHeight(PageText(full, prefix, start, breaks[mid])) <= height)
                    {
                        best = mid;
                        lo = mid + 1;
                    }
                    else
                        hi = mid - 1;
                }
                // Una frase resaltada no se parte entre páginas si hay un corte anterior fuera de ella
                if (best >= 0)
                {
                    int keep = best;
                    while (keep > first && OpenTags(full, breaks[keep]).Count > 0)
                        keep--;
                    if (OpenTags(full, breaks[keep]).Count == 0)
                        best = keep;
                }
                int end = breaks[best >= 0 ? best : first];
                _pages.Add(new Page { start = start, end = end, prefix = prefix, text = PageText(full, prefix, start, end) });
                start = SkipBlank(full, end);
            }
        }

        // Cortes posibles: espacios y saltos fuera de etiquetas, más el final del texto
        private static System.Collections.Generic.List<int> BreakPoints(string text)
        {
            var list = new System.Collections.Generic.List<int>();
            bool inTag = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '<')
                    inTag = true;
                else if (c == '>')
                    inTag = false;
                else if (!inTag && (c == ' ' || c == '\n'))
                    list.Add(i);
            }
            list.Add(text.Length);
            return list;
        }

        private static int SkipBlank(string text, int i)
        {
            while (i < text.Length && (text[i] == ' ' || text[i] == '\n'))
                i++;
            return i;
        }

        // Cada página reabre las etiquetas que venían abiertas y cierra las que quedan abiertas
        private static string PageText(string full, string prefix, int start, int end)
        {
            var open = OpenTags(full, end);
            var sb = new System.Text.StringBuilder(prefix);
            sb.Append(full, start, end - start);
            for (int i = open.Count - 1; i >= 0; i--)
                sb.Append("</").Append(TagName(open[i])).Append('>');
            return sb.ToString();
        }

        private static System.Collections.Generic.List<string> OpenTags(string text, int end)
        {
            var open = new System.Collections.Generic.List<string>();
            int i = 0;
            while (i < end)
            {
                int a = text.IndexOf('<', i);
                if (a < 0 || a >= end)
                    break;
                int b = text.IndexOf('>', a);
                if (b < 0 || b >= end)
                    break;
                string tag = text.Substring(a, b - a + 1);
                if (tag.StartsWith("</"))
                {
                    string name = tag.Substring(2, tag.Length - 3);
                    int k = open.FindLastIndex(t => TagName(t) == name);
                    if (k >= 0)
                        open.RemoveAt(k);
                }
                else
                    open.Add(tag);
                i = b + 1;
            }
            return open;
        }

        private static string TagName(string tag)
        {
            int stop = tag.IndexOfAny(new[] { '=', '>' }, 1);
            return stop > 1 ? tag.Substring(1, stop - 1) : tag.Trim('<', '>');
        }

        private void ShowPage(int index)
        {
            if (bodyLabel == null || _pages.Count == 0)
                return;
            _page = Mathf.Clamp(index, 0, _pages.Count - 1);
            var p = _pages[_page];
            bodyLabel.text = p.text;
            if (_ring != null)
                StopCoroutine(_ring);
            _ring = null;
            HideMark(bodyLabel.transform, "InkRing");
            // El aro solo se dibuja en la página donde está la marca
            if (_markStart >= p.start && _markStart + _mark.Length <= p.end && isActiveAndEnabled)
                _ring = StartCoroutine(PlaceRing(_mark, p.prefix + _fullBody.Substring(p.start, _markStart - p.start)));
            RefreshPager();
        }

        private void RefreshPager()
        {
            bool show = _pages.Count > 1;
            if (pagePrev != null)
            {
                pagePrev.gameObject.SetActive(show);
                pagePrev.interactable = _page > 0;
            }
            if (pageNext != null)
            {
                pageNext.gameObject.SetActive(show);
                pageNext.interactable = _page + 1 < _pages.Count;
            }
            if (pageLabel != null)
            {
                pageLabel.gameObject.SetActive(show);
                pageLabel.text = (_page + 1) + "/" + Mathf.Max(1, _pages.Count);
            }
            if (show && bodyLabel != null)
                TintPager(bodyLabel.color, bodyLabel.font);
        }

        // Los controles toman la tinta de la hoja: fósforo en la pantalla, tinta en el papel
        private void TintPager(Color ink, Font font)
        {
            if (pageLabel != null)
            {
                pageLabel.color = ink;
                pageLabel.font = font;
                pageLabel.fontSize = Mathf.RoundToInt(PagerFontSize * UiScale(pageLabel));
            }
            foreach (var b in new[] { pagePrev, pageNext })
            {
                if (b == null)
                    continue;
                var label = b.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.color = ink;
                    label.font = font;
                    label.fontSize = Mathf.RoundToInt(PagerArrowSize * UiScale(label));
                }
                var colors = b.colors;
                colors.disabledColor = new Color(1f, 1f, 1f, 0.25f);
                b.colors = colors;
            }
        }

        private void ShowCornerLetter(Transform paper, string letter)
        {
            var ring = ShowOverlay(paper, "CornerLetter", DocArt.Ring(), new Vector2(0.8f, 0.04f), new Vector2(0.96f, 0.17f), -6f);
            var t = ring.transform.Find("Letter");
            Text label = t != null ? t.GetComponent<Text>() : null;
            if (label == null)
            {
                var go = new GameObject("Letter");
                go.transform.SetParent(ring.transform, false);
                var lrt = go.AddComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                label = go.AddComponent<Text>();
                label.raycastTarget = false;
                label.alignment = TextAnchor.MiddleCenter;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 10;
                label.resizeTextMaxSize = 72;
            }
            // Marcador rojo: misma letra que los números de impresión
            if (_script != null)
                label.font = _script;
            else if (bodyLabel != null)
                label.font = bodyLabel.font;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.62f, 0.09f, 0.07f, 1f);
            label.text = letter;
            label.transform.localRotation = Quaternion.Euler(0f, 0f, 6f);
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

        private static bool IsImageOnly(DocumentData document)
        {
            return document != null && document.imageOnly && document.viewImage != null;
        }

        private static bool IsPrint(DocumentData document)
        {
            return document != null && !string.IsNullOrEmpty(document.id) && document.id.StartsWith("print_");
        }

        public void OpenFromJournal(DocumentData document)
        {
            if (document != null && document.audioOnly)
            {
                Listen(document);
                return;
            }
            Open(document, null, false);
            _fromJournal = true;
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
            _fromJournal = false;
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
            _fullBody = body ?? string.Empty;
            _mark = null;
            _markStart = -1;
            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(false);
                bodyLabel.text = _fullBody;
            }
            _paged = false;
            LayoutPhysical(null, null);
            OpenModal();
            Paginate();
        }

        public override void Close()
        {
            _fromJournal = false;
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
                Remember(closing);
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

        private void LayoutImage(DocumentData document)
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

            // Encabezado chico de la carpeta, sobre la primera línea
            if (titleLabel != null)
            {
                bool hasTitle = document != null && !string.IsNullOrEmpty(document.title);
                titleLabel.gameObject.SetActive(hasTitle);
                if (hasTitle)
                {
                    ApplyFont(titleLabel, ItemVisuals.HeaderFont, 18, new Color(0.36f, 0.42f, 0.55f, 1f));
                    titleLabel.alignment = TextAnchor.MiddleCenter;
                    titleLabel.text = document.title;
                    SitIn(titleLabel.rectTransform, paper.transform, new Vector2(0.08f, 0.90f), new Vector2(0.92f, 0.96f));
                }
            }

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
                SitIn(bodyLabel.rectTransform, paper.transform, new Vector2(0.07f, 0.12f), new Vector2(0.93f, 0.82f));
                _paged = true;
                _shrinkBody = false;
            }
        }

        private void LayoutPhysical(Texture photo, DocumentData document)
        {
            var host = Host();
            EnsureDimmer(host).gameObject.SetActive(true);
            SetHostBackdrop(host, false);
            SetPaper(host, true, new Color(0.96f, 0.93f, 0.85f, 1f));
            ClearPrintLines(host);
            KillBurnHoles(host);
            var paper = EnsurePaper(host);

            Font face = _antiqua != null ? _antiqua : _legacy;
            Font titleFace = document != null && document.titleFont != null ? document.titleFont : face;
            Font bodyFace = document != null && document.bodyFont != null ? document.bodyFont : face;
            Color ink = new Color(0.14f, 0.11f, 0.09f, 1f);
            bool hasPhoto = photo != null;

            if (titleLabel != null)
            {
                titleLabel.gameObject.SetActive(true);
                ApplyFont(titleLabel, titleFace, 22, ink);
                // Un título largo se achica en su franja en vez de caer sobre el cuerpo
                titleLabel.resizeTextForBestFit = true;
                titleLabel.resizeTextMaxSize = titleLabel.fontSize;
                titleLabel.resizeTextMinSize = Mathf.RoundToInt(titleLabel.fontSize * 0.6f);
                titleLabel.verticalOverflow = VerticalWrapMode.Truncate;
                titleLabel.alignment = TextAnchor.UpperLeft;
                SitIn(titleLabel.rectTransform, paper.transform,
                    new Vector2(0.07f, 0.84f),
                    new Vector2(hasPhoto ? 0.66f : 0.93f, 0.96f));
            }

            if (bodyLabel != null)
            {
                bodyLabel.gameObject.SetActive(true);
                ApplyFont(bodyLabel, bodyFace, 17, ink);
                bodyLabel.alignment = TextAnchor.UpperLeft;
                SitIn(bodyLabel.rectTransform, paper.transform,
                    new Vector2(0.07f, 0.12f),
                    new Vector2(hasPhoto ? 0.66f : 0.93f, 0.82f));
                // Si no entra se achica y, si igual no entra, se parte en páginas
                _paged = true;
                _shrinkBody = true;
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
            go.layer = host.gameObject.layer;
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
            go.layer = host.gameObject.layer;
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
                _script = Resources.Load<Font>("Fonts/GiveYouGlory-Regular");
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
            label.fontSize = Mathf.RoundToInt(size * UiScale(label));
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.resizeTextForBestFit = false;
        }

        // Los tamaños están pensados para un Canvas de referencia 800x600 (L1 usa 1920x1080)
        private static float UiScale(Component c)
        {
            var scaler = c.GetComponentInParent<CanvasScaler>(true);
            if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                return 1f;
            Vector2 r = scaler.referenceResolution;
            return Mathf.Lerp(r.x / 800f, r.y / 600f, scaler.matchWidthOrHeight);
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
