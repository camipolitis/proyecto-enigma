using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Enigma.AI
{
    // NavMesh del lab y cámara de seguridad, armados al grabar la escena. Sin muerte.
    public class LabSystems : MonoBehaviour
    {
        [SerializeField] private Vector3 watcherPosition = new Vector3(7.55f, 2.68f, 4.85f);
        [SerializeField] private Vector3 watcherLookAt = new Vector3(0f, 1f, 0f);

        // La grabación guarda los datos del NavMesh como asset; acá solo se arma la superficie
        public NavMeshSurface BuildSurface()
        {
            var t = transform.Find("NavSurface");
            var surfaceGo = t != null ? t.gameObject : new GameObject("NavSurface");
            surfaceGo.transform.SetParent(transform, false);
            var surface = surfaceGo.GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = surfaceGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            return surface;
        }

        public void BuildWatcher(Transform player, Material body, Material lens)
        {
            var old = transform.Find("Watcher");
            if (old != null)
                DestroyImmediate(old.gameObject);

            var root = new GameObject("Watcher");
            root.transform.SetParent(transform, false);
            root.transform.position = watcherPosition;
            root.transform.rotation = Quaternion.LookRotation(watcherLookAt - watcherPosition);

            Part(PrimitiveType.Cube, "CamMount", root.transform, new Vector3(0f, 0.04f, -0.08f), Quaternion.identity, new Vector3(0.08f, 0.08f, 0.16f), body);
            Part(PrimitiveType.Cube, "CamBody", root.transform, new Vector3(0f, 0f, 0.02f), Quaternion.identity, new Vector3(0.16f, 0.11f, 0.2f), body);
            Part(PrimitiveType.Cylinder, "CamLens", root.transform, new Vector3(0f, 0f, 0.13f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.09f, 0.04f, 0.09f), lens);
            var led = Part(PrimitiveType.Sphere, "CamLed", root.transform, new Vector3(0.05f, 0.05f, 0.1f), Quaternion.identity, Vector3.one * 0.035f, lens);

            var ai = root.AddComponent<WatcherAI>();
            ai.Setup(player, led.transform);
        }

        // Las piezas no chocan: la cámara cuelga fuera del paso
        private static GameObject Part(PrimitiveType shape, string name, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var col = go.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
            return go;
        }
    }
}
