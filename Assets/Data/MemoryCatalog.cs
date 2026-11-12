using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Data
{
    // Todas las memorias del juego: la partida guardada solo tiene ids y de acá salen los documentos
    [CreateAssetMenu(fileName = "SO_MemoryCatalog", menuName = "Enigma/Memory Catalog")]
    public class MemoryCatalog : ScriptableObject
    {
        public List<DocumentData> memories = new List<DocumentData>();

        public DocumentData Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            return memories.Find(d => d != null && d.id == id);
        }
    }
}
