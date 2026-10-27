using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>How two animals react when they collide and neither eats the other.</summary>
    public interface IRepelStrategy
    {
        void Repel(Animal first, Animal second);
    }
}
