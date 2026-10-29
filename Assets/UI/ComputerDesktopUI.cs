using Enigma.Core;
using Enigma.Data;
using Enigma.Inventory;
using Enigma.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Escritorio del PC: carpetas, impresión al piso y restauración con clave.
    public class ComputerDesktopUI : ModalUIBase
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject folderListRoot;
        [SerializeField] private GameObject restoreRoot;
        [SerializeField] private GameObject docsRoot;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string corruptDialogueId = "folder_corrupt";
        [SerializeField] private CodeEntryUI adminCodeUi;
        [SerializeField] private DocumentReaderUI readerUi;
        [SerializeField] private DocumentData origenDoc;
        [SerializeField] private DocumentData subjectsDoc;
        [SerializeField] private DocumentData staffDoc;
        [SerializeField] private DocumentData paraMDoc;
        [SerializeField] private string adminFlagId = "admin_restored";
        [SerializeField] private string paraMFlagId = "para_m_read";
        [SerializeField] private GameObject sheetOrigen;
        [SerializeField] private GameObject sheetSubjects;
        [SerializeField] private GameObject sheetStaff;
        [SerializeField] private string printFlagOrigen = "print_origen";
        [SerializeField] private string printFlagSubjects = "print_subjects";
        [SerializeField] private string printFlagStaff = "print_staff";
        [SerializeField] private string printDialogueId = "print_sent";

        private Button _firstFolder;
        private Transform _restoreFolder;
        private Transform _docOrigenBtn;
        private Transform _docSubjectsBtn;
        private Transform _docStaffBtn;
        private Transform _docParaMBtn;

        protected override ModalKind Kind => ModalKind.Computer;
        protected override GameObject ModalRoot => root;
        protected override PlayerInputHandler Input => input;

        private void OnEnable()
        {
            BindFolderRefs();
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
        }

        private void Start()
        {
            BindFolderRefs();
            if (GameFlagSystem.Instance != null)
            {
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
                GameFlagSystem.Instance.OnFlagChanged += OnFlag;
            }
        }

        private void OnDisable()
        {
            if (GameFlagSystem.Instance != null)
                GameFlagSystem.Instance.OnFlagChanged -= OnFlag;
        }

        private void BindFolderRefs()
        {
            if (folderListRoot != null && _restoreFolder == null)
                _restoreFolder = folderListRoot.transform.Find("Btn_Sistema");
            if (docsRoot == null)
                return;
            if (_docOrigenBtn == null)
                _docOrigenBtn = docsRoot.transform.Find("Btn_Origen");
            if (_docSubjectsBtn == null)
                _docSubjectsBtn = docsRoot.transform.Find("Btn_SujetosDoc");
            if (_docStaffBtn == null)
                _docStaffBtn = docsRoot.transform.Find("Btn_PersonalDoc");
            if (_docParaMBtn == null)
                _docParaMBtn = docsRoot.transform.Find("Btn_ParaM");
        }

        private void OnFlag(string id, bool value)
        {
            if (!value)
                return;

            if (!string.IsNullOrEmpty(adminFlagId) && id == adminFlagId)
            {
                ConsumePrintSheets();
                ShowDocs();
                return;
            }

            if (id == printFlagOrigen || id == printFlagSubjects || id == printFlagStaff)
                RefreshRestoreFolder();
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (ModalStack.Instance != null && !ModalStack.Instance.IsTop(Kind))
                return;

            // Al volver del pad o del lector, se reabre el escritorio.
            if (root != null && !root.activeSelf)
            {
                if (GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(adminFlagId))
                    ShowDocs();
                else
                    ShowFolders();
                return;
            }

            if (input == null || !input.TryConsumeBack())
                return;

            if (restoreRoot != null && restoreRoot.activeSelf)
                ShowFolders();
            else
                Close();
        }

        public void Open()
        {
            if (root == null)
                return;

            BindFolderRefs();

            if (GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(adminFlagId))
                ShowDocs();
            else
                ShowFolders();

            if (folderListRoot != null)
                _firstFolder = folderListRoot.GetComponentInChildren<Button>(true);

            OpenModal();

            if (_firstFolder != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_firstFolder.gameObject);
        }

        public void OnFolderCorrupt()
        {
            if (dialogue != null && dialogue.TryGet(corruptDialogueId, out var line))
                subtitles?.Show(line.text, line.duration, line.voice);
        }

        public void OnFolderRestore()
        {
            if (folderListRoot != null)
                folderListRoot.SetActive(false);
            if (docsRoot != null)
                docsRoot.SetActive(false);
            if (restoreRoot != null)
                restoreRoot.SetActive(true);

            var yes = restoreRoot != null ? restoreRoot.transform.Find("BtnYes") : null;
            if (yes != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(yes.gameObject);
        }

        public void OnRestoreYes()
        {
            if (adminCodeUi == null)
                return;

            // Apaga el escritorio antes de abrir el pad.
            if (restoreRoot != null)
                restoreRoot.SetActive(false);
            if (folderListRoot != null)
                folderListRoot.SetActive(false);
            if (docsRoot != null)
                docsRoot.SetActive(false);
            if (root != null)
                root.SetActive(false);

            adminCodeUi.Open();
        }

        public void OnRestoreNo()
        {
            ShowFolders();
        }

        public void OpenDoc(DocumentData doc)
        {
            if (readerUi == null || doc == null)
                return;
            if (doc == paraMDoc && !string.IsNullOrEmpty(paraMFlagId))
                GameFlagSystem.Instance?.Set(paraMFlagId, true);

            // El lector se abre solo; el escritorio se apaga.
            if (root != null)
                root.SetActive(false);

            readerUi.Open(doc, null);
        }

        public void OpenOrigenDoc() => OpenWorkDoc(origenDoc, sheetOrigen, printFlagOrigen);
        public void OpenSubjectsDoc() => OpenWorkDoc(subjectsDoc, sheetSubjects, printFlagSubjects);
        public void OpenStaffDoc() => OpenWorkDoc(staffDoc, sheetStaff, printFlagStaff);
        public void OpenParaMDoc() => OpenDoc(paraMDoc);

        private void OpenWorkDoc(DocumentData doc, GameObject sheet, string printFlag)
        {
            OpenDoc(doc);

            bool already = GameFlagSystem.Instance != null &&
                           !string.IsNullOrEmpty(printFlag) &&
                           GameFlagSystem.Instance.Get(printFlag);

            if (sheet != null && !already)
                sheet.SetActive(true);

            if (!already && !string.IsNullOrEmpty(printFlag))
            {
                GameFlagSystem.Instance?.Set(printFlag, true);
                Enigma.Audio.SfxPlayer.Instance?.Play("printer");
                if (dialogue != null && dialogue.TryGet(printDialogueId, out var line))
                    subtitles?.Show(line.text, line.duration, line.voice);
            }

            RefreshRestoreFolder();
        }

        private void ShowFolders()
        {
            if (root != null)
                root.SetActive(true);
            if (folderListRoot != null)
                folderListRoot.SetActive(true);
            if (restoreRoot != null)
                restoreRoot.SetActive(false);
            if (docsRoot != null)
                docsRoot.SetActive(false);
            RefreshRestoreFolder();
        }

        private void ShowDocs()
        {
            if (root != null)
                root.SetActive(true);
            if (folderListRoot != null)
                folderListRoot.SetActive(false);
            if (restoreRoot != null)
                restoreRoot.SetActive(false);
            if (docsRoot != null)
                docsRoot.SetActive(true);

            BindFolderRefs();
            // Tras restaurar, solo queda PARA M.
            if (_docOrigenBtn != null)
                _docOrigenBtn.gameObject.SetActive(false);
            if (_docSubjectsBtn != null)
                _docSubjectsBtn.gameObject.SetActive(false);
            if (_docStaffBtn != null)
                _docStaffBtn.gameObject.SetActive(false);
            if (_docParaMBtn != null)
                _docParaMBtn.gameObject.SetActive(true);

            var first = _docParaMBtn != null
                ? _docParaMBtn.GetComponent<Button>()
                : (docsRoot != null ? docsRoot.GetComponentInChildren<Button>(true) : null);
            if (first != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        private void RefreshRestoreFolder()
        {
            BindFolderRefs();
            if (_restoreFolder == null)
                return;

            bool admin = GameFlagSystem.Instance != null && GameFlagSystem.Instance.Get(adminFlagId);
            bool printed =
                GameFlagSystem.Instance != null &&
                GameFlagSystem.Instance.Get(printFlagOrigen) &&
                GameFlagSystem.Instance.Get(printFlagSubjects) &&
                GameFlagSystem.Instance.Get(printFlagStaff);

            _restoreFolder.gameObject.SetActive(!admin && printed);
        }

        private void ConsumePrintSheets()
        {
            var inv = InventorySystem.Instance;
            if (inv != null)
            {
                inv.RemoveById("print_10");
                inv.RemoveById("print_109");
                inv.RemoveById("print_3");
            }

            if (sheetOrigen != null)
                sheetOrigen.SetActive(false);
            if (sheetSubjects != null)
                sheetSubjects.SetActive(false);
            if (sheetStaff != null)
                sheetStaff.SetActive(false);
        }
    }
}
