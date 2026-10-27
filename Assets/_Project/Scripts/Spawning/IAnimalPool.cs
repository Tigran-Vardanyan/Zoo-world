using ZooWorld.Animals;

namespace ZooWorld.Spawning
{
    public interface IAnimalPool
    {
        /// <summary>
        /// Returns an inactive instance of <paramref name="prefab"/>, ready for <see cref="Animal.Spawn"/>.
        /// The instance goes back to the pool by itself when its corpse is despawned.
        /// </summary>
        Animal Get(Animal prefab);
    }
}
