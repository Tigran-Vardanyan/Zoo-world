using UnityEngine;

namespace ZooWorld.Animals.Presentation
{
    /// <summary>
    /// Highlights the animal with an outline for a moment every time it eats.
    /// The outline material is added to the model's renderers only while it is visible,
    /// and its width is animated per renderer through a MaterialPropertyBlock (no material copies).
    /// </summary>
    public sealed class OutlineOnEat : MonoBehaviour
    {
        private static readonly int WidthProperty = Shader.PropertyToID("_OutlineWidth");

        [SerializeField] private Animal _animal;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Material _outlineMaterial;

        [SerializeField, Min(0.05f)] private float _duration = 1f;
        [SerializeField, Min(0f)] private float _width = 0.06f;

        [Tooltip("Outline width over the duration (0..1 on both axes): pops in, holds, shrinks away.")]
        [SerializeField] private AnimationCurve _widthOverTime = new(
            new Keyframe(0f, 0f), new Keyframe(0.1f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f));

        private Material[][] _originalMaterials;
        private Material[][] _outlinedMaterials;
        private MaterialPropertyBlock _properties;
        private float _elapsed;
        private bool _isShown;

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            _originalMaterials = new Material[_renderers.Length][];
            _outlinedMaterials = new Material[_renderers.Length][];
            for (var i = 0; i < _renderers.Length; i++)
            {
                var original = _renderers[i].sharedMaterials;
                var outlined = new Material[original.Length + 1];
                original.CopyTo(outlined, 0);
                outlined[original.Length] = _outlineMaterial;

                _originalMaterials[i] = original;
                _outlinedMaterials[i] = outlined;
            }
        }

        private void OnEnable()
        {
            _animal.Ate += OnAte;
            _animal.Died += OnDied;
        }

        private void OnDisable()
        {
            _animal.Ate -= OnAte;
            _animal.Died -= OnDied;
            Hide();
        }

        private void Update()
        {
            if (!_isShown)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                Hide();
                return;
            }

            SetWidth(_width * _widthOverTime.Evaluate(_elapsed / _duration));
        }

        private void OnAte(Animal prey)
        {
            _elapsed = 0f;
            if (!_isShown)
            {
                _isShown = true;
                for (var i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].sharedMaterials = _outlinedMaterials[i];
                }
            }

            SetWidth(0f);
        }

        private void OnDied(Animal animal) => Hide();

        private void Hide()
        {
            if (!_isShown)
            {
                return;
            }

            _isShown = false;
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sharedMaterials = _originalMaterials[i];
                _renderers[i].SetPropertyBlock(null);
            }
        }

        private void SetWidth(float width)
        {
            foreach (var target in _renderers)
            {
                target.GetPropertyBlock(_properties);
                _properties.SetFloat(WidthProperty, width);
                target.SetPropertyBlock(_properties);
            }
        }

        private void Reset()
        {
            _animal = GetComponentInParent<Animal>();
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }
    }
}
