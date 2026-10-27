using UnityEngine;

namespace ZooWorld.Utils
{
    /// <summary>
    /// Helpers for horizontal (XZ-plane) directions.
    /// </summary>
    public static class GroundDirection
    {
        public static Vector3 Random()
        {
            var circle = UnityEngine.Random.insideUnitCircle;
            return circle.sqrMagnitude > Mathf.Epsilon
                ? new Vector3(circle.x, 0f, circle.y).normalized
                : Vector3.forward;
        }

        public static Vector3 Flatten(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.forward;
        }

        /// <summary>
        /// Mirrors <paramref name="direction"/> off a surface when it points into it; otherwise returns it unchanged.
        /// Only the horizontal part of <paramref name="surfaceNormal"/> is used, so landing on top of something is ignored.
        /// </summary>
        public static Vector3 Deflect(Vector3 direction, Vector3 surfaceNormal)
        {
            surfaceNormal.y = 0f;
            if (surfaceNormal.sqrMagnitude < 0.01f)
            {
                return direction;
            }

            surfaceNormal.Normalize();
            return Vector3.Dot(direction, surfaceNormal) < 0f
                ? Flatten(Vector3.Reflect(direction, surfaceNormal))
                : direction;
        }
    }
}
