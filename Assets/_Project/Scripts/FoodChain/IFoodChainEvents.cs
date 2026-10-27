using System;
using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    public interface IFoodChainEvents
    {
        /// <summary>Raised after <c>prey</c> died; arguments are (predator, prey).</summary>
        event Action<Animal, Animal> PreyEaten;
    }
}
