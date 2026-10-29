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
    }

    [CreateAssetMenu(fileName = "SO_SfxBank", menuName = "Enigma/Sfx Bank")]
    public class SfxBank : ScriptableObject
    {
        public SfxEntry[] entries;
    }
}
