using Enigma.UI;
using UnityEngine;

namespace Enigma.AI
{
    // Cámara de seguridad: paneo, te enfoca, alerta. No hace daño.
    public class WatcherAI : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Track,
            Alert
        }

        [SerializeField] private float viewAngle = 52f;
        [SerializeField] private float viewRange = 11f;
        [SerializeField] private float idleTurnSpeed = 14f;
        [SerializeField] private float panLimit = 38f;
        [SerializeField] private float alertSeconds = 2.6f;
        [SerializeField] private float cooldownSeconds = 8f;
        [SerializeField] private string alertLine = "La cámara me enfoca...";

        private Transform _player;
        private Transform _led;
        private State _state = State.Idle;
        private float _timer;
        private float _cool;
        private float _restYaw;
        private float _pan;
        private float _panSign = 1f;

        public void Setup(Transform player, Transform led)
        {
            _player = player;
            _led = led;
            _restYaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (_player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null)
                    _player = go.transform;
                if (_player == null)
                    return;
            }

            if (_cool > 0f)
                _cool -= Time.deltaTime;

            bool inCone = InCone();
            bool seen = inCone && HasLineOfSight();
            SetLed(_state == State.Alert);

            if (_state == State.Idle)
            {
                PanIdle();
                if (inCone)
                    _state = State.Track;
                return;
            }

            if (_state == State.Track)
            {
                FacePlayer();
                if (!inCone)
                    _state = State.Idle;
                else if (seen && _cool <= 0f)
                    EnterAlert();
                return;
            }

            FacePlayer();
            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            _state = State.Track;
            _cool = cooldownSeconds;
        }

        private void PanIdle()
        {
            _pan += idleTurnSpeed * _panSign * Time.deltaTime;
            if (_pan > panLimit)
            {
                _pan = panLimit;
                _panSign = -1f;
            }
            else if (_pan < -panLimit)
            {
                _pan = -panLimit;
                _panSign = 1f;
            }

            Vector3 e = transform.eulerAngles;
            e.y = _restYaw + _pan;
            e.x = 22f;
            transform.eulerAngles = e;
        }

        private void EnterAlert()
        {
            _state = State.Alert;
            _timer = alertSeconds;
            _cool = cooldownSeconds;
            SubtitleSystem.Instance?.Show(alertLine, alertSeconds);
        }

        private void FacePlayer()
        {
            Vector3 to = _player.position + Vector3.up * 1.1f - transform.position;
            if (to.sqrMagnitude < 0.01f)
                return;

            Quaternion want = Quaternion.LookRotation(to);
            Vector3 euler = want.eulerAngles;
            float pitch = euler.x > 180f ? euler.x - 360f : euler.x;
            pitch = Mathf.Clamp(pitch, 6f, 48f);
            euler.x = pitch;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), 6f * Time.deltaTime);
        }

        private bool InCone()
        {
            Vector3 to = _player.position + Vector3.up * 1.2f - transform.position;
            float dist = to.magnitude;
            if (dist > viewRange || dist < 0.15f)
                return false;
            return Vector3.Angle(transform.forward, to) <= viewAngle * 0.5f;
        }

        private bool HasLineOfSight()
        {
            Vector3 origin = transform.position + transform.forward * 0.12f;
            Vector3 target = _player.position + Vector3.up * 1.1f;
            Vector3 dir = target - origin;
            if (!Physics.Raycast(origin, dir.normalized, out var hit, dir.magnitude, ~0, QueryTriggerInteraction.Ignore))
                return false;
            return hit.transform == _player || hit.transform.IsChildOf(_player);
        }

        private void SetLed(bool on)
        {
            if (_led == null)
                return;
            var rend = _led.GetComponent<MeshRenderer>();
            if (rend == null)
                return;
            Color c = on ? new Color(1f, 0.15f, 0.12f) : new Color(0.35f, 0.05f, 0.05f);
            Paint(rend, c);
        }

        public static void Paint(MeshRenderer rend, Color color)
        {
            if (rend == null)
                return;
            var mat = rend.sharedMaterial;
            if (mat == null)
                return;
            if (!mat.name.EndsWith("_Led"))
            {
                mat = new Material(mat);
                mat.name = mat.name + "_Led";
                rend.sharedMaterial = mat;
            }
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
        }
    }
}
