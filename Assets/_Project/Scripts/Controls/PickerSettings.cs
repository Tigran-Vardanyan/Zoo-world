using System;
using UnityEngine;

namespace ZooWorld.Controls
{
    [Serializable]
    public sealed class PickerSettings
    {
        [Tooltip("Layers animals are on. Obstacles are not included, so animals under a tree can still be picked.")]
        [SerializeField] private LayerMask _animalLayers;

        [Tooltip("Tolerance around the click, in world units, so small or moving animals are easy to hit (also for fingers).")]
        [SerializeField, Min(0f)] private float _radius = 0.5f;

        public PickerSettings()
        {
        }

        public PickerSettings(LayerMask animalLayers, float radius)
        {
            _animalLayers = animalLayers;
            _radius = radius;
        }

        public LayerMask AnimalLayers => _animalLayers;
        public float Radius => _radius;
    }
}
