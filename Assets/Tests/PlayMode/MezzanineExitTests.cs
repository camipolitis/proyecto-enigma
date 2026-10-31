using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Enigma.Tests
{
    // Regresión: cruzar la puerta abierta del entrepiso carga el Nivel 4 sin caer al vacío.
    public class MezzanineExitTests
    {
        const BindingFlags All = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        static object Field(object o, string name)
        {
            for (var t = o.GetType(); t != null; t = t.BaseType)
            {
                var f = t.GetField(name, All);
                if (f != null)
                    return f.GetValue(o);
            }
            return null;
        }

        [UnityTest]
        public IEnumerator WalkingThroughOpenDoorLoadsLevel04()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            yield return null;
            yield return null;
            SceneManager.LoadScene("Level03_Mezzanine");
            yield return new WaitForSeconds(1f);

            MonoBehaviour door = null;
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb.GetType().Name == "ConditionalDoorInteractable" && mb.name == "Interactable_DoorN3")
                    door = mb;
            Assert.IsNotNull(door, "sin Interactable_DoorN3");

            // Puzzle resuelto: mismo flag que setea la puerta al abrirse
            string flag = (string)Field(Field(door, "config"), "successFlagId");
            var gfsType = door.GetType().Assembly.GetType("Enigma.Core.GameFlagSystem");
            var gfs = gfsType.GetProperty("Instance", All).GetValue(null);
            gfsType.GetMethod("Set").Invoke(gfs, new object[] { flag, true });
            var pivot = (Transform)Field(door, "doorPivot");
            if (pivot != null)
                pivot.localRotation = Quaternion.Euler(0f, 90f, 0f);
            yield return new WaitForSeconds(1.2f);

            var player = GameObject.FindWithTag("Player");
            var cc = player.GetComponent<CharacterController>();
            Component state = null;
            foreach (var c in player.GetComponents<Component>())
                if (c.GetType().Name == "PlayerStateController")
                    state = c;
            var canMove = state.GetType().GetProperty("CanMove");
            float minY = player.transform.position.y;
            float end = Time.time + 16f;
            while (Time.time < end && SceneManager.GetActiveScene().name == "Level03_Mezzanine")
            {
                if (player != null)
                {
                    // Como el input real: solo avanza si el estado permite moverse
                    if ((bool)canMove.GetValue(state))
                        cc.Move((Vector3.forward * 1.5f + Vector3.down * 3f) * Time.deltaTime);
                    minY = Mathf.Min(minY, player.transform.position.y);
                }
                yield return null;
            }
            Debug.Log($"[MezzExit] escena={SceneManager.GetActiveScene().name} minY={minY:F3}");
            Assert.AreEqual("Level04_Origen", SceneManager.GetActiveScene().name, "no cargó el Nivel 4");
            Assert.Greater(minY, -0.5f, "el Player cayó al vacío antes del cambio de escena");
        }
    }
}
