using System;
using System.Collections.Generic;

namespace ZooWorld.Animals
{
    public sealed class AnimalRegistry : IAnimalRegistry, IDisposable
    {
        private readonly HashSet<Animal> _alive = new();

        public event Action<Animal> AnimalDied;

        public int AliveCount => _alive.Count;
        public IReadOnlyCollection<Animal> Alive => _alive;

        public void Register(Animal animal)
        {
            if (_alive.Add(animal))
            {
                animal.Died += OnAnimalDied;
            }
        }

        public void Dispose()
        {
            foreach (var animal in _alive)
            {
                animal.Died -= OnAnimalDied;
            }

            _alive.Clear();
        }

        private void OnAnimalDied(Animal animal)
        {
            animal.Died -= OnAnimalDied;
            _alive.Remove(animal);
            AnimalDied?.Invoke(animal);
        }
    }
}
