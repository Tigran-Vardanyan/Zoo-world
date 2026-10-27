using System.Collections.Generic;
using UnityEngine;

namespace ZooWorld.Animals
{
    /// <summary>
    /// Data shared by every instance of one animal species.
    /// Behaviour lives in components on the prefab; this asset only describes the species.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimalConfig", menuName = "Zoo World/Animal Config")]
    public sealed class AnimalConfig : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [Tooltip("Category used for statistics and UI. Who eats whom is defined by the diet, not by the role.")]
        [SerializeField] private AnimalRole _role;

        [Tooltip("Species this animal eats on contact. Add its own config to make members of the species fight each other.")]
        [SerializeField] private List<AnimalConfig> _diet = new();

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
        public AnimalRole Role => _role;
        public IReadOnlyList<AnimalConfig> Diet => _diet;

        public bool CanEat(AnimalConfig species) => species != null && _diet.Contains(species);
    }
}
