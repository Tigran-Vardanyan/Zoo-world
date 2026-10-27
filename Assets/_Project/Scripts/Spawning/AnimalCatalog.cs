using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.Spawning
{
    /// <summary>
    /// Animals that can appear in the zoo and how often. Adding a species = adding its prefab here.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimalCatalog", menuName = "Zoo World/Animal Catalog")]
    public sealed class AnimalCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private Animal _prefab;
            [SerializeField, Min(0f)] private float _weight = 1f;

            public Animal Prefab => _prefab;
            public float Weight => _weight;
        }

        [SerializeField] private List<Entry> _entries = new();

        public IReadOnlyList<Entry> Entries => _entries;

        /// <summary>Weighted random pick; returns null when the catalog has no valid entries.</summary>
        public Animal PickRandom()
        {
            var totalWeight = 0f;
            foreach (var entry in _entries)
            {
                if (entry.Prefab != null)
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            var roll = UnityEngine.Random.value * totalWeight;
            foreach (var entry in _entries)
            {
                if (entry.Prefab == null)
                {
                    continue;
                }

                roll -= entry.Weight;
                if (roll <= 0f)
                {
                    return entry.Prefab;
                }
            }

            return _entries.FindLast(entry => entry.Prefab != null).Prefab;
        }
    }
}
