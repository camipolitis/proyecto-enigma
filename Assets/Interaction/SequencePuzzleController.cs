using Enigma.Core;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Controla el orden de los paneles. Si falla, vuelve a cero.
    public class SequencePuzzleController : MonoBehaviour
    {
        [SerializeField] private int[] expected = { 2, 0, 3, 1 };
        [SerializeField] private string successFlagId = "sequence_ok";
        [SerializeField] private GameObject drawerObject;
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string failDialogueId = "seq_fail";

        private int _index;

        public void RegisterPress(int panelIndex, InteractContext context)
        {
            if (GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(successFlagId))
                return;

            if (expected == null || _index >= expected.Length)
                return;

            if (panelIndex == expected[_index])
            {
                _index++;
                if (_index >= expected.Length)
                    Complete();
            }
            else
            {
                _index = 0;
                if (dialogue != null && !string.IsNullOrEmpty(failDialogueId) &&
                    dialogue.TryGet(failDialogueId, out var line))
                    context?.Subtitles?.Show(line.text, line.duration, line.voice);
            }
        }

        private void Complete()
        {
            if (!string.IsNullOrEmpty(successFlagId))
                GameFlagSystem.Instance?.Set(successFlagId, true);
            if (drawerObject != null)
                drawerObject.SetActive(true);
        }
    }
}
