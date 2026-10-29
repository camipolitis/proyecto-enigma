using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Enigma.Audio
{
    // One-shots desde el banco. Voces hijas: el 3D no mueve al MusicPlayer.
    [DefaultExecutionOrder(-50)]
    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer Instance { get; private set; }

        [SerializeField] private SfxBank bank;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private int voices = 8;

        private readonly Dictionary<string, SfxEntry> _byId = new Dictionary<string, SfxEntry>();
        private readonly Dictionary<string, int> _lastIndex = new Dictionary<string, int>();
        private AudioSource[] _pool;
        private int _next;

        public AudioMixerGroup SfxGroup => sfxGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            RebuildLookup();
            BuildPool();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Play(string id)
        {
            Play(id, Vector3.zero, false);
        }

        public void Play(string id, Vector3 worldPosition)
        {
            Play(id, worldPosition, true);
        }

        private void Play(string id, Vector3 worldPosition, bool world)
        {
            if (string.IsNullOrEmpty(id) || _pool == null)
                return;

            if (!_byId.TryGetValue(id, out var entry) || entry.clips == null)
                return;

            AudioClip clip = Pick(id, entry.clips);
            if (clip == null)
                return;

            AudioSource src = _pool[_next];
            _next = (_next + 1) % _pool.Length;

            src.outputAudioMixerGroup = entry.mixerGroup != null ? entry.mixerGroup : sfxGroup;
            src.spatialBlend = entry.spatialBlend;
            if (world && entry.spatialBlend > 0.01f)
                src.transform.position = worldPosition;
            else
                src.transform.localPosition = Vector3.zero;

            src.pitch = entry.clips != null && CountClips(entry.clips) == 1
                ? Random.Range(0.96f, 1.04f)
                : 1f;
            src.PlayOneShot(clip);
        }

        private AudioClip Pick(string id, AudioClip[] clips)
        {
            int n = CountClips(clips);

            if (n == 0)
                return null;

            int last = _lastIndex.TryGetValue(id, out var prev) ? prev : -1;
            int chosen = 0;
            int guard = clips.Length * 2;
            do
            {
                chosen = Random.Range(0, clips.Length);
                guard--;
            }
            while ((clips[chosen] == null || (n > 1 && chosen == last)) && guard > 0);

            if (clips[chosen] == null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null)
                    {
                        chosen = i;
                        break;
                    }
                }
            }

            _lastIndex[id] = chosen;
            return clips[chosen];
        }

        private static int CountClips(AudioClip[] clips)
        {
            int n = 0;
            if (clips == null)
                return 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                    n++;
            }
            return n;
        }

        private void RebuildLookup()
        {
            _byId.Clear();
            if (bank == null || bank.entries == null)
                return;

            for (int i = 0; i < bank.entries.Length; i++)
            {
                var e = bank.entries[i];
                if (e == null || string.IsNullOrEmpty(e.id))
                    continue;
                _byId[e.id] = e;
            }
        }

        private void BuildPool()
        {
            int count = Mathf.Max(2, voices);
            _pool = new AudioSource[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("SfxVoice");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = 0f;
                src.outputAudioMixerGroup = sfxGroup;
                _pool[i] = src;
            }
        }
    }
}
