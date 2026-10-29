using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Enigma.Audio
{
    // Base: un loop por nivel. Tensión y sting: los mismos en todos. Q vuelve con fade.
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        [SerializeField] private AudioClip menuLoop;
        [SerializeField] private AudioClip pauseLoop;
        [SerializeField] private AudioClip level01Loop;
        [SerializeField] private AudioClip level02Loop;
        [SerializeField] private AudioClip level03Loop;
        [SerializeField] private AudioClip level04Loop;
        [SerializeField] private AudioClip tensionLoop;
        [SerializeField] private AudioClip resolveSting;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private float crossfadeSeconds = 0.6f;
        [SerializeField] private float resolveFadeSeconds = 1.2f;

        private AudioSource _base;
        private AudioSource _tension;
        private AudioSource _pause;
        private AudioSource _sting;
        private bool _pauseMusic;
        private bool _wantTension;
        private bool _resolve;
        private float _baseVol = 1f;
        private float _tensionVol;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _base = MakeSource("MusicBase", true);
            _tension = MakeSource("MusicTension", true);
            _pause = MakeSource("MusicPause", true);
            _sting = MakeSource("MusicSting", false);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            ApplyForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (_pauseMusic)
                return;

            float targetTension;
            float targetBase;
            if (_resolve)
            {
                targetTension = 0f;
                targetBase = 0f;
            }
            else
            {
                targetTension = _wantTension && tensionLoop != null ? 1f : 0f;
                targetBase = 1f - targetTension;
            }

            float dur = _resolve ? resolveFadeSeconds : crossfadeSeconds;
            float speed = dur <= 0.01f ? 100f : 1f / dur;
            float step = speed * Time.unscaledDeltaTime;
            _baseVol = Mathf.MoveTowards(_baseVol, targetBase, step);
            _tensionVol = Mathf.MoveTowards(_tensionVol, targetTension, step);

            if (_base != null)
            {
                _base.volume = _baseVol;
                if (_resolve && _baseVol <= 0.001f && _base.isPlaying)
                    _base.Stop();
            }

            if (_tension != null)
            {
                _tension.volume = _tensionVol;
                if (_tensionVol <= 0.001f && !_wantTension && _tension.isPlaying)
                    _tension.Stop();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _wantTension = false;
            _resolve = false;
            _baseVol = 1f;
            _tensionVol = 0f;
            if (scene.name == "MainMenu")
                _pauseMusic = false;
            if (!_pauseMusic && _pause != null)
                _pause.Stop();
            ApplyForScene(scene.name);
        }

        public void PlayPauseMusic()
        {
            _pauseMusic = true;
            MuteLoops();
            PlayLoop(_pause, pauseLoop, 1f);
        }

        public void RestoreSceneMusic()
        {
            _pauseMusic = false;
            if (_pause != null)
                _pause.Stop();
            ApplyForScene(SceneManager.GetActiveScene().name);
        }

        public void EnterTension()
        {
            if (tensionLoop == null)
                return;
            _wantTension = true;
            _resolve = false;
            PlayLoop(_tension, tensionLoop, _tensionVol);
        }

        public void ExitTension()
        {
            _wantTension = false;
        }

        public void PlayResolve()
        {
            _wantTension = false;
            _resolve = true;
            if (_sting == null || resolveSting == null)
                return;
            _sting.PlayOneShot(resolveSting);
        }

        private void ApplyForScene(string sceneName)
        {
            if (_pauseMusic)
                return;

            AudioClip clip = menuLoop;
            if (sceneName == "Level01_Room")
                clip = level01Loop;
            else if (sceneName == "Level02_Lab")
                clip = level02Loop;
            else if (sceneName == "Level03_Archives" || sceneName == "Level03_Mezzanine")
                clip = level03Loop;
            else if (sceneName == "Level04_Origen")
                clip = level04Loop;
            else if (sceneName != "MainMenu")
                clip = null;

            if (clip == null)
            {
                if (_base != null)
                {
                    _base.Stop();
                    _base.clip = null;
                }
                return;
            }

            PlayLoop(_base, clip, _baseVol);
            if (_wantTension)
                PlayLoop(_tension, tensionLoop, _tensionVol);
            else if (_tension != null && _tension.isPlaying && _tensionVol < 0.02f)
                _tension.Stop();
        }

        private void PlayLoop(AudioSource src, AudioClip clip, float volume)
        {
            if (src == null || clip == null)
                return;
            src.volume = volume;
            if (src.clip == clip && src.isPlaying)
                return;
            src.clip = clip;
            src.Play();
        }

        private void MuteLoops()
        {
            if (_base != null)
                _base.Stop();
            if (_tension != null)
                _tension.Stop();
        }

        private AudioSource MakeSource(string objectName, bool loop)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = musicGroup;
            return src;
        }
    }
}
