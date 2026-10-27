using NUnit.Framework;
using ZooWorld.Animals;
using ZooWorld.FoodChain;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.FoodChain
{
    public sealed class DietEncounterRuleTests
    {
        private AnimalBuilder _animals;
        private StubPredatorDuel _duel;
        private DietEncounterRule _rule;

        private AnimalConfig _frog;
        private AnimalConfig _snake;

        [SetUp]
        public void SetUp()
        {
            _animals = new AnimalBuilder();
            _duel = new StubPredatorDuel();
            _rule = new DietEncounterRule(_duel);

            _frog = _animals.Species(AnimalRole.Prey);
            _snake = _animals.Species(AnimalRole.Predator);
            _animals.SetDiet(_snake, _frog, _snake);
        }

        [TearDown]
        public void TearDown() => _animals.Dispose();

        [Test]
        public void NeitherEatsTheOther_IsHarmless()
        {
            var encounter = _rule.Resolve(_animals.Create(_frog), _animals.Create(_frog));

            Assert.That(encounter.IsHunt, Is.False);
        }

        [Test]
        public void FirstEatsSecond_FirstHunts()
        {
            var snake = _animals.Create(_snake);
            var frog = _animals.Create(_frog);

            var encounter = _rule.Resolve(snake, frog);

            Assert.That(encounter.Predator, Is.SameAs(snake));
            Assert.That(encounter.Prey, Is.SameAs(frog));
        }

        [Test]
        public void SecondEatsFirst_SecondHunts()
        {
            var snake = _animals.Create(_snake);
            var frog = _animals.Create(_frog);

            var encounter = _rule.Resolve(frog, snake);

            Assert.That(encounter.Predator, Is.SameAs(snake));
            Assert.That(encounter.Prey, Is.SameAs(frog));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BothEatEachOther_DuelWinnerEatsLoser(int winnerIndex)
        {
            var snakes = new[] { _animals.Create(_snake), _animals.Create(_snake) };
            _duel.Winner = snakes[winnerIndex];

            var encounter = _rule.Resolve(snakes[0], snakes[1]);

            Assert.That(encounter.Predator, Is.SameAs(snakes[winnerIndex]));
            Assert.That(encounter.Prey, Is.SameAs(snakes[1 - winnerIndex]));
        }

        [Test]
        public void DietIsPerSpecies_NotPerRole()
        {
            // A predator that only eats spiders ignores frogs, even though frogs are prey.
            var bird = _animals.Species(AnimalRole.Predator);
            var spider = _animals.Species(AnimalRole.Prey);
            _animals.SetDiet(bird, spider);

            Assert.That(_rule.Resolve(_animals.Create(bird), _animals.Create(_frog)).IsHunt, Is.False);
            Assert.That(_rule.Resolve(_animals.Create(bird), _animals.Create(spider)).IsHunt, Is.True);
        }

        [Test]
        public void PredatorsOfDifferentSpecies_WithoutEachOtherInDiet_AreHarmless()
        {
            var bird = _animals.Species(AnimalRole.Predator);

            Assert.That(_rule.Resolve(_animals.Create(bird), _animals.Create(_snake)).IsHunt, Is.False);
        }
    }
}
