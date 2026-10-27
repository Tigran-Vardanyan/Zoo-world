using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.World;

namespace ZooWorld.Spawning
{
    [Serializable]
    public sealed class ObstacleSettings
    {
        [Tooltip("Obstacle prefabs picked at random.")]
        [SerializeField] private List<Obstacle> _prefabs = new();

        [Tooltip("How many obstacles are placed when the game starts (x = min, y = max, inclusive).")]
        [SerializeField] private Vector2Int _countRange = new(4, 7);

        [Tooltip("Extra free space kept between an obstacle and anything else.")]
        [SerializeField, Min(0f)] private float _spacing = 1.5f;

        public IReadOnlyList<Obstacle> Prefabs => _prefabs;
        public float Spacing => _spacing;

        public int NextCount() => UnityEngine.Random.Range(_countRange.x, Mathf.Max(_countRange.x, _countRange.y) + 1);
    }
}
