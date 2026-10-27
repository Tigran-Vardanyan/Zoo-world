using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>Coin flip: each predator has an equal chance to survive.</summary>
    public sealed class RandomPredatorDuel : IPredatorDuel
    {
        public Animal ChooseWinner(Animal first, Animal second) => Random.value < 0.5f ? first : second;
    }
}
