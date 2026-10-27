using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Spawning;
using ZooWorld.Utils;
using ZooWorld.World;
using Random = UnityEngine.Random;

namespace ZooWorld.Controls
{
    public sealed class ZooCommands : IZooCommands, IDisposable
    {
        private const float SpawnClearance = 1.6f;
        private const float EncounterGap = 2.6f;
        private const float EncounterPush = 4f;
        private const float MaxTimeScale = 5f;

        private readonly IAnimalSpawner _spawner;
        private readonly IAnimalRegistry _registry;
        private readonly IFreeSpotFinder _freeSpots;
        private readonly ObstacleSpawner _obstacles;
        private readonly IAnimalPicker _picker;
        private readonly AnimalCatalog _catalog;
        private readonly List<Animal> _species;

        public ZooCommands(
            AnimalCatalog catalog,
            IAnimalSpawner spawner,
            IAnimalRegistry registry,
            IFreeSpotFinder freeSpots,
            ObstacleSpawner obstacles,
            IAnimalPicker picker)
        {
            _catalog = catalog;
            _spawner = spawner;
            _registry = registry;
            _freeSpots = freeSpots;
            _obstacles = obstacles;
            _picker = picker;
            _species = catalog.Entries.Select(entry => entry.Prefab).Where(prefab => prefab != null).ToList();
        }

        public IReadOnlyList<Animal> Species => _species;

        public bool IsSpawningPaused
        {
            get => _spawner.IsPaused;
            set => _spawner.IsPaused = value;
        }

        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = Mathf.Clamp(value, 0f, MaxTimeScale);
        }

        public int ObstacleCount => _obstacles.Placed.Count;

        public Animal Spawn(Animal species)
        {
            return species != null && _freeSpots.TryFind(SpawnClearance, out var point)
                ? _spawner.Spawn(species, point, GroundDirection.Random())
                : null;
        }

        public int SpawnRandom(int count)
        {
            var spawned = 0;
            for (var i = 0; i < count; i++)
            {
                spawned += Spawn(_catalog.PickRandom()) != null ? 1 : 0;
            }

            return spawned;
        }

        public bool ForceEncounter(Animal first, Animal second)
        {
            // Each animal lands half a gap from the center and needs its own clearance around it.
            if (first == null || second == null || !_freeSpots.TryFind(EncounterGap * 0.5f + SpawnClearance, out var center))
            {
                return false;
            }

            var axis = GroundDirection.Random();
            var a = _spawner.Spawn(first, center - axis * (EncounterGap * 0.5f), axis);
            var b = _spawner.Spawn(second, center + axis * (EncounterGap * 0.5f), -axis);
            a.Push(axis * EncounterPush);
            b.Push(-axis * EncounterPush);
            return true;
        }

        public void RebuildObstacles() => _obstacles.Rebuild();

        public Animal KillRandom()
        {
            if (_registry.AliveCount == 0)
            {
                return null;
            }

            var victim = _registry.Alive.ElementAt(Random.Range(0, _registry.AliveCount));
            victim.Die();
            return victim;
        }

        public Animal KillAt(Vector2 screenPoint)
        {
            if (!_picker.TryPick(screenPoint, out var animal))
            {
                return null;
            }

            animal.Die();
            return animal;
        }

        public int KillAll()
        {
            var alive = _registry.Alive.ToArray();
            foreach (var animal in alive)
            {
                animal.Die();
            }

            return alive.Length;
        }

        public void Dispose()
        {
            // Time scale is global; do not leak a paused or fast-forwarded game into the next scene.
            Time.timeScale = 1f;
        }
    }
}
