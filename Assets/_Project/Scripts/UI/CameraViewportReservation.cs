using UnityEngine;

namespace ZooWorld.UI
{
    /// <summary>
    /// Keeps the game view above this screen-space overlay panel by limiting the cameras' viewport to the
    /// screen area above the panel's top edge. It follows the panel while it slides. The play area is derived
    /// from the arena camera's viewport, so animals stay in the visible part and never walk under the panel.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class CameraViewportReservation : MonoBehaviour
    {
        private static readonly Rect FullScreen = new(0f, 0f, 1f, 1f);
        private static readonly Vector3[] Corners = new Vector3[4];

        [Tooltip("Every camera that can render the zoo (arena and free camera).")]
        [SerializeField] private Camera[] _cameras;

        private RectTransform _rectTransform;
        private float _appliedTop = -1f;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Canvas.ForceUpdateCanvases();
            _appliedTop = -1f;
            Apply();
        }

        private void OnDisable()
        {
            foreach (var target in _cameras)
            {
                if (target != null)
                {
                    target.rect = FullScreen;
                }
            }
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            if (Screen.height <= 0)
            {
                return;
            }

            // On a Screen Space - Overlay canvas, world corners are screen pixels.
            _rectTransform.GetWorldCorners(Corners);
            var panelTop = Mathf.Clamp01(Corners[1].y / Screen.height);
            if (Mathf.Approximately(panelTop, _appliedTop))
            {
                return;
            }

            _appliedTop = panelTop;
            var viewport = new Rect(0f, panelTop, 1f, 1f - panelTop);
            foreach (var target in _cameras)
            {
                if (target != null)
                {
                    target.rect = viewport;
                }
            }
        }
    }
}
