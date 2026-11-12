using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Core
{
    // Punto de guardado al entrar a cada nivel: la escena y las memorias leídas hasta ese momento.
    public static class GameSave
    {
        const string LevelKey = "Enigma.Save.Level";
        const string MemoriesKey = "Enigma.Save.Memories";
        const char Separator = '|';

        static bool _continuePending;

        // Arranques de prueba (Editor y tests) no pisan la partida guardada
        public static bool Suspended { get; set; }

        public static string Level => PlayerPrefs.GetString(LevelKey, string.Empty);

        public static bool HasSave => !string.IsNullOrEmpty(Level) && Application.CanStreamedLevelBeLoaded(Level);

        public static void Save(string level, IEnumerable<string> memoryIds)
        {
            if (Suspended || string.IsNullOrEmpty(level))
                return;
            var ids = new List<string>();
            if (memoryIds != null)
                foreach (var id in memoryIds)
                    if (!string.IsNullOrEmpty(id) && id.IndexOf(Separator) < 0)
                        ids.Add(id);
            PlayerPrefs.SetString(LevelKey, level);
            PlayerPrefs.SetString(MemoriesKey, string.Join(Separator.ToString(), ids));
            PlayerPrefs.Save();
        }

        public static string[] MemoryIds()
        {
            string raw = PlayerPrefs.GetString(MemoriesKey, string.Empty);
            return string.IsNullOrEmpty(raw) ? new string[0] : raw.Split(Separator);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.DeleteKey(MemoriesKey);
            PlayerPrefs.Save();
        }

        // «Continuar» pide restaurar las memorias en el journal del nivel que se carga
        public static void RequestContinue() => _continuePending = true;

        public static bool ConsumeContinue()
        {
            bool pending = _continuePending;
            _continuePending = false;
            return pending;
        }
    }
}
