using ZooWorld.Animals;
using ZooWorld.Utils;

namespace ZooWorld.FoodChain
{
    /// <summary>Pushes both animals away from each other with an equal impulse.</summary>
    public sealed class ImpulseRepel : IRepelStrategy
    {
        private readonly FoodChainSettings _settings;

        public ImpulseRepel(FoodChainSettings settings)
        {
            _settings = settings;
        }

        public void Repel(Animal first, Animal second)
        {
            var direction = GroundDirection.Flatten(second.transform.position - first.transform.position);
            first.Push(-direction * _settings.RepelImpulse);
            second.Push(direction * _settings.RepelImpulse);
        }
    }
}
