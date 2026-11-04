using System.Collections;
using Enigma.Core;
using UnityEngine;

namespace Enigma.Environment
{
    // Hoja impresa: al prenderse espera adentro de la impresora a que no haya ventanas abiertas, sale a tirones y cae a la bandeja
    public class PrintFeed : MonoBehaviour
    {
        // Pose dentro de la impresora (escondida) y dirección de salida, en mundo
        [SerializeField] private Vector3 slot;
        [SerializeField] private Vector3 feedDir = Vector3.back;
        [SerializeField] private float duration = 1.2f;
        [SerializeField] private float stackStep = 0.0025f;
        [SerializeField] private float stackShift = 0.03f;
        [SerializeField] private float stackYaw = 5f;

        private const float Stagger = 0.5f;
        private static float _nextStart;

        private Vector3 _rest;
        private Quaternion _restRot;
        private bool _landed;
        private bool _ready;
        private int _place = -1;

        public bool Landed => _landed;

        public void Configure(Vector3 slotPos, Vector3 dir)
        {
            slot = slotPos;
            dir.y = 0f;
            feedDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.back;
        }

        private void Awake()
        {
            CaptureRest();
        }

        private void CaptureRest()
        {
            if (_ready)
                return;
            _ready = true;
            // La pose grabada es la de la primera hoja en la bandeja
            _rest = transform.position;
            _restRot = transform.rotation;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying || _landed)
                return;
            CaptureRest();
            SetColliders(false);
            transform.SetPositionAndRotation(slot, _restRot);
            StartCoroutine(Feed());
        }

        private IEnumerator Feed()
        {
            while (ModalStack.Instance != null && !ModalStack.Instance.IsEmpty)
                yield return null;

            float start = Mathf.Max(Time.time, _nextStart);
            _nextStart = start + Stagger;
            while (Time.time < start)
                yield return null;

            int n = FreePlace();
            _place = n;
            float yaw = n == 0 ? 0f : (n % 2 == 1 ? stackYaw : -stackYaw);
            Quaternion endRot = Quaternion.AngleAxis(yaw, Vector3.up) * _restRot;
            Vector3 end = _rest + feedDir * (stackShift * n) + Vector3.up * (stackStep * n);
            Vector3 lip = new Vector3(end.x, slot.y, end.z);

            // Tres tirones de la impresora hasta que la hoja queda afuera, después cae
            float slide = duration * 0.8f;
            float t = 0f;
            while (t < slide)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / slide);
                float pulse = (Mathf.Floor(k * 3f) + Smooth(k * 3f - Mathf.Floor(k * 3f))) / 3f;
                transform.position = Vector3.Lerp(slot, lip, Mathf.Clamp01(pulse));
                yield return null;
            }
            float drop = duration - slide;
            t = 0f;
            while (t < drop)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / drop);
                transform.SetPositionAndRotation(Vector3.Lerp(lip, end, k * k), Quaternion.Slerp(_restRot, endRot, k));
                yield return null;
            }
            transform.SetPositionAndRotation(end, endRot);
            _landed = true;
            SetColliders(true);
        }

        private static float Smooth(float k)
        {
            k = Mathf.Clamp01(k);
            return k * k * (3f - 2f * k);
        }

        // Primer lugar de la bandeja que no ocupa otra hoja (las que ya se levantaron lo liberan)
        private int FreePlace()
        {
            var taken = new System.Collections.Generic.HashSet<int>();
            foreach (var p in Object.FindObjectsByType<PrintFeed>(FindObjectsSortMode.None))
                if (p != this && p._place >= 0 && p.gameObject.scene == gameObject.scene)
                    taken.Add(p._place);
            int n = 0;
            while (taken.Contains(n))
                n++;
            return n;
        }

        private void SetColliders(bool on)
        {
            foreach (var c in GetComponents<Collider>())
                c.enabled = on;
        }
    }
}
