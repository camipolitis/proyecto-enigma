using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Enigma.Audio
{
    [Serializable]
    public class SfxEntry
    {
        public string id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float spatialBlend;
        public AudioMixerGroup mixerGroup;
        // Empareja clips grabados a niveles distintos; el grupo SFX da el nivel general
        [Range(0f, 1f)] public float volume = 1f;
    }

    [CreateAssetMenu(fileName = "SO_SfxBank", menuName = "Enigma/Sfx Bank")]
    public class SfxBank : ScriptableObject
    {
        public SfxEntry[] entries;
    }
}
