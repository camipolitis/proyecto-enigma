using Enigma.Data;
using Enigma.Inventory;
using Enigma.Memory;
using Enigma.UI;
using UnityEngine;

namespace Enigma.Interaction
{
    // Contexto que reciben los interactables al intentar Interact.
    public class InteractContext
    {
        public GameObject Actor;
        public InventorySystem Inventory;
        public MemoryJournal Memory;
        public SubtitleSystem Subtitles;
        public DialogueLineSet Dialogue;
        public InventoryItem SelectedItem;
    
    }

    public interface IInteractable
    {
        string GetPrompt(InteractContext context);
        bool CanAttemptInteract(InteractContext context);
        void Interact(InteractContext context);
        bool RequiresZoom { get; }
        bool AllowsInventoryWhileZoom { get; }
        Transform ZoomAnchor { get; }
    }
}
