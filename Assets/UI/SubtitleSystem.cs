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

        public event Action<string> OnLinePlayed;

        private readonly Queue<(string text, float duration, AudioClip voice)> _queue = new Queue<(string, float, AudioClip)>();
        private float _timer;
        private bool _showing;

        private void Awake()
        {
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

        private static float HotbarTop(RectTransform strip)
        {
            float fallback = 148f;
            if (strip == null || strip.parent == null)
                return fallback;
            var bar = strip.parent.Find("InventoryHotBar") as RectTransform;
            if (bar == null)
                return fallback;
            float h = bar.rect.height * Mathf.Abs(bar.localScale.y);
            return bar.anchoredPosition.y + h;
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
            if (string.IsNullOrEmpty(text))
                return;
            _queue.Enqueue((text, duration, voice));
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

            BringToFront();
            if (root != null)
                root.SetActive(true);
            if (label != null)
                label.text = text;

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
