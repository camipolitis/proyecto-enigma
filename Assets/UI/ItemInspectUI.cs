using Enigma.Core;
using Enigma.Data;
using Enigma.Environment;
using Enigma.Interaction;
using Enigma.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Enigma.UI
{
    // Tecla 1-8: el objeto en 3D. El texto queda como holder vacío.
    public class ItemInspectUI : ModalUIBase
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Text holderLabel;
        // Grados que gira el objeto por cada píxel arrastrado con clic izquierdo
        [SerializeField] private float dragDegreesPerPixel = 0.35f;
        // Rueda: acerca o aleja el objeto entre estas distancias a la cámara
        [SerializeField] private float viewDistance = 1.05f;
        [SerializeField] private float zoomNear = 0.55f;
        [SerializeField] private float zoomFar = 1.7f;
        [SerializeField] private float zoomStep = 0.12f;
        // Desenfoque del fondo mientras se mira un objeto (perfil Rendering/InspectBlur)
        [SerializeField] private Volume blur;
        [SerializeField] private float blurFade = 0.2f;

        private GameObject _root;
        private GameObject _preview;
        private Camera _cam;
        private float _distance;
        private UniversalAdditionalCameraData _camData;
        private bool _camPost;

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
            FadeBlur();
            if (!IsOpen)
                return;
            WheelZoom();
            PlacePreview();
            DragRotate();
            TryCloseWithBack();
        }

        public void OpenItem(InventoryItem item)
        {
            if (item == null)
                return;

            EnsureRoot();
            ClearPreview();

            _preview = BuildModel(item);
            if (_preview != null)
            {
                FitPreview(_preview);
                _preview = CenterPivot(_preview);
                var rends = _preview.GetComponentsInChildren<MeshRenderer>(true);
                for (int i = 0; i < rends.Length; i++)
                    rends[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (holderLabel != null)
                holderLabel.text = string.Empty;

            _cam = Camera.main;
            _distance = viewDistance;
            OpenModal();
            PlacePreview();
            BlurOn();
        }

        public override void Close()
        {
            ClearPreview();
            base.Close();
        }

        // Volume grabado en la escena como hijo de esta UI
        public void EnsureBlur()
        {
            if (blur != null)
                return;
            var t = transform.Find("InspectBlur");
            var go = t != null ? t.gameObject : new GameObject("InspectBlur");
            go.transform.SetParent(transform, false);
            blur = go.GetComponent<Volume>();
            if (blur == null)
                blur = go.AddComponent<Volume>();
            blur.isGlobal = true;
            blur.priority = 50f;
            blur.weight = 0f;
            blur.sharedProfile = Resources.Load<VolumeProfile>("Rendering/InspectBlur");
        }

        // La cámara de juego no siempre tiene post-proceso: se prende solo mientras dura el desenfoque
        private void BlurOn()
        {
            if (blur == null || _cam == null)
                return;
            if (_camData == null || _camData.gameObject != _cam.gameObject)
            {
                _camData = _cam.GetUniversalAdditionalCameraData();
                _camPost = _camData.renderPostProcessing;
            }
            _camData.renderPostProcessing = true;
        }

        private void FadeBlur()
        {
            if (blur == null)
                return;
            float target = IsOpen ? 1f : 0f;
            float step = blurFade > 0.01f ? Time.unscaledDeltaTime / blurFade : 1f;
            blur.weight = Mathf.MoveTowards(blur.weight, target, step);
            if (!IsOpen && blur.weight <= 0f && _camData != null)
            {
                _camData.renderPostProcessing = _camPost;
                _camData = null;
            }
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

        private void PlacePreview()
        {
            if (_preview == null)
                return;
            if (_cam == null)
                _cam = Camera.main;
            if (_cam == null)
                return;
            if (_preview.transform.parent != _cam.transform)
                _preview.transform.SetParent(_cam.transform, false);
            _preview.transform.localPosition = new Vector3(0.12f, -0.06f, _distance);
        }

        private void WheelZoom()
        {
            if (ModalStack.Instance != null && !ModalStack.Instance.IsTop(Kind))
                return;
            var mouse = Mouse.current;
            if (mouse == null)
                return;
            float s = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(s) < 0.01f)
                return;
            _distance = Mathf.Clamp(_distance - Mathf.Sign(s) * zoomStep, zoomNear, zoomFar);
        }

        // Clic izquierdo sostenido: horizontal gira, vertical inclina, siempre respecto de la cámara
        private void DragRotate()
        {
            if (_preview == null || _cam == null)
                return;
            if (ModalStack.Instance != null && !ModalStack.Instance.IsTop(Kind))
                return;
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed)
                return;
            Vector2 d = mouse.delta.ReadValue() * dragDegreesPerPixel;
            var t = _preview.transform;
            t.Rotate(_cam.transform.up, -d.x, Space.World);
            t.Rotate(_cam.transform.right, d.y, Space.World);
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

        // El origen del modelo no siempre es su centro (el cuadro lo tiene en un borde): se gira sobre lo que se ve
        private static GameObject CenterPivot(GameObject model)
        {
            var pivot = new GameObject("Kit_InspectPreview");
            var rends = model.GetComponentsInChildren<MeshRenderer>(true);
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++)
                    b.Encapsulate(rends[i].bounds);
                pivot.transform.position = b.center;
            }
            model.transform.SetParent(pivot.transform, true);
            return pivot;
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

        // Modelo del ítem: el mismo para inspeccionar y para la miniatura del hotbar
        public static GameObject BuildModel(InventoryItem item)
        {
            if (item == null)
                return null;
            if (ItemVisuals.HasCustom(item.id))
                return ItemVisuals.Build(item.id, item.displayName);
            string kit = KitOf(item);
            var prefab = string.IsNullOrEmpty(kit) ? null : Resources.Load<GameObject>("Kitbash/" + kit);
            if (prefab == null)
                return null;
            var go = Object.Instantiate(prefab);
            KitbashDressing.StripColliders(go);
            return go;
        }

        private static string KitOf(InventoryItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.id))
                return "UI_Paper";
            if (item.id == "override_key")
                return "Key4";
            if (item.id.StartsWith("print_"))
                return "UI_Paper";
            if (item.id.IndexOf("tape") >= 0 || item.id.IndexOf("cassette") >= 0)
                return "UI_Paper";
            return "UI_Paper";
        }
    }
}
