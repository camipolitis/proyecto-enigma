using UnityEngine;
using UnityEngine.SceneManagement;

namespace Enigma.Core
{
    /// Modelo reutilizable para cargar escenas. N1 solo usa fade + log.
    public class SceneLoader : MonoBehaviour
    {
        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("Aún no hay escenas! jeje");
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            // timeScale se restaura por si veníamos de pause.
        }
    }
}
