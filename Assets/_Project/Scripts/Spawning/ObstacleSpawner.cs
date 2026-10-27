using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.World;

namespace ZooWorld.Spawning
{
    /// <summary>
    /// Places a random set of obstacle prefabs at free points of the play area when the game starts.
    /// </summary>
    public sealed class ObstacleSpawner : IStartable
    {
        private readonly ObstacleSettings _settings;
        private readonly IFreeSpotFinder _freeSpots;
        private readonly IObjectResolver _resolver;
        private readonly Transform _root;
        private readonly List<Obstacle> _placed = new();

        public ObstacleSpawner(ObstacleSettings settings, IFreeSpotFinder freeSpots, IObjectResolver resolver, Transform root)
        {
            _settings = settings;
            _freeSpots = freeSpots;
            _resolver = resolver;
            _root = root;
        }

        public IReadOnlyList<Obstacle> Placed => _placed;

        public void Start() => PlaceObstacles();

        /// <summary>Removes the current obstacles and places a new random set.</summary>
        public void Rebuild()
        {
            foreach (var obstacle in _placed)
            {
                if (obstacle != null)
                {
                    Object.Destroy(obstacle.gameObject);
                }
            }

            _placed.Clear();

            // Destroy is deferred to the end of the frame; move the old colliders out of the way of the free-spot search now.
            foreach (Transform child in _root)
            {
                child.gameObject.SetActive(false);
            }

            PlaceObstacles();
        }

        private void PlaceObstacles()
        {
            if (_settings.Prefabs.Count == 0)
            {
                return;
            }

            var count = _settings.NextCount();
            for (var i = 0; i < count; i++)
            {
                var prefab = _settings.Prefabs[Random.Range(0, _settings.Prefabs.Count)];
                if (prefab != null && _freeSpots.TryFind(prefab.FootprintRadius + _settings.Spacing, out var point))
                {
                    var rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    _placed.Add(_resolver.Instantiate(prefab, point, rotation, _root));
                }
            }
        }
    }
}
