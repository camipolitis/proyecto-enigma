using Enigma.Core;
using UnityEngine;

namespace Enigma.UI
{
    // Pantalla de inicio: Jugar, Cómo jugar, Créditos.
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string playSceneName = "Level01_Room";
        [SerializeField] private GameObject howToPanel;
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            HideOverlays();
        }

        public void Play()
        {
            if (sceneLoader != null)
                sceneLoader.LoadSceneByName(playSceneName);
        }

        public void ShowHowTo()
        {
            if (howToPanel != null)
                howToPanel.SetActive(true);
            if (creditsPanel != null)
                creditsPanel.SetActive(false);
        }

        public void ShowCredits()
        {
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
