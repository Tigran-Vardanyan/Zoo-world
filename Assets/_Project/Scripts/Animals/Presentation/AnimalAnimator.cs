using UnityEngine;

namespace ZooWorld.Animals.Presentation
{
    /// <summary>
    /// Drives an animal's Animator from what the animal does. The controller decides what each species
    /// shows; this component only feeds it the same parameters for every animal:
    /// <list type="bullet">
    /// <item><c>Speed</c> (float) - horizontal speed, e.g. to switch between standing and running.</item>
    /// <item><c>Die</c> (trigger) - fired once when the animal dies.</item>
    /// </list>
    /// </summary>
    public sealed class AnimalAnimator : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int DieParameter = Animator.StringToHash("Die");

        [SerializeField] private Animal _animal;
        [SerializeField] private Animator _animator;

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
            if (!_animal.IsAlive)
            {
                return;
            }

            var velocity = _animal.Body.linearVelocity;
            velocity.y = 0f;
            _animator.SetFloat(SpeedParameter, velocity.magnitude);
        }

        private void OnSpawned(Animal animal)
        {
            // Pooled instances come back in whatever state they died in; start over from the default state.
            _animator.Rebind();
            _animator.Update(0f);
        }

        private void OnDied(Animal animal)
        {
            _animator.SetFloat(SpeedParameter, 0f);
            _animator.SetTrigger(DieParameter);
        }

        private void Reset()
        {
            _animal = GetComponentInParent<Animal>();
            _animator = GetComponentInChildren<Animator>();
        }
    }
}
