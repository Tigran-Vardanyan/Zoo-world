using UnityEngine;

namespace ZooWorld.World
{
    /// <summary>
    /// The camera the player is currently looking through. It can differ from the arena camera
    /// that defines the play area (e.g. when a free, zoomable camera is used).
    /// </summary>
    public interface IActiveCamera
    {
        Camera Current { get; }
    }
}
