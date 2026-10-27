using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>
    /// Outcome of two animals meeting: either one eats the other, or nothing happens.
    /// </summary>
    public readonly struct Encounter
    {
        public static readonly Encounter Harmless = default;

        private Encounter(Animal predator, Animal prey)
        {
            Predator = predator;
            Prey = prey;
        }

        public Animal Predator { get; }
        public Animal Prey { get; }
        public bool IsHunt => Predator != null;

        public static Encounter Hunt(Animal predator, Animal prey) => new(predator, prey);
    }
}
