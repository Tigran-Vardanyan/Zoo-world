using System;
using System.Collections.Generic;

namespace ZooWorld.Animals
{
    /// <summary>
    /// Single source of truth about living animals, whatever spawned them or killed them.
    /// </summary>
    public interface IAnimalRegistry
    {
        event Action<Animal> AnimalDied;

        int AliveCount { get; }
        IReadOnlyCollection<Animal> Alive { get; }

        /// <summary>Starts tracking <paramref name="animal"/> until it dies.</summary>
        void Register(Animal animal);
    }
}
