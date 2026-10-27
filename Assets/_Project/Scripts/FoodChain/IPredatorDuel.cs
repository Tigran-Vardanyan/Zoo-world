using ZooWorld.Animals;

namespace ZooWorld.FoodChain
{
    /// <summary>Picks the survivor when two animals that can eat each other collide.</summary>
    public interface IPredatorDuel
    {
        Animal ChooseWinner(Animal first, Animal second);
    }
}
