using NUnit.Framework;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.FoodChain;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.FoodChain
{
    public sealed class RandomPredatorDuelTests
    {
        private AnimalBuilder _animals;

        [SetUp]
        public void SetUp() => _animals = new AnimalBuilder();

        [TearDown]
        public void TearDown() => _animals.Dispose();

        [Test]
        public void ChooseWinner_ReturnsEachPredatorSometimes_AndNothingElse()
        {
            var first = _animals.Create(AnimalRole.Predator);
            var second = _animals.Create(AnimalRole.Predator);
            var duel = new RandomPredatorDuel();
            Random.InitState(12345);

            int firstWins = 0, secondWins = 0;
            for (var i = 0; i < 200; i++)
            {
                var winner = duel.ChooseWinner(first, second);
                Assert.That(winner, Is.SameAs(first).Or.SameAs(second));
                if (winner == first)
                {
                    firstWins++;
                }
                else
                {
                    secondWins++;
                }
            }

            Assert.That(firstWins, Is.GreaterThan(0));
            Assert.That(secondWins, Is.GreaterThan(0));
        }
    }
}
