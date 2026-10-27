using UnityEngine;

namespace ZooWorld.World
{
    /// <summary>
    /// Marks a static object that animals bounce off. Put it on the root of an obstacle prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Obstacle : MonoBehaviour
    {
        [Tooltip("Radius of the area this obstacle covers on the ground; used to keep obstacles apart when placed.")]
        [SerializeField, Min(0.1f)] private float _footprintRadius = 1f;

        public float FootprintRadius => _footprintRadius;
    }
}
