using System;
using VContainer.Unity;
using ZooWorld.Animals;

namespace ZooWorld.Statistics
{
    /// <summary>
    /// Counts dead animals by their role, whatever the cause of death.
    /// </summary>
    public sealed class DeathStatistics : IDeathStatistics, IInitializable, IDisposable
    {
        private readonly IAnimalRegistry _registry;

        public DeathStatistics(IAnimalRegistry registry)
        {
            _registry = registry;
        }

        public event Action Changed;

        public int DeadPrey { get; private set; }
        public int DeadPredators { get; private set; }

        public void Initialize()
        {
            _registry.AnimalDied += OnAnimalDied;
        }

        public void Dispose()
        {
            _registry.AnimalDied -= OnAnimalDied;
        }

        private void OnAnimalDied(Animal animal)
        {
            switch (animal.Role)
            {
                case AnimalRole.Prey:
                    DeadPrey++;
                    break;
                case AnimalRole.Predator:
                    DeadPredators++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animal), animal.Role, "Unknown animal role.");
            }

            Changed?.Invoke();
        }
    }
}
