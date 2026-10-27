using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Spawning;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.Spawning
{
    public sealed class AnimalCatalogTests
    {
        private AnimalBuilder _animals;
        private AnimalCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _animals = new AnimalBuilder();
            _catalog = ScriptableObject.CreateInstance<AnimalCatalog>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            _animals.Dispose();
        }

        [Test]
        public void PickRandom_EmptyCatalog_ReturnsNull()
        {
            Assert.That(_catalog.PickRandom(), Is.Null);
        }

        [Test]
        public void PickRandom_NeverReturnsZeroWeightOrMissingPrefabs()
        {
            var expected = _animals.Create(AnimalRole.Predator);
            SetEntries((null, 5f), (_animals.Create(AnimalRole.Prey), 0f), (expected, 1f));
            Random.InitState(42);

            for (var i = 0; i < 100; i++)
            {
                Assert.That(_catalog.PickRandom(), Is.SameAs(expected));
            }
        }

        [Test]
        public void PickRandom_FollowsWeights()
        {
            var common = _animals.Create(AnimalRole.Prey);
            var rare = _animals.Create(AnimalRole.Predator);
            SetEntries((common, 9f), (rare, 1f));
            Random.InitState(7);

            var commonCount = 0;
            const int picks = 2000;
            for (var i = 0; i < picks; i++)
            {
                if (_catalog.PickRandom() == common)
                {
                    commonCount++;
                }
            }

            Assert.That(commonCount / (float)picks, Is.EqualTo(0.9f).Within(0.05f));
        }

        private void SetEntries(params (Animal Prefab, float Weight)[] entries)
        {
            var serialized = new SerializedObject(_catalog);
            var list = serialized.FindProperty("_entries");
            list.ClearArray();
            for (var i = 0; i < entries.Length; i++)
            {
                list.InsertArrayElementAtIndex(i);
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_prefab").objectReferenceValue = entries[i].Prefab;
                element.FindPropertyRelative("_weight").floatValue = entries[i].Weight;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
