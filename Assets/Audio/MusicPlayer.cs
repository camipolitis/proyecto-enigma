using UnityEngine;
using UnityEngine.SceneManagement;

namespace Enigma.Audio
{
    // Música en loop: menú, pausa y cada nivel. Clips vacíos = silencio.
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        [SerializeField] private AudioClip menuLoop;
        [SerializeField] private AudioClip pauseLoop;
        [SerializeField] private AudioClip level01Loop;
        [SerializeField] private AudioClip level02Loop;

        private AudioSource _source;
        private bool _pauseMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyForScene(scene.name);
        }

        public void PlayPauseMusic()
        {
            if (_source == null || pauseLoop == null)
                return;

            _pauseMusic = true;
            if (_source.clip == pauseLoop && _source.isPlaying)
                return;

            _source.clip = pauseLoop;
            _source.Play();
        }

        public void RestoreSceneMusic()
        {
            _pauseMusic = false;
            ApplyForScene(SceneManager.GetActiveScene().name);
        }

        private void ApplyForScene(string sceneName)
        {
            if (_source == null || _pauseMusic)
                return;

            AudioClip clip = menuLoop;
            if (sceneName == "Level01_Room")
                clip = level01Loop;
            else if (sceneName == "Level02_Lab")
                clip = level02Loop;
            else if (sceneName != "MainMenu")
                clip = level01Loop;

            if (clip == null)
            {
                _source.Stop();
                _source.clip = null;
                return;
            }

            if (_source.clip == clip && _source.isPlaying)
                return;

            _source.clip = clip;
            _source.Play();
        }
    }
}
