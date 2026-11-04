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
        [Header("Orden aleatorio")]
        [SerializeField] private bool randomizePerSession = true;
        // Signo de la diapositiva de cada panel (índice = panelIndex)
        [SerializeField] private Transform[] slideGlyphs;

        // Mismo orden durante toda la partida, aunque se recargue la escena
        private static int[] _sessionOrder;

        private int _index;

        public int[] Expected => expected;
        public string SuccessFlagId => successFlagId;
        // Cada vez que cambia el avance (acierto, error o fin)
        public event System.Action Changed;

        // Encendido: ya pulsado en la racha actual, o la secuencia resuelta
        public bool IsLit(int panelIndex)
        {
            if (!string.IsNullOrEmpty(successFlagId) && GameFlagSystem.Instance != null &&
                GameFlagSystem.Instance.Get(successFlagId))
                return true;
            if (expected == null)
                return false;
            for (int k = 0; k < _index && k < expected.Length; k++)
            {
                if (expected[k] == panelIndex)
                    return true;
            }
            return false;
        }

        private void Awake()
        {
            if (!randomizePerSession || expected == null || expected.Length < 2)
                return;
            if (_sessionOrder == null || _sessionOrder.Length != expected.Length)
                _sessionOrder = Shuffled(expected.Length);
            expected = (int[])_sessionOrder.Clone();
            ArrangeGlyphs();
        }

        private static int[] Shuffled(int count)
        {
            var order = new int[count];
            for (int i = 0; i < count; i++)
                order[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            return order;
        }

        // La diapositiva muestra los signos de izquierda a derecha en el orden esperado
        private void ArrangeGlyphs()
        {
            if (slideGlyphs == null || slideGlyphs.Length != expected.Length)
                return;
            var slots = new Vector3[slideGlyphs.Length];
            for (int i = 0; i < slideGlyphs.Length; i++)
            {
                if (slideGlyphs[i] == null)
                    return;
                slots[i] = slideGlyphs[i].localPosition;
            }
            System.Array.Sort(slots, (a, b) => a.x.CompareTo(b.x));
            for (int k = 0; k < expected.Length; k++)
                slideGlyphs[expected[k]].localPosition = slots[k];
        }

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
            Changed?.Invoke();
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
