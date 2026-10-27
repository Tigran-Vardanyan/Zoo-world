using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.Controls
{
    /// <summary>Finds the living animal under a point on the screen (mouse click or tap).</summary>
    public interface IAnimalPicker
    {
        bool TryPick(Vector2 screenPoint, out Animal animal);
    }
}
