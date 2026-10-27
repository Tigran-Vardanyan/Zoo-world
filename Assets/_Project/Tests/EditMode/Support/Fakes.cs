using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.FoodChain;
using ZooWorld.World;

namespace ZooWorld.Tests.Support
{
    internal sealed class FakePlayArea : IPlayArea
    {
        public bool Contains(Vector3 position) => true;
        public Vector3 GetRandomPoint(float inset = 0f) => Vector3.zero;
        public Vector3 RedirectInside(Vector3 position, Vector3 direction) => direction;
    }

    /// <summary>Predator duel whose winner is chosen by the test.</summary>
    internal sealed class StubPredatorDuel : IPredatorDuel
    {
        public Animal Winner { get; set; }

        public Animal ChooseWinner(Animal first, Animal second) => Winner;
    }

    internal sealed class SpyRepelStrategy : IRepelStrategy
    {
        public List<(Animal First, Animal Second)> Calls { get; } = new();

        public void Repel(Animal first, Animal second) => Calls.Add((first, second));
    }
}
