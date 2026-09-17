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
            {
                Debug.LogWarning("SceneLoader: nombre de escena vacío.");
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            // timeScale se restaura por si veníamos de pause.
        }
    }
}
