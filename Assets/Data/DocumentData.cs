using UnityEngine;

namespace Enigma.Data
{
    [CreateAssetMenu(fileName = "SO_Document", menuName = "Enigma/Document Data")]
    public class DocumentData : ScriptableObject
    {
        public string id;
        // Id para MemoryJournal (evita duplicar la misma nota)

        public string title;
        // Título del panel de lectura.

        [TextArea(8, 20)] public string body;
        // Contenido completo de la nota.

        public string releaseSubtitle = "M...";
        // Línea al soltar con Q (voz del PJ).

        public string memoryCollectedMessage = "Memoria recolectada";
        // Toast la primera vez que se guarda en el journal.
    }
}
