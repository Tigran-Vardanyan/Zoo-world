using System;
using UnityEngine;

namespace ZooWorld.FoodChain
{
    [Serializable]
    public sealed class FoodChainSettings
    {
        [Tooltip("Impulse that pushes two animals apart when neither eats the other.")]
        [SerializeField, Min(0f)] private float _repelImpulse = 2f;

        public float RepelImpulse => _repelImpulse;
    }
}
