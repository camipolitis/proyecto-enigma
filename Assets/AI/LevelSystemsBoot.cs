using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Enigma.AI
{
    // NavMesh del lab y cámara de seguridad. Sin muerte.
    public class LevelSystemsBoot : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnScene;
            SceneManager.sceneLoaded += OnScene;
            OnScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnScene(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid() || scene.name == "MainMenu")
                return;
            if (FindFirstObjectByType<LevelSystemsBoot>() != null)
                return;

            var go = new GameObject("LevelSystems");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<LevelSystemsBoot>().Build(scene.name);
        }

        private void Build(string sceneName)
        {
            if (sceneName != "Level02_Lab")
                return;

            var surfaceGo = new GameObject("NavSurface");
            surfaceGo.transform.SetParent(transform, false);
            var surface = surfaceGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            try
            {
                surface.BuildNavMesh();
            }
            catch (System.Exception)
            {
                // La cámara no camina; el bake es para la consigna de IA.
            }

            SpawnCamera();
        }

        private void SpawnCamera()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var root = new GameObject("Watcher");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(7.55f, 2.68f, 4.85f);
            Vector3 look = new Vector3(0f, 1f, 0f) - root.transform.position;
            root.transform.rotation = Quaternion.LookRotation(look);

            var mount = MakeBox("CamMount", root.transform, new Vector3(0f, 0.04f, -0.08f), new Vector3(0.08f, 0.08f, 0.16f), new Color(0.18f, 0.18f, 0.2f));
            mount.transform.localRotation = Quaternion.identity;

            var body = MakeBox("CamBody", root.transform, new Vector3(0f, 0f, 0.02f), new Vector3(0.16f, 0.11f, 0.2f), new Color(0.14f, 0.14f, 0.16f));
            body.transform.localRotation = Quaternion.identity;

            var lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "CamLens";
            lens.transform.SetParent(root.transform, false);
            lens.transform.localPosition = new Vector3(0f, 0f, 0.13f);
            lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lens.transform.localScale = new Vector3(0.09f, 0.04f, 0.09f);
            WatcherAI.Paint(lens.GetComponent<MeshRenderer>(), new Color(0.08f, 0.09f, 0.12f));
            SetTrigger(lens);

            var led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            led.name = "CamLed";
            led.transform.SetParent(root.transform, false);
            led.transform.localPosition = new Vector3(0.05f, 0.05f, 0.1f);
            led.transform.localScale = Vector3.one * 0.035f;
            WatcherAI.Paint(led.GetComponent<MeshRenderer>(), new Color(0.35f, 0.05f, 0.05f));
            SetTrigger(led);

            SetTrigger(mount);
            SetTrigger(body);

            var ai = root.AddComponent<WatcherAI>();
            ai.Setup(player != null ? player.transform : null, led.transform);
        }

        private static GameObject MakeBox(string name, Transform parent, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            WatcherAI.Paint(go.GetComponent<MeshRenderer>(), color);
            return go;
        }

        private static void SetTrigger(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }
    }
}
