using UnityEngine;

namespace Enigma.Player
{
    // Visual low-poly del jugador (stand / crouch / prone).
    public class LowPolyPlayerVisual : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private PlayerStateController state;

        private bool _built;

        public void BuildIfNeeded()
        {
            if (_built)
                return;

            var existing = transform.Find("VisualRoot");
            if (existing != null)
            {
                root = existing;
                var t = existing.Find("Torso");
                if (t != null)
                    torso = t;
                var h = existing.Find("Head");
                if (h != null)
                    head = h;
                EnsureHair();
                _built = true;
                return;
            }

            Build();
        }

        private void Awake()
        {
            if (state == null)
                state = GetComponent<PlayerStateController>();
            BuildIfNeeded();
        }

        private void LateUpdate()
        {
            if (root == null)
                return;

            bool prone = state != null && state.State == PlayerGameplayState.Prone;
            var cc = GetComponent<CharacterController>();
            bool crouch = cc != null && cc.height < 1.45f && !prone;

            if (prone)
            {
                root.localRotation = Quaternion.Euler(90f, 0f, 0f);
                root.localPosition = new Vector3(0f, 0.25f, 0f);
            }
            else if (crouch)
            {
                root.localRotation = Quaternion.identity;
                root.localPosition = Vector3.zero;
                if (torso != null)
                {
                    torso.localScale = new Vector3(0.35f, 0.35f, 0.2f);
                    torso.localPosition = new Vector3(0f, 0.85f, 0f);
                }
                if (head != null)
                    head.localPosition = new Vector3(0f, 1.15f, 0f);
            }
            else
            {
                root.localRotation = Quaternion.identity;
                root.localPosition = Vector3.zero;
                if (torso != null)
                {
                    torso.localScale = new Vector3(0.4f, 0.55f, 0.22f);
                    torso.localPosition = new Vector3(0f, 1.05f, 0f);
                }
                if (head != null)
                    head.localPosition = new Vector3(0f, 1.55f, 0f);
            }
        }

        private void Build()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr != null)
                mr.enabled = false;

            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                if (Application.isPlaying)
                    Destroy(mf);
                else
                    DestroyImmediate(mf);
            }

            var capsule = GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                if (Application.isPlaying)
                    Destroy(capsule);
                else
                    DestroyImmediate(capsule);
            }

            var skin = new Color(0.85f, 0.72f, 0.62f);
            var cloth = new Color(0.25f, 0.35f, 0.55f);

            root = new GameObject("VisualRoot").transform;
            root.SetParent(transform, false);

            Part("Hips", root, new Vector3(0f, 0.55f, 0f), new Vector3(0.38f, 0.2f, 0.22f), cloth);
            torso = Part("Torso", root, new Vector3(0f, 1.05f, 0f), new Vector3(0.4f, 0.55f, 0.22f), cloth).transform;
            head = Part("Head", root, new Vector3(0f, 1.55f, 0f), new Vector3(0.28f, 0.28f, 0.28f), skin).transform;
            Part("LegL", root, new Vector3(-0.12f, 0.25f, 0f), new Vector3(0.14f, 0.5f, 0.14f), cloth);
            Part("LegR", root, new Vector3(0.12f, 0.25f, 0f), new Vector3(0.14f, 0.5f, 0.14f), cloth);
            Part("ArmL", torso, new Vector3(-0.32f, 0.05f, 0f), new Vector3(0.12f, 0.45f, 0.12f), skin);
            Part("ArmR", torso, new Vector3(0.32f, 0.05f, 0f), new Vector3(0.12f, 0.45f, 0.12f), skin);
            EnsureHair();

            _built = true;
        }

        public void SetVisible(bool visible)
        {
            if (root != null)
                root.gameObject.SetActive(visible);
        }

        private void EnsureHair()
        {
            if (head == null)
                return;

            var blonde = new Color(0.93f, 0.8f, 0.42f);
            var pos = new Vector3(0f, 0.28f, -0.55f);
            var scale = new Vector3(1.4f, 1.1f, 0.95f);
            var existing = head.Find("Hair");
            if (existing != null)
            {
                existing.localPosition = pos;
                existing.localScale = scale;
                return;
            }

            Part("Hair", head, pos, scale, blonde);
        }

        private static GameObject Part(string name, Transform parent, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.layer = parent.gameObject.layer;

            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying)
                    Destroy(col);
                else
                    DestroyImmediate(col);
            }

            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");
                r.sharedMaterial = new Material(shader) { color = color };
            }

            return go;
        }
    }
}
