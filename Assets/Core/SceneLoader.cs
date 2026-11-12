using Enigma.Memory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Enigma.Core
{
    // Carga de escenas por nombre (Build Settings).
    public class SceneLoader : MonoBehaviour
    {
        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            Time.timeScale = 1f;

            if (sceneName == "MainMenu")
            {
                GameFlagSystem.ClearCarry();
                if (MemoryJournal.Instance != null)
                    Destroy(MemoryJournal.Instance.gameObject);
                // Partida nueva: flags y notas no cruzan al menú.
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
