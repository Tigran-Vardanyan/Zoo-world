using UnityEngine;
using VContainer.Unity;
using ZooWorld.Animals;
using ZooWorld.Utils;
using ZooWorld.World;

namespace ZooWorld.Spawning
{
    /// <summary>
    /// Every 1-2 seconds takes a random animal from the catalog and releases it at a free point of the play area.
    /// When every point tried is occupied, it retries shortly instead of spawning on top of something.
    /// </summary>
    public sealed class AnimalSpawner : IAnimalSpawner, IStartable, ITickable
    {
        private readonly AnimalCatalog _catalog;
        private readonly IAnimalPool _pool;
        private readonly IAnimalRegistry _registry;
        private readonly IFreeSpotFinder _freeSpots;
        private readonly SpawnerSettings _settings;

        private float _timeUntilSpawn;

        public AnimalSpawner(
            AnimalCatalog catalog,
            IAnimalPool pool,
            IAnimalRegistry registry,
            IFreeSpotFinder freeSpots,
            SpawnerSettings settings)
        {
            _catalog = catalog;
            _pool = pool;
            _registry = registry;
            _freeSpots = freeSpots;
            _settings = settings;
        }

        public bool IsPaused { get; set; }

        public void Start()
        {
            _timeUntilSpawn = _settings.NextInterval();
        }

        public void Tick()
        {
            if (IsPaused)
            {
                return;
            }

            _timeUntilSpawn -= Time.deltaTime;
            if (_timeUntilSpawn > 0f)
            {
                return;
            }

            if (IsFull() || TrySpawnRandomAnimal())
            {
                _timeUntilSpawn = _settings.NextInterval();
            }
            else
            {
                _timeUntilSpawn = _settings.RetryDelay;
            }
        }

        public Animal Spawn(Animal prefab, Vector3 groundPoint, Vector3 direction)
        {
            var animal = _pool.Get(prefab);
            _registry.Register(animal);
            animal.Spawn(groundPoint + Vector3.up * _settings.SpawnHeight, direction);
            return animal;
        }

        private bool IsFull() => _settings.MaxAliveAnimals > 0 && _registry.AliveCount >= _settings.MaxAliveAnimals;

        private bool TrySpawnRandomAnimal()
        {
            if (!_freeSpots.TryFind(_settings.Clearance, out var point))
            {
                return false;
            }

            var prefab = _catalog.PickRandom();
            if (prefab == null)
            {
                Debug.LogWarning($"{nameof(AnimalSpawner)}: {_catalog.name} has no animals to spawn.", _catalog);
                return true;
            }

            Spawn(prefab, point, GroundDirection.Random());
            return true;
        }
    }
}
