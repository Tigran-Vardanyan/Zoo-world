using UnityEngine;

namespace ZooWorld.World
{
    /// <summary>
    /// Ground area animals are allowed to roam in (the visible part of the screen).
    /// </summary>
    public interface IPlayArea
    {
        /// <summary>The area on the ground plane: x = world X, y = world Z.</summary>
        Rect Bounds { get; }

        bool Contains(Vector3 position);

        /// <summary>Random ground point at least <paramref name="inset"/> away from the area's edges.</summary>
        Vector3 GetRandomPoint(float inset = 0f);

        /// <summary>
        /// Mirrors <paramref name="direction"/> on every axis where <paramref name="position"/>
        /// is outside the area and still moving away from it; otherwise returns it unchanged.
        /// </summary>
        Vector3 RedirectInside(Vector3 position, Vector3 direction);
    }
}
