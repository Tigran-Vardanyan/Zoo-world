using UnityEngine;

namespace ZooWorld.World
{
    /// <summary>
    /// Play area equal to the part of the ground plane visible through the camera.
    /// The rectangle is recalculated lazily whenever the screen size changes.
    /// </summary>
    /// <remarks>
    /// Assumes a top-down camera looking roughly along -Y with screen-up pointing to +Z.
    /// For a tilted perspective camera the visible ground is a trapezoid; the inner rectangle is used.
    /// </remarks>
    public sealed class CameraPlayArea : IPlayArea
    {
        private readonly Camera _camera;
        private readonly PlayAreaSettings _settings;

        private Rect _bounds;
        private Vector2Int _cachedScreenSize;

        public CameraPlayArea(Camera camera, PlayAreaSettings settings)
        {
            _camera = camera;
            _settings = settings;
        }

        private Rect Bounds
        {
            get
            {
                var screenSize = new Vector2Int(Screen.width, Screen.height);
                if (screenSize != _cachedScreenSize)
                {
                    _cachedScreenSize = screenSize;
                    _bounds = CalculateBounds();
                }

                return _bounds;
            }
        }

        public bool Contains(Vector3 position) => Bounds.Contains(new Vector2(position.x, position.z));

        public Vector3 GetRandomPoint(float inset = 0f)
        {
            var bounds = Bounds;
            var insetX = Mathf.Min(inset, bounds.width * 0.5f);
            var insetZ = Mathf.Min(inset, bounds.height * 0.5f);
            return new Vector3(
                Random.Range(bounds.xMin + insetX, bounds.xMax - insetX),
                _settings.GroundHeight,
                Random.Range(bounds.yMin + insetZ, bounds.yMax - insetZ));
        }

        public Vector3 RedirectInside(Vector3 position, Vector3 direction)
        {
            var bounds = Bounds;

            if ((position.x < bounds.xMin && direction.x < 0f) || (position.x > bounds.xMax && direction.x > 0f))
            {
                direction.x = -direction.x;
            }

            if ((position.z < bounds.yMin && direction.z < 0f) || (position.z > bounds.yMax && direction.z > 0f))
            {
                direction.z = -direction.z;
            }

            return direction;
        }

        private Rect CalculateBounds()
        {
            var ground = new Plane(Vector3.up, new Vector3(0f, _settings.GroundHeight, 0f));

            var bottomLeft = ProjectViewportPoint(ground, new Vector2(0f, 0f));
            var topLeft = ProjectViewportPoint(ground, new Vector2(0f, 1f));
            var topRight = ProjectViewportPoint(ground, new Vector2(1f, 1f));
            var bottomRight = ProjectViewportPoint(ground, new Vector2(1f, 0f));

            var xMin = Mathf.Max(bottomLeft.x, topLeft.x) + _settings.Padding;
            var xMax = Mathf.Min(bottomRight.x, topRight.x) - _settings.Padding;
            var zMin = Mathf.Max(bottomLeft.z, bottomRight.z) + _settings.Padding;
            var zMax = Mathf.Min(topLeft.z, topRight.z) - _settings.Padding;

            return Rect.MinMaxRect(xMin, zMin, Mathf.Max(xMin, xMax), Mathf.Max(zMin, zMax));
        }

        private Vector3 ProjectViewportPoint(Plane ground, Vector2 viewportPoint)
        {
            var ray = _camera.ViewportPointToRay(viewportPoint);
            if (!ground.Raycast(ray, out var distance))
            {
                Debug.LogError($"{nameof(CameraPlayArea)}: camera does not see the ground plane at viewport {viewportPoint}.");
                return ray.origin;
            }

            return ray.GetPoint(distance);
        }
    }
}
