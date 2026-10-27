namespace ZooWorld.Animals
{
    /// <summary>
    /// Receives collisions between two living animals. Animals only report what happened;
    /// what it means (eat, fight, bounce) is decided by the implementation.
    /// </summary>
    public interface IAnimalEncounterHandler
    {
        void HandleEncounter(Animal first, Animal second);
    }
}
