using Enigma.Memory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Enigma.Core
{
    // Carga de escenas por nombre (Build Settings).
    public class SceneLoader : MonoBehaviour
    {
        static bool _firstSceneHandled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetBoot()
        {
            _firstSceneHandled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootOnMenu()
        {
            // Primera carga de la sesión: si no es el menú, lo abrimos.
            if (_firstSceneHandled)
                return;
            _firstSceneHandled = true;

            var scene = SceneManager.GetActiveScene();
            if (scene.name == "MainMenu")
                return;

            SceneManager.LoadScene("MainMenu");
        }

        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("SceneLoader: nombre de escena vacío.");
                return;
            }

            Time.timeScale = 1f;

            if (sceneName == "MainMenu" && MemoryJournal.Instance != null)
                Destroy(MemoryJournal.Instance.gameObject);
            // Partida nueva: las notas no cruzan al menú.

            SceneManager.LoadScene(sceneName);
        }
    }
}
