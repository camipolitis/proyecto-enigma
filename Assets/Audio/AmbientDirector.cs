using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Enigma.Audio
{
    // Loops de sala (2D). Las fuentes 3D viven en cada escena con Output en Ambiente; acá solo se pausan.
    [DefaultExecutionOrder(-40)]
    public class AmbientDirector : MonoBehaviour
    {
        public static AmbientDirector Instance { get; private set; }

        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioClip interiorLoop;
        [SerializeField] private AudioClip pitLoop;
        // Cada loop tiene su nivel: el interior viene mucho más fuerte que el del pozo
        [SerializeField] private float interiorVolume = 0.11f;
        [SerializeField] private float pitVolume = 0.6f;

        private AudioSource _bed;
        private readonly List<AudioSource> _live = new List<AudioSource>();
        private bool _paused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _bed = MakeBed();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            Apply(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
                Instance = null;
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
            if (!paused)
            {
                if (_bed != null)
                    _bed.UnPause();
                for (int i = 0; i < _live.Count; i++)
                {
                    if (_live[i] != null)
                        _live[i].UnPause();
                }
                return;
            }

            if (_bed != null)
                _bed.Pause();

            for (int i = 0; i < _live.Count; i++)
            {
                if (_live[i] != null)
                    _live[i].Pause();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
                _paused = false;
            Apply(scene);
        }

        private void Apply(Scene scene)
        {
            AudioClip bed;
            float volume = interiorVolume;
            if (scene.name == "Level04_Origen")
            {
                bed = pitLoop;
                volume = pitVolume;
            }
            else if (scene.name == "MainMenu")
                bed = null;
            else
                bed = interiorLoop;

            PlayBed(bed, volume);
            CollectPoints(scene);

            if (!_paused)
                return;
            if (_bed != null)
                _bed.Pause();
            for (int i = 0; i < _live.Count; i++)
            {
                if (_live[i] != null)
                    _live[i].Pause();
            }
        }

        private void PlayBed(AudioClip clip, float volume)
        {
            if (_bed == null)
                return;

            if (clip == null)
            {
                _bed.Stop();
                _bed.clip = null;
                return;
            }

            _bed.volume = volume;
            if (_bed.clip == clip)
            {
                if (_paused)
                    _bed.Pause();
                else
                    _bed.UnPause();
                return;
            }

            _bed.clip = clip;
            _bed.Play();
        }

        private void CollectPoints(Scene scene)
        {
            _live.Clear();
            if (!scene.IsValid() || ambientGroup == null)
                return;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var src in root.GetComponentsInChildren<AudioSource>(true))
                {
                    if (src.outputAudioMixerGroup == ambientGroup)
                        _live.Add(src);
                }
            }
        }

        private AudioSource MakeBed()
        {
            var go = new GameObject("AmbBed");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = ambientGroup;
            src.dopplerLevel = 0f;
            return src;
        }
    }
}
