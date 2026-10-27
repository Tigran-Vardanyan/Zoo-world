using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using ZooWorld.Animals;

namespace ZooWorld.Spawning
{
    /// <summary>
    /// One object pool per animal prefab. Instances are created through the container,
    /// so their [Inject] members are filled in, and are returned to their pool when their corpse is despawned.
    /// </summary>
    public sealed class AnimalPool : IAnimalPool, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly Transform _root;
        private readonly Dictionary<Animal, ObjectPool<Animal>> _poolsByPrefab = new();
        private readonly Dictionary<Animal, ObjectPool<Animal>> _poolsByInstance = new();

        public AnimalPool(IObjectResolver resolver, Transform root)
        {
            _resolver = resolver;
            _root = root;
        }

        public Animal Get(Animal prefab)
        {
            if (!_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = CreatePool(prefab);
                _poolsByPrefab.Add(prefab, pool);
            }

            return pool.Get();
        }

        public void Dispose()
        {
            foreach (var pool in _poolsByPrefab.Values)
            {
                pool.Dispose();
            }

            _poolsByPrefab.Clear();
            _poolsByInstance.Clear();
        }

        private ObjectPool<Animal> CreatePool(Animal prefab)
        {
            ObjectPool<Animal> pool = null;
            pool = new ObjectPool<Animal>(
                createFunc: () => CreateInstance(prefab, pool),
                actionOnRelease: animal => animal.gameObject.SetActive(false),
                actionOnDestroy: DestroyInstance,
                collectionCheck: false);
            return pool;
        }

        private Animal CreateInstance(Animal prefab, ObjectPool<Animal> pool)
        {
            var animal = _resolver.Instantiate(prefab, _root);
            animal.gameObject.SetActive(false);
            animal.Despawned += Release;
            _poolsByInstance.Add(animal, pool);
            return animal;
        }

        private void Release(Animal animal)
        {
            _poolsByInstance[animal].Release(animal);
        }

        private void DestroyInstance(Animal animal)
        {
            _poolsByInstance.Remove(animal);
            if (animal != null)
            {
                animal.Despawned -= Release;
                UnityEngine.Object.Destroy(animal.gameObject);
            }
        }
    }
}
