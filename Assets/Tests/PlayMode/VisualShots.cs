using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Enigma.Tests
{
    // Capturas de documentos y modelos de ítems para revisión visual (Logs/shots).
    public class VisualShots
    {
        static string Dir => Path.Combine(Application.dataPath, "../Logs/shots");

        static void Save(Camera cam, RenderTexture rt, string name)
        {
            cam.targetTexture = rt;
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(Dir);
            File.WriteAllBytes(Path.Combine(Dir, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator Documents()
        {
#if UNITY_EDITOR
            yield return null;
            yield return null;
            SceneManager.LoadScene("Level03_Archives");
            yield return new WaitForSeconds(0.5f);

            MonoBehaviour reader = null;
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb.GetType().Name == "DocumentReaderUI")
                    reader = mb;
            Assert.IsNotNull(reader);
            var asm = reader.GetType().Assembly;
            var docType = asm.GetType("Enigma.Data.DocumentData");
            var ctxType = asm.GetType("Enigma.Interaction.InteractContext");
            var open = reader.GetType().GetMethod("Open", new[] { docType, ctxType, typeof(bool) });
            var close = reader.GetType().GetMethod("Close");

            var camGo = new GameObject("ShotCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;

            string[] docs =
            {
                "Assets/ScriptableObjects/Level04/SO_Doc_ShiftLog.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Staff_BurnA.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Staff_BurnB.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Staff_Rest.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Subject_206.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Subject_129.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Origen_Incomplete.asset",
                "Assets/ScriptableObjects/Documents/SO_Doc_Print_109.asset",
            };
            foreach (var path in docs)
            {
                var doc = UnityEditor.AssetDatabase.LoadAssetAtPath(path, docType);
                Assert.IsNotNull(doc, path);
                open.Invoke(reader, new object[] { doc, null, false });
                yield return null;
                yield return null;
                foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                {
                    if (!c.isRootCanvas || c.renderMode != RenderMode.ScreenSpaceOverlay)
                        continue;
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.worldCamera = cam;
                    c.planeDistance = 1f;
                }
                cam.cullingMask = ~0;
                Canvas.ForceUpdateCanvases();
                yield return null;
                Save(cam, rt, "doc_" + Path.GetFileNameWithoutExtension(path));
                close.Invoke(reader, null);
                yield return null;
            }
#else
            yield break;
#endif
        }

        [UnityTest]
        public IEnumerator Items()
        {
            yield return null;
            yield return null;
            SceneManager.LoadScene("Level04_Origen");
            yield return new WaitForSeconds(0.5f);

            var asm = System.AppDomain.CurrentDomain.GetAssemblies();
            System.Type vis = null;
            foreach (var a in asm)
            {
                vis = a.GetType("Enigma.Interaction.ItemVisuals");
                if (vis != null)
                    break;
            }
            Assert.IsNotNull(vis);
            var build = vis.GetMethod("Build");

            var stage = new Vector3(0f, 50f, 0f);
            var camGo = new GameObject("ItemCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.18f, 0.2f);
            cam.fieldOfView = 30f;
            camGo.transform.position = stage + new Vector3(0f, 0.25f, -0.75f);
            camGo.transform.LookAt(stage);
            var lightGo = new GameObject("ItemLight");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.6f;
            lightGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            var rt = new RenderTexture(640, 480, 24);

            foreach (var id in new[] { "override_key", "battery", "tape", "fuse", "credential", "print_109" })
            {
                var go = (GameObject)build.Invoke(null, new object[] { id, null });
                Assert.IsNotNull(go, id);
                go.transform.position = stage;
                go.transform.rotation = Quaternion.Euler(-20f, 30f, 0f);
                float size = (float)vis.GetMethod("SizeOf").Invoke(null, new object[] { id });
                camGo.transform.position = stage + new Vector3(0f, 0.35f, -1f) * size * 3.2f;
                camGo.transform.LookAt(stage);
                yield return null;
                Save(cam, rt, "item_" + id);
                Object.Destroy(go);
            }

            // Llave en el mundo, donde la deja el nivel
            var key = GameObject.Find("Interactables")?.transform.Find("Interactable_OverrideKey");
            if (key != null)
            {
                key.gameObject.SetActive(true);
                yield return null;
                yield return null;
                var b = (Bounds)vis.GetMethod("Bounds").Invoke(null, new object[] { key.gameObject });
                Debug.Log($"[Shots] llave mundo pos={key.position:F2} escala={key.lossyScale:F2} bounds={b.size:F3}");
                camGo.transform.position = key.position + new Vector3(0.25f, 0.35f, -0.45f);
                camGo.transform.LookAt(key.position);
                Object.Destroy(lightGo);
                yield return null;
                Save(cam, rt, "world_key");
            }
        }
    }
}
