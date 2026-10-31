using Enigma.Core;
using Enigma.Data;
using Enigma.Environment;
using Enigma.Interaction;
using Enigma.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Tecla 1-8: el objeto en 3D. El texto queda como holder vacío.
    public class ItemInspectUI : ModalUIBase
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Text holderLabel;

        private GameObject _root;
        private GameObject _preview;
        private Camera _cam;

        protected override ModalKind Kind => ModalKind.ItemInspect;
        protected override GameObject ModalRoot => _root;
        protected override PlayerInputHandler Input
        {
            get
            {
                if (input == null)
                    input = FindFirstObjectByType<PlayerInputHandler>();
                return input;
            }
        }

        private void Update()
        {
            if (!IsOpen)
                return;
            SpinPreview();
            TryCloseWithBack();
        }

        public void OpenItem(InventoryItem item)
        {
            if (item == null)
                return;

            EnsureRoot();
            ClearPreview();

            if (ItemVisuals.HasCustom(item.id))
            {
                _preview = ItemVisuals.Build(item.id, item.displayName);
                if (_preview != null)
                    FitPreview(_preview);
            }
            else
            {
                string kit = KitOf(item);
                var prefab = string.IsNullOrEmpty(kit) ? null : Resources.Load<GameObject>("Kitbash/" + kit);
                if (prefab != null)
                {
                    _preview = Object.Instantiate(prefab);
                    KitbashDressing.StripColliders(_preview);
                    FitPreview(_preview);
                }
            }
            if (_preview != null)
            {
                _preview.name = "Kit_InspectPreview";
                var rends = _preview.GetComponentsInChildren<MeshRenderer>(true);
                for (int i = 0; i < rends.Length; i++)
                    rends[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (holderLabel != null)
                holderLabel.text = string.Empty;

            _cam = Camera.main;
            OpenModal();
            SpinPreview();
        }

        public override void Close()
        {
            ClearPreview();
            base.Close();
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;
            _root = new GameObject("ItemInspectRoot");
            _root.transform.SetParent(transform, false);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            _root.AddComponent<CanvasScaler>();
            _root.AddComponent<GraphicRaycaster>();

            var hold = new GameObject("HolderText");
            hold.transform.SetParent(_root.transform, false);
            var rt = hold.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.04f);
            rt.anchorMax = new Vector2(0.5f, 0.04f);
            rt.sizeDelta = new Vector2(640f, 36f);
            holderLabel = hold.AddComponent<Text>();
            holderLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (holderLabel.font == null)
                holderLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            holderLabel.alignment = TextAnchor.MiddleCenter;
            holderLabel.fontSize = 14;
            holderLabel.color = new Color(1f, 1f, 1f, 0.15f);
            holderLabel.text = string.Empty;
            holderLabel.raycastTarget = false;
        }

        private void SpinPreview()
        {
            if (_preview == null)
                return;
            if (_cam == null)
                _cam = Camera.main;
            if (_cam == null)
                return;
            _preview.transform.SetParent(_cam.transform, false);
            _preview.transform.localPosition = new Vector3(0.12f, -0.06f, 1.05f);
            _preview.transform.Rotate(0f, 48f * Time.deltaTime, 0f, Space.Self);
        }

        private static void FitPreview(GameObject go)
        {
            var rends = go.GetComponentsInChildren<MeshRenderer>(true);
            if (rends.Length == 0)
                return;
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            float m = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            if (m > 0.01f)
                go.transform.localScale *= 0.42f / m;
        }

        private void ClearPreview()
        {
            if (_preview == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(_preview);
            else
                Object.DestroyImmediate(_preview);
            _preview = null;
        }

        private static string KitOf(InventoryItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.id))
                return "UI_Paper";
            if (item.id == "override_key")
                return "Key4";
            if (item.id == "credential")
                return "Pickup_KeyCard";
            if (item.id.StartsWith("print_"))
                return "UI_Paper";
            if (item.id.IndexOf("tape") >= 0 || item.id.IndexOf("cassette") >= 0)
                return "UI_Paper";
            return "UI_Paper";
        }
    }
}
