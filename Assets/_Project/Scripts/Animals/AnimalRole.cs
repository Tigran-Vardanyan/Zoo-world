namespace ZooWorld.Animals
{
    /// <summary>
    /// Category of a species, used for statistics and UI.
    /// Who eats whom is defined by <see cref="AnimalConfig.Diet"/>.
    /// </summary>
    public enum AnimalRole
    {
        Prey = 0,
        Predator = 1,
    }
}
