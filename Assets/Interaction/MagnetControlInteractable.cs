using Enigma.Core;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Cada E sube el magnetismo y arrastra el metal hacia el imán.
    public class MagnetControlInteractable : InteractableBase
    {
        [SerializeField] private Transform[] metalParts;
        [SerializeField] private Vector3[] pullTargets;
        [SerializeField] private GameObject paintingObject;
        [SerializeField] private string usedFlagId = "magnet_used";
        [SerializeField] private float step = 0.34f;

        private Vector3[] _starts;
        private float _pull;
        private bool _done;

        private void Awake()
        {
            CacheStarts();
        }

        protected override bool IsCompleted() => _done;

        protected override bool MeetsSuccessConditions(InteractContext context)
        {
            if (_done || metalParts == null || metalParts.Length == 0)
                return false;
            return base.MeetsSuccessConditions(context);
        }

        protected override void OnSuccess(InteractContext context)
        {
            _pull = Mathf.Clamp01(_pull + step);
            ApplyPull();
            PlayDialogue(context, config != null ? config.successDialogueId : null);

            if (_pull < 0.99f)
                return;

            _done = true;
            if (!string.IsNullOrEmpty(usedFlagId))
                GameFlagSystem.Instance?.Set(usedFlagId, true);
            MarkCompleted();
        }

        private void CacheStarts()
        {
            if (metalParts == null)
                return;
            _starts = new Vector3[metalParts.Length];
            for (int i = 0; i < metalParts.Length; i++)
                _starts[i] = metalParts[i] != null ? metalParts[i].position : Vector3.zero;
        }

        private void ApplyPull()
        {
            if (metalParts == null || pullTargets == null || _starts == null)
                return;

            int n = Mathf.Min(metalParts.Length, Mathf.Min(pullTargets.Length, _starts.Length));
            for (int i = 0; i < n; i++)
            {
                if (metalParts[i] == null)
                    continue;
                foreach (var col in metalParts[i].GetComponentsInChildren<Collider>())
                    col.isTrigger = true;
                metalParts[i].position = Vector3.Lerp(_starts[i], pullTargets[i], _pull);
            }
        }
    }
}
