using System;
using UnityEngine;

namespace ZooWorld.World
{
    [Serializable]
    public sealed class PlayAreaSettings
    {
        [Tooltip("World Y of the ground plane the camera frustum is projected on.")]
        [SerializeField] private float _groundHeight;

        [Tooltip("Shrinks the visible area so animals turn back before touching the screen edge.")]
        [SerializeField, Min(0f)] private float _padding = 0.5f;

        public float GroundHeight => _groundHeight;
        public float Padding => _padding;
    }
}
