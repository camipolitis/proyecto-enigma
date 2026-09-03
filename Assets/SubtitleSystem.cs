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

        public event Action<string> OnLinePlayed;
        // Hook futuro para audio de voz.

        private readonly Queue<(string text, float duration)> _queue = new Queue<(string, float)>();
        private float _timer;
        private bool _showing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (root != null)
                root.SetActive(false);
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

        public void Show(string text, float duration = 2.5f)
        {
            if (string.IsNullOrEmpty(text))
                return;
            _queue.Enqueue((text, duration));
        }

        private void ShowNext()
        {
            var (text, duration) = _queue.Dequeue();
            _showing = true;
            _timer = duration;

            if (root != null)
                root.SetActive(true);
            if (label != null)
                label.text = text;

            OnLinePlayed?.Invoke(text);
        }
    }
}
