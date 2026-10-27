using System;
using UnityEngine;

namespace ZooWorld.World
{
    [Serializable]
    public sealed class FreeSpotSettings
    {
        [Tooltip("Layers that make a spot occupied (animals, obstacles). Never include the ground.")]
        [SerializeField] private LayerMask _blockingLayers;

        [Tooltip("Height above the ground at which the occupancy check is made.")]
        [SerializeField, Min(0f)] private float _probeHeight = 0.5f;

        [Tooltip("Random points tried before giving up for this frame.")]
        [SerializeField, Min(1)] private int _maxAttempts = 20;

        public FreeSpotSettings()
        {
        }

        public FreeSpotSettings(LayerMask blockingLayers, float probeHeight, int maxAttempts)
        {
            _blockingLayers = blockingLayers;
            _probeHeight = probeHeight;
            _maxAttempts = maxAttempts;
        }

        public LayerMask BlockingLayers => _blockingLayers;
        public float ProbeHeight => _probeHeight;
        public int MaxAttempts => _maxAttempts;
    }
}
