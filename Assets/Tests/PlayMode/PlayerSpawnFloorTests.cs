using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Enigma.Tests
{
    // Regresión de la caída al vacío: el Player debe quedar apoyado en el piso de cada nivel.
    public class PlayerSpawnFloorTests
    {
        const float SettleSeconds = 6f;
        const float MaxDrop = 0.25f;

        static readonly string[] Levels =
        {
            "Level01_Room",
            "Level02_Lab",
            "Level03_Archives",
            "Level03_Mezzanine",
            "Level04_Origen"
        };

        readonly List<string> _errors = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _errors.Clear();
            LogAssert.ignoreFailingMessages = true;
            Application.logMessageReceived += OnLog;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= OnLog;
        }

        void OnLog(string msg, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
                _errors.Add(msg);
        }

        static RaycastHit? FloorUnder(GameObject player, Vector3 pos)
        {
            var hits = Physics.RaycastAll(pos + Vector3.up * 1f, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore);
            RaycastHit? best = null;
            foreach (var h in hits)
            {
                if (h.collider.transform.IsChildOf(player.transform))
                    continue;
                if (best == null || h.distance < best.Value.distance)
                    best = h;
            }
            return best;
        }

        // Un collider sólido más grande que la sala envuelve al Player y lo expulsa fuera del nivel
        [UnityTest]
        public IEnumerator NoOversizedColliders([ValueSource(nameof(Levels))] string level)
        {
            yield return null;
            yield return null;
            SceneManager.LoadScene(level);
            yield return new WaitForSeconds(0.5f);

            var bad = new List<string>();
            foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (c.isTrigger || !c.enabled)
                    continue;
                var s = c.bounds.size;
                if (s.x > 21f || s.y > 14f || s.z > 21f)
                    bad.Add($"{c.name} {s:F1}");
            }
            Debug.Log($"[SpawnFloor] {level} colliders gigantes={bad.Count} {string.Join(" | ", bad)}");
            Assert.IsEmpty(bad, level + ": colliders sólidos más grandes que la sala");
        }

        [UnityTest]
        public IEnumerator PlayerStaysOnFloor([ValueSource(nameof(Levels))] string level)
        {
            // Dos frames para que el arranque al menú termine antes de cargar el nivel
            yield return null;
            yield return null;

            SceneManager.LoadScene(level);
            yield return null;
            Assert.AreEqual(level, SceneManager.GetActiveScene().name);

            var player = GameObject.FindWithTag("Player");
            Assert.IsNotNull(player, level + ": sin Player");
            var cc = player.GetComponent<CharacterController>();
            Assert.IsNotNull(cc, level + ": sin CharacterController");

            float spawnY = player.transform.position.y;
            var floor = FloorUnder(player, player.transform.position);
            Assert.IsTrue(floor.HasValue, level + ": no hay piso sólido bajo el spawn");
            string floorName = floor.Value.collider.name;
            float floorY = floor.Value.point.y;

            float minY = spawnY;
            float end = Time.time + SettleSeconds;
            while (Time.time < end)
            {
                minY = Mathf.Min(minY, player.transform.position.y);
                yield return null;
            }

            var pos = player.transform.position;
            Debug.Log($"[SpawnFloor] {level} piso={floorName} pisoY={floorY:F3} spawnY={spawnY:F3} minY={minY:F3} finalY={pos.y:F3} grounded={cc.isGrounded} errores={_errors.Count}");
            foreach (var e in _errors)
                Debug.Log($"[SpawnFloor] {level} error: {e}");

            // Un spawn al ras de un Plane (sin grosor) deja el skin dentro de la malla y el CharacterController la atraviesa
            Assert.Greater(spawnY, floorY + 0.01f, level + ": el spawn está al ras o debajo del piso");
            Assert.Greater(minY, spawnY - MaxDrop, level + ": el Player cayó por debajo del piso");
            Assert.IsTrue(FloorUnder(player, pos).HasValue, level + ": no hay piso sólido bajo el Player");
        }
    }
}
