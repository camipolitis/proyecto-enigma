using Enigma.Audio;
using Enigma.Core;
using Enigma.Data;
using UnityEngine;

namespace Enigma.Interaction
{
    // Cada E sube el magnetismo y arrastra el metal hacia el imán.
    public class MagnetControlInteractable : InteractableBase
    {
        [SerializeField] private Transform[] metalParts;
        [SerializeField] private GameObject paintingObject;
        [SerializeField] private Transform[] flyBits;
        [SerializeField] private string usedFlagId = "magnet_used";
        [SerializeField] private float step = 0.33f;

        private Vector3[] _starts;
        private Vector3[] _flyStarts;
        private float _pull;
        private bool _done;
        private Transform _needle;
        private Renderer _lamp;
        private Material _gaugeMat;

        private void Awake()
        {
            CacheStarts();
            EnsureGauge();
            SetPaintingReady(false);
        }

        public void BakerSetup(GameObject painting, Transform[] metals, Transform[] flies)
        {
            paintingObject = painting;
            if (metals != null && metals.Length > 0)
                metalParts = metals;
            flyBits = flies;
            step = 0.33f;
            CacheStarts();
            EnsureGauge();
            SetPaintingReady(false);
            ApplyPull();
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
            SfxPlayer.Instance?.Play("magnet", transform.position);
            PlayDialogue(context, config != null ? config.successDialogueId : null);

            if (_pull < 0.99f)
                return;

            _done = true;
            SetPaintingReady(true);
            if (!string.IsNullOrEmpty(usedFlagId))
                GameFlagSystem.Instance?.Set(usedFlagId, true);
            MarkCompleted();
        }

        private void CacheStarts()
        {
            if (metalParts != null)
            {
                _starts = new Vector3[metalParts.Length];
                for (int i = 0; i < metalParts.Length; i++)
                    _starts[i] = metalParts[i] != null ? metalParts[i].position : Vector3.zero;
            }

            if (flyBits != null)
            {
                _flyStarts = new Vector3[flyBits.Length];
                for (int i = 0; i < flyBits.Length; i++)
                    _flyStarts[i] = flyBits[i] != null ? flyBits[i].position : Vector3.zero;
            }
        }

        private void ApplyPull()
        {
            if (metalParts != null && _starts != null)
            {
                int n = Mathf.Min(metalParts.Length, _starts.Length);
                for (int i = 0; i < n; i++)
                {
                    if (metalParts[i] == null)
                        continue;
                    bool pulling = _pull > 0.05f;
                    foreach (var col in metalParts[i].GetComponentsInChildren<Collider>())
                        col.isTrigger = pulling;
                    metalParts[i].position = Vector3.Lerp(_starts[i], transform.position, _pull);
                }
            }

            if (_pull >= 0.45f && flyBits != null && _flyStarts != null)
            {
                float fly = Mathf.InverseLerp(0.45f, 0.72f, _pull);
                for (int i = 0; i < flyBits.Length; i++)
                {
                    if (flyBits[i] == null)
                        continue;
                    flyBits[i].position = Vector3.Lerp(_flyStarts[i], transform.position, fly);
                }
            }

            UpdateGauge();
        }

        private void SetPaintingReady(bool ready)
        {
            if (paintingObject == null)
                return;
            paintingObject.SetActive(true);
            var col = paintingObject.GetComponent<Collider>();
            if (col != null)
                col.enabled = ready;
        }

        private void EnsureGauge()
        {
            var face = transform.Find("MagnetFace");
            if (face == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "MagnetFace";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.62f, 0.42f);
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                go.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
                DropCollider(go);
                face = go.transform;
            }

            _lamp = face.GetComponent<Renderer>();
            EnsureGaugeMat();

            var needle = transform.Find("MagnetNeedle");
            if (needle == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "MagnetNeedle";
                go.transform.SetParent(face, false);
                go.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = new Vector3(0.08f, 0.08f, 0.85f);
                DropCollider(go);
                needle = go.transform;
            }

            _needle = needle;
            UpdateGauge();
        }

        private static void DropCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(col);
            else
                Object.DestroyImmediate(col);
        }

        private void EnsureGaugeMat()
        {
            if (_lamp == null)
                return;
            // Instancia propia una vez. renderer.material clona en el editor.
            if (_gaugeMat == null)
            {
                var src = _lamp.sharedMaterial;
                _gaugeMat = src != null ? new Material(src) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                _gaugeMat.name = "MagnetGaugeMat";
            }
            _lamp.sharedMaterial = _gaugeMat;
        }

        private void UpdateGauge()
        {
            Color c;
            float angle;
            if (_pull < 0.4f)
            {
                c = new Color(0.18f, 0.72f, 0.22f);
                angle = -48f;
            }
            else if (_pull < 0.85f)
            {
                c = new Color(0.92f, 0.78f, 0.12f);
                angle = 0f;
            }
            else
            {
                c = new Color(0.82f, 0.12f, 0.1f);
                angle = 48f;
            }

            if (_gaugeMat == null)
                EnsureGaugeMat();
            if (_gaugeMat != null)
                _gaugeMat.color = c;

            if (_needle != null)
                _needle.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
