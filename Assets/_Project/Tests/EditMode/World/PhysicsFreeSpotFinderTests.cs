using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZooWorld.World;

namespace ZooWorld.Tests.World
{
    public sealed class PhysicsFreeSpotFinderTests
    {
        private const int BlockingLayer = 31;

        private readonly List<GameObject> _created = new();
        private ScriptedPlayArea _playArea;
        private PhysicsFreeSpotFinder _finder;

        [SetUp]
        public void SetUp()
        {
            _playArea = new ScriptedPlayArea();
            _finder = new PhysicsFreeSpotFinder(_playArea, new FreeSpotSettings(1 << BlockingLayer, probeHeight: 0.5f, maxAttempts: 3));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var gameObject in _created)
            {
                Object.DestroyImmediate(gameObject);
            }

            _created.Clear();
        }

        [Test]
        public void EmptyArea_ReturnsFirstPoint()
        {
            _playArea.Points.Enqueue(new Vector3(3f, 0f, 3f));

            Assert.That(_finder.TryFind(1f, out var point), Is.True);
            Assert.That(point, Is.EqualTo(new Vector3(3f, 0f, 3f)));
        }

        [Test]
        public void OccupiedPoint_IsSkipped()
        {
            CreateBlocker(Vector3.zero, BlockingLayer);
            _playArea.Points.Enqueue(Vector3.zero);
            _playArea.Points.Enqueue(new Vector3(10f, 0f, 0f));

            Assert.That(_finder.TryFind(1f, out var point), Is.True);
            Assert.That(point, Is.EqualTo(new Vector3(10f, 0f, 0f)));
        }

        [Test]
        public void EveryPointOccupied_ReturnsFalse()
        {
            CreateBlocker(Vector3.zero, BlockingLayer);
            _playArea.Points.Enqueue(Vector3.zero);

            Assert.That(_finder.TryFind(1f, out _), Is.False);
        }

        [Test]
        public void CollidersOnOtherLayers_DoNotBlock()
        {
            CreateBlocker(Vector3.zero, layer: 0);
            _playArea.Points.Enqueue(Vector3.zero);

            Assert.That(_finder.TryFind(1f, out _), Is.True);
        }

        [Test]
        public void ClearanceIsRespected()
        {
            CreateBlocker(new Vector3(2f, 0f, 0f), BlockingLayer);
            _playArea.Points.Enqueue(Vector3.zero);

            Assert.That(_finder.TryFind(0.5f, out _), Is.True, "Blocker is 2 units away, outside a 0.5 clearance.");
            Assert.That(_finder.TryFind(2f, out _), Is.False, "Blocker is inside a 2 unit clearance.");
        }

        private void CreateBlocker(Vector3 groundPosition, int layer)
        {
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.layer = layer;
            blocker.transform.position = groundPosition + Vector3.up * 0.5f;
            _created.Add(blocker);
        }

        /// <summary>Returns the queued points in order, then keeps returning the last one.</summary>
        private sealed class ScriptedPlayArea : IPlayArea
        {
            public Queue<Vector3> Points { get; } = new();

            public Rect Bounds => Rect.MinMaxRect(-100f, -100f, 100f, 100f);
            public bool Contains(Vector3 position) => true;
            public Vector3 GetRandomPoint(float inset = 0f) => Points.Count > 1 ? Points.Dequeue() : Points.Peek();
            public Vector3 RedirectInside(Vector3 position, Vector3 direction) => direction;
        }
    }
}
