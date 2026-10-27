using System;
using UnityEngine;
using VContainer;
using ZooWorld.Animals.Movement;
using ZooWorld.Utils;

namespace ZooWorld.Animals
{
    /// <summary>
    /// Root component of every animal prefab. Owns the animal's lifecycle (spawn / death)
    /// and delegates behaviour to the components it is composed of.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Animal : MonoBehaviour
    {
        [SerializeField] private AnimalConfig _config;
        [SerializeField] private AnimalMovement _movement;
        [SerializeField] private Rigidbody _body;

        private IAnimalEncounterHandler _encounterHandler;

        public event Action<Animal> Died;

        /// <summary>Raised on the predator after it ate another animal; the argument is the prey.</summary>
        public event Action<Animal> Ate;

        public AnimalConfig Config => _config;
        public AnimalRole Role => _config.Role;
        public bool IsAlive { get; private set; }

        [Inject]
        public void Construct(IAnimalEncounterHandler encounterHandler)
        {
            _encounterHandler = encounterHandler;
        }

        public void Spawn(Vector3 position, Vector3 direction)
        {
            direction = GroundDirection.Flatten(direction);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            IsAlive = true;
            gameObject.SetActive(true);
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
            Died?.Invoke(this);
        }

        public void NotifyAte(Animal prey) => Ate?.Invoke(prey);

        public void Push(Vector3 impulse) => _body.AddForce(impulse, ForceMode.Impulse);

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
