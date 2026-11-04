using System.Collections.Generic;
using UnityEngine;

namespace Enigma.Environment
{
    // Movimiento corto cuando un intento falla: golpe, sacudida, empuje o parpadeo, y vuelta exacta al reposo
    public class AttemptMotion : MonoBehaviour
    {
        public enum Kind { Knock, Shake, Push, Blink }

        [SerializeField] private Transform target;
        [SerializeField] private Kind kind;
        // Knock y Shake: eje local del objetivo. Push: dirección en el mundo
        [SerializeField] private Vector3 axis = Vector3.up;
        // Grados para Knock y Shake, metros para Push, parpadeos para Blink
        [SerializeField] private float amount = 1f;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private Renderer blinkRenderer;
        [SerializeField] private Color blinkColor = new Color(1f, 0.08f, 0.05f);
        [SerializeField] private float blinkIntensity = 4f;

        private static readonly List<AttemptMotion> Running = new List<AttemptMotion>();
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        private bool _playing;
        private float _time;
        private Vector3 _restPos;
        private Quaternion _restRot;
        private Rigidbody _body;
        private MaterialPropertyBlock _saved;
        private MaterialPropertyBlock _block;

        public Transform Target => target;
        public bool Playing => _playing;
        public float Amount => amount;
        public float Duration => duration;
        public Kind Motion => kind;

        public void Configure(Transform what, Kind how, Vector3 direction, float size, float seconds)
        {
            target = what;
            kind = how;
            axis = direction;
            amount = size;
            duration = seconds;
            blinkRenderer = how == Kind.Blink && what != null ? what.GetComponent<Renderer>() : null;
        }

        public void Play()
        {
            if (_playing || target == null || !isActiveAndEnabled)
                return;
            _body = target.GetComponent<Rigidbody>();
            if (_body != null && !_body.isKinematic)
                return;
            _playing = true;
            _time = 0f;
            _restPos = target.localPosition;
            _restRot = target.localRotation;
            if (kind == Kind.Blink && blinkRenderer != null)
            {
                _saved = new MaterialPropertyBlock();
                blinkRenderer.GetPropertyBlock(_saved);
                _block = new MaterialPropertyBlock();
            }
            Running.Add(this);
        }

        // El éxito corta el intento y deja el objetivo en reposo en este mismo cuadro
        public static void StopAll(Transform what)
        {
            for (int i = Running.Count - 1; i >= 0; i--)
            {
                var m = Running[i];
                if (m == null)
                    Running.RemoveAt(i);
                else if (what == null || m.target == what)
                    m.Stop(true);
            }
        }

        public void Stop(bool restore)
        {
            if (!_playing)
                return;
            _playing = false;
            Running.Remove(this);
            if (!restore || target == null)
                return;
            if (kind == Kind.Blink)
            {
                if (blinkRenderer != null)
                    blinkRenderer.SetPropertyBlock(_saved);
                return;
            }
            target.localPosition = _restPos;
            target.localRotation = _restRot;
            if (_body != null)
            {
                _body.position = target.position;
                _body.rotation = target.rotation;
            }
        }

        private void OnDisable()
        {
            Stop(true);
        }

        private bool Physical => _body != null;

        private void Update()
        {
            if (_playing && !Physical)
                Step(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_playing && Physical)
                Step(Time.fixedDeltaTime);
        }

        private void Step(float dt)
        {
            if (target == null)
            {
                Stop(false);
                return;
            }
            // El candado ya está cayendo: no se lo vuelve a colgar
            if (_body != null && !_body.isKinematic)
            {
                Stop(false);
                return;
            }
            _time += dt;
            if (_time >= duration)
            {
                Stop(true);
                return;
            }
            Apply(Sample(_time / duration));
        }

        // 0 en el reposo; Knock y Push golpean dos veces hacia un lado, Shake va y viene amortiguado
        private float Sample(float k)
        {
            float fade = 1f - k;
            switch (kind)
            {
                case Kind.Knock:
                case Kind.Push:
                    return Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)) * (k < 0.5f ? 1f : 0.6f);
                case Kind.Shake:
                    return Mathf.Sin(k * Mathf.PI * 6f) * fade;
                default:
                    return Mathf.Sin(k * Mathf.PI * Mathf.Max(1f, amount) * 2f) > 0f ? 1f : 0f;
            }
        }

        private void Apply(float s)
        {
            switch (kind)
            {
                case Kind.Knock:
                case Kind.Shake:
                    SetPose(_restPos, _restRot * Quaternion.AngleAxis(amount * s, axis));
                    break;
                case Kind.Push:
                    Vector3 local = target.parent != null ? target.parent.InverseTransformVector(axis.normalized * amount) : axis.normalized * amount;
                    SetPose(_restPos + local * s, _restRot);
                    break;
                case Kind.Blink:
                    if (blinkRenderer == null)
                        return;
                    if (s > 0f)
                    {
                        _block.Clear();
                        _block.SetColor(BaseColorId, blinkColor);
                        _block.SetColor(EmissionId, blinkColor * blinkIntensity);
                        blinkRenderer.SetPropertyBlock(_block);
                    }
                    else
                        blinkRenderer.SetPropertyBlock(_saved);
                    break;
            }
        }

        private void SetPose(Vector3 localPos, Quaternion localRot)
        {
            if (_body == null)
            {
                target.localPosition = localPos;
                target.localRotation = localRot;
                return;
            }
            var parent = target.parent;
            Vector3 worldPos = parent != null ? parent.TransformPoint(localPos) : localPos;
            Quaternion worldRot = parent != null ? parent.rotation * localRot : localRot;
            _body.MovePosition(worldPos);
            _body.MoveRotation(worldRot);
        }
    }
}
