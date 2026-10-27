using NUnit.Framework;
using UnityEngine;
using ZooWorld.Utils;

namespace ZooWorld.Tests.Utils
{
    public sealed class GroundDirectionTests
    {
        private static readonly Vector3Comparer Approximately = new(1e-4f);

        [Test]
        public void Deflect_HeadOnIntoWall_TurnsAround()
        {
            var deflected = GroundDirection.Deflect(Vector3.right, Vector3.left);

            Assert.That(deflected, Is.EqualTo(Vector3.left).Using(Approximately));
        }

        [Test]
        public void Deflect_AtAnAngle_MirrorsOffTheSurface()
        {
            var direction = new Vector3(1f, 0f, 1f).normalized;

            var deflected = GroundDirection.Deflect(direction, Vector3.left);

            Assert.That(deflected, Is.EqualTo(new Vector3(-1f, 0f, 1f).normalized).Using(Approximately));
        }

        [Test]
        public void Deflect_MovingAwayFromSurface_KeepsDirection()
        {
            Assert.That(GroundDirection.Deflect(Vector3.left, Vector3.left), Is.EqualTo(Vector3.left));
        }

        [Test]
        public void Deflect_LandingOnTop_IgnoresVerticalNormal()
        {
            Assert.That(GroundDirection.Deflect(Vector3.forward, Vector3.up), Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void Deflect_ResultStaysOnTheGround()
        {
            var deflected = GroundDirection.Deflect(Vector3.forward, new Vector3(0f, 0.5f, -1f));

            Assert.That(deflected.y, Is.Zero);
            Assert.That(deflected.magnitude, Is.EqualTo(1f).Within(1e-4f));
        }

        private sealed class Vector3Comparer : System.Collections.Generic.IEqualityComparer<Vector3>
        {
            private readonly float _tolerance;

            public Vector3Comparer(float tolerance) => _tolerance = tolerance;

            public bool Equals(Vector3 a, Vector3 b) => Vector3.Distance(a, b) <= _tolerance;

            public int GetHashCode(Vector3 vector) => 0;
        }
    }
}
