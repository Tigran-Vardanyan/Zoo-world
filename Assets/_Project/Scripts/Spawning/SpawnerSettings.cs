using System;
using UnityEngine;

namespace ZooWorld.Spawning
{
    [Serializable]
    public sealed class SpawnerSettings
    {
        [Tooltip("Random delay between two spawns, in seconds (x = min, y = max).")]
        [SerializeField] private Vector2 _intervalRange = new(1f, 2f);

        [Tooltip("Height above the ground at which animals appear.")]
        [SerializeField, Min(0f)] private float _spawnHeight = 0.5f;

        [Tooltip("Free radius required around a spawn point; no animal or obstacle may be inside it.")]
        [SerializeField, Min(0.1f)] private float _clearance = 1.2f;

        [Tooltip("Delay before trying again when no free spot was found.")]
        [SerializeField, Min(0.02f)] private float _retryDelay = 0.2f;

        [Tooltip("Spawning pauses while this many animals are alive. 0 = unlimited.")]
        [SerializeField, Min(0)] private int _maxAliveAnimals = 50;

        public float NextInterval() => UnityEngine.Random.Range(_intervalRange.x, Mathf.Max(_intervalRange.x, _intervalRange.y));
        public float SpawnHeight => _spawnHeight;
        public float Clearance => _clearance;
        public float RetryDelay => _retryDelay;
        public int MaxAliveAnimals => _maxAliveAnimals;
    }
}
