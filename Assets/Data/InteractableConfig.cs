using UnityEngine;

namespace Enigma.Data
{
    [CreateAssetMenu(fileName = "SO_InteractableConfig", menuName = "Enigma/Interactable Config")]
    public class InteractableConfig : ScriptableObject
    {
        public string prompt = "Interactuar";
        // Texto base del prompt E

        public string requiredFlagId;
        // Flag que debe estar true para Success (vacío = sin requisito de flag)

        public string successFlagId;
        // Flag que se setea al Success.

        public InventoryItem requiredItem;
        // Ítem que debe estar seleccionado en el inventario (si aplica)

        public string failDialogueId;
        public string successDialogueId;
        // Ids dentro de un DialogueLineSet asignado en el interactable

        public bool requiresZoom;
        public bool allowsInventoryWhileZoom;
        // Solo true en receptáculos que necesitan elegir un ítem.
    }
}
