using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>
    /// Food chain defined by each species' diet: an animal eats what is in its diet,
    /// animals that cannot eat each other are harmless to each other,
    /// and when both can eat each other <see cref="IPredatorDuel"/> picks the survivor.
    /// </summary>
    public sealed class DietEncounterRule : IEncounterRule
    {
        private readonly IPredatorDuel _duel;

        public DietEncounterRule(IPredatorDuel duel)
        {
            _duel = duel;
        }

        public Encounter Resolve(Animal first, Animal second)
        {
            var firstEatsSecond = first.Config.CanEat(second.Config);
            var secondEatsFirst = second.Config.CanEat(first.Config);

            return (firstEatsSecond, secondEatsFirst) switch
            {
                (true, true) => ResolveDuel(first, second),
                (true, false) => Encounter.Hunt(first, second),
                (false, true) => Encounter.Hunt(second, first),
                _ => Encounter.Harmless,
            };
        }

        private Encounter ResolveDuel(Animal first, Animal second)
        {
            var winner = _duel.ChooseWinner(first, second);
            var loser = winner == first ? second : first;
            return Encounter.Hunt(winner, loser);
        }
    }
}
