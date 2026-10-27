using NUnit.Framework;
using UnityEngine;
using ZooWorld.World;

namespace ZooWorld.Tests.World
{
    public sealed class CameraPlayAreaTests
    {
        private const float FarAway = 1000f;

        private GameObject _cameraObject;
        private CameraPlayArea _playArea;

        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("TopDownCamera");
            _cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 20f, 0f), Quaternion.Euler(90f, 0f, 0f));
            var camera = _cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;

            _playArea = new CameraPlayArea(camera, new PlayAreaSettings());
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_cameraObject);

        [Test]
        public void Contains_CenterIsInside_FarPointIsOutside()
        {
            Assert.That(_playArea.Contains(Vector3.zero), Is.True);
            Assert.That(_playArea.Contains(new Vector3(FarAway, 0f, 0f)), Is.False);
        }

        [Test]
        public void GetRandomPoint_IsInsideTheArea()
        {
            for (var i = 0; i < 50; i++)
            {
                Assert.That(_playArea.Contains(_playArea.GetRandomPoint()), Is.True);
            }
        }

        [Test]
        public void GetRandomPoint_WithInset_KeepsTheWholeInsetCircleInside()
        {
            const float inset = 3f;
            for (var i = 0; i < 50; i++)
            {
                var point = _playArea.GetRandomPoint(inset);
                Assert.That(_playArea.Contains(point + Vector3.right * inset), Is.True);
                Assert.That(_playArea.Contains(point + Vector3.left * inset), Is.True);
                Assert.That(_playArea.Contains(point + Vector3.forward * inset), Is.True);
                Assert.That(_playArea.Contains(point + Vector3.back * inset), Is.True);
            }
        }

        [Test]
        public void RedirectInside_InsideArea_KeepsDirection()
        {
            var direction = new Vector3(1f, 0f, 1f).normalized;

            Assert.That(_playArea.RedirectInside(Vector3.zero, direction), Is.EqualTo(direction));
        }

        [TestCase(1f, 0f)]
        [TestCase(-1f, 0f)]
        [TestCase(0f, 1f)]
        [TestCase(0f, -1f)]
        public void RedirectInside_OutsideAndMovingAway_TurnsBack(float x, float z)
        {
            var outwards = new Vector3(x, 0f, z);
            var position = outwards * FarAway;

            var redirected = _playArea.RedirectInside(position, outwards);

            Assert.That(redirected, Is.EqualTo(-outwards));
        }

        [Test]
        public void RedirectInside_OutsideButAlreadyReturning_KeepsDirection()
        {
            var position = new Vector3(FarAway, 0f, 0f);
            var returning = new Vector3(-1f, 0f, 0.5f);

            Assert.That(_playArea.RedirectInside(position, returning), Is.EqualTo(returning));
        }

        [Test]
        public void RedirectInside_OutsideOnOneAxis_FlipsOnlyThatAxis()
        {
            var position = new Vector3(FarAway, 0f, 0f);

            var redirected = _playArea.RedirectInside(position, new Vector3(1f, 0f, 0.5f));

            Assert.That(redirected, Is.EqualTo(new Vector3(-1f, 0f, 0.5f)));
        }
    }
}
