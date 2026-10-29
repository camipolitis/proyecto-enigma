using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Enigma.Audio
{
    [Serializable]
    public class AmbientPointSpec
    {
        public string id;
        public string sceneName;
        public Vector3 worldPosition;
        public AudioClip clip;
        public float minDistance = 1.5f;
        public float maxDistance = 14f;
        public float volume = 0.35f;
    }

    // Loops de sala (2D) y puntos 3D. Clip vacío = silencio (holder).
    [DefaultExecutionOrder(-40)]
    public class AmbientDirector : MonoBehaviour
    {
        public static AmbientDirector Instance { get; private set; }

        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioClip interiorLoop;
        [SerializeField] private AudioClip pitLoop;
        [SerializeField] private float bedVolume = 0.22f;
        [SerializeField] private AmbientPointSpec[] points;

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
            _bed = MakeSource("AmbBed", 0f, true);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            Apply(SceneManager.GetActiveScene().name);
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
            Apply(scene.name);
        }

        private void Apply(string sceneName)
        {
            ClearPoints();

            AudioClip bed = pitLoop;
            if (sceneName == "Level04_Origen")
                bed = pitLoop;
            else if (sceneName == "MainMenu")
                bed = null;
            else
                bed = interiorLoop;

            PlayBed(bed);
            SpawnPoints(sceneName);

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

        private void PlayBed(AudioClip clip)
        {
            if (_bed == null)
                return;

            if (clip == null)
            {
                _bed.Stop();
                _bed.clip = null;
                return;
            }

            _bed.volume = bedVolume;
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

        private void SpawnPoints(string sceneName)
        {
            if (points == null)
                return;

            for (int i = 0; i < points.Length; i++)
            {
                var spec = points[i];
                if (spec == null || spec.clip == null)
                    continue;
                if (spec.sceneName != sceneName)
                    continue;

                var src = MakeSource(
                    string.IsNullOrEmpty(spec.id) ? "AmbPoint" : spec.id,
                    1f,
                    true);
                src.transform.position = spec.worldPosition;
                src.minDistance = spec.minDistance;
                src.maxDistance = spec.maxDistance;
                src.rolloffMode = AudioRolloffMode.Logarithmic;
                src.volume = spec.volume;
                src.clip = spec.clip;
                src.Play();
                _live.Add(src);
            }
        }

        private void ClearPoints()
        {
            for (int i = 0; i < _live.Count; i++)
            {
                if (_live[i] != null)
                    Destroy(_live[i].gameObject);
            }
            _live.Clear();
        }

        private AudioSource MakeSource(string objectName, float spatial, bool loop)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = spatial;
            src.outputAudioMixerGroup = ambientGroup;
            src.dopplerLevel = 0f;
            return src;
        }
    }
}
