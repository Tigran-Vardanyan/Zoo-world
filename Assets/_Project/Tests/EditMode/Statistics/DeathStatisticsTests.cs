using NUnit.Framework;
using ZooWorld.Animals;
using ZooWorld.Statistics;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.Statistics
{
    public sealed class DeathStatisticsTests
    {
        private AnimalBuilder _animals;
        private AnimalRegistry _registry;
        private DeathStatistics _statistics;

        [SetUp]
        public void SetUp()
        {
            _animals = new AnimalBuilder();
            _registry = new AnimalRegistry();
            _statistics = new DeathStatistics(_registry);
            _statistics.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _statistics.Dispose();
            _registry.Dispose();
            _animals.Dispose();
        }

        [Test]
        public void StartsAtZero()
        {
            Assert.That(_statistics.DeadPrey, Is.Zero);
            Assert.That(_statistics.DeadPredators, Is.Zero);
        }

        [Test]
        public void CountsDeadAnimalsByTheirRole()
        {
            Kill(AnimalRole.Prey);
            Kill(AnimalRole.Prey);
            Kill(AnimalRole.Predator);

            Assert.That(_statistics.DeadPrey, Is.EqualTo(2));
            Assert.That(_statistics.DeadPredators, Is.EqualTo(1));
        }

        [Test]
        public void RaisesChangedOnEveryDeath()
        {
            var changedCount = 0;
            _statistics.Changed += () => changedCount++;

            Kill(AnimalRole.Prey);

            Assert.That(changedCount, Is.EqualTo(1));
        }

        [Test]
        public void StopsCountingAfterDispose()
        {
            _statistics.Dispose();

            Kill(AnimalRole.Prey);

            Assert.That(_statistics.DeadPrey, Is.Zero);
        }

        private void Kill(AnimalRole role)
        {
            var animal = _animals.Alive(_animals.Species(role));
            _registry.Register(animal);
            animal.Die();
        }
    }
}
