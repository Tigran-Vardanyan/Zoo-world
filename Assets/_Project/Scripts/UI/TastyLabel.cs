using UnityEngine;
using VContainer;
using ZooWorld.Animals;

namespace ZooWorld.UI
{
    /// <summary>
    /// World-space label shown under an animal for a moment every time it eats.
    /// Lives inside the animal prefab, so any predator gets it by nesting the TastyLabel prefab.
    /// </summary>
    public sealed class TastyLabel : MonoBehaviour
    {
        [SerializeField] private Animal _animal;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Tooltip("World offset from the animal; negative Z places the label below it on a top-down screen.")]
        [SerializeField] private Vector3 _offset = new(0f, 1f, -0.9f);

        [SerializeField, Min(0.1f)] private float _duration = 1f;
        [SerializeField, Min(0f)] private float _fadeDuration = 0.3f;
        [SerializeField] private AnimationCurve _scaleOverTime = AnimationCurve.EaseInOut(0f, 0.6f, 0.15f, 1f);

        private Transform _camera;
        private Vector3 _baseScale;
        private float _elapsed;

        [Inject]
        public void Construct(Camera mainCamera)
        {
            _camera = mainCamera.transform;
        }

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            _animal.Ate += OnAte;
            Hide();
        }

        private void OnDisable()
        {
            _animal.Ate -= OnAte;
        }

        private void OnAte(Animal prey)
        {
            _elapsed = 0f;
            _canvas.enabled = true;
            UpdateVisuals();
        }

        private void LateUpdate()
        {
            if (!_canvas.enabled)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                Hide();
                return;
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // The label is a child of a rotating animal, so its pose is set in world space every frame.
            transform.SetPositionAndRotation(_animal.transform.position + _offset, _camera.rotation);
            transform.localScale = _baseScale * _scaleOverTime.Evaluate(_elapsed);
            _canvasGroup.alpha = _fadeDuration > 0f ? Mathf.Clamp01((_duration - _elapsed) / _fadeDuration) : 1f;
        }

        private void Hide()
        {
            _canvas.enabled = false;
        }

        private void Reset()
        {
            _animal = GetComponentInParent<Animal>();
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
