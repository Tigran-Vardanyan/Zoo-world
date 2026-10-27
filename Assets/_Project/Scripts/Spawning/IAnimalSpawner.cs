using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.Spawning
{
    public interface IAnimalSpawner
    {
        /// <summary>Stops the automatic 1-2 second spawning; <see cref="Spawn"/> still works.</summary>
        bool IsPaused { get; set; }

        /// <summary>Takes an instance of <paramref name="prefab"/> from the pool and releases it at a ground point.</summary>
        Animal Spawn(Animal prefab, Vector3 groundPoint, Vector3 direction);
    }
}
