using UnityEngine;

namespace ZooWorld.World
{
    /// <summary>Finds a random point of the play area that nothing else occupies.</summary>
    public interface IFreeSpotFinder
    {
        /// <summary>
        /// Looks for a ground point with no animal or obstacle within <paramref name="clearance"/>,
        /// and at least <paramref name="clearance"/> away from the play area's edges.
        /// Returns false when every attempt hit something; callers should try again later.
        /// </summary>
        bool TryFind(float clearance, out Vector3 point);
    }
}
