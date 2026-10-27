using NUnit.Framework;
using ZooWorld.Animals;
using ZooWorld.FoodChain;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.FoodChain
{
    public sealed class FoodChainServiceTests
    {
        private AnimalBuilder _animals;
        private StubPredatorDuel _duel;
        private SpyRepelStrategy _repel;
        private FoodChainService _service;

        private AnimalConfig _frog;
        private AnimalConfig _snake;

        [SetUp]
        public void SetUp()
        {
            _duel = new StubPredatorDuel();
            _repel = new SpyRepelStrategy();
            _service = new FoodChainService(new DietEncounterRule(_duel), _repel);
            _animals = new AnimalBuilder(_service);

            _frog = _animals.Species(AnimalRole.Prey);
            _snake = _animals.Species(AnimalRole.Predator);
            _animals.SetDiet(_snake, _frog, _snake);
        }

        [TearDown]
        public void TearDown() => _animals.Dispose();

        [Test]
        public void Hunt_PreyDies_PredatorSurvives()
        {
            var snake = _animals.Alive(_snake);
            var frog = _animals.Alive(_frog);

            _service.HandleEncounter(frog, snake);

            Assert.That(frog.IsAlive, Is.False);
            Assert.That(snake.IsAlive, Is.True);
        }

        [Test]
        public void Hunt_RaisesPreyEaten_WithPredatorAndPrey()
        {
            var snake = _animals.Alive(_snake);
            var frog = _animals.Alive(_frog);
            (Animal Predator, Animal Prey)? eaten = null;
            _service.PreyEaten += (predator, prey) => eaten = (predator, prey);

            _service.HandleEncounter(snake, frog);

            Assert.That(eaten, Is.EqualTo((snake, frog)));
        }

        [Test]
        public void Hunt_PredatorIsNotifiedThatItAte()
        {
            var snake = _animals.Alive(_snake);
            var frog = _animals.Alive(_frog);
            Animal ateWhat = null;
            snake.Ate += food => ateWhat = food;

            _service.HandleEncounter(snake, frog);

            Assert.That(ateWhat, Is.SameAs(frog));
        }

        [Test]
        public void Hunt_PreyRaisesDied_AndNobodyIsRepelled()
        {
            var snake = _animals.Alive(_snake);
            var frog = _animals.Alive(_frog);
            var diedCount = 0;
            frog.Died += _ => diedCount++;

            _service.HandleEncounter(snake, frog);

            Assert.That(diedCount, Is.EqualTo(1));
            Assert.That(_repel.Calls, Is.Empty);
        }

        [Test]
        public void PredatorDuel_LoserDies_WinnerSurvives()
        {
            var winner = _animals.Alive(_snake);
            var loser = _animals.Alive(_snake);
            _duel.Winner = winner;

            _service.HandleEncounter(loser, winner);

            Assert.That(winner.IsAlive, Is.True);
            Assert.That(loser.IsAlive, Is.False);
        }

        [Test]
        public void HarmlessEncounter_BothSurvive_AndAreRepelled()
        {
            var first = _animals.Alive(_frog);
            var second = _animals.Alive(_frog);
            var eatenCount = 0;
            _service.PreyEaten += (_, _) => eatenCount++;

            _service.HandleEncounter(first, second);

            Assert.That(first.IsAlive && second.IsAlive, Is.True);
            Assert.That(eatenCount, Is.Zero);
            Assert.That(_repel.Calls, Is.EqualTo(new[] { (first, second) }));
        }

        [Test]
        public void EncounterWithDeadAnimal_IsIgnored()
        {
            var snake = _animals.Alive(_snake);
            var deadFrog = _animals.Alive(_frog);
            deadFrog.Die();
            var eatenCount = 0;
            _service.PreyEaten += (_, _) => eatenCount++;

            _service.HandleEncounter(snake, deadFrog);

            Assert.That(eatenCount, Is.Zero);
            Assert.That(_repel.Calls, Is.Empty);
        }

        [Test]
        public void SameEncounterReportedTwice_PreyIsEatenOnce()
        {
            var snake = _animals.Alive(_snake);
            var frog = _animals.Alive(_frog);
            var eatenCount = 0;
            _service.PreyEaten += (_, _) => eatenCount++;

            _service.HandleEncounter(snake, frog);
            _service.HandleEncounter(frog, snake);

            Assert.That(eatenCount, Is.EqualTo(1));
        }
    }
}
