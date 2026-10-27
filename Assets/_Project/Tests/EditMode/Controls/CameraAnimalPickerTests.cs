using NUnit.Framework;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Controls;
using ZooWorld.Tests.Support;
using ZooWorld.World;

namespace ZooWorld.Tests.Controls
{
    public sealed class CameraAnimalPickerTests
    {
        private const int AnimalLayer = 30;
        private const float Radius = 0.5f;

        private AnimalBuilder _animals;
        private GameObject _cameraObject;
        private Camera _camera;
        private CameraAnimalPicker _picker;

        [SetUp]
        public void SetUp()
        {
            _animals = new AnimalBuilder();
            _cameraObject = new GameObject("TopDownCamera");
            _cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 20f, 0f), Quaternion.Euler(90f, 0f, 0f));
            _camera = _cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 10f;
            _picker = new CameraAnimalPicker(new FixedCamera(_camera), new PickerSettings(1 << AnimalLayer, Radius));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cameraObject);
            _animals.Dispose();
        }

        [Test]
        public void ClickOnAnimal_PicksIt()
        {
            var animal = AliveAt(new Vector3(3f, 0.5f, 2f));

            Assert.That(_picker.TryPick(ScreenPointOf(animal.transform.position), out var picked), Is.True);
            Assert.That(picked, Is.SameAs(animal));
        }

        [Test]
        public void ClickSlightlyNextToAnimal_StillPicksIt()
        {
            var animal = AliveAt(new Vector3(-2f, 0.5f, 1f));

            // Collider radius 0.5 + pick radius 0.5: a click 0.8 away from the center still hits.
            Assert.That(_picker.TryPick(ScreenPointOf(animal.transform.position + Vector3.right * 0.8f), out var picked), Is.True);
            Assert.That(picked, Is.SameAs(animal));
        }

        [Test]
        public void ClickOnEmptyGround_PicksNothing()
        {
            AliveAt(new Vector3(-5f, 0.5f, -5f));

            Assert.That(_picker.TryPick(ScreenPointOf(new Vector3(5f, 0f, 5f)), out _), Is.False);
        }

        [Test]
        public void DeadAnimal_IsNotPicked()
        {
            var animal = AliveAt(new Vector3(1f, 0.5f, 1f));
            animal.Die();
            Physics.SyncTransforms();

            Assert.That(_picker.TryPick(ScreenPointOf(animal.transform.position), out _), Is.False);
        }

        [Test]
        public void AnimalOnAnotherLayer_IsIgnored()
        {
            var animal = AliveAt(new Vector3(2f, 0.5f, -2f));
            animal.gameObject.layer = 0;
            Physics.SyncTransforms();

            Assert.That(_picker.TryPick(ScreenPointOf(animal.transform.position), out _), Is.False);
        }

        [Test]
        public void ClickOutsideCameraViewport_PicksNothing()
        {
            var animal = AliveAt(Vector3.zero + Vector3.up * 0.5f);
            _camera.rect = new Rect(0f, 0.25f, 1f, 0.75f);

            // A point in the reserved strip at the bottom of the screen (where the UI panel is).
            var belowViewport = new Vector2(_camera.pixelRect.center.x, _camera.pixelRect.yMin * 0.5f);

            Assert.That(_picker.TryPick(belowViewport, out _), Is.False);
            Assert.That(_picker.TryPick(ScreenPointOf(animal.transform.position), out _), Is.True);
        }

        private Animal AliveAt(Vector3 position)
        {
            var animal = _animals.Create(AnimalRole.Prey);
            animal.gameObject.layer = AnimalLayer;
            animal.Spawn(position, Vector3.forward);
            Physics.SyncTransforms();
            return animal;
        }

        private Vector2 ScreenPointOf(Vector3 worldPosition) => _camera.WorldToScreenPoint(worldPosition);

        private sealed class FixedCamera : IActiveCamera
        {
            public FixedCamera(Camera camera) => Current = camera;

            public Camera Current { get; }
        }
    }
}
