using Enigma.Core;
using Enigma.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Enigma.Audio
{
    // MusicDuck baja el padre Musica; el fader Musica (MusicaNivel) lo mezcla A.
    [DefaultExecutionOrder(-50)]
    public class AudioStateDriver : MonoBehaviour
    {
        public static AudioStateDriver Instance { get; private set; }

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;
        [SerializeField] private string musicDuckParam = "MusicDuck";
        [SerializeField] private string tensionLowpassParam = "TensionLowpass";
        [SerializeField] private float duckDb = -12f;
        [SerializeField] private float openCutoff = 22000f;
        [SerializeField] private float muffledCutoff = 900f;
        [SerializeField] private float lerpSpeed = 6f;

        private float _duck;
        private float _cutoff;

        public AudioMixerGroup MusicGroup => musicGroup;
        public AudioMixerGroup SfxGroup => sfxGroup;
        public AudioMixerGroup AmbientGroup => ambientGroup;
        public AudioMixerGroup VoiceGroup => voiceGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _duck = 0f;
            _cutoff = openCutoff;
            Apply();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            float duckTarget = VoicePlaying() ? duckDb : 0f;
            float cutTarget = WantsMuffle() ? muffledCutoff : openCutoff;
            float t = 1f - Mathf.Exp(-lerpSpeed * Time.unscaledDeltaTime);
            _duck = Mathf.Lerp(_duck, duckTarget, t);
            _cutoff = Mathf.Lerp(_cutoff, cutTarget, t);
            Apply();
        }

        private static bool VoicePlaying()
        {
            var subs = SubtitleSystem.Instance;
            return subs != null && subs.IsVoicePlaying;
        }

        private static bool WantsMuffle()
        {
            string scene = SceneManager.GetActiveScene().name;
            var flags = GameFlagSystem.Instance;

            if (scene == "Level01_Room")
                return flags == null || !flags.Get("fuse_installed");
            if (scene == "Level02_Lab")
                return flags == null || !flags.Get("generator_on");
            if (scene == "Level03_Archives" || scene == "Level03_Mezzanine")
                return flags == null || !flags.Get("power_restored");
            if (scene == "Level04_Origen")
                return true;

            return false;
        }

        private void Apply()
        {
            if (mixer == null)
                return;
            mixer.SetFloat(musicDuckParam, _duck);
            mixer.SetFloat(tensionLowpassParam, _cutoff);
        }
    }
}
