using System;
using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>
    /// Applies the outcome of an encounter: the eaten animal dies and the predator is notified,
    /// or, when nobody gets eaten, the animals react through <see cref="IRepelStrategy"/>.
    /// </summary>
    public sealed class FoodChainService : IAnimalEncounterHandler, IFoodChainEvents
    {
        private readonly IEncounterRule _rule;
        private readonly IRepelStrategy _repel;

        public FoodChainService(IEncounterRule rule, IRepelStrategy repel)
        {
            _rule = rule;
            _repel = repel;
        }

        public event Action<Animal, Animal> PreyEaten;

        public void HandleEncounter(Animal first, Animal second)
        {
            if (!first.IsAlive || !second.IsAlive)
            {
                return;
            }

            var encounter = _rule.Resolve(first, second);
            if (encounter.IsHunt)
            {
                Hunt(encounter.Predator, encounter.Prey);
            }
            else
            {
                _repel.Repel(first, second);
            }
        }

        private void Hunt(Animal predator, Animal prey)
        {
            prey.Die();
            predator.NotifyAte(prey);
            PreyEaten?.Invoke(predator, prey);
        }
    }
}
