using Enigma.Core;
using UnityEngine;

namespace Enigma.UI
{
    // Pantalla de inicio: Continuar, Jugar, Cómo jugar, Créditos.
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string playSceneName = "Level01_Room";
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject howToPanel;
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            HideOverlays();
            if (continueButton != null)
                continueButton.SetActive(GameSave.HasSave);
        }

        public void Play()
        {
            Enigma.Audio.SfxPlayer.Instance?.Play("ui_click");
            if (sceneLoader != null)
                sceneLoader.LoadSceneByName(playSceneName);
        }

        // Vuelve al último nivel al que se entró, con las memorias que se tenían
        public void Continue()
        {
            Enigma.Audio.SfxPlayer.Instance?.Play("ui_click");
            if (sceneLoader == null || !GameSave.HasSave)
                return;
            GameSave.RequestContinue();
            sceneLoader.LoadSceneByName(GameSave.Level);
        }

        public void ShowHowTo()
        {
            Enigma.Audio.SfxPlayer.Instance?.Play("ui_click");
            if (howToPanel != null)
                howToPanel.SetActive(true);
            if (creditsPanel != null)
                creditsPanel.SetActive(false);
        }

        public void ShowCredits()
        {
            Enigma.Audio.SfxPlayer.Instance?.Play("ui_click");
            if (creditsPanel != null)
                creditsPanel.SetActive(true);
            if (howToPanel != null)
                howToPanel.SetActive(false);
        }

        public void HideOverlays()
        {
            if (howToPanel != null)
                howToPanel.SetActive(false);
            if (creditsPanel != null)
                creditsPanel.SetActive(false);
        }
    }
}
