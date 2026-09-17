using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Tres carpetas post-password: dos corruptas, una con el registro (código 129).
    public class NotebookFolderUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject folderListRoot;
        [SerializeField] private GameObject noteRoot;
        [SerializeField] private Text noteTitle;
        [SerializeField] private Text noteBody;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string corruptDialogueId = "folder_corrupt";

        [TextArea(6, 14)]
        [SerializeField] private string registroBody =
            "REGISTRO DE OBSERVACIÓN - ORIGEN / 129\n\n" +
            "La prueba presentó irregularidades durante la recuperación.\n" +
            "El sujeto manifestó desorientación y pérdida parcial de memoria.\n" +
            "La reconstrucción no alcanzó los niveles de estabilidad esperados.\n" +
            "Se recomienda suspender nuevas pruebas hasta determinar la causa.\n- M.";

        private bool _open;
        private bool _showingNote;

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Update()
        {
            if (!_open || input == null)
                return;

            if (input.BackPressedThisFrame &&
                (ModalStack.Instance == null || ModalStack.Instance.IsTop(ModalKind.Document)))
            {
                if (_showingNote)
                    ShowFolders();
                else
                    Close();
            }
        }

        public void Open()
        {
            _open = true;
            ShowFolders();
            if (root != null)
                root.SetActive(true);

            ModalStack.Instance?.Push(ModalKind.Document);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var first = folderListRoot != null ? folderListRoot.GetComponentInChildren<Button>(true) : null;
            if (first != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        public void Close()
        {
            if (!_open)
                return;

            _open = false;
            _showingNote = false;
            if (root != null)
                root.SetActive(false);

            ModalStack.Instance?.TryPopSpecific(ModalKind.Document);
            ModalStack.Instance?.ApplyCursorForTop();
        }

        public void OnFolderCorrupt()
        {
            PlayLine(corruptDialogueId, "parece que no puedo acceder...", 2.5f);
        }

        public void OnFolderRegistro()
        {
            _showingNote = true;
            if (folderListRoot != null)
                folderListRoot.SetActive(false);
            if (noteRoot != null)
                noteRoot.SetActive(true);
            if (noteTitle != null)
                noteTitle.text = "REGISTRO DE OBSERVACIÓN";
            if (noteBody != null)
                noteBody.text = registroBody;
            PlayLine("folder_sign", "firma: M...", 2f);
        }

        private void PlayLine(string id, string fallback, float duration)
        {
            if (dialogue != null && dialogue.TryGet(id, out var line))
                subtitles?.Show(line.text, line.duration, line.voice);
            else
                subtitles?.Show(fallback, duration);
        }

        private void ShowFolders()
        {
            _showingNote = false;
            if (folderListRoot != null)
                folderListRoot.SetActive(true);
            if (noteRoot != null)
                noteRoot.SetActive(false);
        }
    }
}
