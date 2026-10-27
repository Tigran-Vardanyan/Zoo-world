using System;
using UnityEngine;
using VContainer;
using ZooWorld.Animals.Movement;
using ZooWorld.Utils;

namespace ZooWorld.Animals
{
    /// <summary>
    /// Root component of every animal prefab. Owns the animal's lifecycle (spawn → alive → corpse → despawn)
    /// and delegates behaviour and presentation to the components it is composed of.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Animal : MonoBehaviour
    {
        [SerializeField] private AnimalConfig _config;
        [SerializeField] private AnimalMovement _movement;
        [SerializeField] private Rigidbody _body;

        [Tooltip("How long the body stays on the ground after death (death animation) before it is removed.")]
        [SerializeField, Min(0f)] private float _corpseDuration = 2.5f;

        private IAnimalEncounterHandler _encounterHandler;
        private Collider[] _colliders;
        private float _timeUntilDespawn;
        private bool _isCorpse;

        /// <summary>Raised every time the animal enters the world, including reuse from a pool.</summary>
        public event Action<Animal> Spawned;

        /// <summary>Raised the moment the animal dies; its body stays as a corpse until <see cref="Despawned"/>.</summary>
        public event Action<Animal> Died;

        /// <summary>Raised when the corpse should be removed from the world.</summary>
        public event Action<Animal> Despawned;

        /// <summary>Raised on the predator after it ate another animal; the argument is the prey.</summary>
        public event Action<Animal> Ate;

        public AnimalConfig Config => _config;
        public AnimalRole Role => _config.Role;
        public bool IsAlive { get; private set; }
        public Rigidbody Body => _body;

        [Inject]
        public void Construct(IAnimalEncounterHandler encounterHandler)
        {
            _encounterHandler = encounterHandler;
        }

        public void Spawn(Vector3 position, Vector3 direction)
        {
            direction = GroundDirection.Flatten(direction);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            SetPhysicsEnabled(true);
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            IsAlive = true;
            _isCorpse = false;
            gameObject.SetActive(true);
            Spawned?.Invoke(this);
            _movement.StartMoving(direction);
        }

        public void Die()
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            _movement.StopMoving();

            // The corpse is visual only: it no longer collides, gets eaten or blocks spawning.
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            SetPhysicsEnabled(false);

            _isCorpse = true;
            _timeUntilDespawn = _corpseDuration;
            Died?.Invoke(this);

            if (_corpseDuration <= 0f)
            {
                Despawn();
            }
        }

        /// <summary>Removes the corpse right away instead of waiting for the corpse duration.</summary>
        public void Despawn()
        {
            if (!_isCorpse)
            {
                return;
            }

            _isCorpse = false;
            Despawned?.Invoke(this);
        }

        public void NotifyAte(Animal prey) => Ate?.Invoke(prey);

        public void Push(Vector3 impulse) => _body.AddForce(impulse, ForceMode.Impulse);

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        private void Update()
        {
            if (!_isCorpse)
            {
                return;
            }

            _timeUntilDespawn -= Time.deltaTime;
            if (_timeUntilDespawn <= 0f)
            {
                Despawn();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsAlive || collision.rigidbody == null || !collision.rigidbody.TryGetComponent(out Animal other))
            {
                return;
            }

            // Both animals receive this callback; only the one with the lower instance id reports the encounter.
            if (GetInstanceID() < other.GetInstanceID())
            {
                _encounterHandler.HandleEncounter(this, other);
            }
        }

        private void SetPhysicsEnabled(bool isEnabled)
        {
            _colliders ??= GetComponentsInChildren<Collider>(true);
            foreach (var bodyCollider in _colliders)
            {
                bodyCollider.enabled = isEnabled;
            }

            _body.isKinematic = !isEnabled;
        }

        private void Reset()
        {
            _movement = GetComponent<AnimalMovement>();
            _body = GetComponent<Rigidbody>();
        }

        private void OnValidate()
        {
            if (_config == null)
            {
                Debug.LogWarning($"{name}: {nameof(AnimalConfig)} is not assigned.", this);
            }
        }
    }
}
