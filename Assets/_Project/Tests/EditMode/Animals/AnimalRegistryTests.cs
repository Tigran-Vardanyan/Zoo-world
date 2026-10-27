using NUnit.Framework;
using ZooWorld.Animals;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.Animals
{
    public sealed class AnimalRegistryTests
    {
        private AnimalBuilder _animals;
        private AnimalRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _animals = new AnimalBuilder();
            _registry = new AnimalRegistry();
        }

        [TearDown]
        public void TearDown()
        {
            _registry.Dispose();
            _animals.Dispose();
        }

        [Test]
        public void Register_CountsAliveAnimals_OncePerAnimal()
        {
            var animal = _animals.Alive(_animals.Species(AnimalRole.Prey));

            _registry.Register(animal);
            _registry.Register(animal);
            _registry.Register(_animals.Alive(_animals.Species(AnimalRole.Predator)));

            Assert.That(_registry.AliveCount, Is.EqualTo(2));
        }

        [Test]
        public void Death_RemovesAnimal_AndRaisesAnimalDiedOnce()
        {
            var animal = _animals.Alive(_animals.Species(AnimalRole.Prey));
            _registry.Register(animal);
            var diedCount = 0;
            _registry.AnimalDied += dead => diedCount += dead == animal ? 1 : 0;

            animal.Die();
            animal.Die();

            Assert.That(_registry.AliveCount, Is.Zero);
            Assert.That(diedCount, Is.EqualTo(1));
        }

        [Test]
        public void RespawnedAnimal_IsTrackedAgain()
        {
            var animal = _animals.Alive(_animals.Species(AnimalRole.Prey));
            _registry.Register(animal);
            animal.Die();
            var diedCount = 0;
            _registry.AnimalDied += _ => diedCount++;

            _registry.Register(animal);
            animal.Spawn(UnityEngine.Vector3.zero, UnityEngine.Vector3.forward);
            animal.Die();

            Assert.That(diedCount, Is.EqualTo(1));
        }
    }
}
