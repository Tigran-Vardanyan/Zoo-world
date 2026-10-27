using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>
    /// Decides who eats whom. Swap the implementation in the container to change the food chain
    /// (e.g. size-based or species-specific diets) without touching animals.
    /// </summary>
    public interface IEncounterRule
    {
        Encounter Resolve(Animal first, Animal second);
    }
}
