using UnityEngine;

namespace Enigma.Interaction
{
    // Radio vieja: con E se prende, queda en estática y suelta una frase cortada.
    public class RadioInteractable : InteractableBase
    {
        [SerializeField] private AudioClip staticLoop;
        [SerializeField] private float staticVolume = 0.35f;
        [SerializeField] private Light dialLight;

        private AudioSource _source;
        private bool _on;

        private void Awake()
        {
            if (dialLight != null)
                dialLight.enabled = false;
        }

        protected override bool IsCompleted() => _on || base.IsCompleted();

        protected override void HandleSuccessExtra(InteractContext context)
        {
            _on = true;
            if (dialLight != null)
                dialLight.enabled = true;
            if (staticLoop == null)
                return;
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.loop = true;
                _source.spatialBlend = 1f;
                _source.minDistance = 1f;
                _source.maxDistance = 9f;
                var sfx = Enigma.Audio.SfxPlayer.Instance;
                if (sfx != null)
                    _source.outputAudioMixerGroup = sfx.SfxGroup;
            }
            _source.clip = staticLoop;
            _source.volume = staticVolume;
            _source.Play();
        }
    }
}
