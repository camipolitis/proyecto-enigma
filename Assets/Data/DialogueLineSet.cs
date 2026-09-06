using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Data
{
    [Serializable]
    public class DialogueLine
    {
        public string id;
        [TextArea] public string text;
        public float duration = 2.5f;
        // Duración del subtítulo.
    }

    [CreateAssetMenu(fileName = "SO_DialogueLineSet", menuName = "Enigma/Dialogue Line Set")]
    public class DialogueLineSet : ScriptableObject
    {
        public List<DialogueLine> lines = new List<DialogueLine>();
        // Todas las frases del nivel editables 

        public bool TryGet(string id, out DialogueLine line)
        {
            line = lines.Find(l => l != null && l.id == id);
            return line != null;
        }

        public string GetText(string id, string fallback = "")
        {
            return TryGet(id, out var line) ? line.text : fallback;
        }
    }
}
