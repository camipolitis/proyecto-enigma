using UnityEngine;

namespace Enigma.Data
{
    [CreateAssetMenu(fileName = "SO_IntroConfig", menuName = "Enigma/Intro Sequence Config")]
    public class IntroSequenceConfig : ScriptableObject
    {
        [TextArea] public string openingSubtitle = "¿Dónde estoy? ...¿Qué pasó?";
        public float openingSubtitleDuration = 3f;
        // (Tiempo que se muestra la frase antes del prompt de levantarse)

        public string standPrompt = "Presioná E para levantarte";
        // Texto de UI hasta que el jugador confirma.
    }
}
