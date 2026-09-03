using UnityEngine;

namespace Enigma.Data
{
    [CreateAssetMenu(fileName = "SO_InventoryItem", menuName = "Enigma/Inventory Item")]
    public class InventoryItem : ScriptableObject
    {
        public string id;
        // Ids para comparar receptáculos 

        public string displayName;
        // Nombre visible en UI.

        public Sprite icon;
        // Icono opcional de la barra de acceso rápido

        [TextArea] public string description;
        // Texto auxiliar... no es un documento de memoria
    }
}
