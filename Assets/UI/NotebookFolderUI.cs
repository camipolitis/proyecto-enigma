using Enigma.Core;
using Enigma.Data;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Tres carpetas post-password: dos corruptas, una con el registro de la prueba.
    public class NotebookFolderUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject folderListRoot;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string corruptDialogueId = "folder_corrupt";
        [SerializeField] private string signDialogueId = "folder_sign";
        [SerializeField] private DocumentReaderUI reader;
        [SerializeField] private DocumentData record;

        private bool _readingRecord;

        protected override ModalKind Kind => ModalKind.Notebook;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private void Start()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (ModalStack.Instance != null && !ModalStack.Instance.IsTop(Kind))
                return;

            // Al cerrar el lector vuelve la lista de carpetas
            if (root != null && !root.activeSelf)
            {
                ShowFolders();
                if (_readingRecord)
                    PlayLine(signDialogueId);
                _readingRecord = false;
                return;
            }

            TryCloseWithBack();
        }

        public void Open()
        {
            ShowFolders();
            Enigma.Audio.SfxPlayer.Instance?.Play("notebook");
            OpenModal();

            var first = folderListRoot != null ? folderListRoot.GetComponentInChildren<Button>(true) : null;
            if (first != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        public void OnFolderCorrupt()
        {
            PlayLine(corruptDialogueId);
        }

        public void OnFolderRegistro()
        {
            if (reader == null || record == null)
                return;
            _readingRecord = true;
            if (root != null)
                root.SetActive(false);
            reader.Open(record, null, true);
        }

        private void PlayLine(string id)
        {
            if (dialogue != null && dialogue.TryGet(id, out var line))
                subtitles?.Show(line.text, line.duration, line.voice);
        }

        private void ShowFolders()
        {
            if (root != null)
                root.SetActive(true);
            if (folderListRoot != null)
                folderListRoot.SetActive(true);
        }
    }
}
