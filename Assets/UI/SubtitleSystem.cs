using System;
using System.Collections.Generic;
using Enigma.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Cola de subtítulos del protagonista.
    public class SubtitleSystem : MonoBehaviour
    {
        public static SubtitleSystem Instance { get; private set; }

        [SerializeField] private GameObject root;
        [SerializeField] private Text label;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private float repeatCooldown = 4f;
        // Segundos desde que termina una línea hasta que la misma puede volver a sonar

        // Las líneas sin voz son pensamientos: cursiva y este color
        [SerializeField] private Color thoughtColor = new Color(1f, 0.84f, 0.32f, 1f);

        public Text Label => label;

        public event Action<string> OnLinePlayed;

        private readonly Queue<(string text, float duration, AudioClip voice)> _queue = new Queue<(string, float, AudioClip)>();
        private readonly Dictionary<string, float> _readyAt = new Dictionary<string, float>();
        private float _timer;
        private bool _showing;
        private string _currentText;
        private Color _spokenColor = Color.white;
        private FontStyle _spokenStyle = FontStyle.Normal;

        private void Awake()
        {
            if (label != null)
            {
                _spokenColor = label.color;
                _spokenStyle = label.fontStyle;
            }
            if (Instance != null && Instance != this &&
                Instance.gameObject.scene == gameObject.scene)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (root != null)
                root.SetActive(false);
            if (voiceSource == null)
                voiceSource = GetComponent<AudioSource>();

            RouteVoice();
            BringToFront();
        }

        private void Start()
        {
            RouteVoice();
            BringToFront();
        }

        // Tira por encima del hotbar, no debajo.
        private void BringToFront()
        {
            if (root == null)
                return;

            var img = root.GetComponent<Image>();
            if (img != null)
                img.enabled = false;

            var rt = root.GetComponent<RectTransform>();
            RectTransform strip = rt;
            var parent = root.transform.parent as RectTransform;
            if (parent != null && parent.name == "SubtitleSystemHost")
            {
                var hostImg = parent.GetComponent<Image>();
                if (hostImg != null)
                    hostImg.enabled = false;
                strip = parent;
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.localScale = Vector3.one;
                }
            }

            if (strip != null)
            {
                strip.SetAsLastSibling();
                strip.anchorMin = new Vector2(0.08f, 0f);
                strip.anchorMax = new Vector2(0.92f, 0f);
                strip.pivot = new Vector2(0.5f, 0f);
                strip.anchoredPosition = new Vector2(0f, HotbarTop(strip) + 18f);
                strip.sizeDelta = new Vector2(0f, 96f);
                strip.localScale = Vector3.one;
            }

            var canvas = root.GetComponent<Canvas>();
            if (canvas == null)
                canvas = root.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5000;

            if (label == null)
                return;
            label.raycastTarget = false;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = label.GetComponent<Outline>();
            if (outline == null)
                outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.88f);
            outline.effectDistance = new Vector2(1.4f, -1.4f);
            var lrt = label.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            lrt.pivot = new Vector2(0.5f, 0.5f);
        }

        // Borde superior del hotbar medido desde el piso del contenedor del subtítulo
        private float HotbarTop(RectTransform strip)
        {
            float fallback = 148f;
            var parent = strip != null ? strip.parent as RectTransform : null;
            if (parent == null)
                return fallback;
            RectTransform bar = null;
            foreach (var ui in FindObjectsByType<Enigma.Inventory.InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (ui.gameObject.scene == gameObject.scene)
                    bar = ui.HotbarRect;
            if (bar == null)
                return fallback;
            var corners = new Vector3[4];
            bar.GetWorldCorners(corners);
            return parent.InverseTransformPoint(corners[1]).y - parent.rect.yMin;
        }

        public bool IsVoicePlaying => voiceSource != null && voiceSource.isPlaying;

        private void RouteVoice()
        {
            if (voiceSource == null || voiceSource.outputAudioMixerGroup != null)
                return;
            var buses = AudioStateDriver.Instance;
            if (buses != null)
                voiceSource.outputAudioMixerGroup = buses.VoiceGroup;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (!_showing)
            {
                if (_queue.Count > 0)
                    ShowNext();
                return;
            }

            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0f)
            {
                _showing = false;
                if (root != null)
                    root.SetActive(false);

                if (_queue.Count > 0)
                    ShowNext();
            }
        }

        public void Show(string text, float duration = 2.5f, AudioClip voice = null)
        {
            if (string.IsNullOrEmpty(text) || IsCoolingDown(text))
                return;
            _queue.Enqueue((text, duration, voice));
        }

        // Solo frena la repetición de la misma línea; líneas distintas pasan siempre
        private bool IsCoolingDown(string text)
        {
            if (_showing && text == _currentText)
                return true;
            foreach (var queued in _queue)
                if (queued.text == text)
                    return true;
            return _readyAt.TryGetValue(text, out float ready) && Time.unscaledTime < ready;
        }

        private void PruneCooldowns()
        {
            float now = Time.unscaledTime;
            var expired = new List<string>();
            foreach (var kv in _readyAt)
                if (kv.Value <= now)
                    expired.Add(kv.Key);
            foreach (var key in expired)
                _readyAt.Remove(key);
        }

        public void PlayClip(AudioClip clip)
        {
            if (clip == null || voiceSource == null)
                return;
            voiceSource.PlayOneShot(clip);
        }

        public void StopVoice()
        {
            if (voiceSource != null)
                voiceSource.Stop();
        }

        private void ShowNext()
        {
            var (text, duration, voice) = _queue.Dequeue();
            _showing = true;
            _timer = duration;
            if (voice != null && voice.length > _timer)
                _timer = voice.length;
            _currentText = text;
            if (_readyAt.Count > 64)
                PruneCooldowns();
            _readyAt[text] = Time.unscaledTime + _timer + repeatCooldown;

            BringToFront();
            if (root != null)
                root.SetActive(true);
            if (label != null)
            {
                label.text = text;
                label.fontStyle = voice == null ? FontStyle.Italic : _spokenStyle;
                label.color = voice == null ? thoughtColor : _spokenColor;
            }

            if (voiceSource != null)
            {
                voiceSource.Stop();
                if (voice != null)
                    voiceSource.PlayOneShot(voice);
            }

            OnLinePlayed?.Invoke(text);
        }
    }
}
