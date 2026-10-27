using ZooWorld.Animals;

namespace ZooWorld.Controls
{
    /// <summary>Describes, from the species' diets, what should happen when two animals meet.</summary>
    public static class EncounterForecast
    {
        public static string Describe(AnimalConfig first, AnimalConfig second)
        {
            var firstEats = first.CanEat(second);
            var secondEats = second.CanEat(first);

            if (firstEats && secondEats)
            {
                return "Duel: one random survivor says \"Tasty!\"";
            }

            if (firstEats || secondEats)
            {
                var predator = firstEats ? first : second;
                var prey = firstEats ? second : first;
                return $"The {predator.DisplayName} eats the {prey.DisplayName}";
            }

            return "They bounce apart, both survive";
        }
    }
}
