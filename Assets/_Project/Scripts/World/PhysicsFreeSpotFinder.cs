using UnityEngine;

namespace ZooWorld.World
{
    /// <summary>
    /// Tries random points of the play area and keeps the first one whose surroundings
    /// contain no collider on the blocking layers.
    /// </summary>
    public sealed class PhysicsFreeSpotFinder : IFreeSpotFinder
    {
        private readonly IPlayArea _playArea;
        private readonly FreeSpotSettings _settings;

        public PhysicsFreeSpotFinder(IPlayArea playArea, FreeSpotSettings settings)
        {
            _playArea = playArea;
            _settings = settings;
        }

        public bool TryFind(float clearance, out Vector3 point)
        {
            // Objects placed this frame are not known to physics until transforms are synced.
            Physics.SyncTransforms();

            for (var attempt = 0; attempt < _settings.MaxAttempts; attempt++)
            {
                // The whole cleared circle stays inside the play area, so nothing is placed half off-screen.
                var candidate = _playArea.GetRandomPoint(clearance);
                var probe = candidate + Vector3.up * _settings.ProbeHeight;
                if (!Physics.CheckSphere(probe, clearance, _settings.BlockingLayers, QueryTriggerInteraction.Ignore))
                {
                    point = candidate;
                    return true;
                }
            }

            point = default;
            return false;
        }
    }
}
