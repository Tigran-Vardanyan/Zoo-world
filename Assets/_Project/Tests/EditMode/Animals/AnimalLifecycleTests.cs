using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Tests.Support;

namespace ZooWorld.Tests.Animals
{
    public sealed class AnimalLifecycleTests
    {
        private AnimalBuilder _animals;
        private Animal _animal;
        private int _spawned;
        private int _died;
        private int _despawned;

        [SetUp]
        public void SetUp()
        {
            _spawned = _died = _despawned = 0;
            _animals = new AnimalBuilder();
            _animal = _animals.Create(AnimalRole.Prey);
            _animal.Spawned += _ => _spawned++;
            _animal.Died += _ => _died++;
            _animal.Despawned += _ => _despawned++;
            _animal.Spawn(Vector3.zero, Vector3.forward);
        }

        [TearDown]
        public void TearDown() => _animals.Dispose();

        [Test]
        public void Spawn_RaisesSpawned_AndAnimalIsAlive()
        {
            Assert.That(_spawned, Is.EqualTo(1));
            Assert.That(_animal.IsAlive, Is.True);
            Assert.That(_animal.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void Die_LeavesACorpse_InsteadOfRemovingTheAnimal()
        {
            _animal.Die();

            Assert.That(_died, Is.EqualTo(1));
            Assert.That(_despawned, Is.Zero, "The corpse stays until its death animation is over.");
            Assert.That(_animal.IsAlive, Is.False);
            Assert.That(_animal.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void Corpse_DoesNotCollide()
        {
            _animal.Die();

            Assert.That(_animal.GetComponent<Collider>().enabled, Is.False);
            Assert.That(_animal.Body.isKinematic, Is.True);
        }

        [Test]
        public void Despawn_AfterDeath_RaisesDespawnedOnce()
        {
            _animal.Die();

            _animal.Despawn();
            _animal.Despawn();

            Assert.That(_despawned, Is.EqualTo(1));
        }

        [Test]
        public void Despawn_WhileAlive_DoesNothing()
        {
            _animal.Despawn();

            Assert.That(_despawned, Is.Zero);
            Assert.That(_animal.IsAlive, Is.True);
        }

        [Test]
        public void ZeroCorpseDuration_DespawnsRightAway()
        {
            var serialized = new SerializedObject(_animal);
            serialized.FindProperty("_corpseDuration").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _animal.Die();

            Assert.That(_despawned, Is.EqualTo(1));
        }

        [Test]
        public void Respawn_RestoresPhysics()
        {
            _animal.Die();
            _animal.Despawn();

            _animal.Spawn(Vector3.zero, Vector3.forward);

            Assert.That(_animal.IsAlive, Is.True);
            Assert.That(_animal.GetComponent<Collider>().enabled, Is.True);
            Assert.That(_animal.Body.isKinematic, Is.False);
            Assert.That(_spawned, Is.EqualTo(2));
        }
    }
}
