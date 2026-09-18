using System.Collections;
using Enigma.Data;
using Enigma.Player;
using Enigma.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.Sequences
{
    // Intro: subtítulo → "Presioná E" → Exploring.
    public class IntroSequence : MonoBehaviour
    {
        [SerializeField] private IntroSequenceConfig config;
        [SerializeField] private DialogueLineSet dialogue;
        [SerializeField] private string openingDialogueId = "intro_wake";
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private PlayerStateController state;
        [SerializeField] private ThirdPersonController controller;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private GameObject standPromptRoot;
        [SerializeField] private Text standPromptLabel;

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            input?.SetGameplayInputEnabled(false);
            state?.SetState(PlayerGameplayState.Prone);
            controller?.SetProneVisual(true);

            if (standPromptRoot != null)
                standPromptRoot.SetActive(false);

            string opening = config != null ? config.openingSubtitle : "¿Dónde estoy? ...¿Qué pasó?";
            float duration = config != null ? config.openingSubtitleDuration : 3f;
            if (dialogue != null && dialogue.TryGet(openingDialogueId, out var line))
            {
                opening = line.text;
                duration = line.duration;
                if (line.voice != null && line.voice.length > duration)
                    duration = line.voice.length;
                subtitles?.Show(line.text, line.duration, line.voice);
            }
            else
                subtitles?.Show(opening, duration);

            yield return new WaitForSecondsRealtime(duration);

            if (standPromptLabel != null)
                standPromptLabel.text = config != null ? config.standPrompt : "Presioná E para levantarte";
            if (standPromptRoot != null)
                standPromptRoot.SetActive(true);

            // Esperamos E (Interact) para levantarse.
            while (input == null || !input.InteractPressedThisFrame)
                yield return null;

            if (standPromptRoot != null)
                standPromptRoot.SetActive(false);

            controller?.SetProneVisual(false);
            state?.SetState(PlayerGameplayState.Exploring);
            input?.SetGameplayInputEnabled(true);
        }
    }
}
