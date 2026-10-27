using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.Controls
{
    /// <summary>
    /// Player-facing actions on the running zoo. Shared by the in-game control panel and the editor
    /// debug window, so both go through exactly the same game services.
    /// </summary>
    public interface IZooCommands
    {
        /// <summary>Animal prefabs that can be spawned, in catalog order.</summary>
        IReadOnlyList<Animal> Species { get; }

        bool IsSpawningPaused { get; set; }
        float TimeScale { get; set; }
        int ObstacleCount { get; }

        /// <summary>Spawns one <paramref name="species"/> at a free spot; null when no free spot was found.</summary>
        Animal Spawn(Animal species);

        /// <summary>Spawns up to <paramref name="count"/> random animals; returns how many found a free spot.</summary>
        int SpawnRandom(int count);

        /// <summary>Spawns both animals next to each other and pushes them together; false when there is no room.</summary>
        bool ForceEncounter(Animal first, Animal second);

        void RebuildObstacles();

        /// <summary>Kills one random living animal; returns it, or null when nobody is alive.</summary>
        Animal KillRandom();

        /// <summary>Kills the living animal under a screen point (click or tap); returns it, or null when there is none.</summary>
        Animal KillAt(Vector2 screenPoint);

        /// <summary>Kills every living animal; returns how many died.</summary>
        int KillAll();
    }
}
