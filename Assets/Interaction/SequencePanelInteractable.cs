using Enigma.Core;
using UnityEngine;

namespace Enigma.Interaction
{
    // Panel de la fosa: avisa al controlador su índice.
    public class SequencePanelInteractable : InteractableBase
    {
        [SerializeField] private SequencePuzzleController controller;
        [SerializeField] private int panelIndex;
        [SerializeField] private string completedFlagId = "sequence_ok";
        [Header("Neón")]
        // Botón y cable que se prenden con el acierto y se apagan con el error
        [SerializeField] private Renderer[] neon;
        [SerializeField] private Light neonLight;
        [SerializeField] private Color neonColor = Color.white;
        [SerializeField] private float neonIntensity = 3f;
        [SerializeField, Range(0f, 1f)] private float offShade = 0.4f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _block;

        public void Configure(Renderer[] renderers, Light glow, Color color)
        {
            neon = renderers;
            neonLight = glow;
            neonColor = color;
        }

        private void OnEnable()
        {
            if (controller != null)
                controller.Changed += Refresh;
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.Changed -= Refresh;
        }

        private void Refresh()
        {
            bool lit = controller != null && controller.IsLit(panelIndex);
            if (_block == null)
                _block = new MaterialPropertyBlock();
            Color off = neonColor * offShade;
            off.a = 1f;
            if (neon != null)
            {
                for (int i = 0; i < neon.Length; i++)
                {
                    var r = neon[i];
                    if (r == null)
                        continue;
                    r.GetPropertyBlock(_block);
                    _block.SetColor(BaseColorId, lit ? neonColor : off);
                    _block.SetColor(EmissionId, lit ? neonColor * neonIntensity : Color.black);
                    r.SetPropertyBlock(_block);
                }
            }
            if (neonLight != null)
                neonLight.enabled = lit;
        }

        protected override bool IsCompleted()
        {
            if (string.IsNullOrEmpty(completedFlagId) || GameFlagSystem.Instance == null)
                return false;
            return GameFlagSystem.Instance.Get(completedFlagId);
        }

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (controller == null)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        // El pie hunde el botón en todo intento, acierte o no
        protected override void OnSuccess(InteractContext context)
        {
            PlayAttempt();
            controller.RegisterPress(panelIndex, context);
        }
    }
}
