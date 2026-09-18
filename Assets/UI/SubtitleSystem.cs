using System;
using System.Collections.Generic;
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

        private void ShowNext()
        {
            var (text, duration, voice) = _queue.Dequeue();
            _showing = true;
            _timer = duration;
            if (voice != null && voice.length > _timer)
                _timer = voice.length;

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
