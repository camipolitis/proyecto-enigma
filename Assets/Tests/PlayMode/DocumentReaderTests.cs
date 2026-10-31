using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Enigma.Tests
{
    // Regresión: abrir documentos seguidos no debe achicar la hoja ni el texto.
    public class DocumentReaderTests
    {
        [UnityTest]
        public IEnumerator RepeatedOpensKeepSameTextArea()
        {
            yield return null;
            yield return null;
            SceneManager.LoadScene("Level02_Lab");
            yield return new WaitForSeconds(0.5f);

            MonoBehaviour reader = null;
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb.GetType().Name == "DocumentReaderUI")
                    reader = mb;
            Assert.IsNotNull(reader, "sin DocumentReaderUI");

            var asm = reader.GetType().Assembly;
            var docType = asm.GetType("Enigma.Data.DocumentData");
            var ctxType = asm.GetType("Enigma.Interaction.InteractContext");
            var open = reader.GetType().GetMethod("Open", new[] { docType, ctxType, typeof(bool) });
            var close = reader.GetType().GetMethod("Close");
            var body = (Text)reader.GetType().GetField("bodyLabel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(reader);

            float first = -1f;
            for (int i = 0; i < 5; i++)
            {
                var doc = ScriptableObject.CreateInstance(docType);
                docType.GetField("id").SetValue(doc, "test_doc_" + i);
                docType.GetField("title").SetValue(doc, "Documento " + i);
                docType.GetField("body").SetValue(doc, "Texto de prueba");
                open.Invoke(reader, new object[] { doc, null, false });
                yield return null;
                Canvas.ForceUpdateCanvases();
                var corners = new Vector3[4];
                body.rectTransform.GetWorldCorners(corners);
                float w = corners[2].x - corners[0].x;
                Debug.Log($"[DocReader] apertura {i} ancho={w:F1} padre={body.transform.parent.name} abuelo={body.transform.parent.parent.name}");
                if (first < 0f)
                    first = w;
                Assert.AreEqual(first, w, 1f, "el texto se achicó en la apertura " + i);
                close.Invoke(reader, null);
                yield return null;
            }
        }
    }
}
