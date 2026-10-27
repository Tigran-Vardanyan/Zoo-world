using UnityEngine;

namespace ZooWorld.Animals.Presentation
{
    /// <summary>
    /// Rolls the animal's model belly-up when it dies, with a small hop. Used for animals that have no
    /// death animation of their own; the model is restored when the animal is spawned again.
    /// </summary>
    public sealed class FlipOnDeath : MonoBehaviour
    {
        [SerializeField] private Animal _animal;

        [Tooltip("The model to flip (a child of the animal), not the physics root.")]
        [SerializeField] private Transform _visual;

        [SerializeField, Min(0.01f)] private float _duration = 0.4f;

        [Tooltip("How much the model is raised once upside down, so it lies on the ground instead of under it.")]
        [SerializeField, Min(0f)] private float _lift = 0.12f;

        [SerializeField, Min(0f)] private float _hopHeight = 0.3f;

        private Vector3 _restPosition;
        private Quaternion _restRotation;
        private float _elapsed;
        private bool _isFlipping;

        private void Awake()
        {
            _restPosition = _visual.localPosition;
            _restRotation = _visual.localRotation;
        }

        private void OnEnable()
        {
            _animal.Spawned += OnSpawned;
            _animal.Died += OnDied;
        }

        private void OnDisable()
        {
            _animal.Spawned -= OnSpawned;
            _animal.Died -= OnDied;
        }

        private void Update()
        {
            if (!_isFlipping)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(_elapsed / _duration);
            var eased = Mathf.SmoothStep(0f, 1f, progress);

            var height = _lift * eased + _hopHeight * Mathf.Sin(progress * Mathf.PI);
            _visual.localPosition = _restPosition + Vector3.up * height;
            _visual.localRotation = Quaternion.AngleAxis(180f * eased, Vector3.forward) * _restRotation;

            _isFlipping = progress < 1f;
        }

        private void OnSpawned(Animal animal)
        {
            _isFlipping = false;
            _visual.localPosition = _restPosition;
            _visual.localRotation = _restRotation;
        }

        private void OnDied(Animal animal)
        {
            _elapsed = 0f;
            _isFlipping = true;
        }

        private void Reset()
        {
            _animal = GetComponentInParent<Animal>();
        }
    }
}
